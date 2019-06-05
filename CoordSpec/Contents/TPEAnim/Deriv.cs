using BenLib.Framework;
using BenLib.WPF;
using Coord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using static BenLib.Framework.Animating;
using static BenLib.Standard.Interval<int>;
using static Coord.VisualObjects;

namespace CoordSpec
{
    public class Deriv : VisualObjectGroupBase
    {
        public override string Type => "Deriv";

        public const double FuncEnd = 6.5235;
        public const double PointsRadius = 6;
        public const double StrokeThickness = 4;
        private static double SpeedF(double x) => x * (4 - x) * (x - 5) / 10;
        private double AccF(double x) => (SpeedF(x + Length) - SpeedF(x)) / Length;

        private readonly SynchronizedProgress m_syncpa = new SynchronizedProgress(0);
        private readonly SynchronizedProgress m_syncpb = new SynchronizedProgress(1);
        private readonly SynchronizedProgress m_syncpc = new SynchronizedProgress(1);
        private readonly SynchronizedProgress m_syncpd = new SynchronizedProgress(1);
        private readonly SynchronizedProgress m_syncpe = new SynchronizedProgress(0);
        private readonly SynchronizedProgress m_syncpf = new SynchronizedProgress(0);

        public double X { get => (double)GetValue(XProperty); set => SetValue(XProperty, value); }
        public static readonly DependencyProperty XProperty = CreateProperty<double>(true, true, "X", typeof(Deriv));

        public double Length { get => (double)GetValue(LengthProperty); set => SetValue(LengthProperty, value); }
        public static readonly DependencyProperty LengthProperty = CreateProperty<double>(true, true, "Length", typeof(Deriv), 1);

        public double SpeedEnd { get => (double)GetValue(SpeedEndProperty); set => SetValue(SpeedEndProperty, value); }
        public static readonly DependencyProperty SpeedEndProperty = CreateProperty<double>(true, true, "SpeedEnd", typeof(Deriv));

        public double AccEnd { get => (double)GetValue(AccEndProperty); set => SetValue(AccEndProperty, value); }
        public static readonly DependencyProperty AccEndProperty = CreateProperty<double>(true, true, "AccEnd", typeof(Deriv));

        public TextVisualObject AccL { get; }
        public TextVisualObject SpeedL { get; }
        public CurveVisualObject AccC { get; }
        public CurveVisualObject SpeedC { get; }
        public AsymptoteVisualObject Asymptote { get; }
        public SegmentVisualObject Segment { get; }
        public PointVisualObject SpeedP { get; }
        public PointVisualObject AccP { get; }

        public Deriv()
        {
            SpeedC = Curve(new FunctionSeries { Function = SpeedF, Interval = (0, SpeedEnd), Type = SeriesType.Y }, false, false).Style(new PlanePen(FlatBrushes.Alizarin, StrokeThickness));
            AccC = Curve(new FunctionSeries { Function = AccF, Interval = (0, AccEnd), Type = SeriesType.Y }, false, false).Style(new PlanePen(FlatBrushes.PeterRiver, StrokeThickness));
            Asymptote = new AsymptoteVisualObject { Stroke = new PlanePen(FlatBrushes.SunFlower, StrokeThickness), Template = Point(default).Extend(PointsRadius).Style(FlatBrushes.SunFlower), Function = SpeedF, X = X, Length = Length }.Scale(Single(3), 0, 0, RectPoint.Center, 1, m_syncpd).Stroke(Single(0), false, 0, m_syncpa).Opacity(Single(4), 0, 0, 1, m_syncpb);
            SpeedP = Point(X, SpeedF(X)).Extend(PointsRadius).Style(FlatBrushes.SunFlower);
            AccP = Point(X, AccF(X)).Extend(PointsRadius).Style(FlatBrushes.PeterRiver).Scale(Single(0), 0, 0, RectPoint.Center, 1, m_syncpc);
            Segment = Segment(SpeedP, AccP).Style(new PlanePen(FlatBrushes.PeterRiver, 5) { DashStyle = new DashStyle(new double[] { 2 }, 0) }).Scale(Single(0), 1, 0, RectPoint.TopLeft, 1, m_syncpc);
            AccL = InTex("a(t)", 0.4, Point(0.25, -1.6)).Color(FlatBrushes.PeterRiver).Write(PositiveReals, 1, false, new Progress(0, ProgressMode.LaggedStart, 1.5), m_syncpe);
            SpeedL = InTex("v(t)", 0.4, Point(0.3, -0.17)).Color(FlatBrushes.Alizarin).Write(PositiveReals, 1, false, new Progress(0, ProgressMode.LaggedStart, 1.5), m_syncpf);

            Children = new VisualObjectCollection(AccC, SpeedC, Asymptote, Segment, AccP, AccL, SpeedL);
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "X":
                    Asymptote.X = X;
                    SpeedP.SetInPoint(X, SpeedF(X));
                    AccP.SetInPoint(X, AccF(X));
                    break;
                case "Length":
                    Asymptote.Length = Length;
                    AccC.NotifyChanged();
                    break;
                case "SpeedEnd":
                    (SpeedC.Series as FunctionSeries).Interval = (0, SpeedEnd);
                    break;
                case "AccEnd":
                    (AccC.Series as FunctionSeries).Interval = (0, AccEnd);
                    break;
            }
            base.OnPropertyChanged(e);
        }

        public Task Step1() => Task.WhenAll(m_syncpf.Animate(1.15), Animate<double>(null, value => SpeedEnd = value, 0, FuncEnd, TimeSpan.FromSeconds(1.5), default, false, false, new CubicEase { EasingMode = EasingMode.EaseInOut }, 60));
        public async Task Step2()
        {
            await m_syncpd.Animate(1, new CubicEase { EasingMode = EasingMode.EaseOut }, 1, 0).AtMost(30);
            await Task.WhenAll(
                m_syncpa.Animate(1, new CubicEase { EasingMode = EasingMode.EaseOut }),
                m_syncpb.Animate(1, new CubicEase { EasingMode = EasingMode.EaseOut }, 1, 0),
                Animate<double>(null, value => Length = value, 1, 0.00001, TimeSpan.FromSeconds(1), default, false, false, new CubicEase { EasingMode = EasingMode.EaseOut }, 60)).AtMost(30);
            await m_syncpc.Animate(1, new CubicEase { EasingMode = EasingMode.EaseOut }, 1, 0);
        }
        public Task Step3() => Task.WhenAll(m_syncpe.Animate(1.15), Animate<double>(null, value => AccEnd = X = value, 0, FuncEnd, TimeSpan.FromSeconds(3), default, false, false, new CubicEase { EasingMode = EasingMode.EaseInOut }, 60));

        public async Task Animate()
        {
            await Step1();
            await TimingFramework.FramesDelay(60);
            await Step2();
            await TimingFramework.FramesDelay(60);
            await Step3();
        }
    }
}
