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
        public override string Type
        {
            get
            {
                var definitionType = Definition?.GetType();
                if (definitionType == typeof(PointPointDefinition)) return "PointPoint";
                if (definitionType == typeof(MiddlePointDefinition)) return "MiddlePoint";
                if (definitionType == typeof(LineIntersectionPointDefinition)) return "LineIntersectionPoint";
                if (definitionType == typeof(TranslationPointDefinition)) return "TranslationPoint";
                if (definitionType == typeof(OperationsPointDefinition)) return "OperationsPoint";
                if (definitionType == typeof(MultiOperationsPointDefinition)) return "MultiOperationsPoint";
                else return "Point";
            }
        }

        private PointDefinition m_definition;
        private double m_radius = 10;

        public PointVisualObject(Point inPoint) : this(new PointPointDefinition(inPoint)) { }
        public PointVisualObject(PointDefinition definition) => Definition = definition;

        /// <summary>
        /// Détermine les propriétés fondamentales de ce <see cref="PointVisualObject"/>
        /// </summary>
        public PointDefinition Definition
        {
            get => m_definition;
            set
            {
                UnRegister(m_definition);
                m_definition = value;
                Register(m_definition);
                NotifyChanged();
            }
        }

        /// <summary>
        /// Rayon à d'écran du disque représentant le point
        /// </summary>
        public double Radius
        {
            get => m_radius;
            set
            {
                m_radius = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Dans le cas où <see cref="Definition"/> est une <see cref="PointPointDefinition"/>, définit <see cref="PointPointDefinition.InPoint"/> de cette définition
        /// </summary>
        /// <param name="inPoint">Point du plan</param>
        public void SetInPoint(Point inPoint) { if (Definition is PointPointDefinition definition) definition.InPoint = inPoint; }

        public override string ToString() => Definition.InPoint.ToString();

        public override IReadOnlyCollection<Character> GetCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager) => GetCharacters(coordinatesSystemManager, Fill, Stroke, Definition, Radius);
        public static Character[] GetCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager, Brush fill, Pen stroke, PointDefinition definition, double radius) => new[] { Character.Ellipse(coordinatesSystemManager.ComputeOutCoordinates(definition.InPoint), radius, radius, fill, stroke) };

        public static implicit operator PointVisualObject(Point inPoint) => new PointVisualObject(inPoint);
        public static implicit operator Point(PointVisualObject pointVisualObject) => pointVisualObject.Definition.InPoint;
    }
}
