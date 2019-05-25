using BenLib.Framework;
using BenLib.Standard;
using BenLib.WPF;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static Coord.VisualObjects;

namespace Coord
{
    /// <summary>
    /// Plan pouvant contenir des objets visuels utilisant un système de coordonnées.
    /// </summary>
    public class Plane : FrameworkElement
    {
        #region Champs

        private Point m_previousPoint;
        private Point m_outSelectionClick;
        private Character[] m_previousRectContent;

        private readonly CoordinatesSystemManager m_coordinatesSystemManager = new CoordinatesSystemManager();

        private int m_axesTextVisualIndex;
        private VisualObject m_overAxesNumbers;

        private double m_restoreWidthRatio;
        private double m_restoreHeightRatio;

        protected readonly Dictionary<VisualObject, DrawingVisual> Visuals = new Dictionary<VisualObject, DrawingVisual>();
        private readonly VisualObjectRenderer BackgroundVisualObject;
        protected DrawingVisual BackgroundVisual = new DrawingVisual();
        protected DrawingVisual AxesTextVisual = new DrawingVisual();
        protected DrawingVisual SelectionRectangle = new DrawingVisual();
        private Cursor m_restoreCursor;
        private bool m_enableSelectionRect;
        private bool m_moving;
        private bool m_selecting;
        private bool m_charactersScaling;
        private bool m_charactersTranslating;
        private TranslateCharacterEffect m_charactersManipulatingTranslation;
        private ScaleCharacterEffect m_charactersManipulatingScale;

        #endregion

        #region Propriétés

        protected override int VisualChildrenCount => VisualObjects.Count + 3;
        public VisualObjectCollection VisualObjects { get; } = new VisualObjectCollection();

        public TrackingCharacterSelection Selection { get; }
        public CharacterSelection CurrentSelection
        {
            get => Selection.Current;
            set
            {
                foreach (var vo in Selection.Keys.Except(value.Keys).ToArray()) vo.ClearSelection();
                foreach (var kvp in value) kvp.Key.Selection = kvp.Value;
            }
        }

        public VisualObject OverAxesNumbers
        {
            get => m_overAxesNumbers;
            set
            {
                if (value == null || VisualObjects.Contains(value))
                {
                    var old = m_overAxesNumbers;
                    m_overAxesNumbers = value;
                    ComputeAxesTextVisualIndex();
                    OverAxesNumbersChanged?.Invoke(this, new PropertyChangedExtendedEventArgs<VisualObject>("OverAxesNumbers", old, value));
                }
            }
        }
        public int AxesNumbersIndex => OverAxesNumbers == null ? VisualObjects.Count + 2 : VisualObjects.IndexOf(OverAxesNumbers) + 2;

        public AxesNumbers AxesNumbers { get; }
        public AxesVisualObject Axes { get; }
        public GridVisualObject Grid { get; }
        public CadreVisualObject Cadre { get; }

        public bool EnableCharacterEvents { get; set; }
        public bool EnableSelection { get; set; }
        public bool EnableMove { get; set; } = true;
        public bool EnableZoom { get; set; } = true;
        public bool EnableSelectionRect { get => m_enableSelectionRect && EnableSelection; set => m_enableSelectionRect = value; }
        public bool ThroughSelection { get; set; }
        public bool EnableCharactersManipulating { get => Cadre.IsEnabled; set => Cadre.IsEnabled = value; }

        public bool Moving
        {
            get => m_moving;
            private set
            {
                if (m_moving != value)
                {
                    m_moving = value;
                    BehaviorChanged?.Invoke(this, new PropertyChangedExtendedEventArgs<bool>("Moving", !value, value));
                }
            }
        }
        public bool Selecting
        {
            get => m_selecting;
            private set
            {
                if (m_selecting != value)
                {
                    m_selecting = value;
                    BehaviorChanged?.Invoke(this, new PropertyChangedExtendedEventArgs<bool>("Selecting", !value, value));
                }
            }
        }
        public bool CharactersScaling
        {
            get => m_charactersScaling;
            private set
            {
                if (m_charactersScaling != value)
                {
                    m_charactersScaling = value;
                    BehaviorChanged?.Invoke(this, new PropertyChangedExtendedEventArgs<bool>("CharactersScaling", !value, value));
                }
            }
        }
        public bool CharactersTranslating
        {
            get => m_charactersTranslating;
            private set
            {
                if (m_charactersTranslating != value)
                {
                    m_charactersTranslating = value;
                    BehaviorChanged?.Invoke(this, new PropertyChangedExtendedEventArgs<bool>("CharactersTranslating", !value, value));
                }
            }
        }
        public bool CharactersManipulating => CharactersTranslating || CharactersScaling;

