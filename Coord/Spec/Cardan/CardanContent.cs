using BenLib;
using BenLib.WPF;
using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using static BenLib.Animating;
using static BenLib.IntInterval;
using static Coord.VisualObjects;

namespace Coord
{
    public partial class MainWindow
    {
        public const int FPS = 60;

        private void ConfigurePlane()
        {
            plane.InputRange = new MathRect(-28, -7.5, 65, 32.5);
            plane.Grid.Primary = false;
            plane.Grid.Secondary = false;
            plane.Axes.Direction = AxesDirection.None;
            plane.AxesNumbers.Direction = AxesDirection.None;
            plane.RenderAtChange = false;
            SaveImages = false;
            ImagesPath = "Cardan";
        }

        private void OnLoaded()
        {
            ConfigurePlane();

            double textSize = 80;
            var inAnchorPoint = Point(-25, 12.5);
            var curveA = Geometry.Parse("M0,0 c0-140.72,255.69-140.33,255.69,0");
            var brush = FlatBrushes.Clouds;
            var group = new VisualObjectRenderer();

            //Objects===================================================================================================================================================================================================

            var equation = OutTex("x^3+px+q=0", textSize, inAnchorPoint, RectPoint.NaN);
            var s1 = Characters(inAnchorPoint, RectPoint.NaN, false, Character.FromCanvas(Resources["s8"] as Canvas), NSet).Fit(NSet, (7, 7 + 1), equation, (8, 8 + 1), 1).Translate(NSet, new Vector(200, 250), false, 1).Hide();
            var s2 = OutTex("u^3+v^3+q=0", textSize, inAnchorPoint, RectPoint.NaN).Insert(NSet, null, s1, (8, 8 + 9), 1);
            var system = Group(InCharacters(s1, (0, 0 + 8)), s2);
            group.Children.Add(equation);
            group.Children.Add(system);

            //Step1----------------------------------------------------------------------------------------------------------------------------------------------------------

            var syncp1a = new SynchronizedProgress(0);
            var syncp1b = new SynchronizedProgress(1);

            var step1 = OutTex("(u+v)^3+p(u+v)+q=0", textSize, inAnchorPoint, RectPoint.NaN).Align(NSet, equation, 1);
            var s1c = InCharacters(step1, NSet)
                .Brace(0, 3, 1, syncp1b)
                .Brace(8, 3, 1, syncp1b)
                .Insert((4, 12), (6, 6 + 1), equation, (2, 2 + 1), 1, true, false, default, syncp1b)
                .Insert((12, null), (13, 13 + 1), equation, (5, 5 + 1), 1, true, false, default, syncp1b).Hide((IntRange)(1, 1 + 3) + (9, 9 + 3));

            var x1touv1 = InMorphing(InCharacters(equation, (0, 0 + 1)), InCharacters(step1, (1, 1 + 3)), null, syncp1a);
            var x2touv2 = InMorphing(InCharacters(equation, (4, 4 + 1)), InCharacters(step1, (9, 9 + 3)), null, syncp1a);

            group.Children.Add(s1c);
            group.Children.Add(x1touv1);
            group.Children.Add(x2touv2);

            //Step2----------------------------------------------------------------------------------------------------------------------------------------------------------

            var syncp2a = new SynchronizedProgress(0);
            var syncp2b = new SynchronizedProgress(1);

            var step2 = OutTex("(u+v)(u+v)^2+p(u+v)+q=0", textSize, inAnchorPoint, RectPoint.NaN).Align(NSet, equation, 1);
            var s2c = InCharacters(step2, NSet).Insert((11, null), (11, 11 + 1), step1, (6, 6 + 1), 1, syncp2b).Hide((5, 5 + 6));

            var uv2 = InCharacters(step2, (0, 0 + 5)).Insert(NSet, null, step2, (5, 5 + 5), 0, true, false, default, syncp2a);
            var pow3topow2 = InMorphing(InCharacters(step1, (5, 5 + 1)), InCharacters(step2, (10, 10 + 1)), null, syncp2a);

            group.Children.Add(s2c);
            group.Children.Add(uv2);
            group.Children.Add(pow3topow2);

            //Step3----------------------------------------------------------------------------------------------------------------------------------------------------------

            var syncp3a = new SynchronizedProgress(0);
            var syncp3b = new SynchronizedProgress(1);

            var step3 = OutTex("(u+v)(u^2+2uv+v^2)+p(u+v)+q=0", textSize, inAnchorPoint, RectPoint.NaN).Align(NSet, equation, 1);
            var s3c = OutTex("(u+v)(u^2+2uv+v^2)^2+p(u+v)+q=0", textSize, inAnchorPoint, RectPoint.NaN).Align(NSet, equation, 1)
                .Insert((15, 17), (15, 15 + 1), step2, (9, 9 + 1), 1, true, false, default, syncp3b)
                .Insert((17, null), (17, 17 + 1), step3, (16, 16 + 1), 1).Insert((17, null), (17, 17 + 1), step2, (11, 11 + 1), 1, true, false, default, syncp3b).Hide((6, 6 + 9));

            var devuv2 = InMorphing(InCharacters(step2, (6, 6 + 3)), InCharacters(step3, (6, 6 + 9)), null, syncp3a);

            group.Children.Add(s3c);
            group.Children.Add(devuv2);

            //Step4----------------------------------------------------------------------------------------------------------------------------------------------------------

            var syncp4a = new SynchronizedProgress(0);
            var syncp4b = new SynchronizedProgress(1);
            var syncp4c = new SynchronizedProgress(1);

            var step4 = OutTex("u^2(u+v)+v^2(u+v)+2uv(u+v)+p(u+v)+q=0", textSize, inAnchorPoint, RectPoint.NaN).Align(NSet, equation, 1);
            var s4c = InCharacters(step4, NSet)
                .Insert((7, 15), (7, 7 + 1), step3, (8, 8 + 1), 1, true, false, default, syncp4b)
                .Insert((15, 24), (15, 15 + 1), step3, (12, 12 + 1), 1, true, false, default, syncp4b)
                .Insert((24, null), (24, 24 + 1), step3, (16, 16 + 1), 1, true, false, default, syncp4b).Hide((IntRange)(0, 0 + 7) + (8, 8 + 7) + (16, 16 + 8));

            var brace1 = InCharacters(step3, (5, 5 + 1)).Scale(NSet, 0, 0, RectPoint.Left, 0, syncp4c);
            var brace2 = InCharacters(step3, (15, 15 + 1)).Scale(NSet, 0, 0, RectPoint.Left, 0, syncp4c);

            var upv1 = InCharacters(step3, (0, 0 + 5)).Insert(NSet, null, step4, (2, 2 + 5), 0, true, false, default, syncp4a);
            var upv2 = InCharacters(step3, (0, 0 + 5)).Insert(NSet, null, step4, (10, 10 + 5), 0, true, false, default, syncp4a);
            var upv3 = InCharacters(step3, (0, 0 + 5)).Insert(NSet, null, step4, (19, 19 + 5), 0, true, false, default, syncp4a);

            var u2 = InCharacters(step3, (6, 6 + 2)).Insert(NSet, null, step4, (0, 0 + 2), 0, true, false, default, syncp4a)/*.TranslateAlongPath(NSet, curveA, Translations.Y, 0, syncp4a)*/;
            var v2 = InCharacters(step3, (13, 13 + 2)).Insert(NSet, null, step4, (8, 8 + 2), 0, true, false, default, syncp4a)/*.TranslateAlongPath(NSet, curveA, Translations.Y, 0, syncp4a)*/;
            var twouv = InCharacters(step3, (9, 9 + 3)).Insert(NSet, null, step4, (16, 16 + 3), 0, true, false, default, syncp4a)/*.TranslateAlongPath(NSet, curveA, Translations.Y, 0, syncp4a)*/;

            group.Children.Add(s4c);
            group.Children.Add(brace1);
            group.Children.Add(brace2);
            group.Children.Add(upv1);
            group.Children.Add(upv2);
            group.Children.Add(upv3);
            group.Children.Add(u2);
            group.Children.Add(v2);
            group.Children.Add(twouv);

            //Step5----------------------------------------------------------------------------------------------------------------------------------------------------------

            var syncp5a = new SynchronizedProgress(0);
            var syncp5b = new SynchronizedProgress(1);
            var syncp5c = new SynchronizedProgress(1);

            var step5 = OutTex("u^3+v^3+vu^2+uv^2+2uv(u+v)+p(u+v)+q=0", textSize, inAnchorPoint, RectPoint.NaN).Align(NSet, equation, 1);
            var s5c = InCharacters(step5, NSet)
                .Insert((2, 5), (2, 2 + 1), step4, (4, 4 + 1), 1, true, false, default, syncp5b)
                .Insert((5, 9), (5, 5 + 1), step4, (7, 7 + 1), 1, true, false, default, syncp5b)
                .Insert((9, 13), (9, 9 + 1), step4, (12, 12 + 1), 1, true, false, default, syncp5b)
                .Insert((13, null), (13, 13 + 1), step4, (15, 15 + 1), 1, true, false, default, syncp5b)
                .Hide((IntRange)(0, 0 + 2) + (3, 3 + 2) + (6, 6 + 3) + (10, 10 + 3));

            var brace51o = InCharacters(step4, (2, 2 + 1)).Scale(NSet, 0, 0, RectPoint.Left, 0, syncp5c);
            var brace51f = InCharacters(step4, (6, 6 + 1)).Scale(NSet, 0, 0, RectPoint.Left, 0, syncp5c);
            var brace52o = InCharacters(step4, (10, 10 + 1)).Scale(NSet, 0, 0, RectPoint.Left, 0, syncp5c);
            var brace52f = InCharacters(step4, (14, 14 + 1)).Scale(NSet, 0, 0, RectPoint.Left, 0, syncp5c);

            var u25a = InCharacters(step4, (0, 0 + 1)).Insert(NSet, null, step5, (0, 0 + 1), 0, true, false, default, syncp5a);
            var u25b = InCharacters(step4, (0, 0 + 2)).Insert(NSet, null, step5, (7, 7 + 2), 0, true, false, default, syncp5a);
            var v25a = InCharacters(step4, (8, 8 + 1)).Insert(NSet, null, step5, (3, 3 + 1), 0, true, false, default, syncp5a);
            var v25b = InCharacters(step4, (8, 8 + 2)).Insert(NSet, null, step5, (11, 11 + 2), 0, true, false, default, syncp5a);

            var u5a = InCharacters(step4, (3, 3 + 1)).Insert(NSet, null, step5, (0, 0 + 1), 0, true, false, default, syncp5a)/*.TranslateAlongPath(NSet, curveA, Translations.Y, 0, syncp5a)*/;
            var v5a = InCharacters(step4, (5, 5 + 1)).Insert(NSet, null, step5, (3, 3 + 1), 0, true, false, default, syncp5a)/*.TranslateAlongPath(NSet, curveA, Translations.Y, 0, syncp5a)*/;
            var u5b = InCharacters(step4, (11, 11 + 1)).Insert(NSet, null, step5, (10, 10 + 1), 0, true, false, default, syncp5a)/*.TranslateAlongPath(NSet, curveA, Translations.Y, 0, syncp5a)*/;
            var v5b = InCharacters(step4, (13, 13 + 1)).Insert(NSet, null, step5, (6, 6 + 1), 0, true, false, default, syncp5a)/*.TranslateAlongPath(NSet, curveA, Translations.Y, 0, syncp5a)*/;

            var pow2topow35a = InMorphing(InCharacters(step4, (1, 1 + 1)), InCharacters(step5, (1, 1 + 1)), null, syncp5a);
            var pow2topow35b = InMorphing(InCharacters(step4, (9, 9 + 1)), InCharacters(step5, (4, 4 + 1)), null, syncp5a);

            group.Children.Add(s5c);
            group.Children.Add(brace51o);
            group.Children.Add(brace51f);
            group.Children.Add(brace52o);
            group.Children.Add(brace52f);
            group.Children.Add(u25a);
            group.Children.Add(u25b);
            group.Children.Add(v25a);
            group.Children.Add(v25b);
            group.Children.Add(u5a);
            group.Children.Add(v5a);
            group.Children.Add(u5b);
            group.Children.Add(v5b);
            group.Children.Add(pow2topow35a);
            group.Children.Add(pow2topow35b);

            //Step6----------------------------------------------------------------------------------------------------------------------------------------------------------

            var syncp6a = new SynchronizedProgress(0);
            var syncp6b = new SynchronizedProgress(1);
            var syncp6c = new SynchronizedProgress(1);

            var step6 = OutTex("u^3+v^3+uv(u+v)+2uv(u+v)+p(u+v)+q=0", textSize, inAnchorPoint, RectPoint.NaN).Align(NSet, equation, 1);
            var s6c = InCharacters(step6, NSet)
                .Brace(8, 3, 1, syncp6b)
                .Insert((10, 13), (10, 10 + 1), step5, (9, 9 + 1), 1, true, false, default, syncp6b)
                .Insert((13, null), (13, 13 + 1), step5, (13, 13 + 1), 1, true, false, default, syncp6b)
                .Hide((IntRange)(6, 6 + 2) + (9, 9 + 1) + (11, 11 + 1));

            var uvuvtouv = InMorphing(InCharacters(step5, (IntRange)(6, 6 + 2) + (10, 10 + 2)), InCharacters(step6, (6, 6 + 2)), null, syncp6a);

            var u26 = InCharacters(step5, (7, 7 + 2)).Insert(NSet, null, step6, (9, 9 + 1), 0, true, false, default, syncp6a);
            var v26 = InCharacters(step5, (11, 11 + 2)).Insert(NSet, null, step6, (11, 11 + 1), 0, true, false, default, syncp6a);

            group.Children.Add(s6c);
            group.Children.Add(uvuvtouv);
            group.Children.Add(u26);
            group.Children.Add(v26);

            //Step7----------------------------------------------------------------------------------------------------------------------------------------------------------

            var syncp7a = new SynchronizedProgress(0);
            var syncp7b = new SynchronizedProgress(1);
            var syncp7c = new SynchronizedProgress(1);

            var step7 = OutTex("u^3+v^3+(u+v)(3uv+p)+q=0", textSize, inAnchorPoint, RectPoint.NaN).Align(NSet, equation, 1);
            var s7c = InCharacters(step7, NSet)
                .Brace(11, 5, 1, syncp7b)
                .Insert((15, null), (16, 16 + 1), step6, (23, 23 + 1), 1, true, false, default, syncp7b)
                .Insert((18, null), (19, 19 + 1), step6, (30, 30 + 1), 1, true, false, default, syncp7b)
                .Hide((IntRange)(6, 6 + 5) + (12, 12 + 3) + (16, 16 + 1));

            var upv7a = InCharacters(step6, (8, 8 + 5)).Insert(NSet, null, step7, (6, 6 + 5), 0, true, false, default, syncp7a);
            var upv7b = InCharacters(step6, (17, 17 + 5)).Insert(NSet, null, step7, (6, 6 + 5), 0, true, false, default, syncp7a);
            var upv7c = InCharacters(step6, (24, 24 + 5)).Insert(NSet, null, step7, (6, 6 + 5), 0, true, false, default, syncp7a);

            var uv7 = InCharacters(step6, (6, 6 + 2)).Insert(NSet, null, step7, (13, 13 + 2), 0, true, false, default, syncp7a);
            var twouvto3uv = InMorphing(InCharacters(step6, (13, 13 + 4)), InCharacters(step7, (12, 12 + 3)), null, syncp7a);
            var p7 = InCharacters(step6, (23, 23 + 1)).Insert(NSet, null, step7, (16, 16 + 1), 0, true, false, default, syncp7a);

            group.Children.Add(s7c);
            group.Children.Add(upv7a);
            group.Children.Add(upv7b);
            group.Children.Add(upv7c);
            group.Children.Add(uv7);
            group.Children.Add(twouvto3uv);
            group.Children.Add(p7);

            //Step8----------------------------------------------------------------------------------------------------------------------------------------------------------

            var syncp8a = new SynchronizedProgress(0);
            var syncp8b = new SynchronizedProgress(1);

            var sys = InCharacters(system, NSet);
            var s7c3 = InCharacters(step7, NSet);
            var threeuvpz = InCharacters(system, (1, 1 + 7)).Insert(NSet, (0, 0 + 5), step7, (12, 12 + 5), 1, syncp8b);

            group.Children.Add(sys);
            group.Children.Add(s7c3);
            group.Children.Add(threeuvpz);

            //Step9----------------------------------------------------------------------------------------------------------------------------------------------------------

            var syncp9a = new SynchronizedProgress(0);
            var syncp9b = new SynchronizedProgress(1);

            var step9 = OutTex("u^3+v^3+(u+v)(0)+q=0", textSize, inAnchorPoint, RectPoint.NaN).Align(NSet, equation, 1);
            var s9c = InCharacters(step9, NSet).Insert((13, null), (15, 15 + 1), step7, (19, 19 + 1), 1, true, false, default, syncp9b).Hide((12, 12 + 1));

            var zero = InCharacters(system, (7, 7 + 1)).Insert(NSet, null, step9, (12, 12 + 1), 0, syncp9a);
            var threeuvptozero = InMorphing(InCharacters(step7, (12, 12 + 5)), InCharacters(step9, (12, 12 + 1)), null, syncp9a);

            group.Children.Add(s9c);
            group.Children.Add(zero);
            group.Children.Add(threeuvptozero);

            //Step10----------------------------------------------------------------------------------------------------------------------------------------------------------

            var syncp10a = new SynchronizedProgress(0);
            var syncp10b = new SynchronizedProgress(1);

            var step10 = OutTex("u^3+v^3+q=0", textSize, inAnchorPoint, RectPoint.NaN).Align(NSet, equation, 1);
            var s10c = InCharacters(step10, NSet).Insert((5, null), (6, 6 + 1), step9, (15, 15 + 1), 1, true, false, default, syncp10b);

            var uvzerotoempty = InCharacters(step9, (5, 5 + 9)).Scale(NSet, 0, 0, RectPoint.Left, 0, syncp10a);

            group.Children.Add(s10c);
            group.Children.Add(uvzerotoempty);

            //Step11----------------------------------------------------------------------------------------------------------------------------------------------------------

            var syncp11a = new SynchronizedProgress(0);
            var syncp11b = new SynchronizedProgress(1);

            var u3pv3pqz = InCharacters(system, (8, 8 + 9)).Insert(NSet, null, step10, NSet, 1, syncp11b);

            group.Children.Add(u3pv3pqz);

            //Step12----------------------------------------------------------------------------------------------------------------------------------------------------------

            var syncp12a = new SynchronizedProgress(0);
            var syncp12b = new SynchronizedProgress(1);

            var u3pv3eqmq = OutTex("u^3+v^3=-q", textSize, inAnchorPoint, RectPoint.NaN).Insert(NSet, (0, 0 + 5), system, (8, 8 + 5), 1);
            var uveqmps3 = OutTex(@"uv=-\frac{p}{3}", textSize, inAnchorPoint, RectPoint.NaN).Insert(NSet, (0, 0 + 2), system, (2, 2 + 2), 1).Align(NSet, u3pv3eqmq, 1, true, false, default);

            var sys1 = InCharacters(system, (1, 1 + 7)).Insert((1, 5), (1, 1 + 2), uveqmps3, (0, 0 + 2), 0, true, false, default, syncp12a).Insert((5, null), (5, 5 + 1), uveqmps3, (2, 2 + 1), 0, true, false, default, syncp12a).Hide((IntRange)(0, 0 + 1) + (3, 3 + 2)).Opacity((6, 6 + 1), 0, 0, 0, syncp12a);
            var sys2 = InCharacters(system, (8, 8 + 9)).Insert((6, 7), null, u3pv3eqmq, (7, 7 + 1), 0, true, false, default, syncp12a).Insert((7, null), (7, 7 + 1), u3pv3eqmq, (5, 5 + 1), 0, true, false, default, syncp12a).Hide((5, 5 + 1)).Opacity((8, 8 + 1), 0, 0, 0, syncp12a);

            var p = InCharacters(system, (5, 5 + 1)).Insert(NSet, null, uveqmps3, (4, 4 + 1), 0, syncp12a);
            var three = InCharacters(system, (1, 1 + 1)).Insert(NSet, null, uveqmps3, (6, 6 + 1), 0, syncp12a);
            var frac = InCharacters(uveqmps3, (5, 5 + 1)).Scale(NSet, 0, 1, RectPoint.Center, 1, syncp12b);

            var ptom1 = InMorphing(InCharacters(sys1, (3, 3 + 1)), InCharacters(uveqmps3, (3, 3 + 1)), null, syncp12a);
            var ptom2 = InMorphing(InCharacters(sys2, (5, 5 + 1)), InCharacters(u3pv3eqmq, (6, 6 + 1)), null, syncp12a);

            group.Children.Add(uveqmps3);
            group.Children.Add(p);
            group.Children.Add(frac);
            group.Children.Add(three);
            group.Children.Add(sys1);
            group.Children.Add(sys2);
            group.Children.Add(ptom1);
            group.Children.Add(ptom2);

            //===================================================================================================================================================================================================

            Display(equation);

            plane.VisualObjects.Add(group);

            void Display(params VisualObject[] objects) { foreach (var visualObject in objects) visualObject.Effects.Insert(0, new ColorCharacterEffect(NSet, brush, null, 1)); }
            void Hide(params VisualObject[] objects) { foreach (var visualObject in objects) visualObject.Hide(); }
            void Color(Brush br, params VisualObject[] objects) { foreach (var visualObject in objects) visualObject.Color(br); }
            void Destroy(params VisualObject[] objects) { foreach (var visualObject in objects) visualObject.Destroy(); }

            async Task Animate()
            {
                Hide(equation);

                //Step1----------------------------------------------------------------------------------------------------------------------------------------------------------

                Display(s1c, x1touv1, x2touv2);

                await Task.WhenAll(
                    syncp1b.Animate(1, new CubicEase { EasingMode = EasingMode.EaseInOut }, 1, 0),
                    syncp1a.Animate(1, new CubicEase { EasingMode = EasingMode.EaseInOut }));

                Destroy(s1c, x1touv1, x2touv2);

                //Step2----------------------------------------------------------------------------------------------------------------------------------------------------------

                Display(s2c, uv2, pow3topow2);

                await Task.WhenAll(
                    syncp2b.Animate(1, new CubicEase { EasingMode = EasingMode.EaseInOut }, 1, 0),
                    syncp2a.Animate(1, new CubicEase { EasingMode = EasingMode.EaseInOut }));

                Destroy(s2c, uv2, pow3topow2);

                //Step3----------------------------------------------------------------------------------------------------------------------------------------------------------

                Display(s3c, devuv2);

                await Task.WhenAll(
                    syncp3b.Animate(1, new CubicEase { EasingMode = EasingMode.EaseInOut }, 1, 0),
                    syncp3a.Animate(1, new CubicEase { EasingMode = EasingMode.EaseInOut }),
                    s3c.UnWrite((16, 16 + 1), true, 1, 0.75));

                Destroy(s3c, devuv2);

                //Step4----------------------------------------------------------------------------------------------------------------------------------------------------------

                Display(s4c, brace1, brace2, upv1, upv2, upv3, u2, v2, twouv);

                await Task.WhenAll(
                    syncp4b.Animate(1, new CubicEase { EasingMode = EasingMode.EaseInOut }, 1, 0),
                    syncp4a.Animate(1, new CubicEase { EasingMode = EasingMode.EaseInOut }),
                    syncp4c.Animate(0.5, new CubicEase { EasingMode = EasingMode.EaseInOut }));

                Destroy(s4c, brace1, brace2, upv1, upv2, upv3, u2, v2, twouv);

                //Step5----------------------------------------------------------------------------------------------------------------------------------------------------------

                Display(s5c, brace51o, brace51f, brace52o, brace52f, u25a, u25b, v25a, v25b, u5a, v5a, u5b, v5b, pow2topow35a, pow2topow35b);

                await Task.WhenAll(
                    syncp5b.Animate(1, new CubicEase { EasingMode = EasingMode.EaseInOut }, 1, 0),
                    syncp5a.Animate(1, new CubicEase { EasingMode = EasingMode.EaseInOut }),
                    syncp5c.Animate(0.5, new CubicEase { EasingMode = EasingMode.EaseInOut }));

                Destroy(s5c, brace51o, brace51f, brace52o, brace52f, u25a, u25b, v25a, v25b, u5a, v5a, u5b, v5b, pow2topow35a, pow2topow35b);

                //Step6----------------------------------------------------------------------------------------------------------------------------------------------------------

                Display(s6c, uvuvtouv, u26, v26);

                await Task.WhenAll(
                    syncp6b.Animate(1, new CubicEase { EasingMode = EasingMode.EaseInOut }, 1, 0),
                    syncp6a.Animate(1, new CubicEase { EasingMode = EasingMode.EaseInOut }),
                    syncp6c.Animate(0.5, new CubicEase { EasingMode = EasingMode.EaseInOut }),
                    u26.UnWrite((1, 1 + 1), true, 1, 0.75),
                    v26.UnWrite((1, 1 + 1), true, 1, 0.75));

                Destroy(s6c, uvuvtouv, u26, v26);

                //Step7----------------------------------------------------------------------------------------------------------------------------------------------------------

                Display(s7c, upv7a, upv7b, upv7c, uv7, twouvto3uv, p7);

                await Task.WhenAll(
                    syncp7b.Animate(1, new CubicEase { EasingMode = EasingMode.EaseInOut }, 1, 0),
                    syncp7a.Animate(1, new CubicEase { EasingMode = EasingMode.EaseInOut }),
                    syncp7c.Animate(0.5, new CubicEase { EasingMode = EasingMode.EaseInOut }));

                Destroy(s7c, upv7a, upv7b, upv7c, uv7, twouvto3uv, p7);

                //Step8----------------------------------------------------------------------------------------------------------------------------------------------------------

                Display(s7c3, threeuvpz);
                sys.Color((0, 0 + 1), brush, null);

                await Task.WhenAll(
                    syncp8b.Animate(1, new CubicEase { EasingMode = EasingMode.EaseInOut }, 1, 0),
                    syncp8a.Animate(1, new CubicEase { EasingMode = EasingMode.EaseInOut }),
                    threeuvpz.Write((5, 5 + 2), true),
                    sys.Write((0, 0 + 1), true, 1, double.NaN, 1));

                Destroy(s7c3, threeuvpz);

                //Step9----------------------------------------------------------------------------------------------------------------------------------------------------------

                Display(s9c, zero, threeuvptozero);
                sys.Color((1, 1 + 7), brush, null);

                await Task.WhenAll(
                    syncp9b.Animate(1, new CubicEase { EasingMode = EasingMode.EaseInOut }, 1, 0),
                    syncp9a.Animate(1, new CubicEase { EasingMode = EasingMode.EaseInOut }));

                Destroy(s9c, zero, threeuvptozero);

                //Step10----------------------------------------------------------------------------------------------------------------------------------------------------------

                Display(s10c, uvzerotoempty);

                await Task.WhenAll(
                    syncp10b.Animate(1, new CubicEase { EasingMode = EasingMode.EaseInOut }, 1, 0),
                    syncp10a.Animate(1, new CubicEase { EasingMode = EasingMode.EaseInOut }));

                Destroy(s10c, uvzerotoempty);

                //Step11----------------------------------------------------------------------------------------------------------------------------------------------------------

                Display(u3pv3pqz);

                await Task.WhenAll(
                    syncp11b.Animate(1, new CubicEase { EasingMode = EasingMode.EaseInOut }, 1, 0),
                    syncp11a.Animate(1, new CubicEase { EasingMode = EasingMode.EaseInOut }));

                Destroy(u3pv3pqz);

                sys.Color((8, 8 + 9), brush, null);

                //Step12----------------------------------------------------------------------------------------------------------------------------------------------------------

                sys.Hide((1, null));
                Display(p, three, sys1, sys2, frac, ptom1, ptom2);
                //Color(FlatBrushes.Alizarin, uveqmps3);

                await Task.WhenAll(
                    syncp12b.Animate(1, new CubicEase { EasingMode = EasingMode.EaseInOut }, 1, 0),
                    syncp12a.Animate(1, new CubicEase { EasingMode = EasingMode.EaseInOut }),
                    frac.Write(true));

                Destroy(p, three, sys1, sys2, frac, ptom1, ptom2);
            }

            this.Animate = Animate;
        }
    }
}
