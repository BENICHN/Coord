using BenLib;
using BenLib.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using static BenLib.Animating;
using static Coord.VisualObjects;

namespace Coord.Spec.TPEAnim
{
    public class MAF : VisualObject
    {
        public override string Type => "MAF";

        public const double TextSize = 2.4;
        private readonly TextVisualObject m_objm = InTex(@"\mathrm{m}", TextSize, Point(0, 0), RectPoint.Center).Color(FlatBrushes.Wisteria);
        private readonly TextVisualObject m_objf = InTex(@"\mathrm{F}", TextSize, Point(10, 2), RectPoint.Top).Color(FlatBrushes.Alizarin);
        private readonly TextVisualObject m_obja = InTex(@"\mathrm{a=\frac{F}{m}}", TextSize, Point(7.5, 2), RectPoint.Top).Color((0, 1), FlatBrushes.PeterRiver, null, false).Color((IntRange)(1, 2) + (3, 4), FlatBrushes.Clouds, null, false);

        private readonly SynchronizedProgress m_syncpb = new SynchronizedProgress(1);
        private readonly SynchronizedProgress m_syncpc = new SynchronizedProgress(1);
        private readonly SynchronizedProgress m_syncpd = new SynchronizedProgress(0);
        private readonly SynchronizedProgress m_syncpe = new SynchronizedProgress(1);
        private readonly SynchronizedProgress m_syncpf = new SynchronizedProgress(1);
        private readonly SynchronizedProgress m_syncpg = new SynchronizedProgress(0);

        private double m_arrowLength;
        public double ArrowLength
        {
            get => m_arrowLength;
            set
            {
                m_arrowLength = value;
                NotifyChanged();
            }
        }

        public MAF() => this.Opacity((2, 3), 0, 0, 1, m_syncpb).Translate((2, 3), new Vector(-50, 0), false, 0, m_syncpb).Scale((3, 4), 0, 0, RectPoint.Center, 1, m_syncpc).Scale((4, 5), 0, 0, RectPoint.Center, 1, m_syncpf).Insert((3, 4), null, m_obja, (4, 5), 0, m_syncpd).Insert((4, 5), null, m_obja, (2, 3), 0, m_syncpd).Write((5, 7), false, 1, new Progress(0, ProgressMode.LaggedStart, 1.5), m_syncpg).Scale((8, 9), 0, 1, RectPoint.Center, 1, m_syncpe);

        public override IReadOnlyCollection<Character> GetCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager) => VectorVisualObject.GetCharacters(Point(0, 0), new Vector(ArrowLength, 0), new TriangleArrow(true, true, 0.78, 0.52), ArrowEnd.End, null, new Pen(FlatBrushes.Alizarin, 1.3 * coordinatesSystemManager.WidthRatio), coordinatesSystemManager).Concat( //[0;2[
            CircleVisualObject.GetCharacters(Point(0, 0), 3, FlatBrushes.Concrete, null, coordinatesSystemManager)).Concat( //[2;3[
            m_objm.GetTransformedCharacters(coordinatesSystemManager, false)).Concat( //[3;4[
            m_objf.GetTransformedCharacters(coordinatesSystemManager, false)).Concat( //[4;5[
            m_obja.GetTransformedCharacters(coordinatesSystemManager, false)).ToArray(); //[5;10[

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
            await Timing.FramesDelay(60);
            await Step2();
            await Timing.FramesDelay(60);
            await Step3();
        }
    }
}
