using BenLib.WPF;
using Coord;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static Coord.VisualObjects;

namespace CoordAnimation
{
    /// <summary>
    /// Logique d'interaction pour ConfigEdit.xaml
    /// </summary>
    public partial class ConfigEdit : UserControl
    {
        public object Current { get; private set; }
        public VisualObject CurrentCurrent { get; private set; }
        public bool Moving { get; private set; }
        private PointVisualObject m_basePoint;

        private void ConfigurePlane()
        {
            plane.InputRange = new MathRect(0, 0, 24, 10);
            plane.Grid.Primary = true;
            plane.Grid.Secondary = true;
            plane.Axes.Direction = AxesDirection.Both;
            plane.AxesNumbers.Direction = AxesDirection.Both;
            plane.RenderAtChange = false;
        }

        public ConfigEdit() => InitializeComponent();

        private void Window_Loaded(object sender, RoutedEventArgs e) => ConfigurePlane();//var point = Point(12, 5).Style(FlatBrushes.Alizarin);//var line = Line(new LinearEquation(1, 0)).Style(new PlanePen(FlatBrushes.Alizarin, 3));//var perpend = PerpendicularLine(point, line).Style(new PlanePen(FlatBrushes.PeterRiver, 3));//var text = InTex("a=b", 3, Point(12, 6), RectPoint.Center).Color(FlatBrushes.Clouds);//var circle = Circle(point, 2).Style(new PlanePen(FlatBrushes.Carrot, 3));//var inter = Intersection(text, perpend, 5).Style(FlatBrushes.Wisteria, new PlanePen(FlatBrushes.MidnightBlue, 2));//plane.VisualObjects.Add(text);//plane.VisualObjects.Add(circle);//plane.VisualObjects.Add(point);//plane.VisualObjects.Add(line);//plane.VisualObjects.Add(perpend);//plane.VisualObjects.Add(inter);//plane.VisualObjects.Add(Polygon(inter.GetIntersectionPoints()).Style(new PlanePen(FlatBrushes.PeterRiver.EditFreezable(brush => brush.Opacity = 0.3), 10)));//plane.VisualObjects.Add(ParallelLine(inter.GetIntersectionPoint(0), line).Style(new PlanePen(FlatBrushes.GreenSea, 3)));//plane.VisualObjects.Add(Circle(inter.GetIntersectionPoint(1), inter.GetIntersectionPoint(2)).Style(new PlanePen(FlatBrushes.Nephritis, 3)));//plane.VisualObjects.Add(Vector(point, point, inter.GetIntersectionPoint(0)).Style(FlatBrushes.Amethyst, new PlanePen(FlatBrushes.Amethyst, 3)));

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => plane.VisualObjects.Clear();

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Current = (sender as Button).Name switch
            {
                "Point" => new PointPointConfiguration(),
                "InTex" => new InTexConfiguration(),
                "Line" => new PointPointLineConfiguration(),
                "MidPoint" => new MiddlePointConfiguration(),
                "Manipulation" => new ManipulationConfiguration(),
                _ => (object)null
            };
            if (!(Current is ManipulationConfiguration)) plane.RestoreCursor = Cursors.Cross;
            plane.EnableSelectionRect = Current is ManipulationConfiguration;
            plane.EnableCharactersManipulating = Current is ManipulationConfiguration;
        }

