using BenLib;
using BenLib.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using static BenLib.Animating;
using static BenLib.IntInterval;
using static Coord.VisualObjects;

namespace Coord.Spec.TPEAnim
{
    public class Deriv : VisualObject
    {
        public const double FuncEnd = 6.5235;
        public const double PointsRadius = 6;
        public const double StrokeThickness = 4;
        private static double SpeedF(double x) => x * (4 - x) * (x - 5) / 10;
        private static double AccF(double x) => -2 - 3 * x * (x - 6) / 10;

        private readonly SynchronizedProgress m_syncpa = new SynchronizedProgress(0);
        private readonly SynchronizedProgress m_syncpb = new SynchronizedProgress(1);
        private readonly SynchronizedProgress m_syncpc = new SynchronizedProgress(1);
        private readonly SynchronizedProgress m_syncpd = new SynchronizedProgress(1);
        private readonly SynchronizedProgress m_syncpe = new SynchronizedProgress(0);
        private readonly SynchronizedProgress m_syncpf = new SynchronizedProgress(0);

        private double m_x = 0;
        public double X
        {
            get => m_x;
            set
            {
                m_x = value;
                NotifyChanged();
            }
        }

        private double m_length = 1;
        public double Length
        {
            get => m_length;
            set
            {
                m_length = value;
                NotifyChanged();
            }
        }

        private decimal m_speedEnd = 0;
        public decimal SpeedEnd
        {
            get => m_speedEnd;
            set
            {
                m_speedEnd = value;
                NotifyChanged();
            }
        }

        private decimal m_accEnd = 0;
        public decimal AccEnd
        {
            get => m_accEnd;
            set
            {
                m_accEnd = value;
                NotifyChanged();
            }
        }

        private readonly TextVisualObject m_accL = OutTex("a(t)", 80, Point(0.25, -1.6)).Color(FlatBrushes.PeterRiver);
        private readonly TextVisualObject m_speedL = OutTex("v(t)", 80, Point(0.3, -0.17)).Color(FlatBrushes.Alizarin);
        private readonly FunctionSeries m_speedS = new FunctionSeries(SpeedF, DecimalInterval.EmptySet, SeriesType.Y);
        private readonly AsymptoteVisualObject m_asy = new AsymptoteVisualObject(SpeedF, 0, 1) { Stroke = new Pen(FlatBrushes.SunFlower, 5), PointsRadius = 8, PointsFill = FlatBrushes.SunFlower };

        public Deriv()
        {
            this.Scale((5, 6), 0, 0, RectPoint.Center, 1, m_syncpd).Stroke((2, 3), false, 0, m_syncpa).Opacity((6, 7), 0, 0, 1, m_syncpb).Scale((7, 8), 1, 0, RectPoint.TopLeft, 1, m_syncpc).Scale((8, 9), 0, 0, RectPoint.Center, 1, m_syncpc);
            m_speedL.Write(NSet, false, 1, new Progress(0, ProgressMode.LaggedStart, 1.5), m_syncpf);
            m_accL.Write(NSet, false, 1, new Progress(0, ProgressMode.LaggedStart, 1.5), m_syncpe);
        }

        public override IReadOnlyCollection<Character> GetCharacters(CoordinatesSystemManager coordinatesSystemManager) =>
            CurveVisualObject.GetCharacters(coordinatesSystemManager, null, new Pen(FlatBrushes.Alizarin, StrokeThickness), new FunctionSeries(SpeedF, (0, SpeedEnd), SeriesType.Y), false, false).Concat( //[0;1[
            CurveVisualObject.GetCharacters(coordinatesSystemManager, null, new Pen(FlatBrushes.PeterRiver, StrokeThickness), new FunctionSeries(AccF, (0, AccEnd), SeriesType.Y), false, false)).Concat( //[1;2[
            AsymptoteVisualObject.GetCharacters(coordinatesSystemManager, new Pen(FlatBrushes.SunFlower, StrokeThickness), null, (FlatBrushes.SunFlower, null, PointsRadius), SpeedF, X, Length)).Concat( //[2;7[
            SegmentVisualObject.GetCharacters(coordinatesSystemManager, new Pen(FlatBrushes.PeterRiver, 5) { DashStyle = new DashStyle(new double[] { 2 }, 0) }, new PointPointSegmentDefinition(Point(X, SpeedF(X)), Point(X, AccF(X))))).Concat( //[7;8[
            PointVisualObject.GetCharacters(coordinatesSystemManager, FlatBrushes.PeterRiver, null, new PointPointDefinition(new Point(X, AccF(X))), PointsRadius).Concat( //[8;9[
            m_speedL.GetTransformedCharacters(coordinatesSystemManager, false))).Concat( //[9;10[
            m_accL.GetTransformedCharacters(coordinatesSystemManager, false)).ToArray(); //[10;11[

        public Task Step1() => Task.WhenAll(m_syncpf.Animate(1.15), AnimateDouble(null, value => SpeedEnd = (decimal)value, 0, FuncEnd, TimeSpan.FromSeconds(1.5), default, false, false, new CubicEase { EasingMode = EasingMode.EaseInOut }, 60));
        public async Task Step2()
        {
            await m_syncpd.Animate(1, new CubicEase { EasingMode = EasingMode.EaseOut }, 1, 0).AtMost(30);
            await Task.WhenAll(
                m_syncpa.Animate(1, new CubicEase { EasingMode = EasingMode.EaseOut }),
                m_syncpb.Animate(1, new CubicEase { EasingMode = EasingMode.EaseOut }, 1, 0),
                AnimateDouble(null, value => Length = value, 1, 0.00001, TimeSpan.FromSeconds(1), default, false, false, new CubicEase { EasingMode = EasingMode.EaseOut }, 60)).AtMost(30);
            await m_syncpc.Animate(1, new CubicEase { EasingMode = EasingMode.EaseOut }, 1, 0);
        }
        public Task Step3() => Task.WhenAll(m_syncpe.Animate(1.15), AnimateDouble(null, value => AccEnd = (decimal)(X = value), 0, FuncEnd, TimeSpan.FromSeconds(3), default, false, false, new CubicEase { EasingMode = EasingMode.EaseInOut }, 60));

        public async Task Animate()
        {
            await Step1();
            await Step2();
            await Step3();
        }
    }
}
