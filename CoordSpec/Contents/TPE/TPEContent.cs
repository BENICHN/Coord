using BenLib.Framework;
using BenLib.Standard;
using BenLib.WPF;
using Coord;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static BenLib.Framework.NumFramework;
using static Coord.VisualObjects;
using static System.Math;

namespace CoordSpec
{
    public partial class MainWindow
    {
        public const int FPS = 60;

        private void ConfigurePlane()
        {
            plane.InputRange = new MathRect(-67.994455231128015, -14.104361337573064, 292.92869287526509, 219.69651965644886);
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

            var stopwatch = new FrameStopwatch(FPS);
            bool curves = true;
            bool trace = true;

            var accLS = new StaticPointSeries();
            var vitLS = new StaticPointSeries();
            var distLS = new StaticPointSeries();
            var curvesL = Renderer(Curve(accLS, false, false).Style(new PlanePen(FlatBrushes.PeterRiver, 3)), Curve(vitLS, false, false).Style(new PlanePen(FlatBrushes.SunFlower, 3)), Curve(distLS, false, false).Style(new PlanePen(FlatBrushes.Nephritis, 3)));

            var accXS = new StaticPointSeries();
            var vitXS = new StaticPointSeries();
            var distXS = new StaticPointSeries();
            var curvesX = Renderer(Curve(accXS, false, false).Style(new PlanePen(FlatBrushes.PeterRiver, 3)), Curve(vitXS, false, false).Style(new PlanePen(FlatBrushes.SunFlower, 3)), Curve(distXS, false, false).Style(new PlanePen(FlatBrushes.Nephritis, 3)));

            var accYS = new StaticPointSeries();
            var vitYS = new StaticPointSeries();
            var distYS = new StaticPointSeries();
            var curvesY = Renderer(Curve(accYS, false, false).Style(new PlanePen(FlatBrushes.PeterRiver, 3)), Curve(vitYS, false, false).Style(new PlanePen(FlatBrushes.SunFlower, 3)), Curve(distYS, false, false).Style(new PlanePen(FlatBrushes.Nephritis, 3)));

            var traceS = new StaticPointSeries();

            var canvas = Resources["Corps"] as Canvas;
            var corps = Character.FromCanvas(canvas).ToArray();
            canvas.Children.RemoveAt(canvas.Children.Count - 1);

            var watney = new Watney
            {
                Location = (Point)VectorFromPolarCoordinates(200, PI / 4),
                Speed = VectorFromPolarCoordinates(12, PI / 12),
                Mass = 100,

                Pressure = 3.3,
                HoleArea = 100,
                MassRate = 0.0021,
                Size = new Size(0.875, 2.0),
                ArmAngle = PI / 2,
                ArmResistance = 1.3,

                BodyGraphic = corps,
                BodyGraphicBounds = Character.FromCanvas(canvas).Geometry().Bounds,
                ArmGraphic = Character.FromCanvas(Resources["Bras"] as Canvas).ToArray(),
                ArmLocation = new RectPoint(0.19, 0.41),
                ArmAnchor = new RectPoint(1, 0.76),
                Minimal = false,

                Fill = FlatBrushes.Alizarin,
                Stroke = new PlanePen(FlatBrushes.Alizarin, 4)
            };

            plane.VisualObjects.Add(curvesL);
            plane.VisualObjects.Add(Curve(traceS, false, false).Style(new PlanePen(FlatBrushes.Amethyst, 3)));
            plane.VisualObjects.Add(watney);
            plane.OverAxesNumbers = watney;

            PreviewKeyDown += (sender, e) =>
            {
                switch (e.Key)
                {
                    case Key.M:
                        switch (watney.Minimal)
                        {
                            case true:
                                watney.Minimal = false;
                                break;
                            case null:
                                watney.Minimal = true;
                                break;
                            case false:
                                watney.Minimal = null;
                                break;
                        }
                        plane.RenderChanged();
                        break;
                    case Key.T:
                        if (trace)
                        {
                            trace = false;
                            traceS.Points.Clear();
                        }
                        else trace = true;
                        break;
                    case Key.Z:
                        timeFactorSlider.Value = 0;
                        break;
                    case Key.U:
                        timeFactorSlider.Value = 1;
                        break;
                }
            };

            angleSlider.ValueChanged += (sender, e) =>
            {
                watney.ArmAngle = PI * angleSlider.Value;
                plane.RenderChanged();
            };

            crL.PreviewMouseDown += (sender, e) =>
            {
                if (curves && e.RightButton == MouseButtonState.Pressed)
                {
                    accLS.Destroy();
                    vitLS.Destroy();
                    distLS.Destroy();
                    curvesL.Destroy();

                    accXS.Destroy();
                    vitXS.Destroy();
                    distXS.Destroy();
                    curvesX.Destroy();

                    accYS.Destroy();
                    vitYS.Destroy();
                    distYS.Destroy();
                    curvesY.Destroy();

                    GC.Collect();
                    curves = false;
                }
            };

            tftb.LostFocus += (sender, e) => timeFactorSlider.Maximum = tftb.Text.ToDouble() ?? 20;
            dmtb.LostFocus += (sender, e) => watney.MassRate = dmtb.Text.ToDouble() ?? 0;

            LRB.Checked += (sender, e) => ChangeCurve("L");
            XRB.Checked += (sender, e) => ChangeCurve("X");
            YRB.Checked += (sender, e) => ChangeCurve("Y");

            void ChangeCurve(string name)
            {
                plane.VisualObjects.Remove(curvesL);
                plane.VisualObjects.Remove(curvesX);
                plane.VisualObjects.Remove(curvesY);

                switch (name)
                {
                    case "L":
                        plane.VisualObjects.Insert(0, curvesL);
                        break;
                    case "X":
                        plane.VisualObjects.Insert(0, curvesX);
                        break;
                    case "Y":
                        plane.VisualObjects.Insert(0, curvesY);
                        break;
                }
            }

            async Task Animate()
            {
                int refresh = 0;

                CompositionTarget.Rendering += (sender, e) =>
                {
                    int timeFactor = (int)timeFactorSlider.Value;
                    double seconds = stopwatch.ElapsedMilliseconds / 1000;
                    timeFactor.Times(i => watney.Update());
                    stopwatch.Spend(timeFactor);

                    if (timeFactor > 0)
                    {
                        if (FTB.IsChecked == true) plane.InputRange = new MathRect(138.7 + watney.Location.X - 141.42135623731, 136 + watney.Location.Y - 141.42135623731, 15.840907872701388, 11.880680904526047);
                        if (curves)
                        {
                            var location = (Vector)watney.Location;
                            var speed = watney.Speed;
                            var acceleration = watney.Acceleration;

                            accLS.Points.Add(new Point(seconds, acceleration.Length));
                            vitLS.Points.Add(new Point(seconds, speed.Length));
                            distLS.Points.Add(new Point(seconds, location.Length));

                            accXS.Points.Add(new Point(seconds, acceleration.X));
                            vitXS.Points.Add(new Point(seconds, speed.X));
                            distXS.Points.Add(new Point(seconds, location.X));

                            accYS.Points.Add(new Point(seconds, acceleration.Y));
                            vitYS.Points.Add(new Point(seconds, speed.Y));
                            distYS.Points.Add(new Point(seconds, location.Y));
                        }

                        if (trace) traceS.Points.Add(watney.Location);

                        if (refresh == 30)
                        {
                            massL.Content = RoundW(watney.Mass);
                            forceL.Content = ToStringW(RoundW(watney.Acceleration * watney.Mass));
                            momentumL.Content = ToStringW(RoundW(watney.Momentum));
                            locationL.Content = ToStringW(RoundW(watney.Location));
                            speedL.Content = ToStringW(RoundW(watney.Speed));
                            accelerationL.Content = ToStringW(RoundW(watney.Acceleration));

                            angularMassL.Content = RoundW(watney.AngularMass);
                            torqueL.Content = RoundW(watney.AngularAcceleration * watney.AngularMass);
                            angularMomentL.Content = RoundW(watney.AngularMoment);
                            angleL.Content = RoundW(watney.Angle);
                            angularSpeedL.Content = RoundW(watney.AngularSpeed);
                            angularAccelerationL.Content = RoundW(watney.AngularAcceleration);

                            refresh = 0;
                        }
                        else refresh++;
                    }
                };

                await Timing.Wait();
            }

            this.Animate = Animate;
        }

        public double RoundW(double x) => Round(x, 2);
        public Point RoundW(Point point) => new Point(RoundW(point.X), RoundW(point.Y));
        public Vector RoundW(Vector vector) => new Vector(RoundW(vector.X), RoundW(vector.Y));
        public string ToStringW(Vector vector) => $"({vector.X} ; {vector.Y})";
        public string ToStringW(Point point) => $"({point.X} ; {point.Y})";

        private void Plane_PreviewMouseDown(object sender, MouseButtonEventArgs e) { if (e.RightButton == MouseButtonState.Pressed) Infos.Visibility = Infos.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible; }

        private void SwitchableTextBox_LostFocus(object sender, RoutedEventArgs e)
        {

        }
    }
}