        public ReadOnlyCoordinatesSystemManager CoordinatesSystemManager { get; private set; }
        public CoordinatesSystem CoordinatesSystem
        {
            get => m_coordinatesSystemManager.CoordinatesSystem;
            set
            {
                m_coordinatesSystemManager.CoordinatesSystem = value;
                m_coordinatesSystemManager.CoordinatesSystem.Changed += (sender, e) => OnForegroundChanged();
                OnForegroundChanged();
            }
        }
        public MathRect InputRange
        {
            get => m_coordinatesSystemManager.InputRange;
            set
            {
                var old = m_coordinatesSystemManager.InputRange;
                m_coordinatesSystemManager.InputRange = value;
                InputRangeChanged?.Invoke(this, new PropertyChangedExtendedEventArgs<MathRect>("InputRange", old, value));
                RenderAll();
            }
        }

        public bool RenderAtChange { get; set; }
        public bool RenderAtSelectionChange { get; set; } = true;
        public PointVisualObject Origin { get; } = Point(0, 0);
        public Rect Bounds => new Rect(0, 0, ActualWidth, ActualHeight);

        public Vector OutOffset { get; private set; }
        public Vector InOffset { get; private set; }
        public Point OutMousePosition { get; private set; }
        public Point InMousePosition => CoordinatesSystemManager.ComputeInCoordinates(OutMousePosition);
        public Point InMouseMagnetPosition => CoordinatesSystemManager.MagnetIn(InMousePosition);
        public Rect SelectionRect { get; private set; }

        public Cursor RestoreCursor
        {
            get => m_restoreCursor;
            set
            {
                m_restoreCursor = value;
                if (!Moving) Cursor = value ?? Cursors.Arrow;
            }
        }
        public Brush SelectionFill { get; set; } = SystemColors.HighlightBrush.EditFreezable(brush => brush.Opacity = 0.4);
        public PlanePen SelectionStroke { get; set; } = new Pen(SystemColors.HighlightBrush, 1);

        #endregion

        public const double MaxWidthRatio = 200000;
        public const double MaxHeightRatio = 200000;

        public event EventHandler<EventArgs<MouseButtonEventArgs, Character[]>> PreviewCharacterMouseDown;
        public event EventHandler<EventArgs<MouseButtonEventArgs, Character[]>> PreviewCharacterMouseUp;
        public event EventHandler<EventArgs<MouseEventArgs, Character[]>> PreviewCharacterMouseMove;
        public event PropertyChangedExtendedEventHandler<MathRect> InputRangeChanged;
        public event PropertyChangedExtendedEventHandler<VisualObject> OverAxesNumbersChanged;
        public event PropertyChangedExtendedEventHandler<bool> BehaviorChanged;
        public event EventHandler<EventArgs<double, Point>> Zoomed;

        public Plane()
        {
            VisualObjects.CollectionChanged += VisualObjects_CollectionChanged;

            AxesNumbers = new AxesNumbers { Direction = AxesDirection.Both, Fill = Brushes.White };
            Axes = new AxesVisualObject { Direction = AxesDirection.Both, Numbers = AxesNumbers, Stroke = new Pen(Brushes.White, 2.0) };
            Grid = new GridVisualObject { Primary = true, Secondary = true, Fill = Brushes.Black, Stroke = new Pen(Brushes.DeepSkyBlue, 1.0).GetAsTypedFrozen(), SecondaryStroke = new Pen(new SolidColorBrush(Color.FromRgb(0, 53, 72)), 1.0).GetAsTypedFrozen() };
            BackgroundVisualObject = Renderer(Grid, Axes);

            Selection = new TrackingCharacterSelection(this);
            Selection.SelectionChanged += (sender, e) => { if (RenderAtSelectionChange && sender is VisualObject visualObject && visualObject.RenderAtSelectionChange) Render(visualObject); };

            Cadre = new CadreVisualObject { CharacterSelection = Selection, Stroke = new Pen(FlatBrushes.Carrot, 1) };

            Visuals.Add(BackgroundVisualObject, BackgroundVisual);
            Visuals.Add(AxesNumbers, AxesTextVisual);

            AddVisualChild(BackgroundVisual);
            AddVisualChild(AxesTextVisual);
            AddVisualChild(SelectionRectangle);

            Grid.Changed += (sender, e) => OnBackgroundChanged();
            Axes.Changed += (sender, e) => OnBackgroundChanged();
            AxesNumbers.Changed += (sender, e) => OnBackgroundChanged();

            ComputeAxesTextVisualIndex();

            m_coordinatesSystemManager.Changed += (sender, e) => CoordinatesSystemManager = m_coordinatesSystemManager.AsReadOnly();
        }

