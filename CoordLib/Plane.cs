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
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static Coord.VisualObjects;

namespace Coord
{
    /// <summary>
    /// Plan pouvant contenir des objets visuels utilisant un système de coordonnées.
    /// </summary>
    [ContentProperty("Items")]
    public class Plane : FrameworkElement
    {
        #region Champs

        private Point m_outSelectionClick;
        private Character[] m_previousRectContent;
        private Character[] m_clickHitTest;

        private int m_axesTextVisualIndex;

        private double m_restoreWidthRatio;
        private double m_restoreHeightRatio;

        protected readonly Dictionary<VisualObject, DrawingVisual> Visuals = new Dictionary<VisualObject, DrawingVisual>();
        protected DrawingVisual BackgroundVisual = new DrawingVisual();
        protected DrawingVisual AxesTextVisual = new DrawingVisual();
        protected DrawingVisual SelectionRectangle = new DrawingVisual();
        private readonly VisualObject m_backgroundVisualObject;
        private bool m_isMoving;
        private bool m_isSelecting;
        private bool m_isCharactersScaling;
        private bool m_isCharactersTranslating;
        private TranslateTransform m_charactersManipulatingTranslation;
        private ScaleTransform m_charactersManipulatingScale;

        #endregion

        #region Propriétés

        protected override int VisualChildrenCount => VisualObjects.Count + 3;

        public VisualObjectCollection Items { get => (VisualObjectCollection)GetValue(ItemsProperty); set => SetValue(ItemsProperty, value); }
        public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register("Items", typeof(VisualObjectCollection), typeof(Plane));

        public VisualObjectCollection VisualObjects { get; } = new VisualObjectCollection { IsLocked = true };
        public TrackingCharacterSelection Selection { get; }

        public VisualObject OverAxesNumbers { get => (VisualObject)GetValue(OverAxesNumbersProperty); set => SetValue(OverAxesNumbersProperty, value); }
        public static readonly DependencyProperty OverAxesNumbersProperty = DependencyProperty.Register("OverAxesNumbers", typeof(VisualObject), typeof(Plane), new PropertyMetadata { CoerceValueCallback = (d, v) => v is VisualObject value && !(d as Plane).VisualObjects.Contains(value) ? null : v });

        public int AxesNumbersIndex => OverAxesNumbers == null ? VisualObjects.Count + 2 : VisualObjects.IndexOf(OverAxesNumbers) + 2;

        public AxesNumbers AxesNumbers { get; }
        public AxesVisualObject Axes { get; }
        public GridVisualObject Grid { get; }
        public CadreVisualObject Cadre { get; }

        public bool EnableSelection { get => (bool)GetValue(EnableSelectionProperty); set => SetValue(EnableSelectionProperty, value); }
        public static readonly DependencyProperty EnableSelectionProperty = DependencyProperty.Register("EnableSelection", typeof(bool), typeof(Plane));

        public bool EnableMove => MoveDirection != AxesDirection.None;
        public AxesDirection MoveDirection { get => (AxesDirection)GetValue(MoveDirectionProperty); set => SetValue(MoveDirectionProperty, value); }
        public static readonly DependencyProperty MoveDirectionProperty = DependencyProperty.Register("MoveDirection", typeof(AxesDirection), typeof(Plane), new PropertyMetadata(AxesDirection.Both));

        public bool EnableZoom => ZoomDirection != AxesDirection.None;
        public AxesDirection ZoomDirection { get => (AxesDirection)GetValue(ZoomDirectionProperty); set => SetValue(ZoomDirectionProperty, value); }
        public static readonly DependencyProperty ZoomDirectionProperty = DependencyProperty.Register("ZoomDirection", typeof(AxesDirection), typeof(Plane), new PropertyMetadata(AxesDirection.Both));

        public bool EnableSelectionRect { get => (bool)GetValue(EnableSelectionRectProperty); set => SetValue(EnableSelectionRectProperty, value); }
        public static readonly DependencyProperty EnableSelectionRectProperty = DependencyProperty.Register("EnableSelectionRect", typeof(bool), typeof(Plane));

