using BenLib.Framework;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Coord
{
    public class AsymptoteVisualObject : VisualObject
    {
        protected override Freezable CreateInstanceCore() => new AsymptoteVisualObject();

        public override string Type => "Asymptote";

        public Func<double, double> Function { get => (Func<double, double>)GetValue(FunctionProperty); set => SetValue(FunctionProperty, value); }
        public static readonly DependencyProperty FunctionProperty = CreateProperty<AsymptoteVisualObject, Func<double, double>>(true, true, true, "Function");

        public double X { get => (double)GetValue(XProperty); set => SetValue(XProperty, value); }
        public static readonly DependencyProperty XProperty = CreateProperty<AsymptoteVisualObject, double>(true, true, true, "X");

        public double Length { get => (double)GetValue(LengthProperty); set => SetValue(LengthProperty, value); }
        public static readonly DependencyProperty LengthProperty = CreateProperty<AsymptoteVisualObject, double>(true, true, true, "Length");

        public Pen DiffStroke { get => (Pen)GetValue(DiffStrokeProperty); set => SetValue(DiffStrokeProperty, value); }
        public static readonly DependencyProperty DiffStrokeProperty = CreateProperty<AsymptoteVisualObject, Pen>(true, true, true, "DiffStroke");

        public PointVisualObject Template { get => (PointVisualObject)GetValue(TemplateProperty); set => SetValue(TemplateProperty, value); }
        public static readonly DependencyProperty TemplateProperty = CreateProperty<AsymptoteVisualObject, PointVisualObject>(true, true, true, "Template");

        public static Character[] GetCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager, Pen stroke, Pen diffStroke, (Brush fill, Pen stroke, double radius) points, Func<double, double> function, double x, double length)
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
