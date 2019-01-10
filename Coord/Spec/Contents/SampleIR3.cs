using BenLib;
using BenLib.WPF;
using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using static BenLib.Animating;
using static Coord.VisualObjects;

namespace Coord
{
    public partial class MainWindow
    {
        public const int FPS = 60;

        private void ConfigurePlane()
        {
            plane.InputRange = new MathRect(-32.5, -16.25, 65, 32.5);
            plane.Grid.Primary = true;
            plane.Grid.Secondary = true;
            plane.Axes.Direction = AxesDirection.Both;
            plane.AxesNumbers.Direction = AxesDirection.Both;
            plane.RenderAtChange = false;
        }

        private void OnLoaded()
        {
            ConfigurePlane();

            var group = new VisualObjectRenderer();
            var syncp1 = new SynchronizedProgress(0);
            var syncp2 = new SynchronizedProgress(1);

            plane.Grid.Stroke = null;
            plane.Grid.SecondaryStroke = null;
            plane.Axes.Style(null, null);
            plane.AxesNumbers.Style(null, null);

            var curveExp = Geometry.Parse("M0,0 c0-73.08,133.23-21.34,133.23,0");
            var curve = Geometry.Parse("M0,0 c0-140.72,255.69-140.33,255.69,0");

            var xyz = Tex(@"xyz", new Point(-10, 6)).Color(0, -1, FlatBrushes.Alizarin, null);
            var zfgyufdzu = Tex(@"\frac{df}{dx}?", new Point(5, 12.5)).Color(0, -1, FlatBrushes.Clouds, null).Color(0, 2, FlatBrushes.PeterRiver, null).Color(3, 2, FlatBrushes.SunFlower, null);
            var xzfgyufdzu = new MorphingVisualObject(zfgyufdzu.GetTransformedCharacters(true), xyz.GetTransformedCharacters(true)).Style(FlatBrushes.Alizarin);
            zfgyufdzu.Color(0, -1, null, null);
            //xzfgyufdzu.Correspondances = new CorrespondanceDictionary { { 2, 2 } };
            var text = Tex("(a+b)(a-b)=a^2-b^2", new Point(-25, 12.5), 130).Brace(0, 0, 4, 4, syncp2);
            var step1 = Tex("(a+b)(a-b)=aa+-bb^2", new Point(-25, 12.5), 130);

            var eq = Characters(text, 10, 1, false);

            var a1 = Characters(text, 1, 1, false).Insert(0, -1, step1, 11, 1, 0, syncp1).TranslateAlongPath(0, -1, curve, Translations.Y, 0, syncp1);
            var a2 = Characters(text, 6, 1, false).Insert(0, -1, step1, 12, 1, 0, syncp1).TranslateAlongPath(0, -1, curve, Translations.Y, 0, syncp1);

            var b1 = Characters(text, 3, 1, false).Insert(0, -1, step1, 15, 1, 0, syncp1).TranslateAlongPath(0, -1, curve, Translations.MinusY, 0, syncp1);
            var b2 = Characters(text, 8, 1, false).Insert(0, -1, step1, 16, 1, 0, syncp1).TranslateAlongPath(0, -1, curve, Translations.MinusY, 0, syncp1);

            var p = Characters(text, 2, 1, false).Insert(0, -1, step1, 13, 1, 0, syncp1);
            var m = Characters(text, 7, 1, false).Insert(0, -1, step1, 14, 1, 0, syncp1);
            var m2 = Characters(text, 13, 1, false).Insert(0, -1, p, 0, -1, 1, true, false, default);

            var asq = Characters(text, 11, 2, false);
            var bsq = Characters(text, 14, 2, false);

            var elc = new ELC { PointsRadius = 7, Center = plane.Origin, Rank = 5 };

            group.Children.Add(zfgyufdzu);
            group.Children.Add(xzfgyufdzu);
            group.Children.Add(text);
            group.Children.Add(eq);
            group.Children.Add(a1);
            group.Children.Add(a2);
            group.Children.Add(b1);
            group.Children.Add(b2);
            group.Children.Add(p);
            group.Children.Add(m);
            group.Children.Add(m2);
            group.Children.Add(asq);
            group.Children.Add(bsq);

            plane.VisualObjects.Add(group);
            plane.VisualObjects.Add(elc);

            async Task Animate()
            {
                await AnimateDouble(null, value => xzfgyufdzu.Progress = value, 0, 1, TimeSpan.FromSeconds(0.7), new RepeatBehavior(4), true, false, new CubicEase { EasingMode = EasingMode.EaseInOut }, 60);
                elc.Stroke = new Pen(FlatBrushes.Alizarin, 3);
                elc.PointsFill = FlatBrushes.SunFlower;
                elc.PointsStroke = new Pen(FlatBrushes.SunFlower.Edit(brush => brush.Opacity = 0.6), 0);
                await elc.Animate(true, TimeSpan.FromSeconds(2), null, new ELCWriteCharacterEffect(0, -1, new Progress(0, ProgressMode.LaggedStart)));
                await elc.Animate(false, TimeSpan.FromSeconds(0.5), null, new ELCFocusCharacterEffect(0, -1, i => i == 3, 0.1, 0)).AtLeast(200);
                plane.Grid.Stroke = new Pen(Brushes.DeepSkyBlue, 1);
                plane.Grid.SecondaryStroke = new Pen(Brushes.DeepSkyBlue.Edit(fill => fill.Opacity = 0.3), 1);
                await plane.Grid.Animate(true, TimeSpan.FromSeconds(4), null, new StrokeCharacterEffect(0, -1, new Progress(0, ProgressMode.LaggedStart)) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } });
                plane.Axes.Style(null, new Pen(FlatBrushes.Clouds, 2));
                await plane.Axes.Animate(true, TimeSpan.FromSeconds(1), null, new StrokeCharacterEffect(0, -1, 0) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } });
                plane.AxesNumbers.Style(FlatBrushes.Clouds, null);
                await plane.AxesNumbers.Write(false);

                await zfgyufdzu.Color(0, -1, FlatBrushes.Clouds, null).Color(0, 2, FlatBrushes.PeterRiver, null).Color(3, 2, FlatBrushes.SunFlower, null).Write().AtLeast(120);
                await zfgyufdzu.ReColor(0, -1, null, null).AtMost(15);
                await text.ReColor(0, 10, FlatBrushes.Clouds, null);
                await syncp2.Animate(0.8, new CubicEase { EasingMode = EasingMode.EaseInOut }, 1, 0);
                await eq.Color(0, -1, FlatBrushes.Clouds, null).Write();

                a1.Color(0, -1, FlatBrushes.Clouds, null);
                a2.Color(0, -1, FlatBrushes.Clouds, null);
                b1.Color(0, -1, FlatBrushes.Clouds, null);
                b2.Color(0, -1, FlatBrushes.Clouds, null);
                p.Color(0, -1, FlatBrushes.Clouds, null);
                m.Color(0, -1, FlatBrushes.Clouds, null);

                await syncp1.Animate(1.5, new CubicEase { EasingMode = EasingMode.EaseInOut });
                await Task.WhenAll(asq.Color(0, -1, FlatBrushes.Clouds, null).PowerFrom(a1, a2), bsq.Color(0, -1, FlatBrushes.Clouds, null).PowerFrom(Characters(text, 14, 1), b1, b2), Group(true, false, Characters(text, 13, 1), p, m)).AtMost(35);
                m2.Color(0, -1, FlatBrushes.Clouds, null);
                await p.Animate(false, TimeSpan.FromSeconds(0.9), new CubicEase { EasingMode = EasingMode.EaseOut }, new SizeCharacterEffect(0, -1, new Size(text.Size("-").Width, 0), new RectPoint(0.5, 0.5), 0)).AtMost(20);
                m2.Destroy();
                await Timing.FramesDelay(65);
            }

            this.Animate = Animate;
        }
    }
}
