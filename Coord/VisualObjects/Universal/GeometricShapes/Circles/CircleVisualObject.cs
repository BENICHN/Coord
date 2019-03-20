using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Coord
{
    /// <summary>
    /// Objet visuel représentant un cercle du plan
    /// </summary>
    public class CircleVisualObject : VisualObject
    {
        public override string Type
        {
            get
            {
                var definitionType = Definition?.GetType();
                if (definitionType == typeof(CenterRadiusCircleDefinition)) return "CenterRadiusCircle";
                if (definitionType == typeof(CenterPointCircleDefinition)) return "CenterPointCircle";
                else return "Circle";
            }
        }

        private CircleDefinition m_definition;

        public CircleVisualObject(CircleDefinition definition) => Definition = definition;

        /// <summary>
        /// Détermine les propriétés fondamentales de ce <see cref="CircleVisualObject"/>
        /// </summary>
        public CircleDefinition Definition
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

        public override IReadOnlyCollection<Character> GetCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager) => GetCharacters(Definition.Center, Definition.Radius, Fill, Stroke, coordinatesSystemManager);

        public static Character[] GetCharacters(Point center, double radius, Brush fill, Pen stroke, ReadOnlyCoordinatesSystemManager coordinatesSystemManager) => new[] { Character.Ellipse(coordinatesSystemManager.ComputeOutCoordinates(center), radius * coordinatesSystemManager.WidthRatio, radius * coordinatesSystemManager.HeightRatio, fill, stroke) };
    }
}
