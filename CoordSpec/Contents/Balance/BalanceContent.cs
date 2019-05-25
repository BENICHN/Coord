using BenLib.Standard;
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
using static BenLib.Standard.Interval<int>;
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
            ImagesPath = "";
        }

        private void OnLoaded()
        {
            ConfigurePlane();

            var balance = new Balance
            {
                Balls = new[] { (0.08, -2.0), (0.05, 3.0) },
                Mass = 1,
                AngularMass = 1,
                Location = new Point(0, 1),
                Radius = 1,
                Length = 4,
                Fill = null,
                Stroke = new PlanePen(FlatBrushes.Alizarin, 3)
            };
            plane.VisualObjects.Add(balance);

            async Task Animate()
            {
                CompositionTarget.Rendering += (sender, e) => balance.Update();
                await Timing.Wait();
            }

            this.Animate = Animate;
        }
    }
}