        #region Méthodes

        #region Children

        protected void AddVisualObject(VisualObject visualObject)
        {
            var addedVisual = new DrawingVisual();
            Visuals.Add(visualObject, addedVisual);
            AddVisualChild(addedVisual);
            visualObject.Changed += OnVisualObjectChanged;
            Render(visualObject);
        }

        protected void RemoveVisualObject(VisualObject visualObject)
        {
            if (OverAxesNumbers == visualObject) OverAxesNumbers = null;
            RemoveVisualChild(Visuals[visualObject]);
            Visuals.Remove(visualObject);
            visualObject.Changed -= OnVisualObjectChanged;
        }

        protected override Visual GetVisualChild(int index) =>
            m_axesTextVisualIndex < 1 ? null :
            index == 0 ? BackgroundVisual :
            index == VisualChildrenCount - 1 ? SelectionRectangle :
            index == m_axesTextVisualIndex ? AxesTextVisual :
            index < m_axesTextVisualIndex ? Visuals[VisualObjects[index - 1]] : Visuals[VisualObjects[index - 2]];

        private void ComputeAxesTextVisualIndex() => m_axesTextVisualIndex = OverAxesNumbers == null ? VisualChildrenCount - 2 : VisualObjects.IndexOf(OverAxesNumbers) + 1;

        public IEnumerable<VisualObject> AllChildren()
        {
            yield return Grid;
            yield return Axes;
            int offset = 0;
            int axesNumbersIndex = AxesNumbersIndex - 2;
            for (int i = 0; i <= VisualObjects.Count; i++)
            {
                if (i == axesNumbersIndex)
                {
                    offset++;
                    yield return AxesNumbers;
                }
                else yield return VisualObjects[i - offset];
            }
        }

        #endregion 

        #region Save

        public BitmapSource GetSourceFrame()
        {
            var rtb = new RenderTargetBitmap((int)ActualWidth, (int)ActualHeight, 96, 96, PixelFormats.Pbgra32);
            for (int i = 0; i < VisualChildrenCount; i++) rtb.Render(GetVisualChild(i));

            return rtb;
        }

        public System.Drawing.Bitmap GetFrame() => GetSourceFrame().ToBitmap();

        public void SaveImage(string fileName)
        {
            var rtb = GetSourceFrame();

            var png = new PngBitmapEncoder();
            png.Frames.Add(BitmapFrame.Create(rtb));

            using (var stream = File.Create(fileName)) png.Save(stream);
        }

        #endregion 

        #region Render

        public void Render(VisualObject visualObject)
        {
            if (visualObject == Grid || visualObject == Axes) Render(BackgroundVisualObject, BackgroundVisual);
            else if (visualObject == AxesNumbers) Render(AxesNumbers, AxesTextVisual);
            else if (Visuals.TryGetValue(visualObject, out var visual)) Render(visualObject, visual);
        }
        public void Render(VisualObject visualObject, DrawingVisual visual)
        {
            using var drawingContext = visual.RenderOpen();
            drawingContext.PushClip(new RectangleGeometry(CoordinatesSystemManager.OutputRange));
            visualObject.Render(drawingContext, CoordinatesSystemManager);
            drawingContext.Pop();
        }

