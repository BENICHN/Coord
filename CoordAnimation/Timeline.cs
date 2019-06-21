using BenLib.Framework;
using BenLib.Standard;
using BenLib.WPF;
using Coord;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using static CoordAnimation.TimelineGraphics;

namespace CoordAnimation
{
    public static class TimelineGraphics
    {
        public const double HeaderHeight = 30;
        public const double Space = 30;

        public static Character Cursor { get; set; }
        public static (Character Left, Character Right) DiscreteKeyFrame { get; set; }
        public static (Character Left, Character Right) LinearKeyFrame { get; set; }
        public static (Character Left, Character Right) EasingKeyFrame { get; set; }
        public static (Character Left, Character Right) SplineKeyFrame { get; set; }

        public static (Character Left, Character Right) FromKeyFrame<T>(AbsoluteKeyFrame<T> keyFrame, AbsoluteKeyFrame<T> nextKeyFrame)
        {
            var left = keyFrame switch
            {
                DiscreteAbsoluteKeyFrame<T> _ => DiscreteKeyFrame.Left,
                LinearAbsoluteKeyFrame<T> _ => LinearKeyFrame.Left,
                EasingAbsoluteKeyFrame<T> _ => EasingKeyFrame.Left,
                SplineAbsoluteKeyFrame<T> s => s.KeySpline.ControlPoint2 == new Point(1, 1) ? LinearKeyFrame.Left : SplineKeyFrame.Left,
                _ => default
            };

            var right = nextKeyFrame switch
            {
                DiscreteAbsoluteKeyFrame<T> _ => DiscreteKeyFrame.Right,
                LinearAbsoluteKeyFrame<T> _ => LinearKeyFrame.Right,
                EasingAbsoluteKeyFrame<T> _ => EasingKeyFrame.Right,
                SplineAbsoluteKeyFrame<T> s => s.KeySpline.ControlPoint1 == default ? LinearKeyFrame.Right : SplineKeyFrame.Right,
                _ => LinearKeyFrame.Right
            };

            return (left.Clone(), right.Clone());
        }
    }

    public class TimelineHeader : VisualObject
    {
        protected override Freezable CreateInstanceCore() => new TimelineHeader();
        public override string Type => "TimelineHeader";

        static TimelineHeader() => OverrideDefaultValue<TimelineHeader, bool>(IsSelectableProperty, false);

        protected override IEnumerable<Character> GetCharactersCore(ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            yield return Character.Rectangle(new Rect(0, 0, coordinatesSystemManager.OutputRange.Width, HeaderHeight)).Color(FlatBrushes.WetAsphalt);
            foreach (double i in coordinatesSystemManager.GetHorizontalSteps().Where(i => Math.Truncate(i) == i))
            {
                double oi = coordinatesSystemManager.ComputeOutOrthonormalXCoordinate(i);
                yield return Character.Text(new Point(oi, 0), AxesNumbers.FormatAxisNumber(i).ToString(), new Typeface("Segoe UI"), HeaderHeight / 3, TextAlignment.Center).Color(Brushes.White);
                yield return Character.Line(new Point(oi, HeaderHeight / 3 + HeaderHeight / 6), new Point(oi, HeaderHeight)).Color(new Pen(Brushes.White, 1));
            }
        }

        protected override void OnMouseDown(Point inPosition, Character hitTest) => PropertiesAnimation.GeneralTime = inPosition.X.TrimToLong().Trim(0, long.MaxValue);
        protected override void Move(Point inPosition, Vector totalInOffset, Vector inOffset, Character clickHitTest) => PropertiesAnimation.GeneralTime = inPosition.X.TrimToLong().Trim(0, long.MaxValue);
    }

    public class TimelineLimits : VisualObject
    {
        public override string Type => "TimelineLimits";
        protected override Freezable CreateInstanceCore() => new TimelineLimits();

        static TimelineLimits() => OverrideDefaultValue<TimelineLimits, bool>(IsSelectableProperty, false);

