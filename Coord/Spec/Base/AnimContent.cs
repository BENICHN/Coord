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
using static BenLib.IntInterval;
using static Coord.VisualObjects;
using static System.Math;

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
            ImagesPath = "";
        }

        private void OnLoaded()
        {
            ConfigurePlane();

            double textSize = 80;
            var brush = FlatBrushes.Clouds;
            var group = new VisualObjectRenderer();

            //Objects===================================================================================================================================================================================================

            var obj = Tex(@"", Point(-25, 12.5), textSize);
            group.Children.Add(obj);

            //Step1----------------------------------------------------------------------------------------------------------------------------------------------------------

            var syncp1a = new SynchronizedProgress(0);
            var syncp1b = new SynchronizedProgress(1);

            //===================================================================================================================================================================================================

            plane.VisualObjects.Add(group);

            void Display(params VisualObject[] objects) { foreach (var visualObject in objects) visualObject.Color(brush); }
            void Hide(params VisualObject[] objects) { foreach (var visualObject in objects) visualObject.Hide(); }
            void Destroy(params VisualObject[] objects) { foreach (var visualObject in objects) visualObject.Destroy(); }

            async Task Animate()
            {
                //Step1----------------------------------------------------------------------------------------------------------------------------------------------------------

                Display();

                await Task.WhenAll(syncp1a.Animate(1, new CubicEase { EasingMode = EasingMode.EaseInOut }), syncp1b.Animate(1, new CubicEase { EasingMode = EasingMode.EaseInOut }, 1, 0));

                Destroy();
            }

            this.Animate = Animate;
        }
    }
}