        public void RenderChanged()
        {
            if (BackgroundVisualObject.IsChanged) Render(BackgroundVisualObject, BackgroundVisual);
            if (AxesNumbers.IsChanged) Render(AxesNumbers, AxesTextVisual);
            foreach (var visualObject in VisualObjects.Where(vo => vo.IsChanged)) Render(visualObject);
            RenderSelectionRect();
        }

        public void RenderBackground()
        {
            Render(BackgroundVisualObject, BackgroundVisual);
            Render(AxesNumbers, AxesTextVisual);
        }
        public void RenderForeground()
        {
            foreach (var visualObject in VisualObjects) Render(visualObject);
            RenderSelectionRect();
        }

        public void RenderAll()
        {
            RenderBackground();
            RenderForeground();
        }

        protected void RenderSelectionRect()
        {
            using (var drawingContext = SelectionRectangle.RenderOpen())
            {
                drawingContext.PushClip(new RectangleGeometry(CoordinatesSystemManager.OutputRange));
                drawingContext.DrawRectangle(SelectionFill, SelectionStroke, SelectionRect);
                Cadre.Render(drawingContext, CoordinatesSystemManager);
                drawingContext.Pop();
            }
            Debug.WriteLine($"SelectionRect rendered");
        }

        #endregion

        #region Selection

        #endregion

        #region HitTest

        protected Character[] HitTestCharacters(Point point) => (Input.IsAltPressed() ? AllChildren() : VisualObjects).SelectMany(visualObject => visualObject.HitTestCache(point)).ToArray();
        protected Character[] HitTestCharacters(Rect rect) => (Input.IsAltPressed() ? AllChildren() : VisualObjects).SelectMany(visualObject => visualObject.HitTestCache(rect)).ToArray();

        #endregion

        private bool CheckInputRange(MathRect inputRange)
        {
            var outRange = CoordinatesSystemManager.OutputRange;
            if (outRange.Width / inputRange.Width >= MaxWidthRatio || outRange.Height / inputRange.Height >= MaxHeightRatio) return false;

            try
            {
                decimal newLeft = (decimal)inputRange.Left;
                decimal newRight = (decimal)inputRange.Right;
                decimal newBottom = (decimal)inputRange.Bottom;
                decimal newTop = (decimal)inputRange.Top;

                return true;
            }
            catch { return false; }
        }

        public void ZoomOn(double percentage, Point outAnchorPoint)
        {
            double finalPercentage = 1.0 - percentage;
            var inRange = InputRange;
            var inAnchorPoint = CoordinatesSystemManager.ComputeInCoordinates(outAnchorPoint);

            var newInRange = new MathRect(inRange.X, inRange.Y, inRange.Width * finalPercentage, inRange.Height * finalPercentage);
            if (!CheckInputRange(newInRange)) return;

            m_coordinatesSystemManager.InputRange = newInRange;

            var newOutAnchorPoint = CoordinatesSystemManager.ComputeOutCoordinates(inAnchorPoint);
            var outMove = outAnchorPoint - newOutAnchorPoint;

            var movedNewInRange = InputRange.Move(new Vector(outMove.X / CoordinatesSystemManager.WidthRatio, outMove.Y / CoordinatesSystemManager.HeightRatio));
            if (CheckInputRange(movedNewInRange)) InputRange = movedNewInRange;

            Zoomed?.Invoke(this, EventArgsHelper.Create(percentage, outAnchorPoint));
        }

        #endregion 

        #region Events

        #region Mouse

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            var position = OutMousePosition = e.GetPosition(this);
            var characters = HitTestCharacters(position);

            if (EnableMove && !Moving && e.MiddleButton == MouseButtonState.Pressed) //Moving
            {
                Cursor = Cursors.SizeAll;
                CaptureMouse();
                Moving = true;
            }

