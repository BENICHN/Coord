using BenLib.Framework;
using System.Collections.Generic;
using System.Windows;

namespace Coord
{
    /// <summary>
    /// Objet visuel représentant un segment du plan
    /// </summary>
    public class SegmentVisualObject : LineVisualObjectBase
    {
        public override string Type => Definition switch
        {
            PointPointSegmentDefinition _ => "PointPointSegment",
            _ => "Segment"
        };

        /// <summary>
        /// Détermine les propriétés fondamentales de ce <see cref="SegmentVisualObject"/>
        /// </summary>
        public SegmentDefinition Definition { get => (SegmentDefinition)GetValue(DefinitionProperty); set => SetValue(DefinitionProperty, value); }
        public static readonly DependencyProperty DefinitionProperty = CreateProperty<SegmentDefinition>(true, true, "Definition", typeof(SegmentVisualObject));

        /// <summary>
        /// Équation de droite qui décrit ce <see cref="LineVisualObjectBase"/>
        /// </summary>
        public override LinearEquation Equation => Definition.Equation;

        protected override IReadOnlyCollection<Character> GetCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager) => GetCharacters(coordinatesSystemManager, Stroke, Definition);
        public static Character[] GetCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager, PlanePen stroke, SegmentDefinition definition) => new[] { Character.Line(coordinatesSystemManager.ComputeOutCoordinates(definition.Start), coordinatesSystemManager.ComputeOutCoordinates(definition.End)).Color(stroke) };

        public override string ToString() => Definition.Equation.ToString();
    }
}
