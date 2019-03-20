using BenLib;
using BenLib.WPF;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static BenLib.IntInterval;
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
        private Rect m_selectionRect;
        private CharacterSelection m_baseSelection;

        private readonly CoordinatesSystemManager m_coordinatesSystemManager = new CoordinatesSystemManager();

        private int m_axesTextVisualIndex;
        private VisualObject m_overAxesNumbers;

        private double m_restoreWidthRatio;
        private double m_restoreHeightRatio;

        protected readonly Dictionary<VisualObject, DrawingVisual> Visuals = new Dictionary<VisualObject, DrawingVisual>();
        private readonly VisualObjectRenderer BackgroundVisualObject;

        #endregion

        #region Propriétés

        protected DrawingVisual BackgroundVisual = new DrawingVisual();
        protected DrawingVisual AxesTextVisual = new DrawingVisual();
        protected DrawingVisual SelectionRect = new DrawingVisual();
        private Cursor m_restoreCursor;

        protected override int VisualChildrenCount => VisualObjects.Count + 3;
        public NotifyObjectCollection<VisualObject> VisualObjects { get; } = new NotifyObjectCollection<VisualObject>();

        public VisualObject OverAxesNumbers
        {
            get => m_overAxesNumbers;
            set
            {
                if (VisualObjects.Contains(value))
                {
                    m_overAxesNumbers = value;
                    ComputeAxesTextVisualIndex();
                }
            }
        }

        public AxesNumbers AxesNumbers { get; }
        public AxesVisualObject Axes { get; }
        public GridVisualObject Grid { get; }

        public bool EnableCharacterEvents { get; set; }
        public bool EnableSelection { get; set; }
        public bool EnableMove { get; set; } = true;
        public bool EnableSelectionRect { get; set; }
        public bool ThroughSelection { get; set; }

        public bool Moving { get; private set; }
        public bool Selecting { get; private set; }

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
                m_coordinatesSystemManager.InputRange = value;
                RenderAll();
            }
        }

        public bool RenderAtChange { get; set; }
        public PointVisualObject Origin { get; } = Point(0, 0);
        public Rect Bounds => new Rect(0, 0, ActualWidth, ActualHeight);

        public Vector OutOffset { get; private set; }
        public Vector InOffset { get; private set; }
        public Point OutMousePosition { get; private set; }
        public Point InMousePosition => CoordinatesSystemManager.ComputeInCoordinates(OutMousePosition);

        public Cursor RestoreCursor
        {
            get => m_restoreCursor;
            set
            {
                m_restoreCursor = value;
                if (!Moving) Cursor = value ?? Cursors.Arrow;
            }
        }
        public Brush SelectionFill { get; set; } = SystemColors.HighlightBrush.Edit(brush => brush.Opacity = 0.4);
        public Pen SelectionStroke { get; set; } = new Pen(SystemColors.HighlightBrush, 1);

        #endregion 

        public const double MaxWidthRatio = 200000;
        public const double MaxHeightRatio = 200000;

        public event EventHandler<EventArgs<MouseButtonEventArgs, HitTestCharacterResult[]>> PreviewCharacterMouseDown;
        public event EventHandler<EventArgs<MouseButtonEventArgs, HitTestCharacterResult[]>> PreviewCharacterMouseUp;
        public event EventHandler<EventArgs<MouseEventArgs, HitTestCharacterResult[]>> PreviewCharacterMouseMove;

        public Plane()
        {
            VisualObjects.CollectionChanged += VisualObjects_CollectionChanged;

            AxesNumbers = new AxesNumbers(AxesDirection.Both) { Fill = Brushes.White };
            Axes = new AxesVisualObject(AxesDirection.Both, AxesNumbers) { Stroke = new Pen(Brushes.White, 2.0) };
            Grid = new GridVisualObject(true, true) { Fill = Brushes.Black, Stroke = new Pen(Brushes.DeepSkyBlue, 1.0).GetAsTypedFrozen(), SecondaryStroke = new Pen(new SolidColorBrush(Color.FromRgb(0, 53, 72)), 1.0).GetAsTypedFrozen() };
            BackgroundVisualObject = new VisualObjectRenderer(Grid, Axes);

            Visuals.Add(BackgroundVisualObject, BackgroundVisual);
            Visuals.Add(AxesNumbers, AxesTextVisual);

            AddVisualChild(BackgroundVisual);
            AddVisualChild(AxesTextVisual);
            AddVisualChild(SelectionRect);

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
            if (visualObject == null)
            {
                VisualObjects.Remove(visualObject);
                return;
            }

            var addedVisual = new DrawingVisual();
            Visuals.Add(visualObject, addedVisual);
            AddVisualChild(addedVisual);
            visualObject.Changed += (sndr, args) => OnVisualObjectChanged(visualObject);
            Render(visualObject);
        }

        protected void RemoveVisualObject(VisualObject visualObject)
        {
            if (visualObject == null) return;

            if (OverAxesNumbers == visualObject) OverAxesNumbers = null;
            var removedVisual = Visuals[visualObject];
            Visuals.Remove(visualObject);
            RemoveVisualChild(removedVisual);
        }

        protected override Visual GetVisualChild(int index)
        {
            if (m_axesTextVisualIndex < 1) return null;

            if (index == 0) return BackgroundVisual;
            else if (index == VisualChildrenCount - 1) return SelectionRect;
            else if (index == m_axesTextVisualIndex) return AxesTextVisual;
            else if (index < m_axesTextVisualIndex) return Visuals[VisualObjects[index - 1]];
            else return Visuals[VisualObjects[index - 2]];
        }

        private void ComputeAxesTextVisualIndex() => m_axesTextVisualIndex = OverAxesNumbers == null ? VisualChildrenCount - 2 : VisualObjects.IndexOf(OverAxesNumbers) + 1;

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
            if (Visuals.TryGetValue(visualObject, out var visual))
            {
                try { using (var drawingContext = visual.RenderOpen()) visualObject.Render(drawingContext, CoordinatesSystemManager); }
                catch (InvalidOperationException) { }
            }
        }

        public void RenderChanged()
        {
            if (BackgroundVisualObject.IsChanged || AxesNumbers.IsChanged) RenderBackground();
            foreach (var visualObject in VisualObjects.Where(vo => vo.IsChanged)) Render(visualObject);
        }

        public void RenderBackground()
        {
            try
            {
                using (var drawingContext = BackgroundVisual.RenderOpen()) BackgroundVisualObject.Render(drawingContext, CoordinatesSystemManager);
                using (var drawingContext = AxesTextVisual.RenderOpen()) AxesNumbers.Render(drawingContext, CoordinatesSystemManager);
            }
            catch (InvalidOperationException) { }
        }

        public void RenderForeground()
        {
            foreach (var visualObject in VisualObjects) Render(visualObject);
        }

        public void RenderAll()
        {
            RenderBackground();
            RenderForeground();
        }

        protected void RenderSelectionRect() { using (var drawingContext = SelectionRect.RenderOpen()) drawingContext.DrawRectangle(SelectionFill, SelectionStroke, m_selectionRect); }

        #endregion

        #region Selection

        public void ClearSelection() { foreach (var visualObject in VisualObjects) visualObject.Selected = EmptySet; }

        public void Select(HitTestCharacterResult[] characters)
        {
            foreach (var group in characters.GroupBy(hr => hr.Owner))
            {
                IntInterval selected = EmptySet;
                foreach (var hr in group) selected += (hr.Index, hr.Index + 1);
                group.Key.Selected += selected;
            }
            RenderChanged();
        }

        public void SetSelection(CharacterSelection selection)
        {
            ClearSelection();
            foreach (var kvp in selection.Selection) kvp.Key.Selected = kvp.Value;
            RenderChanged();
        }

        public CharacterSelection GetSelection() => new CharacterSelection(VisualObjects.Where(vo => vo.Selected.Length > 0).Select(vo => (vo, vo.Selected)));

        #endregion

        #region HitTest

        protected IEnumerable<HitTestCharacterResult> HitTestCharacters(VisualObject visualObject, Point point) => visualObject is VisualObjectGroupBase visualObjectGroupBase ? visualObjectGroupBase.Children.SelectMany(vo => HitTestCharacters(vo, point)) : visualObject.HitTestCache(point);
        protected HitTestCharacterResult[] HitTestCharacters(Point point) => VisualObjects.SelectMany(visualObject => HitTestCharacters(visualObject, point)).ToArray();

        protected IEnumerable<HitTestCharacterResult> HitTestCharacters(VisualObject visualObject, Rect rect) => visualObject is VisualObjectGroupBase visualObjectGroupBase ? visualObjectGroupBase.Children.SelectMany(vo => HitTestCharacters(vo, rect)) : visualObject.HitTestCache(rect);
        protected HitTestCharacterResult[] HitTestCharacters(Rect rect) => VisualObjects.SelectMany(visualObject => HitTestCharacters(visualObject, rect)).ToArray();

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

            var newInRange = new MathRect(inRange.X, inRange.Y, inRange.Width * (finalPercentage), inRange.Height * (finalPercentage));
            if (!CheckInputRange(newInRange)) return;

            m_coordinatesSystemManager.InputRange = newInRange;

            var newOutAnchorPoint = CoordinatesSystemManager.ComputeOutCoordinates(inAnchorPoint);
            var outMove = outAnchorPoint - newOutAnchorPoint;

            var movedNewInRange = InputRange.Move(new Vector(outMove.X / CoordinatesSystemManager.WidthRatio, outMove.Y / CoordinatesSystemManager.HeightRatio));
            if (CheckInputRange(movedNewInRange)) InputRange = movedNewInRange;
        }

        #endregion 

        #region Events

        #region Mouse

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            var position = OutMousePosition = e.GetPosition(this);
            var characters = HitTestCharacters(position);

            if (EnableSelection && e.OnlyPressed(MouseButton.Left))
            {
                if ((Keyboard.Modifiers & ModifierKeys.Shift) != ModifierKeys.Shift && !GetSelection().ContainsAny(characters)) ClearSelection();
                Select(ThroughSelection ? characters : characters.Length > 0 ? new[] { characters.Last() } : Array.Empty<HitTestCharacterResult>());
            }

            if (EnableMove && !Moving && e.MiddleButton == MouseButtonState.Pressed)
            {
                Cursor = Cursors.SizeAll;
                CaptureMouse();
                Moving = true;
            }

            if (EnableSelection && EnableSelectionRect && !Selecting && e.LeftButton == MouseButtonState.Pressed && characters.Length == 0)
            {
                m_outSelectionClick = position;
                m_baseSelection = (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift ? GetSelection() + new CharacterSelection(characters) : new CharacterSelection(characters);
                SetSelection(m_baseSelection);
                CaptureMouse();
                Selecting = true;
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
                m_selectionRect = new Rect(m_outSelectionClick, position);
                RenderSelectionRect();
                SetSelection(m_baseSelection + new CharacterSelection(HitTestCharacters(m_selectionRect)));
            }

            if (EnableCharacterEvents) PreviewCharacterMouseMove?.Invoke(this, EventArgsHelper.Create(e, characters));
        }

        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            var position = OutMousePosition = e.GetPosition(this);
            bool moveEnd = Moving && e.MiddleButton == MouseButtonState.Released;
            bool selectionEnd = Selecting && e.LeftButton == MouseButtonState.Released;

            if (moveEnd || selectionEnd)
            {
                if (moveEnd)
                {
                    Moving = false;
                    Cursor = RestoreCursor;
                }
                else
                {
                    Selecting = false;
                    SelectionRect.RenderOpen().TryDispose();
                }

                ReleaseMouseCapture();
            }

            if (EnableCharacterEvents) PreviewCharacterMouseUp?.Invoke(this, EventArgsHelper.Create(e, HitTestCharacters(position)));
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
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

        #endregion

        #region RenderAtChange

        protected void OnBackgroundChanged()
        {
            if (RenderAtChange) RenderBackground();
        }

        protected void OnForegroundChanged()
        {
            if (RenderAtChange) RenderForeground();
        }

        protected void OnVisualObjectChanged(VisualObject visualObject)
        {
            if (RenderAtChange) Render(visualObject);
        }

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
            var oldItem = e.OldItems == null ? null : e.OldItems[0] as VisualObject;
            var newItem = e.NewItems == null ? null : e.NewItems[0] as VisualObject;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddVisualObject(newItem);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    RemoveVisualObject(oldItem);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    RemoveVisualObject(oldItem);
                    AddVisualObject(newItem);
                    break;
            }
            ComputeAxesTextVisualIndex();
        }

        #endregion 
    }
}