            if (EnableSelection && !Selecting && e.LeftButton == MouseButtonState.Pressed)
            {
                if (EnableCharactersManipulating && !Cadre.Focused.IsNaN) //CharactersScaling
                {
                    m_charactersManipulatingScale = new ScaleCharacterEffect { Center = CoordinatesSystemManager.ComputeInCoordinates(Cadre.Focused.Opposite.GetPoint(Cadre.BaseRect)), In = true, ScaleX = 1, ScaleY = 1, Progress = 1 };
                    CharactersScaling = true;
                    Selection.PushEffect(m_charactersManipulatingScale);
                    Cadre.Locked = true;
                }
                else //Selection
                {
                    if (!Input.IsShiftPressed() && !characters.Any(c => c.IsSelected)) Selection.ClearSelection();
                    Selection.Select(ThroughSelection ? characters : characters.Length > 0 ? new[] { characters.Last() } : Array.Empty<Character>(), Input.IsControlPressed());
                    RenderChanged();
                    RenderSelectionRect();

                    if (EnableCharactersManipulating && characters.Length > 0) //CharactersTranslating
                    {
                        m_charactersManipulatingTranslation = new TranslateCharacterEffect { Vector = default, In = true, Progress = 1 };
                        CharactersTranslating = true;
                        Selection.PushEffect(m_charactersManipulatingTranslation);
                        Cadre.Locked = true;
                    }
                }

                if (EnableSelectionRect && !CharactersManipulating && !Selecting && characters.Length == 0) //SelectionRect
                {
                    m_outSelectionClick = position;
                    Selection.Lock();
                    CaptureMouse();
                    Selecting = true;
                }
            }

            if (EnableCharacterEvents) PreviewCharacterMouseDown?.Invoke(this, EventArgsHelper.Create(e, characters));
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            var position = OutMousePosition = e.GetPosition(this);
            var characters = HitTestCharacters(position);

            OutOffset = position - m_previousPoint;
            InOffset = new Vector(OutOffset.X / CoordinatesSystemManager.WidthRatio, -OutOffset.Y / CoordinatesSystemManager.HeightRatio);
            m_previousPoint = position;

            if (e.AllReleased()) Cursor = characters.Length > 0 ? Cursors.Hand : RestoreCursor;

            if (Moving)
            {
                m_outSelectionClick += OutOffset;
                var newInRange = InputRange.Move(new Vector(InOffset.X, -InOffset.Y));
                if (CheckInputRange(newInRange)) InputRange = newInRange;
            }

            if (Selecting)
            {
                SelectionRect = new Rect(m_outSelectionClick, position);
                var rectContent = HitTestCharacters(SelectionRect);
                if (!m_previousRectContent.IsNullOrEmpty()) Selection.UnSelect(m_previousRectContent.Except(rectContent), Input.IsControlPressed());
                Selection.Select(m_previousRectContent.IsNullOrEmpty() ? rectContent : rectContent.Except(m_previousRectContent), Input.IsControlPressed());
                m_previousRectContent = rectContent;
                RenderChanged();
                RenderSelectionRect();
            }

            if (!Selecting && !Moving)
            {
                if (CharactersScaling)
                {
                    Cadre.OutOffset += OutOffset;
                    var scale = m_charactersManipulatingScale;
                    scale.ScaleX = Cadre.NewRect.Width / Cadre.BaseRect.Width;
                    if (Cadre.ReverseX) scale.ScaleX *= -1;
                    scale.ScaleY = Cadre.NewRect.Height / Cadre.BaseRect.Height;
                    if (Cadre.ReverseY) scale.ScaleY *= -1;
                    RenderChanged();
                }
                else if (CharactersTranslating)
                {
                    Cadre.OutOffset += OutOffset;
                    m_charactersManipulatingTranslation.Vector = CoordinatesSystemManager.ComputeInCoordinates(Cadre.OutOffset);
                    RenderChanged();
                }
                else Cursor = Cadre.Contains(position) switch
                    {
                        (0, 0) => Cursors.SizeNWSE, //TopLeft
                        (0.5, 0) => Cursors.SizeNS, //Top
                        (1, 0) => Cursors.SizeNESW, //TopRight
                        (1, 0.5) => Cursors.SizeWE, //Right
                        (1, 1) => Cursors.SizeNWSE, //BottomRight
                        (0.5, 1) => Cursors.SizeNS, //Bottom
                        (0, 1) => Cursors.SizeNESW, //BottomLeft
                        (0, 0.5) => Cursors.SizeWE, //Left
                        _ => Cursor
                    };
            }

            if (EnableCharacterEvents) PreviewCharacterMouseMove?.Invoke(this, EventArgsHelper.Create(e, characters));
        }

        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            var position = OutMousePosition = e.GetPosition(this);
            bool moveEnd = Moving && e.MiddleButton == MouseButtonState.Released;
            bool selectionEnd = Selecting && e.LeftButton == MouseButtonState.Released;
            bool charactersManipulatingEnd = CharactersManipulating && e.LeftButton == MouseButtonState.Released;