        protected override IReadOnlyCollection<Character> GetCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            var offset = coordinatesSystemManager.ComputeOutCoordinates(new Vector(0, -coordinatesSystemManager.InputRange.Bottom));
            var pen = new Pen(Brushes.White, 0.75) { DashStyle = new DashStyle(new double[] { 2 }, 0) };
            return new[]
            {
                Character.Line(new Point(0, HeaderHeight + Space + 8) + offset, new Point(coordinatesSystemManager.OutputRange.Right, HeaderHeight + Space + 8) + offset).Color(pen),
                Character.Line(new Point(0, coordinatesSystemManager.OutputRange.Bottom - Space + 8) + offset, new Point(coordinatesSystemManager.OutputRange.Right, coordinatesSystemManager.OutputRange.Bottom - Space + 8) + offset).Color(pen)
            };
        }
    }

    public class TimelineCursor : VisualObject
    {
        private long m_baseValue;

        public override string Type => "TimelineCursor";
        protected override Freezable CreateInstanceCore() => new TimelineCursor();

        public Point InCurrentPoint { get => (Point)GetValue(InCurrentPointProperty); set => SetValue(InCurrentPointProperty, value); }
        public static readonly DependencyProperty InCurrentPointProperty = CreateProperty<TimelineCursor, Point>(true, true, true, "InCurrentPoint", new Point(double.NaN, double.NaN));

        public long Time { get => (long)GetValue(TimeProperty); set => SetValue(TimeProperty, value); }
        public static readonly DependencyProperty TimeProperty = CreateProperty<TimelineCursor, long>(true, true, true, "Time");

        static TimelineCursor() => OverrideDefaultValue<TimelineCursor, bool>(IsSelectableProperty, false);
        public TimelineCursor() => PropertiesAnimation.GeneralTimeChanged += (sender, e) => Time = e.NewValue;

        protected override void Move(Point inPosition, Vector totalInOffset, Vector inOffset, Character clickHitTest)
        {
            if (clickHitTest.Data.Equals(1))
            {
                if (totalInOffset == inOffset) m_baseValue = PropertiesAnimation.GeneralTime;
                PropertiesAnimation.GeneralTime = (m_baseValue + totalInOffset.X).TrimToLong().Trim(0, long.MaxValue);
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == TimeProperty) PropertiesAnimation.GeneralTime = (long)e.NewValue;
            base.OnPropertyChanged(e);
        }

        protected override IReadOnlyCollection<Character> GetCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            return GetcharactersCore().ToArray();
            IEnumerable<Character> GetcharactersCore()
            {
                double t = Time;
                double ot = coordinatesSystemManager.ComputeOutOrthonormalXCoordinate(t);
                var c = Cursor.Clone().WithData(1);
                c.Transform.Translate(ot, HeaderHeight);
                yield return c;
                yield return Character.Line(new Point(ot, HeaderHeight), new Point(ot, coordinatesSystemManager.OutputRange.Bottom)).Color(new Pen(FlatBrushes.BelizeHole, 1)).WithData(0).HideSelection();
                yield return Character.Ellipse(TimelineCurveSeries<object>.ArrangeProgress(new[] { coordinatesSystemManager.ComputeOutCoordinates(InCurrentPoint) }, coordinatesSystemManager).First(), 5, 5).Color(Brushes.Black, new Pen(Brushes.White, 1));
            }
        }
    }

    public class TimelineCurveSeries<T> : FunctionSeries
    {
        protected override Freezable CreateInstanceCore() => new TimelineCurveSeries<T>();

        public new Func<double, double> Function => base.Function;

        public AbsoluteKeyFrameCollection<T> KeyFrames { get => (AbsoluteKeyFrameCollection<T>)GetValue(KeyFramesProperty); set => SetValue(KeyFramesProperty, value); }
        public static readonly DependencyProperty KeyFramesProperty = CreateProperty<TimelineCurveSeries<T>, AbsoluteKeyFrameCollection<T>>(true, true, true, "KeyFrames");

        public override IEnumerable<Point> GetOutPoints(ReadOnlyCoordinatesSystemManager coordinatesSystemManager) => ArrangeProgress(base.GetOutPoints(coordinatesSystemManager), coordinatesSystemManager);

        public static IEnumerable<Point> ArrangeProgress(IEnumerable<Point> source, ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            var offset = coordinatesSystemManager.ComputeOutCoordinates(new Vector(0, -coordinatesSystemManager.InputRange.Bottom));
            double o0 = coordinatesSystemManager.ComputeOutOrthonormalYCoordinate(0);
            double o1 = coordinatesSystemManager.ComputeOutOrthonormalYCoordinate(1);
            double top = HeaderHeight + Space;
            double bottom = coordinatesSystemManager.OutputRange.Bottom - Space;
            double cd = (top - bottom) / (o1 - o0);
            return source.Select(p => new Point(p.X, cd * (p.Y - o0) + bottom + 8) + offset);
        }

        public TimelineCurveSeries() => base.Function = x => ProgressAt(x, KeyFrames);

        public static double ProgressAt(double x, IAbsoluteKeyFrameCollection keyFrames) => keyFrames.ProgressAt(keyFrames.IndexOfKeyFrameAt((long)Math.Ceiling(x)), x, true);
    }

    public class TimelineKeyFrames<T> : VisualObjectRendererBase
    {
        public override string Type => "TimelineKeyFrames";
        protected override Freezable CreateInstanceCore() => new TimelineKeyFrames<T>();

        static TimelineKeyFrames() => ChildrenProperty.OverrideMetadata(typeof(TimelineKeyFrames<T>), new NotifyObjectPropertyMetadata { Notify = false, Register = false });

        public AbsoluteKeyFrameCollection<T> KeyFrames { get => (AbsoluteKeyFrameCollection<T>)GetValue(KeyFramesProperty); set => SetValue(KeyFramesProperty, value); }
        public static readonly DependencyProperty KeyFramesProperty = CreateProperty<TimelineKeyFrames<T>, AbsoluteKeyFrameCollection<T>>(false, true, true, "KeyFrames");

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == KeyFramesProperty)
            {
                if (e.OldValue is AbsoluteKeyFrameCollection<T> oldValue)
                {
                    oldValue.CollectionChanged -= TimelineKeyFrames_CollectionChanged;
                    oldValue.Changed -= ChangedHandler;
                }

                if (e.NewValue is AbsoluteKeyFrameCollection<T> newValue)
                {
                    Children = new VisualObjectCollection(KeyFrames.Select((kf, i) => CreateKeyFrame(i)));
                    newValue.CollectionChanged += TimelineKeyFrames_CollectionChanged;
                    newValue.Changed += ChangedHandler;
                }
            }
            base.OnPropertyChanged(e);
        }

        protected TimelineKeyFrame<T> CreateKeyFrame(int index) => new TimelineKeyFrame<T> { KeyFrame = KeyFrames[index], NextKeyFrame = index == KeyFrames.Count - 1 ? null : KeyFrames[index + 1] };
        protected void UpdatePreviousKeyFrame(int index) { if (index > 0) ((TimelineKeyFrame<T>)Children[index - 1]).NextKeyFrame = index < KeyFrames.Count ? KeyFrames[index] : null; }

        private void TimelineKeyFrames_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (Children is VisualObjectCollection visualObjects)
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        visualObjects.Insert(e.NewStartingIndex, CreateKeyFrame(e.NewStartingIndex));
                        UpdatePreviousKeyFrame(e.NewStartingIndex);
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        visualObjects[e.OldStartingIndex].Destroy();
                        UpdatePreviousKeyFrame(e.OldStartingIndex);
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        UpdatePreviousKeyFrame(e.OldStartingIndex);
                        ((TimelineKeyFrame<T>)visualObjects[e.OldStartingIndex]).KeyFrame = (AbsoluteKeyFrame<T>)e.NewItems[0];
                        break;
                    case NotifyCollectionChangedAction.Move:
                        visualObjects.Move(e.OldStartingIndex, e.NewStartingIndex);
                        UpdatePreviousKeyFrame(e.NewStartingIndex);
                        UpdatePreviousKeyFrame(e.NewStartingIndex + 1);
                        UpdatePreviousKeyFrame(e.NewStartingIndex > e.OldStartingIndex ? e.OldStartingIndex : e.OldStartingIndex + 1);
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        visualObjects.Clear();
                        break;
                }
            }
        }
    }

    public class TimelineKeyFrame<T> : VisualObject
    {
        public override string Type => "TimelineKeyFrame";
        protected override Freezable CreateInstanceCore() => new TimelineKeyFrame<T>();

        private long m_baseFramescount;
        private Matrix m_cpMatrixInvert;

        public AbsoluteKeyFrame<T> KeyFrame { get => (AbsoluteKeyFrame<T>)GetValue(KeyFrameProperty); set => SetValue(KeyFrameProperty, value); }
        public static readonly DependencyProperty KeyFrameProperty = CreateProperty<TimelineKeyFrame<T>, AbsoluteKeyFrame<T>>(false, false, true, "KeyFrame");

        public AbsoluteKeyFrame<T> NextKeyFrame { get => (AbsoluteKeyFrame<T>)GetValue(NextKeyFrameProperty); set => SetValue(NextKeyFrameProperty, value); }
        public static readonly DependencyProperty NextKeyFrameProperty = CreateProperty<TimelineKeyFrame<T>, AbsoluteKeyFrame<T>>(false, false, true, "NextKeyFrame");

        public int Focus { get; private set; } = -1;

        protected override void OnMouseEnter(Point inPosition, Character hitTest)
        {
            Focus = (int)hitTest.Data;
            base.OnMouseEnter(inPosition, hitTest);
        }

        protected override void OnMouseLeave(Point inPosition, Character hitTest)
        {
            Focus = -1;
            base.OnMouseLeave(inPosition, hitTest);
        }

        protected override void Move(Point inPosition, Vector totalInOffset, Vector inOffset, Character clickHitTest)
        {
            int d = (int)clickHitTest.Data;
            if (d == 1) ((SplineAbsoluteKeyFrame<T>)NextKeyFrame).KeySpline.ControlPoint1 += m_cpMatrixInvert.Transform(inOffset);
            else if (d == 2) ((SplineAbsoluteKeyFrame<T>)NextKeyFrame).KeySpline.ControlPoint2 += m_cpMatrixInvert.Transform(inOffset);
            else if (d == 0)
            {
                if (totalInOffset == inOffset) m_baseFramescount = KeyFrame.FramesCount;
                KeyFrame.FramesCount = m_baseFramescount + (long)totalInOffset.X;
            }
        }

        protected override IEnumerable<Character> GetCharactersCore(ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            var offset = coordinatesSystemManager.ComputeOutCoordinates(new Vector(0, -coordinatesSystemManager.InputRange.Bottom));
            double top = HeaderHeight + Space + offset.Y;
            double bottom = coordinatesSystemManager.OutputRange.Bottom - Space + offset.Y;
            var kf = KeyFrame;
            var nkf = NextKeyFrame;
            var (left, right) = FromKeyFrame(kf, nkf);
            double x = (kf.FramesCount - coordinatesSystemManager.InputRange.Left) * coordinatesSystemManager.WidthRatio;
            left.Transform.Translate(x, top);
            right.Transform.Translate(x, bottom);
            if (nkf is SplineAbsoluteKeyFrame<T> splineKeyFrame)
            {
                double inTop = coordinatesSystemManager.ComputeInOrthonormalYCoordinate(top);
                double inBottom = coordinatesSystemManager.ComputeInOrthonormalYCoordinate(bottom);
                var m = new Matrix(nkf.FramesCount - kf.FramesCount, 0, 0, inTop - inBottom, kf.FramesCount, inBottom);
                if (m.HasInverse)
                {
                    var cp1 = splineKeyFrame.KeySpline.ControlPoint1;
                    var cp2 = splineKeyFrame.KeySpline.ControlPoint2;
                    var v = new Vector(0, 8);

                    if (cp1.X != 0 || cp1.Y != 0)
                    {
                        var p0 = coordinatesSystemManager.ComputeOutCoordinates(m.Transform(new Point(0, 0))) + v;
                        var p1 = coordinatesSystemManager.ComputeOutCoordinates(m.Transform(cp1)) + v;
                        yield return Character.Line(p0, p1).Color(new Pen(FlatBrushes.SunFlower, 1)).WithData(-1).HideSelection();
                        yield return Character.Ellipse(p1, 5, 5).Color(Focus == 1 ? FlatBrushes.Carrot : FlatBrushes.SunFlower).WithData(1).HideSelection();
                    }
                    if (cp2.X != 1 || cp2.Y != 1)
                    {
                        var p2 = coordinatesSystemManager.ComputeOutCoordinates(m.Transform(cp2)) + v;
                        var p3 = coordinatesSystemManager.ComputeOutCoordinates(m.Transform(new Point(1, 1))) + v;
                        yield return Character.Line(p2, p3).Color(new Pen(FlatBrushes.SunFlower, 1)).WithData(-1).HideSelection();
                        yield return Character.Ellipse(p2, 5, 5).Color(Focus == 2 ? FlatBrushes.Carrot : FlatBrushes.SunFlower).WithData(2).HideSelection();
                    }

                    m_cpMatrixInvert = m;
                    m_cpMatrixInvert.Invert();
                }
            }
            yield return Character.Line(new Point(x, HeaderHeight), new Point(x, coordinatesSystemManager.OutputRange.Bottom)).Color(new Pen(Brushes.White, 1) { DashStyle = new DashStyle(new double[] { 4 }, 0) }).WithData(0);
            yield return left.WithData(0);
            yield return right.WithData(0);
        }
    }
}
