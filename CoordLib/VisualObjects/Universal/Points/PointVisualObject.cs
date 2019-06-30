using System.Collections.Generic;
using System.Windows;

namespace Coord
{
    /// <summary>
    /// Objet visuel représentant un point du plan
    /// </summary>
    public class PointVisualObject : VisualObject
    {
        protected override Freezable CreateInstanceCore() => new PointVisualObject();

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
        public static readonly DependencyProperty DefinitionProperty = CreateProperty<PointVisualObject, PointDefinition>(true, true, true, "Definition");

        public double Radius { get => (double)GetValue(RadiusProperty); set => SetValue(RadiusProperty, value); }
        public static readonly DependencyProperty RadiusProperty = CreateProperty<PointVisualObject, double>(true, true, true, "Radius", 10);

        /// <summary>
        /// Dans le cas où <see cref="Definition"/> est une <see cref="PointPointDefinition"/>, définit <see cref="PointPointDefinition.InPoint"/> de cette définition
        /// </summary>
        /// <param name="inPoint">Point du plan</param>
        public void SetInPoint(Point inPoint) { if (Definition is PointPointDefinition definition) definition.InPoint = inPoint; }
        public void SetInPoint(double x, double y) => SetInPoint(new Point(x, y));

        protected internal override void Move(Point inPosition, Vector totalInOffset, Vector inOffset, Character clickHitTest) => SetInPoint(inPosition);

        public override string ToString() => Type + "{" + Definition.InPoint.ToString() + "}";

        protected override IReadOnlyCollection<Character> GetCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager) => new[] { Character.Ellipse(coordinatesSystemManager.ComputeOutCoordinates(Definition.InPoint), Radius, Radius).Color(Fill, Stroke) };

        public static implicit operator PointVisualObject(Point inPoint) => new PointVisualObject { Definition = new PointPointDefinition { InPoint = inPoint } };
        public static implicit operator Point(PointVisualObject pointVisualObject) => pointVisualObject.Definition.InPoint;
    }
}
