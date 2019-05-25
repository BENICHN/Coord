using BenLib.Standard;
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
            plane.InputRange = new MathRect(0, 0, 24, 10);
            plane.Grid.Primary = false;
            plane.Grid.Secondary = false;
            plane.Axes.Direction = AxesDirection.None;
            plane.AxesNumbers.Direction = AxesDirection.None;
            plane.RenderAtChange = false;
            SaveImages = false;
            ImagesPath = "TPEAnim";
        }

        private void OnLoaded()
        {
            ConfigurePlane();

            double textSize = 1.6;
            var inAnchorPoint = Point(12, 5);
            var brush = FlatBrushes.Clouds;
            var group = new VisualObjectRenderer();

            //Objects============================================================================================================================================================================================

            var text = InTex("a=b", textSize, inAnchorPoint, RectPoint.Center).Color(FlatBrushes.Clouds);
            group.Children.Add(text);

            //===================================================================================================================================================================================================

            plane.VisualObjects.Add(group);

            void Display(params VisualObject[] objects) { foreach (var visualObject in objects) visualObject.Effects.Insert(0, new ColorCharacterEffect(PositiveReals, brush, null, false, 1)); }
            void Hide(params VisualObject[] objects) { foreach (var visualObject in objects) visualObject.Hide(); }
            void Color(Brush br, params VisualObject[] objects) { foreach (var visualObject in objects) visualObject.Color(br); }
            void Destroy(params VisualObject[] objects) { foreach (var visualObject in objects) visualObject.Destroy(); }

            async Task Animate()
            {

            }

            this.Animate = Animate;
        }
    }
}
