using BenLib.Standard;
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
using static System.Math;

namespace CoordSpec
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
            ImagesPath = "Oly";
        }

        private void OnLoaded()
        {
            ConfigurePlane();

            double f(double x)
            {
                decimal y = (decimal)Truncate(x);
                decimal result = 0;
                do
                {
                    decimal dec = y % 10;
                    y = (y - dec) / 10;
                    result += dec * dec;
                } while (y > 0);
                return (double)result;
            }

            plane.VisualObjects.Add(Curve(new FunctionSeries(f, SeriesType.Y), false, false).Style(new Pen(FlatBrushes.Amethyst, 3)));

            async Task Animate()
            {

            }

            this.Animate = Animate;
        }
    }
}
