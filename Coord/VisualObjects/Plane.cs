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

namespace Coord
{
    /// <summary>
    /// Plan pouvant contenir des objets visuels utilisant un système de coordonnées.
    /// </summary>
    public class Plane : FrameworkElement
    {
        protected DrawingVisual BackgroundVisual = new DrawingVisual();
        protected DrawingVisual AxesTextVisual = new DrawingVisual();

        public Rect Bounds => new Rect(0, 0, ActualWidth, ActualHeight);

        public PointVisualObject Origin { get; } = new Point(0.0, 0.0);

        public bool RenderAtChange { get; set; } = true;

        private CoordinatesSystemManager m_coordinatesSystemManager = new CoordinatesSystemManager();
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

        protected readonly Dictionary<VisualObject, DrawingVisual> Visuals = new Dictionary<VisualObject, DrawingVisual>();
        public NotifyObjectCollection<VisualObject> VisualObjects { get; } = new NotifyObjectCollection<VisualObject>();

        private int m_axesTextVisualIndex;
        private VisualObject m_overAxesNumbers;

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
        private readonly VisualObjectRenderer BackgroundVisualObject;

        private bool m_clicking;
        private Point m_previousPoint;

        private double m_restoreWidthRatio;
        private double m_restoreHeightRatio;

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

            Grid.Changed += (sender, e) => OnBackgroundChanged();
            Axes.Changed += (sender, e) => OnBackgroundChanged();
            AxesNumbers.Changed += (sender, e) => OnBackgroundChanged();

            ComputeAxesTextVisualIndex();
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

        protected override int VisualChildrenCount => VisualObjects.Count + 2;

        protected override Visual GetVisualChild(int index)
        {
            if (m_axesTextVisualIndex < 1) return null;

            if (index == 0) return BackgroundVisual;
            else if (index == m_axesTextVisualIndex) return AxesTextVisual;
            else if (index < m_axesTextVisualIndex) return Visuals[VisualObjects[index - 1]];
            else return Visuals[VisualObjects[index - 2]];
        }

        private void ComputeAxesTextVisualIndex() => m_axesTextVisualIndex = OverAxesNumbers == null ? VisualChildrenCount - 1 : VisualObjects.IndexOf(OverAxesNumbers) + 1;

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

        public void Render(VisualObject visualObject)
        {
            if (Visuals.TryGetValue(visualObject, out var visual))
            {
                try { using (var drawingContext = visual.RenderOpen()) visualObject.Render(drawingContext, m_coordinatesSystemManager); }
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
                using (var drawingContext = BackgroundVisual.RenderOpen()) BackgroundVisualObject.Render(drawingContext, m_coordinatesSystemManager);
                using (var drawingContext = AxesTextVisual.RenderOpen()) AxesNumbers.Render(drawingContext, m_coordinatesSystemManager);
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

        public const double MaxWidthRatio = 200000.0;
        public const double MaxHeightRatio = 200000.0;

        private bool CheckInputRange(MathRect inputRange)
        {
            var outRange = m_coordinatesSystemManager.OutputRange;
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
            var finalPercentage = 1.0 - percentage;
            var inRange = InputRange;
            var inAnchorPoint = m_coordinatesSystemManager.ComputeInCoordinates(outAnchorPoint);

            var newInRange = new MathRect(inRange.X, inRange.Y, inRange.Width * (finalPercentage), inRange.Height * (finalPercentage));
            if (!CheckInputRange(newInRange)) return;

            m_coordinatesSystemManager.InputRange = newInRange;

            var newOutAnchorPoint = m_coordinatesSystemManager.ComputeOutCoordinates(inAnchorPoint);
            var outMove = outAnchorPoint - newOutAnchorPoint;

            var movedNewInRange = InputRange.Move(new Vector(outMove.X / m_coordinatesSystemManager.WidthRatio, outMove.Y / m_coordinatesSystemManager.HeightRatio));
            if (CheckInputRange(movedNewInRange)) InputRange = movedNewInRange;
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            var widthRatio = m_coordinatesSystemManager.WidthRatio;
            if (double.IsNaN(widthRatio)) widthRatio = m_restoreWidthRatio;

            var heightRatio = m_coordinatesSystemManager.HeightRatio;
            if (double.IsNaN(heightRatio)) heightRatio = m_restoreHeightRatio;

            if (sizeInfo.NewSize.Width == 0.0) m_restoreWidthRatio = widthRatio;
            if (sizeInfo.NewSize.Height == 0.0) m_restoreHeightRatio = heightRatio;

            var diffWidthIn = (sizeInfo.NewSize.Width - sizeInfo.PreviousSize.Width) / widthRatio;
            var diffHeightIn = (sizeInfo.NewSize.Height - sizeInfo.PreviousSize.Height) / heightRatio;

            var bounds = Bounds;
            m_coordinatesSystemManager.OutputRange = bounds;

            var inRange = InputRange;
            var newInRange = new MathRect(inRange.X, inRange.Y - diffHeightIn, inRange.Width + diffWidthIn, inRange.Height + diffHeightIn);
            if (CheckInputRange(newInRange)) InputRange = newInRange;
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            m_previousPoint = e.GetPosition(this);
            m_clicking = true;
            CaptureMouse();
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            m_clicking = false;
            ReleaseMouseCapture();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (m_clicking)
            {
                var position = e.GetPosition(this);

                var outOffset = position - m_previousPoint;
                var inOffset = new Vector(outOffset.X / m_coordinatesSystemManager.WidthRatio, outOffset.Y / m_coordinatesSystemManager.HeightRatio);

                var newInRange = InputRange.Move(inOffset);
                if (CheckInputRange(newInRange)) InputRange = newInRange;

                m_previousPoint = position;
            }
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            var inPosition = e.GetPosition(this);

            double percentage;

            switch (Keyboard.Modifiers)
            {
                case ModifierKeys.Alt:
                    percentage = 0.15;
                    break;
                case ModifierKeys.Control:
                    percentage = 0.01;
                    break;
                case ModifierKeys.Shift:
                    percentage = 0.3;
                    break;
                default:
                    percentage = 0.05;
                    break;
            }

            if (e.Delta > 0) ZoomOn(percentage, inPosition);
            else if (e.Delta < 0) ZoomOn(-percentage, inPosition);
        }
    }
}
