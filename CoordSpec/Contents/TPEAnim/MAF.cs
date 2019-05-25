using BenLib.Framework;
using BenLib.Standard;
using BenLib.WPF;
using Coord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using static BenLib.Framework.Animating;
using static BenLib.Standard.Interval<int>;
using static Coord.VisualObjects;

namespace CoordSpec
{
    public class MAF : VisualObjectGroupBase
    {
        public override string Type => "MAF";

        public const double TextSize = 2.4;
        private readonly TextVisualObject m_objm = InTex(@"\mathrm{m}", TextSize, Point(0, 0), RectPoint.Center).Color(FlatBrushes.Wisteria);
        private readonly TextVisualObject m_objf = InTex(@"\mathrm{F}", TextSize, Point(10, 2), RectPoint.Top).Color(FlatBrushes.Alizarin);
        private readonly TextVisualObject m_obja = InTex(@"\mathrm{a=\frac{F}{m}}", TextSize, Point(7.5, 2), RectPoint.Top).Color((0, 1), FlatBrushes.PeterRiver, null).Color((Range<int>)(1, 2) | (3, 4), FlatBrushes.Clouds, null);

        private readonly SynchronizedProgress m_syncpb = new SynchronizedProgress(1);
        private readonly SynchronizedProgress m_syncpc = new SynchronizedProgress(1);
        private readonly SynchronizedProgress m_syncpd = new SynchronizedProgress(0);
        private readonly SynchronizedProgress m_syncpe = new SynchronizedProgress(1);
        private readonly SynchronizedProgress m_syncpf = new SynchronizedProgress(1);
        private readonly SynchronizedProgress m_syncpg = new SynchronizedProgress(0);

        public double ArrowLength { get => (double)GetValue(ArrowLengthProperty); set => SetValue(ArrowLengthProperty, value); }
        public static readonly DependencyProperty ArrowLengthProperty = CreateProperty<double>(true, true, "ArrowLength", typeof(MAF));

        public MAF()
        {
            Children = new VisualObjectCollection(m_objm, m_objf, m_obja);
            this.Opacity(CO(2, 3), 0, 0, 1, m_syncpb).Translate(CO(2, 3), new Vector(-50, 0), false, 0, m_syncpb).Scale(CO(3, 4), 0, 0, RectPoint.Center, 1, m_syncpc).Scale(CO(4, 5), 0, 0, RectPoint.Center, 1, m_syncpf).Insert(CO(3, 4), null, InCharacters(m_obja, CO(4, 5)), RectPoint.NaN, true, true, 0, m_syncpd).Insert(CO(4, 5), null, InCharacters(m_obja, CO(2, 3)), RectPoint.NaN, true, true, 0, m_syncpd).Write(CO(5, 7), 1, false, new Progress(0, ProgressMode.LaggedStart, 1.5), m_syncpg).Scale(CO(8, 9), 0, 1, RectPoint.Center, 1, m_syncpe);
        }

        protected override IReadOnlyCollection<Character> GetCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager) => VectorVisualObject.GetCharacters(Point(0, 0), new Vector(ArrowLength, 0), new TriangleArrow { In = true, Closed = true, Length = 0.78, Width = 0.52 }, ArrowEnd.End, null, new PlanePen(FlatBrushes.Alizarin, 1.3 * coordinatesSystemManager.WidthRatio), coordinatesSystemManager).Concat( //[0;2[
            CircleVisualObject.GetCharacters(Point(0, 0), 3, FlatBrushes.Concrete, null, coordinatesSystemManager)) //[2;3[
            .Concat(base.GetCharacters(coordinatesSystemManager)).ToArray(); //[3;10[

        public async Task Step1()
        {
            await m_syncpb.Animate(1, new CubicEase { EasingMode = EasingMode.EaseInOut }, 1, 0).AtMost(30);
            await m_syncpc.Animate(1, new CubicEase { EasingMode = EasingMode.EaseInOut }, 1, 0);
        }

        public Task Step2() => Task.WhenAll(AnimateDouble(null, value => ArrowLength = value, 0, 20, TimeSpan.FromSeconds(1), default, false, false, new CubicEase { EasingMode = EasingMode.EaseInOut }, 60), m_syncpf.Animate(1, new CubicEase { EasingMode = EasingMode.EaseInOut }, 1, 0)).AtLeast(90);
        public Task Step3() => Task.WhenAll(AnimateDouble(null, value => ArrowLength = value, 20, 15, TimeSpan.FromSeconds(1), default, false, false, new CubicEase { EasingMode = EasingMode.EaseInOut }, 60), m_syncpd.Animate(1, new CubicEase { EasingMode = EasingMode.EaseInOut }), m_syncpe.Animate(1, new CubicEase { EasingMode = EasingMode.EaseInOut }, 1, 0), m_syncpg.Animate(1.15));

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