            if (moveEnd || selectionEnd || charactersManipulatingEnd)
            {
                if (moveEnd)
                {
                    Moving = false;
                    Cursor = RestoreCursor;
                }
                if (selectionEnd)
                {
                    Selecting = false;
                    SelectionRect = Rect.Empty;
                    m_previousRectContent = null;
                    Selection.UnLock();
                    RenderSelectionRect();
                }
                if (charactersManipulatingEnd)
                {
                    m_charactersManipulatingScale = null;
                    m_charactersManipulatingTranslation = null;
                    CharactersTranslating = CharactersScaling = false;
                    Cadre.Reset();
                    RenderSelectionRect();
                }

                ReleaseMouseCapture();
            }

            if (EnableCharacterEvents) PreviewCharacterMouseUp?.Invoke(this, EventArgsHelper.Create(e, HitTestCharacters(position)));
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            if (EnableZoom)
            {
                var position = OutMousePosition = e.GetPosition(this);
                double percentage = Keyboard.Modifiers switch
                {
                    ModifierKeys.Alt => 0.15,
                    ModifierKeys.Control => 0.01,
                    ModifierKeys.Shift => 0.3,
                    _ => 0.05,
                };

                if (e.Delta > 0) ZoomOn(percentage, position);
                else if (e.Delta < 0) ZoomOn(-percentage, position);
            }
        }

        #endregion

        #region RenderAtChange

        protected void OnBackgroundChanged() { if (RenderAtChange && Grid.RenderAtChange && Axes.RenderAtChange && AxesNumbers.RenderAtChange) RenderBackground(); }
        protected void OnForegroundChanged()
        {
            if (RenderAtChange)
            {
                foreach (var visualObject in VisualObjects.Where(vo => vo.RenderAtChange)) Render(visualObject);
                RenderSelectionRect();
            }
        }
        protected void OnVisualObjectChanged(VisualObject visualObject) { if (RenderAtChange && visualObject.RenderAtChange) Render(visualObject); }
        private void OnVisualObjectChanged(object sender, EventArgs e) => OnVisualObjectChanged(sender as VisualObject);

        #endregion

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            double widthRatio = CoordinatesSystemManager.WidthRatio;
            if (double.IsNaN(widthRatio)) widthRatio = m_restoreWidthRatio;

            double heightRatio = CoordinatesSystemManager.HeightRatio;
            if (double.IsNaN(heightRatio)) heightRatio = m_restoreHeightRatio;

            if (sizeInfo.NewSize.Width == 0.0) m_restoreWidthRatio = widthRatio;
            if (sizeInfo.NewSize.Height == 0.0) m_restoreHeightRatio = heightRatio;

            double diffWidthIn = (sizeInfo.NewSize.Width - sizeInfo.PreviousSize.Width) / widthRatio;
            double diffHeightIn = (sizeInfo.NewSize.Height - sizeInfo.PreviousSize.Height) / heightRatio;

            var bounds = Bounds;
            m_coordinatesSystemManager.OutputRange = bounds;

            var inRange = InputRange;
            var newInRange = new MathRect(inRange.X, inRange.Y - diffHeightIn, inRange.Width + diffWidthIn, inRange.Height + diffHeightIn);
            if (CheckInputRange(newInRange)) InputRange = newInRange;
        }

        private void VisualObjects_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                OverAxesNumbers = null;
                foreach (var visual in Visuals.Values) RemoveVisualChild(visual);
                Visuals.Clear();
                Visuals.Add(BackgroundVisualObject, BackgroundVisual);
                Visuals.Add(AxesNumbers, AxesTextVisual);
            }
            else
            {
                if (e.OldItems != null) { foreach (var visualObject in e.OldItems.OfType<VisualObject>()) RemoveVisualObject(visualObject); }
                if (e.NewItems != null) { foreach (var visualObject in e.NewItems.OfType<VisualObject>()) AddVisualObject(visualObject); }
                ComputeAxesTextVisualIndex();
            }
        }

        #endregion 
    }
}
