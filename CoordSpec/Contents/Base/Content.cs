using BenLib.Standard;
using BenLib.WPF;
using Coord;
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

            async Task Animate()
            {
            }

            this.Animate = Animate;
        }
    }
}
