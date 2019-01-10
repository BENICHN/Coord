using BenLib;
using BenLib.WPF;
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
using static System.Math;

namespace Coord
{
    public partial class MainWindow
    {
        public const int FPS = 60;

        private void ConfigurePlane()
        {
            plane.InputRange = new MathRect(-10, -10, 40, 20);
            plane.Grid.Primary = true;
            plane.Grid.Secondary = true;
            plane.Axes.Direction = AxesDirection.Both;
            plane.AxesNumbers.Direction = AxesDirection.Both;
            plane.RenderAtChange = false;
            SaveImages = false;
            ImagesPath = "";
        }

        private void OnLoaded()
        {
            ConfigurePlane();

            var a = new AsymptoteVisualObject { Function = x => x * x, X = -2, Length = 0.5, DiffStroke = new Pen(FlatBrushes.Alizarin, 2) { DashStyle = new DashStyle(new double[] { 2 }, 0)} }.Style(new Pen(FlatBrushes.SunFlower, 2));

            plane.VisualObjects.Add(Curve(new FunctionSeries(x => x * x, SeriesType.Y), false, false).Style(new Pen(FlatBrushes.PeterRiver, 2)));
            plane.VisualObjects.Add(a);

            async Task Animate()
            {
                await AnimateDouble(null, x => a.X = x, -2, 2, TimeSpan.FromSeconds(5), default, false, false, new CubicEase { EasingMode = EasingMode.EaseInOut }, 60);
            }

            this.Animate = Animate;
        }
    }
}
