using BenLib;
using BenLib.WPF;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static Coord.VisualObjects;

namespace Coord
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const int FPS = 60;

        public object Current { get; private set; }
        public VisualObject CurrentCurrent { get; private set; }
        public bool Moving { get; private set; }

        private void ConfigurePlane()
        {
            plane.InputRange = new MathRect(0, 0, 24, 10);
            plane.Grid.Primary = true;
            plane.Grid.Secondary = true;
            plane.Axes.Direction = AxesDirection.Both;
            plane.AxesNumbers.Direction = AxesDirection.Both;
            plane.RenderAtChange = false;
        }

        public MainWindow() => InitializeComponent();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ConfigurePlane();

            var point = Point(12, 5).Style(FlatBrushes.Alizarin);
            var line = Line(new LinearEquation(1, 0)).Style(new Pen(FlatBrushes.Alizarin, 3));
            var perpend = PerpendicularLine(point, line).Style(new Pen(FlatBrushes.PeterRiver, 3));
            var text = InTex("a=b", 3, Point(12, 6), RectPoint.Center).Color(FlatBrushes.Clouds);
            var circle = Circle(point, 2).Style(new Pen(FlatBrushes.Carrot, 3));
            var inter = Intersection(text, perpend, 5).Style(FlatBrushes.Wisteria, new Pen(FlatBrushes.MidnightBlue, 2));

            plane.VisualObjects.Add(text);
            plane.VisualObjects.Add(circle);
            plane.VisualObjects.Add(point);
            plane.VisualObjects.Add(line);
            plane.VisualObjects.Add(perpend);
            plane.VisualObjects.Add(inter);
            plane.VisualObjects.Add(Polygon(inter.GetIntersectionPoints()).Style(new Pen(FlatBrushes.PeterRiver.Edit(brush => brush.Opacity = 0.3), 10)));
            plane.VisualObjects.Add(ParallelLine(inter.GetIntersectionPoint(0), line).Style(new Pen(FlatBrushes.GreenSea, 3)));
            plane.VisualObjects.Add(Circle(inter.GetIntersectionPoint(1), inter.GetIntersectionPoint(2)).Style(new Pen(FlatBrushes.Nephritis, 3)));
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => plane.VisualObjects.Clear();

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Current = (sender as Button).Name switch
            {
                "Point" => new PointPointConfiguration(),
                "InTex" => new InTexConfiguration(),
                "Line" => new PointPointLineConfiguration(),
                _ => (object)null
            };
            plane.RestoreCursor = Cursors.Cross;
            plane.EnableSelectionRect = false;
        }

        private void Plane_PreviewCharacterMouseDown(object sender, EventArgs<MouseButtonEventArgs, HitTestCharacterResult[]> e)
        {
            if (e.Param1.LeftButton == MouseButtonState.Pressed && !plane.Moving && !plane.Selecting)
            {
                if (Current is PointPointConfiguration pointPointConfiguration)
                {
                    if (!pointPointConfiguration.Placed)
                    {
                        CurrentCurrent = pointPointConfiguration.VisualObject = Point(plane.InMousePosition).Style(FlatBrushes.Alizarin);
                        plane.VisualObjects.Add(pointPointConfiguration.VisualObject);
                    }
                }
                else if (Current is InTexConfiguration inTexConfiguration) { if (inTexConfiguration.Point == null) CurrentCurrent = inTexConfiguration.Point = CreatePoint(); }
                else if (Current is PointPointLineConfiguration pointPointLineConfiguration)
                {
                    if (pointPointLineConfiguration.Point1 == null) CurrentCurrent = pointPointLineConfiguration.Point1 = CreatePoint();
                    else if (pointPointLineConfiguration.Point1.Fixed && pointPointLineConfiguration.Point2 == null)
                    {
                        CurrentCurrent = pointPointLineConfiguration.Point2 = CreatePoint();
                        pointPointLineConfiguration.Line.Definition = new PointPointLineDefinition(pointPointLineConfiguration.Point1, pointPointLineConfiguration.Point2);
                    }
                }
                else if (e.Param2.Length > 0) Moving = true;
            }

            CreatedPoint CreatePoint(bool addToPlane = true)
            {
                var clickedPoint = e.Param2.FirstOrDefault(hr => hr.Owner is PointVisualObject).Owner;
                if (clickedPoint is PointVisualObject pointVisualObject) return new CreatedPoint(pointVisualObject, false);
                else
                {
                    var result = Point(plane.InMousePosition).Style(FlatBrushes.Alizarin);
                    if (addToPlane) plane.VisualObjects.Add(result);
                    return new CreatedPoint(result, true);
                }
            }

            plane.RenderChanged();
        }

        private void Plane_PreviewCharacterMouseMove(object sender, EventArgs<MouseEventArgs, HitTestCharacterResult[]> e)
        {
            if (CurrentCurrent != null)
            {
                switch (CurrentCurrent.Type)
                {
                    case "PointPoint":
                        MovePoint(CurrentCurrent as PointVisualObject);
                        break;
                }
            }
            else if (Moving)
            {
                foreach (PointVisualObject pointVisualObject in plane.GetSelection().Selection.Where(kvp => kvp.Key.Type == "PointPoint").Select(kvp => kvp.Key)) pointVisualObject.SetInPoint(pointVisualObject.Definition.InPoint + plane.InOffset);
            }

            void MovePoint(PointVisualObject pointVisualObject) => pointVisualObject.SetInPoint(plane.InMousePosition);

            plane.RenderChanged();
        }

        private void Plane_PreviewCharacterMouseUp(object sender, EventArgs<MouseButtonEventArgs, HitTestCharacterResult[]> e)
        {
            if (e.Param1.LeftButton == MouseButtonState.Released)
            {
                if (Current is PointPointConfiguration pointPointConfiguration) { if (pointPointConfiguration.Placed) End(); }
                else if (Current is InTexConfiguration inTexConfiguration)
                {
                    var texEdit = new TexEdit(true);
                    if (texEdit.ShowDialog() == true) plane.VisualObjects.Add(InTex(texEdit.Text.Text, texEdit.Text.Scale, inTexConfiguration.Point, RectPoint.BottomLeft).Color(FlatBrushes.Clouds));
                    else inTexConfiguration.Point.DestroyIfCreated();
                    End();
                }
                else if (Current is PointPointLineConfiguration pointPointLineConfiguration)
                {
                    if (!pointPointLineConfiguration.Point1.Fixed)
                    {
                        pointPointLineConfiguration.Point1.Fixed = true;
                        pointPointLineConfiguration.Line = Line(pointPointLineConfiguration.Point1, (PointVisualObject)(CurrentCurrent = Point(plane.InMousePosition))).Style(new Pen(FlatBrushes.SunFlower, 3));
                        plane.VisualObjects.Add(pointPointLineConfiguration.Line);
                    }
                    else End();
                }
                else End();

                void End()
                {
                    Moving = false;
                    Current = null;
                    CurrentCurrent = null;
                    plane.RestoreCursor = null;
                    plane.EnableSelectionRect = true;
                }
            }

            plane.RenderChanged();
        }
    }

    public class PointPointConfiguration
    {
        public bool Placed => VisualObject != null;
        public PointVisualObject VisualObject { get; set; }
    }

    public class InTexConfiguration
    {
        public CreatedPoint Point { get; set; }
    }

    public class PointPointLineConfiguration
    {
        public CreatedPoint Point1 { get; set; }
        public CreatedPoint Point2 { get; set; }
        public LineVisualObject Line { get; set; }
    }

    public class CreatedPoint
    {
        public CreatedPoint(PointVisualObject pointVisualObject, bool created)
        {
            PointVisualObject = pointVisualObject;
            Created = created;
        }

        public PointVisualObject PointVisualObject { get; set; }
        public bool Created { get; set; }
        public bool Fixed { get; set; }

        public void DestroyIfCreated() { if (Created) PointVisualObject.Destroy(); }

        public static implicit operator PointVisualObject(CreatedPoint createdPoint) => createdPoint.PointVisualObject;
    }
}