        public bool ThroughSelection { get => (bool)GetValue(ThroughSelectionProperty); set => SetValue(ThroughSelectionProperty, value); }
        public static readonly DependencyProperty ThroughSelectionProperty = DependencyProperty.Register("ThroughSelection", typeof(bool), typeof(Plane));

        public bool EnableCharactersManipulating { get => Cadre.IsEnabled; set => Cadre.IsEnabled = value; }

        public bool IsMoving
        {
            get => m_isMoving;
            private set
            {
                if (m_isMoving != value)
                {
                    m_isMoving = value;
                    BehaviorChanged?.Invoke(this, new PropertyChangedExtendedEventArgs<bool>("IsMoving", !value, value));
                }
            }
        }
        public bool IsSelecting
        {
            get => m_isSelecting;
            private set
            {
                if (m_isSelecting != value)
                {
                    m_isSelecting = value;
                    BehaviorChanged?.Invoke(this, new PropertyChangedExtendedEventArgs<bool>("IsSelecting", !value, value));
                }
            }
        }
        public bool IsCharactersScaling
        {
            get => m_isCharactersScaling;
            private set
            {
                if (m_isCharactersScaling != value)
                {
                    m_isCharactersScaling = value;
                    BehaviorChanged?.Invoke(this, new PropertyChangedExtendedEventArgs<bool>("IsCharactersScaling", !value, value));
                }
            }
        }
        public bool IsCharactersTranslating
        {
            get => m_isCharactersTranslating;
            private set
            {
                if (m_isCharactersTranslating != value)
                {
                    m_isCharactersTranslating = value;
                    BehaviorChanged?.Invoke(this, new PropertyChangedExtendedEventArgs<bool>("IsCharactersTranslating", !value, value));
                }
            }
        }
        public bool IsCharactersManipulating => IsCharactersTranslating || IsCharactersScaling;

        public CoordinatesSystemManager CoordinatesSystemManager { get; } = new CoordinatesSystemManager();
        public ReadOnlyCoordinatesSystemManager ReadOnlyCoordinatesSystemManager { get; private set; }

        public bool RenderAtChange { get => (bool)GetValue(RenderAtChangeProperty); set => SetValue(RenderAtChangeProperty, value); }
        public static readonly DependencyProperty RenderAtChangeProperty = DependencyProperty.Register("RenderAtChange", typeof(bool), typeof(Plane));

        public bool RenderAtSelectionChange { get => (bool)GetValue(RenderAtSelectionChangeProperty); set => SetValue(RenderAtSelectionChangeProperty, value); }
        public static readonly DependencyProperty RenderAtSelectionChangeProperty = DependencyProperty.Register("RenderAtSelectionChange", typeof(bool), typeof(Plane), new PropertyMetadata(true));

        public PointVisualObject Origin { get; } = Point(0, 0);

        public Rect Bounds => new Rect(0, 0, ActualWidth, ActualHeight);

        public Point OutClickPoint { get; private set; }
        public Point InClickPoint { get; private set; }
        public Vector OutOffset { get; private set; }
        public Vector InOffset { get; private set; }
        public Point OutMousePosition { get; private set; }
        public Point InMousePosition { get; private set; }
        public Point InMouseMagnetPosition => ReadOnlyCoordinatesSystemManager.MagnetIn(InMousePosition);
        public Rect SelectionRect { get; private set; }

        public Character[] LastHitTest { get; private set; } = Array.Empty<Character>();
        public Character LastHitTestTop => LastHitTest?.FirstOrDefault();

        public Cursor RestoreCursor { get => (Cursor)GetValue(RestoreCursorProperty); set => SetValue(RestoreCursorProperty, value); }
        public static readonly DependencyProperty RestoreCursorProperty = DependencyProperty.Register("RestoreCursor", typeof(Cursor), typeof(Plane));

        public Brush SelectionRectFill { get => (Brush)GetValue(SelectionRectFillProperty); set => SetValue(SelectionRectFillProperty, value); }
        public static readonly DependencyProperty SelectionRectFillProperty = DependencyProperty.Register("SelectionRectFill", typeof(Brush), typeof(Plane), new PropertyMetadata(SystemColors.HighlightBrush.EditFreezable(brush => brush.Opacity = 0.4)));

