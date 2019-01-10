using BenLib;
using BenLib.WPF;
using Coord.Spec.TPEAnim;
using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using static BenLib.Animating;
using static Coord.VisualObjects;
using static BenLib.IntInterval;
using static System.Math;

namespace Coord
{
    public partial class MainWindow
    {
        public const int FPS = 60;

        private void ConfigurePlane()
        {
            plane.InputRange = new MathRect(-0.3, -2.5, 7.3, 3.65);
            plane.Grid.Primary = false;
            plane.Grid.Secondary = false;
            plane.Axes.Direction = AxesDirection.Both;
            plane.AxesNumbers.Direction = AxesDirection.None;
            plane.RenderAtChange = false;
            SaveImages = false;
            ImagesPath = "TPEAnim";
        }

        private void OnLoaded()
        {
            ConfigurePlane();

            double textSize = 2.4;
            var inAnchorPoint = Point(-25, 12.5);
            var brush = FlatBrushes.Clouds;
            var group = new VisualObjectRenderer();

            //Objects===================================================================================================================================================================================================

            var macc = InTex(@"\mathrm{masse \cdot acc\acute{e}l\acute{e}ration}", textSize, inAnchorPoint, RectPoint.NaN);
            group.Children.Add(macc);

            //Step1----------------------------------------------------------------------------------------------------------------------------------------------------------

            var syncp1a = new SynchronizedProgress(0);
            var syncp1b = new SynchronizedProgress(1);

            var mass = InCharacters(macc, (0, 0 + 5)).Color(brush, 0, syncp1a).Translate(NSet, new Vector(-50, 0), false, 0, syncp1b);

            group.Children.Add(mass);

            //Step2----------------------------------------------------------------------------------------------------------------------------------------------------------

            var syncp2a = new SynchronizedProgress(0);
            var syncp2b = new SynchronizedProgress(1);

            var dot = InCharacters(macc, (5, 5 + 1)).Size(NSet, default, RectPoint.Center, 1, syncp2b);
            var acc = InCharacters(macc, (6, null)).Color(brush, 0, syncp2a).Translate(NSet, new Vector(50, 0), false, 0, syncp2b);

            group.Children.Add(dot);
            group.Children.Add(acc);

            //Step3----------------------------------------------------------------------------------------------------------------------------------------------------------

            var syncp3a = new SynchronizedProgress(0);
            var syncp3b = new SynchronizedProgress(1);

            var kgms2 = InTex(@"\mathrm{kg \cdot \frac{m}{s^2}}", textSize, inAnchorPoint, RectPoint.NaN);

            var massu = InMorphing(InCharacters(macc, (0, 0 + 5)), InCharacters(kgms2, (0, 0 + 2)), null, syncp3a);
            var accu = InMorphing(InCharacters(macc, (6, 6 + 14)), InCharacters(kgms2, (3, 3 + 4)), null, syncp3a);

            group.Children.Add(massu);
            group.Children.Add(accu);

            //Step4----------------------------------------------------------------------------------------------------------------------------------------------------------

            /*var syncp4b = new SynchronizedProgress(1);
            var syncp4c = new SynchronizedProgress(1);
            var syncp4d = new SynchronizedProgress(0);
            var syncp4e = new SynchronizedProgress(1);
            var syncp4f = new SynchronizedProgress(1);

            var obj = Circle(Point(-10, 10), 3).Style(FlatBrushes.Concrete).Opacity(NSet, 0, 0, 1, syncp4b).Translate(NSet, new Vector(-50, 0), false, 0, syncp4b);
            var objm = InTex(@"\mathrm{m}", textSize, Point(-10, 10), RectPoint.Center).Size(NSet, default, RectPoint.Center, 1, syncp4c);
            var objf = Vector(Point(-10, 10), 0, 0).Style(new Pen(FlatBrushes.Alizarin, 1.3)).Color(NSet, null, new Pen(FlatBrushes.PeterRiver, 1.3), true, 0, syncp4d);
            objf.Arrow = new TriangleArrow(true, true, 0.78, 0.52);
            var objfl = InTex(@"\mathrm{F}", textSize, Point(0, 12), new RectPoint(0.5, 1)).Size(NSet, default, RectPoint.BottomLeft, 1, syncp4f);
            var objal = InTex(@"\mathrm{a=\frac{F}{m}}", textSize, Point(-2.5, 12), new RectPoint(0.5, 1));
            objal.Size((3, 3 + 1), objal.SizeY(@"\frac{}{}"), RectPoint.Center, 1, syncp4e);

            group.Children.Add(objf);
            group.Children.Add(obj);
            group.Children.Add(objm);
            group.Children.Add(objfl);
            group.Children.Add(objal);*/

            var maf = new MAF();
            group.Children.Add(maf);

            //Step5----------------------------------------------------------------------------------------------------------------------------------------------------------

            var deriv = new Deriv();
            group.Children.Add(deriv);

            //===================================================================================================================================================================================================

            plane.VisualObjects.Add(group);

            void Display(params VisualObject[] objects) { foreach (var visualObject in objects) visualObject.Effects.Insert(0, new ColorCharacterEffect(NSet, brush, null, false, 1)); }
            void Hide(params VisualObject[] objects) { foreach (var visualObject in objects) visualObject.Hide(); }
            void Color(Brush br, params VisualObject[] objects) { foreach (var visualObject in objects) visualObject.Color(br); }
            void Destroy(params VisualObject[] objects) { foreach (var visualObject in objects) visualObject.Destroy(); }

            async Task Animate()
            {
                //Step1----------------------------------------------------------------------------------------------------------------------------------------------------------

                /*await Task.WhenAll(syncp1a.Animate(1, new CubicEase { EasingMode = EasingMode.EaseInOut }), syncp1b.Animate(1, new CubicEase { EasingMode = EasingMode.EaseInOut }, 1, 0));

                //Step2----------------------------------------------------------------------------------------------------------------------------------------------------------

                Display(dot);

                await Task.WhenAll(syncp2a.Animate(1, new CubicEase { EasingMode = EasingMode.EaseInOut }), syncp2b.Animate(1, new CubicEase { EasingMode = EasingMode.EaseInOut }, 1, 0));

                Destroy(mass, acc);

                //Step3----------------------------------------------------------------------------------------------------------------------------------------------------------

                Display(massu, accu);

                dot.Insert(NSet, null, kgms2, (2, 2 + 1), 0, syncp3a);

                await Task.WhenAll(syncp3a.Animate(1, new CubicEase { EasingMode = EasingMode.EaseInOut }), syncp3b.Animate(1, new CubicEase { EasingMode = EasingMode.EaseInOut }, 1, 0));*/

                //Step4----------------------------------------------------------------------------------------------------------------------------------------------------------

                await maf.Animate();
                /*objm.Color(FlatBrushes.Wisteria);
                await Task.WhenAll(syncp4a.Animate(1, new CubicEase { EasingMode = EasingMode.EaseInOut }), syncp4b.Animate(1, new CubicEase { EasingMode = EasingMode.EaseInOut }, 1, 0)).AtMost(30);
                await syncp4c.Animate(1, new CubicEase { EasingMode = EasingMode.EaseInOut }, 1, 0);
                objfl.Color(FlatBrushes.Alizarin);
                await Task.WhenAll(AnimateVector(null, vector => objf.SetInVector(vector), default, new Vector(20, 0), TimeSpan.FromSeconds(1), default, false, false, new CubicEase { EasingMode = EasingMode.EaseInOut }, 60), syncp4f.Animate(1, new CubicEase { EasingMode = EasingMode.EaseInOut }, 1, 0)).AtLeast(90);

                objal.Color((0, 0 + 1), FlatBrushes.PeterRiver, null, false).Color((1, 1 + 1), FlatBrushes.Clouds, null, false).Color((3, 3 + 1), FlatBrushes.Clouds, null, false);
                objfl.Insert(NSet, null, objal, (2, 2 + 1), 0, syncp4d);
                objm.Insert(NSet, null, objal, (4, 4 + 1), 0, syncp4d);
                await Task.WhenAll(AnimateVector(null, vector => objf.SetInVector(vector), new Vector(20, 0), new Vector(15, 0), TimeSpan.FromSeconds(1), default, false, false, new CubicEase { EasingMode = EasingMode.EaseInOut }, 60), 
                objal.Write((0, 0 + 2), true, false), 
                syncp4d.Animate(1, new CubicEase { EasingMode = EasingMode.EaseInOut }), 
                syncp4e.Animate(1, new CubicEase { EasingMode = EasingMode.EaseInOut }, 1, 0));

                Destroy(obj, objm, objf, objfl, objal, massu, accu, dot);*/
                Destroy(maf);

                //Step5----------------------------------------------------------------------------------------------------------------------------------------------------------

                await deriv.Animate();

                //Step6----------------------------------------------------------------------------------------------------------------------------------------------------------
            }

            this.Animate = Animate;
        }
    }
}
