using BenLib;
using System.Collections.Generic;
using System.Windows.Media;

namespace Coord
{
    /// <summary>
    /// Objet visuel représentant un segment du plan
    /// </summary>
    public class SegmentVisualObject : LineVisualObjectBase
    {
        public override string Type
        {
            get
            {
                var definitionType = Definition?.GetType();
                if (definitionType == typeof(PointPointSegmentDefinition)) return "PointPointSegment";
                else return "Segment";
            }
        }

        private SegmentDefinition m_definition;

        public SegmentVisualObject(SegmentDefinition definition) => Definition = definition;

        /// <summary>
        /// Détermine les propriétés fondamentales de ce <see cref="SegmentVisualObject"/>
        /// </summary>
        public SegmentDefinition Definition
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
        /// Équation de droite qui décrit ce <see cref="LineVisualObjectBase"/>
        /// </summary>
        public override LinearEquation Equation => Definition.Equation;

        public override string ToString() => Definition.Equation.ToString();

        public override IReadOnlyCollection<Character> GetCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager) => GetCharacters(coordinatesSystemManager, Stroke, Definition);
        public static Character[] GetCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager, Pen stroke, SegmentDefinition definition) => new[] { Character.Line(coordinatesSystemManager.ComputeOutCoordinates(definition.Start), coordinatesSystemManager.ComputeOutCoordinates(definition.End), null, stroke) };
    }
}