        public Pen SelectionRectStroke { get => (Pen)GetValue(SelectionRectStrokeProperty); set => SetValue(SelectionRectStrokeProperty, value); }
        public static readonly DependencyProperty SelectionRectStrokeProperty = DependencyProperty.Register("SelectionRectStroke", typeof(Pen), typeof(Plane), new PropertyMetadata(new Pen(SystemColors.HighlightBrush, 1)));

        #endregion

        public event PropertyChangedExtendedEventHandler<VisualObject> OverAxesNumbersChanged;
        public event PropertyChangedExtendedEventHandler<bool> BehaviorChanged;
        public event EventHandler<EventArgs<double, Point>> Zoomed;

        public Plane()
        {
            Focusable = true;
            VisualObjects.CollectionChanged += VisualObjects_CollectionChanged;

            AxesNumbers = new AxesNumbers { Direction = AxesDirection.Both, Fill = Brushes.White };
            Axes = new AxesVisualObject { Direction = AxesDirection.Both, Stroke = new Pen(Brushes.White, 2.0) };
            Grid = new GridVisualObject { Primary = true, Secondary = true, Fill = Brushes.Black, Stroke = new Pen(Brushes.DeepSkyBlue, 1.0).GetAsTypedFrozen(), SecondaryStroke = new Pen(new SolidColorBrush(Color.FromRgb(0, 53, 72)), 1.0).GetAsTypedFrozen() };
            m_backgroundVisualObject = Renderer(Grid, Axes);

            Selection = new TrackingCharacterSelection(this);
            Selection.Changed += (sender, e) => { if (e.OriginalSource.RenderAtSelectionChange ?? RenderAtSelectionChange) Render(e.OriginalSource); };

            Cadre = new CadreVisualObject { CharacterSelection = Selection, Stroke = new Pen(FlatBrushes.Carrot, 1) };

            Visuals.Add(m_backgroundVisualObject, BackgroundVisual);
            Visuals.Add(AxesNumbers, AxesTextVisual);

            AddVisualChild(BackgroundVisual);
            AddVisualChild(AxesTextVisual);
            AddVisualChild(SelectionRectangle);

            Grid.Changed += (sender, e) => OnBackgroundChanged();
            Axes.Changed += (sender, e) => OnBackgroundChanged();
            AxesNumbers.Changed += (sender, e) => OnBackgroundChanged();

            ComputeAxesTextVisualIndex();
            CoordinatesSystemManager.Changed += (sender, e) =>
            {
                ReadOnlyCoordinatesSystemManager = ((CoordinatesSystemManager)sender).AsReadOnly();
                RenderAll();
            };
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

        public System.Drawing.Bitmap GetFrame() => VisualToBitmapConverter.GetBitmap(this, (int)ActualWidth, (int)ActualHeight);

        public void SaveBitmap(string fileName)
        {
            using var bmp = GetFrame();
            bmp.Save(fileName);
        }

        public BitmapSource GetSourceFrame()
        {
            var rtb = new RenderTargetBitmap((int)ActualWidth, (int)ActualHeight, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(this);
            return rtb;
        }

        public void SaveImage(string fileName)
        {
            var rtb = GetSourceFrame();

            var png = new PngBitmapEncoder();
            png.Frames.Add(BitmapFrame.Create(rtb));

            using var stream = File.Create(fileName);
            png.Save(stream);
        }

        #endregion

        #region Render

        public void RenderdWithoutCache(VisualObject visualObject)
        {
            visualObject.ClearCache();
            Render(visualObject);
        }
        public void Render(VisualObject visualObject)
        {
            if (visualObject == Grid || visualObject == Axes) Render(m_backgroundVisualObject, BackgroundVisual);
            else if (visualObject == AxesNumbers) Render(AxesNumbers, AxesTextVisual);
            else if (Visuals.TryGetValue(visualObject, out var visual)) Render(visualObject, visual);
        }
        public void Render(VisualObject visualObject, DrawingVisual visual)
        {
            var csm = ReadOnlyCoordinatesSystemManager;
            using var drawingContext = visual.RenderOpen();
            drawingContext.PushClip(new RectangleGeometry(csm.OutputRange));
            visualObject.Render(drawingContext, csm);
            drawingContext.Pop();
        }

        public void RenderChanged()
        {
            if (m_backgroundVisualObject.IsChanged) Render(m_backgroundVisualObject, BackgroundVisual);
            if (AxesNumbers.IsChanged) Render(AxesNumbers, AxesTextVisual);
            foreach (var visualObject in VisualObjects.Where(vo => vo.IsChanged)) Render(visualObject);
            RenderSelectionRect();
        }

        public void RenderBackground()
        {
            Render(m_backgroundVisualObject, BackgroundVisual);
            Render(AxesNumbers, AxesTextVisual);
        }
        public void RenderForeground()
        {
            foreach (var visualObject in VisualObjects) Render(visualObject);
            RenderSelectionRect();
        }
        public void RenderForegroundWithoutCache()
        {
            foreach (var visualObject in VisualObjects)
            {
                visualObject.ClearCache();
                Render(visualObject);
            }
            RenderSelectionRect();
        }

        public void RenderAll()
        {
            RenderBackground();
            RenderForeground();
        }

        protected void RenderSelectionRect()
        {
            var csm = ReadOnlyCoordinatesSystemManager;
            using (var drawingContext = SelectionRectangle.RenderOpen())
            {
                drawingContext.PushClip(new RectangleGeometry(csm.OutputRange));
                drawingContext.DrawRectangle(SelectionRectFill, SelectionRectStroke, SelectionRect);
                Cadre.Render(drawingContext, csm);
                drawingContext.Pop();
            }
            Debug.WriteLine($"SelectionRect rendered");
        }

        #endregion

        #region Selection

        #endregion

        #region HitTest

        protected Character[] HitTestCharacters(Point point) => (Input.IsAltPressed() ? AllChildren() : VisualObjects).SelectMany(visualObject => visualObject.HitTestCache(point)).ToArray().Reverse();
        protected Character[] HitTestCharacters(Rect rect) => (Input.IsAltPressed() ? AllChildren() : VisualObjects).SelectMany(visualObject => visualObject.HitTestCache(rect)).ToArray().Reverse();

        #endregion

        public void ZoomOn(double percentage, Point anchorPoint, bool isInAnchorPoint)
        {
            var csm = ReadOnlyCoordinatesSystemManager;
            var direction = ZoomDirection;
            if (direction != AxesDirection.None)
            {
                var (inAnchorPoint, outAnchorPoint) = isInAnchorPoint ? (anchorPoint, csm.ComputeOutCoordinates(anchorPoint)) : (csm.ComputeInCoordinates(anchorPoint), anchorPoint);
                double finalPercentage = 1.0 - percentage;
                var inRange = CoordinatesSystemManager.InputRange;

                CoordinatesSystemManager.InputRange = new MathRect(inRange.X, inRange.Y, inRange.Width * (direction.HasFlag(AxesDirection.Horizontal) ? finalPercentage : 1), inRange.Height * (direction.HasFlag(AxesDirection.Vertical) ? finalPercentage : 1));

                csm = ReadOnlyCoordinatesSystemManager;
                var newInAnchorPoint = csm.ComputeInCoordinates(outAnchorPoint);
                var inMove = newInAnchorPoint - inAnchorPoint;

                CoordinatesSystemManager.InputRange = CoordinatesSystemManager.InputRange.Move(new Vector(inMove.X, inMove.Y));

                Zoomed?.Invoke(this, EventArgsHelper.Create(percentage, inAnchorPoint));
            }
        }

        #endregion 

        #region Events

        #region Mouse

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            var csm = ReadOnlyCoordinatesSystemManager;
            var characters = LastHitTest = HitTestCharacters(e.GetPosition(this));
            var charactersTop = LastHitTestTop;

            Focus();
            CaptureMouse();

            if (EnableMove && !IsMoving && e.MiddleButton == MouseButtonState.Pressed) //Moving
            {
                Cursor = Cursors.SizeAll;
                IsMoving = true;
            }

            if (EnableSelection && !IsSelecting && e.LeftButton == MouseButtonState.Pressed)
            {
                if (EnableCharactersManipulating && !Cadre.Focused.IsNaN) //CharactersScaling
                {
                    m_charactersManipulatingScale = new ScaleTransform { Center = csm.ComputeInCoordinates(Cadre.Focused.Opposite.GetPoint(Cadre.BaseRect)), In = true, ScaleX = 1, ScaleY = 1 };
                    IsCharactersScaling = true;
                    Selection.PushTransform(m_charactersManipulatingScale);
                    Cadre.Locked = true;
                }
                else //Selection
                {
                    if (!Input.IsShiftPressed() && !characters.Any(c => c.IsSelected)) Selection.ClearSelection();
                    Selection.Select(ThroughSelection ? characters : characters.Length > 0 ? new[] { charactersTop } : Array.Empty<Character>(), Input.IsControlPressed());
                    RenderChanged();
                    RenderSelectionRect();

                    if (EnableCharactersManipulating && characters.Length > 0) //CharactersTranslating
                    {
                        m_charactersManipulatingTranslation = new TranslateTransform { Offset = default, In = true };
                        IsCharactersTranslating = true;
                        Selection.PushTransform(m_charactersManipulatingTranslation);
                        Cadre.Locked = true;
                    }
                }

                if (EnableSelectionRect && Selection.AllowMultiple && !IsCharactersManipulating && !IsSelecting && characters.Length == 0) //SelectionRect
                {
                    m_outSelectionClick = OutMousePosition;
                    Selection.Lock();
                    IsSelecting = true;
                }
            }

            if (e.OnlyPressed(MouseButton.Left))
            {
                OutClickPoint = OutMousePosition;
                InClickPoint = InMousePosition;
                m_clickHitTest = characters;
                if (charactersTop is Character character) character.Owner.OnMouseDown(InMousePosition, character);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            var csm = ReadOnlyCoordinatesSystemManager;

            var oldOutPosition = OutMousePosition;
            var oldInPosition = InMousePosition;

            var outPosition = OutMousePosition = e.GetPosition(this);
            var inPosition = InMousePosition = csm.ComputeInCoordinates(outPosition);

            var lastLastCharacter = LastHitTestTop;
            LastHitTest = HitTestCharacters(outPosition);
            var lastCharacter = LastHitTestTop;

            var outOffset = OutOffset = outPosition - oldOutPosition;
            var inOffset = InOffset = csm.ComputeInCoordinates(outOffset);
            if (e.AllReleased()) Cursor = EnableSelection && lastCharacter != null && lastCharacter.Owner.IsSelectable && (Selection.Filter?.Invoke(lastCharacter.Owner) ?? true) ? Cursors.Hand : RestoreCursor;

            if (IsMoving)
            {
                var direction = MoveDirection;
                m_outSelectionClick += outOffset;
                var newInRange = CoordinatesSystemManager.InputRange.Move(new Vector(direction.HasFlag(AxesDirection.Horizontal) ? inOffset.X : 0, direction.HasFlag(AxesDirection.Vertical) ? inOffset.Y : 0));
                CoordinatesSystemManager.InputRange = newInRange;
            }

            if (IsSelecting)
            {
                SelectionRect = new Rect(m_outSelectionClick, outPosition);
                var rectContent = HitTestCharacters(SelectionRect);
                if (!m_previousRectContent.IsNullOrEmpty()) Selection.UnSelect(m_previousRectContent.Except(rectContent), Input.IsControlPressed(), rectContent);
                Selection.Select(m_previousRectContent.IsNullOrEmpty() ? rectContent : rectContent.Except(m_previousRectContent), Input.IsControlPressed());
                m_previousRectContent = rectContent;
                RenderChanged();
                RenderSelectionRect();
            }

            if (!IsSelecting && !IsMoving)
            {
                if (IsCharactersScaling)
                {
                    Cadre.OutOffset += outOffset;
                    var scale = m_charactersManipulatingScale;
                    double scaleX = Cadre.NewRect.Width / Cadre.BaseRect.Width;
                    if (Cadre.ReverseX) scaleX *= -1;
                    double scaleY = Cadre.NewRect.Height / Cadre.BaseRect.Height;
                    if (Cadre.ReverseY) scaleY *= -1;
                    if (Input.IsShiftPressed()) scaleX = scaleY = Math.Abs(scaleX) < Math.Abs(scaleY) ? scaleX : scaleY;
                    scale.ScaleX = scaleX;
                    scale.ScaleY = scaleY;
                    RenderChanged();
                }
                else if (IsCharactersTranslating)
                {
                    Cadre.OutOffset += outOffset;
                    m_charactersManipulatingTranslation.Offset = csm.ComputeInCoordinates(Cadre.OutOffset);
                    RenderChanged();
                }
                else
                {
                    if (e.LeftButton == MouseButtonState.Pressed && !m_clickHitTest.IsNullOrEmpty())
                    {
                        var c = m_clickHitTest.First();
                        var (oldPos, pos) = Input.IsShiftPressed() ? (csm.MagnetIn(oldInPosition), csm.MagnetIn(inPosition)) : (oldInPosition, inPosition);
                        pos = pos.Trim(CoordinatesSystemManager.InputRangeLimits);
                        c.Owner.Move(pos, pos - InClickPoint, pos - oldPos, c);
                    }
                    Cursor = Cadre.Contains(outPosition) switch
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
            }

            if (lastCharacter != lastLastCharacter && (lastCharacter == null ^ lastLastCharacter == null || lastCharacter.Index != lastLastCharacter.Index || lastCharacter.Owner != lastLastCharacter.Owner))
            {
                if (lastCharacter != null) lastCharacter.Owner.OnMouseEnter(inPosition, lastCharacter);
                if (lastLastCharacter != null) lastLastCharacter.Owner.OnMouseLeave(inPosition, lastLastCharacter);
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            var lastCharacter = LastHitTestTop;
            ReleaseMouseCapture();

            bool moveEnd = IsMoving && e.MiddleButton == MouseButtonState.Released;
            bool selectionEnd = IsSelecting && e.LeftButton == MouseButtonState.Released;
            bool charactersManipulatingEnd = IsCharactersManipulating && e.LeftButton == MouseButtonState.Released;

            if (moveEnd || selectionEnd || charactersManipulatingEnd)
            {
                if (moveEnd)
                {
                    IsMoving = false;
                    Cursor = RestoreCursor;
                }
                if (selectionEnd)
                {
                    IsSelecting = false;
                    SelectionRect = Rect.Empty;
                    m_previousRectContent = null;
                    Selection.UnLock();
                    RenderSelectionRect();
                }
                if (charactersManipulatingEnd)
                {
                    m_charactersManipulatingScale = null;
                    m_charactersManipulatingTranslation = null;
                    IsCharactersTranslating = IsCharactersScaling = false;
                    Cadre.Reset();
                    RenderSelectionRect();
                }
            }

            if (e.LeftButton == MouseButtonState.Released) OutClickPoint = InClickPoint = default;
            if (lastCharacter != null) lastCharacter.Owner.OnMouseUp(InMousePosition, lastCharacter);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            if (EnableZoom)
            {
                double percentage = Keyboard.Modifiers switch
                {
                    ModifierKeys.Alt => 0.2,
                    ModifierKeys.Control => 0.02,
                    ModifierKeys.Shift => 0.4,
                    _ => 0.1,
                };

                ZoomOn(e.Delta > 0 ? percentage : -percentage, OutMousePosition, false);
            }
        }

        #endregion

        #region RenderAtChange

        protected void OnBackgroundChanged() { if (CanRenderAtChange(Grid) && CanRenderAtChange(Axes) && CanRenderAtChange(AxesNumbers)) RenderBackground(); }
        protected void OnForegroundChanged()
        {
            foreach (var visualObject in VisualObjects.Where(vo => CanRenderAtChange(vo))) Render(visualObject);
            RenderSelectionRect();
        }
        protected void OnVisualObjectChanged(VisualObject visualObject) { if (CanRenderAtChange(visualObject)) Render(visualObject); }
        private void OnVisualObjectChanged(object sender, EventArgs e) => OnVisualObjectChanged(sender as VisualObject);

        private bool CanRenderAtChange(VisualObject visualObject) => visualObject.RenderAtChange ?? RenderAtChange;

        #endregion

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == ItemsProperty)
            {
                VisualObjects.IsLocked = false;

                if (e.OldValue is VisualObjectCollection oldVisualObjects)
                {
                    oldVisualObjects.CollectionChanged -= Items_CollectionChanged;
                    VisualObjects.Clear();
                }
                if (e.NewValue is VisualObjectCollection newVisualObjects)
                {
                    newVisualObjects.CollectionChanged += Items_CollectionChanged;
                    VisualObjects.AddRange(newVisualObjects);
                }

                VisualObjects.IsLocked = true;
            }
            else if (e.Property == OverAxesNumbersProperty)
            {
                ComputeAxesTextVisualIndex();
                OverAxesNumbersChanged?.Invoke(this, new PropertyChangedExtendedEventArgs<VisualObject>("OverAxesNumbers", (VisualObject)e.OldValue, (VisualObject)e.NewValue));
            }
            else if (e.Property == RestoreCursorProperty) { if (!IsMoving) Cursor = (e.NewValue as Cursor) ?? Cursors.Arrow; }
            base.OnPropertyChanged(e);
        }

        private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            VisualObjects.IsLocked = false;

            if (e.Action == NotifyCollectionChangedAction.Reset) VisualObjects.Clear();
            else if (e.Action == NotifyCollectionChangedAction.Move) VisualObjects.Move(e.OldStartingIndex, e.NewStartingIndex);
            else if (e.Action == NotifyCollectionChangedAction.Replace) VisualObjects[e.OldStartingIndex] = (VisualObject)e.NewItems[0];
            else
            {
                if (e.OldItems != null)
                {
                    for (int i = 0; i < e.OldItems.Count; i++)
                    {
                        VisualObjects.RemoveAt(e.OldStartingIndex + i);
                    }
                }
                if (e.NewItems != null)
                {
                    for (int i = 0; i < e.NewItems.Count; i++)
                    {
                        VisualObjects.Insert(e.NewStartingIndex + i, (VisualObject)e.NewItems[i]);
                    }
                }
            }

            VisualObjects.IsLocked = true;
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            var csm = ReadOnlyCoordinatesSystemManager;
            double widthRatio = csm.WidthRatio;
            if (double.IsNaN(widthRatio)) widthRatio = m_restoreWidthRatio;

            double heightRatio = csm.HeightRatio;
            if (double.IsNaN(heightRatio)) heightRatio = m_restoreHeightRatio;

            if (sizeInfo.NewSize.Width == 0.0) m_restoreWidthRatio = widthRatio;
            if (sizeInfo.NewSize.Height == 0.0) m_restoreHeightRatio = heightRatio;

            double diffWidthIn = (sizeInfo.NewSize.Width - sizeInfo.PreviousSize.Width) / widthRatio;
            double diffHeightIn = (sizeInfo.NewSize.Height - sizeInfo.PreviousSize.Height) / heightRatio;

            var bounds = Bounds;
            CoordinatesSystemManager.OutputRange = bounds;

            var inRange = CoordinatesSystemManager.InputRange;
            var newInRange = new MathRect(inRange.X, inRange.Y - diffHeightIn, inRange.Width + diffWidthIn, inRange.Height + diffHeightIn);
            CoordinatesSystemManager.InputRange = newInRange;
        }

        private void VisualObjects_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                OverAxesNumbers = null;
                foreach (var visual in Visuals.Values) RemoveVisualChild(visual);
                Visuals.Clear();
                Visuals.Add(m_backgroundVisualObject, BackgroundVisual);
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