        private void Plane_PreviewCharacterMouseDown(object sender, EventArgs<MouseButtonEventArgs, Character[]> e)
        {
            if (e.Param1.LeftButton == MouseButtonState.Pressed && !plane.Moving && !plane.Selecting)
            {
                switch (Current)
                {
                    case PointPointConfiguration pointPointConfiguration:
                        {
                            if (!pointPointConfiguration.Placed)
                            {
                                CurrentCurrent = pointPointConfiguration.VisualObject = Point(plane.InMouseMagnetPosition).Style(FlatBrushes.Alizarin);
                                plane.VisualObjects.Add(pointPointConfiguration.VisualObject);
                            }
                        }
                        break;
                    case InTexConfiguration inTexConfiguration:
                        {
                            if (inTexConfiguration.Point == null) CurrentCurrent = inTexConfiguration.Point = CreatePoint();
                        }
                        break;
                    case PointPointLineConfiguration pointPointLineConfiguration:
                        {
                            if (pointPointLineConfiguration.Point1 == null) CurrentCurrent = pointPointLineConfiguration.Point1 = CreatePoint();
                            else if (pointPointLineConfiguration.Point1.Fixed && pointPointLineConfiguration.Point2 == null)
                            {
                                CurrentCurrent = pointPointLineConfiguration.Point2 = CreatePoint();
                                pointPointLineConfiguration.Line.Definition = new PointPointLineDefinition { PointA = pointPointLineConfiguration.Point1, PointB = pointPointLineConfiguration.Point2 };
                            }
                        }
                        break;
                    case MiddlePointConfiguration middlePointConfiguration:
                        {
                            if (middlePointConfiguration.Point1 == null) CurrentCurrent = middlePointConfiguration.Point1 = CreatePoint();
                            else if (middlePointConfiguration.Point1.Fixed && middlePointConfiguration.Point2 == null)
                            {
                                CurrentCurrent = middlePointConfiguration.Point2 = CreatePoint();
                                middlePointConfiguration.MidPoint = MiddlePoint(middlePointConfiguration.Point1, middlePointConfiguration.Point2).Extend(5).Style(FlatBrushes.Pomegranate);
                                plane.VisualObjects.Add(middlePointConfiguration.MidPoint);
                            }
                        }
                        break;
                    case ManipulationConfiguration manipulationConfiguration:
                        break;
                    default:
                        if (e.Param2.Length > 0) Moving = true;
                        break;
                }
            }

            CreatedPoint CreatePoint(bool addToPlane = true)
            {
                var clickedPoint = e.Param2.FirstOrDefault(hr => hr.Owner is PointVisualObject)?.Owner;
                if (clickedPoint is PointVisualObject pointVisualObject) return new CreatedPoint(pointVisualObject, false);
                else
                {
                    var result = Point(plane.InMouseMagnetPosition).Style(FlatBrushes.Alizarin);
                    if (addToPlane) plane.VisualObjects.Add(result);
                    return new CreatedPoint(result, true);
                }
            }

            plane.RenderChanged();
        }

        private void Plane_PreviewCharacterMouseMove(object sender, EventArgs<MouseEventArgs, Character[]> e)
        {
            if (CurrentCurrent != null)
            {
                switch (CurrentCurrent.Type)
                {
                    case "PointPoint":
                        (CurrentCurrent as PointVisualObject).SetInPoint(plane.InMouseMagnetPosition);
                        break;
                }
                plane.RenderChanged();
            }
            else if (Moving)
            {
                var selectedPoints = plane.Selection.VisualObjects.Where(vo => vo.Type == "PointPoint").OfType<PointVisualObject>().ToArray();
                foreach (var pointVisualObject in selectedPoints) { if (m_basePoint == null && e.Param2.Any(hr => hr.Owner == pointVisualObject)) m_basePoint = pointVisualObject; }
                var offset = m_basePoint == null ? plane.InOffset : (Vector)(plane.InMouseMagnetPosition - (Vector)m_basePoint.Definition.InPoint);
                foreach (var pointVisualObject in selectedPoints) pointVisualObject.SetInPoint(pointVisualObject.Definition.InPoint + offset);
                plane.RenderChanged();
            }
        }

        private void Plane_PreviewCharacterMouseUp(object sender, EventArgs<MouseButtonEventArgs, Character[]> e)
        {
            if (e.Param1.LeftButton == MouseButtonState.Released)
            {
                switch (Current)
                {
                    case PointPointConfiguration pointPointConfiguration:
                        {
                            if (pointPointConfiguration.Placed) End();
                        }
                        break;
                    case InTexConfiguration inTexConfiguration:
                        {
                            var texEdit = new TexEdit(true);
                            if (texEdit.ShowDialog() == true) plane.VisualObjects.Add(InTex(texEdit.Text.Text, texEdit.Text.Scale, inTexConfiguration.Point, RectPoint.BottomLeft).Color(FlatBrushes.Clouds));
                            else inTexConfiguration.Point.DestroyIfCreated();
                            End();
                        }
                        break;
                    case PointPointLineConfiguration pointPointLineConfiguration:
                        {
                            if (!pointPointLineConfiguration.Point1.Fixed)
                            {
                                pointPointLineConfiguration.Point1.Fixed = true;
                                pointPointLineConfiguration.Line = Line(pointPointLineConfiguration.Point1, (PointVisualObject)(CurrentCurrent = Point(plane.InMouseMagnetPosition))).Style(new PlanePen(FlatBrushes.SunFlower, 3));
                                plane.VisualObjects.Add(pointPointLineConfiguration.Line);
                            }
                            else End();
                        }
                        break;
                    case MiddlePointConfiguration middlePointConfiguration:
                        {
                            if (!middlePointConfiguration.Point1.Fixed)
                            {
                                middlePointConfiguration.Point1.Fixed = true;
                                CurrentCurrent = null;
                            }
                            else End();
                        }
                        break;
                    case ManipulationConfiguration _:
                        break;
                    default:
                        m_basePoint = null;
                        End();
                        break;
                }

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

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {

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

    public class MiddlePointConfiguration
    {
        public CreatedPoint Point1 { get; set; }
        public CreatedPoint Point2 { get; set; }
        public PointVisualObject MidPoint { get; set; }
    }

    public class ManipulationConfiguration
    {

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
