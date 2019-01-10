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
        public void SetInPoint(Point inPoint)
        {
            if (Definition is PointPointDefinition definition) definition.InPoint = inPoint;
        }

        public override string ToString() => Definition.InPoint.ToString();

        public override IReadOnlyCollection<Character> GetCharacters(CoordinatesSystemManager coordinatesSystemManager) => GetCharacters(coordinatesSystemManager, Fill, Stroke, Definition, Radius);
        public static Character[] GetCharacters(CoordinatesSystemManager coordinatesSystemManager, Brush fill, Pen stroke, PointDefinition definition, double radius) => new[] { Character.Ellipse(coordinatesSystemManager.ComputeOutCoordinates(definition.InPoint), radius, radius, fill, stroke) };

        public static implicit operator PointVisualObject(Point inPoint) => new PointVisualObject(inPoint);
        public static implicit operator Point(PointVisualObject pointVisualObject) => pointVisualObject.Definition.InPoint;
    }
}
