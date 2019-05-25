using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Coord
{
    /// <summary>
    /// Objet visuel représentant un point du plan
    /// </summary>
    public class PointVisualObject : VisualObject
    {
        public override string Type => Definition switch
        {
            PointPointDefinition _ => "PointPoint",
            MiddlePointDefinition _ => "MiddlePoint",
            LineIntersectionPointDefinition _ => "LineIntersectionPoint",
            TranslationPointDefinition _ => "TranslationPoint",
            OperationsPointDefinition _ => "OperationsPoint",
            MultiOperationsPointDefinition _ => "MultiOperationsPoint",
            _ => "Point"
        };

        public PointDefinition Definition { get => (PointDefinition)GetValue(DefinitionProperty); set => SetValue(DefinitionProperty, value); }
        public static readonly DependencyProperty DefinitionProperty = CreateProperty<PointDefinition>(true, true, "Definition", typeof(PointVisualObject));

        public double Radius { get => (double)GetValue(RadiusProperty); set => SetValue(RadiusProperty, value); }
        public static readonly DependencyProperty RadiusProperty = CreateProperty(true, true, "Radius", typeof(PointVisualObject), 10.0);

        /// <summary>
        /// Dans le cas où <see cref="Definition"/> est une <see cref="PointPointDefinition"/>, définit <see cref="PointPointDefinition.InPoint"/> de cette définition
        /// </summary>
        /// <param name="inPoint">Point du plan</param>
        public void SetInPoint(Point inPoint) { if (Definition is PointPointDefinition definition) definition.InPoint = inPoint; }
        public void SetInPoint(double x, double y) => SetInPoint(new Point(x, y));

        public override string ToString() => Definition.InPoint.ToString();

        protected override IReadOnlyCollection<Character> GetCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager) => GetCharacters(coordinatesSystemManager, Fill, Stroke, Definition, Radius);
        public static Character[] GetCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager, Brush fill, PlanePen stroke, PointDefinition definition, double radius) => new[] { Character.Ellipse(coordinatesSystemManager.ComputeOutCoordinates(definition.InPoint), radius, radius).Color(fill, stroke) };

        public static implicit operator PointVisualObject(Point inPoint) => new PointVisualObject { Definition = new PointPointDefinition { InPoint = inPoint} };
        public static implicit operator Point(PointVisualObject pointVisualObject) => pointVisualObject.Definition.InPoint;
    }
}
