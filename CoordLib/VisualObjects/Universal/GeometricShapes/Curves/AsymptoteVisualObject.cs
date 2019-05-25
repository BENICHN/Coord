using BenLib.Framework;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Coord
{
    public class AsymptoteVisualObject : VisualObject
    {
        public override string Type => "Asymptote";

        public Func<double, double> Function { get => (Func<double, double>)GetValue(FunctionProperty); set => SetValue(FunctionProperty, value); }
        public static readonly DependencyProperty FunctionProperty = CreateProperty<Func<double, double>>(true, true, "Function", typeof(AsymptoteVisualObject));

        public double X { get => (double)GetValue(XProperty); set => SetValue(XProperty, value); }
        public static readonly DependencyProperty XProperty = CreateProperty<double>(true, true, "X", typeof(AsymptoteVisualObject));

        public double Length { get => (double)GetValue(LengthProperty); set => SetValue(LengthProperty, value); }
        public static readonly DependencyProperty LengthProperty = CreateProperty<double>(true, true, "Length", typeof(AsymptoteVisualObject));

        public PlanePen DiffStroke { get => (PlanePen)GetValue(DiffStrokeProperty); set => SetValue(DiffStrokeProperty, value); }
        public static readonly DependencyProperty DiffStrokeProperty = CreateProperty<PlanePen>(true, true, "DiffStroke", typeof(AsymptoteVisualObject));

        public PointVisualObject Template { get => (PointVisualObject)GetValue(TemplateProperty); set => SetValue(TemplateProperty, value); }
        public static readonly DependencyProperty TemplateProperty = CreateProperty<PointVisualObject>(true, true, "Template", typeof(AsymptoteVisualObject));

        public static Character[] GetCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager, PlanePen stroke, PlanePen diffStroke, (Brush fill, PlanePen stroke, double radius) points, Func<double, double> function, double x, double length)
        {
            var point1i = new Point(x, function(x));
            var point1 = coordinatesSystemManager.ComputeOutCoordinates(point1i);
            var point2i = new Point(x + length, function(x + length));
            var point2 = coordinatesSystemManager.ComputeOutCoordinates(point2i);
            var line = LinearEquation.FromPoints(point1i, point2i);
            var (start, end) = LineVisualObject.GetEndpoints(line, coordinatesSystemManager);
            var point3 = -line.A / line.B > 0 ? new Point(point2.X, point1.Y) : new Point(point1.X, point2.Y);

            return new[]
            {
                Character.Line(coordinatesSystemManager.ComputeOutCoordinates(start), coordinatesSystemManager.ComputeOutCoordinates(end)).Color(stroke),
                Character.Line(point1, point3).Color(diffStroke),
                Character.Line(point3, point2).Color(diffStroke),
                Character.Ellipse(point1, points.radius, points.radius).Color(points.fill, points.stroke),
                Character.Ellipse(point2, points.radius, points.radius).Color(points.fill, points.stroke)
            };
        }

        protected override IReadOnlyCollection<Character> GetCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager) => GetCharacters(coordinatesSystemManager, Stroke, DiffStroke, (Template.Fill, Template.Stroke, Template.Radius), Function, X, Length);
    }
}
