using BenLib.Framework;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Coord
{
    /// <summary>
    /// Objet visuel représentant un segment du plan
    /// </summary>
    public class SegmentVisualObject : LineVisualObjectBase
    {
        protected override Freezable CreateInstanceCore() => new SegmentVisualObject();

        public override string Type => Definition switch
        {
            PointPointSegmentDefinition _ => "PointPointSegment",
            _ => "Segment"
        };

        /// <summary>
        /// Détermine les propriétés fondamentales de ce <see cref="SegmentVisualObject"/>
        /// </summary>
        public SegmentDefinition Definition { get => (SegmentDefinition)GetValue(DefinitionProperty); set => SetValue(DefinitionProperty, value); }
        public static readonly DependencyProperty DefinitionProperty = CreateProperty<SegmentVisualObject, SegmentDefinition>(true, true, true, "Definition");

        /// <summary>
        /// Équation de droite qui décrit ce <see cref="LineVisualObjectBase"/>
        /// </summary>
        public override LinearEquation Equation => Definition.Equation;

        protected override IReadOnlyCollection<Character> GetCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager) => GetCharacters(coordinatesSystemManager, Stroke, Definition);
        public static Character[] GetCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager, Pen stroke, SegmentDefinition definition) => new[] { Character.Line(coordinatesSystemManager.ComputeOutCoordinates(definition.Start), coordinatesSystemManager.ComputeOutCoordinates(definition.End)).Color(stroke) };

        public override string ToString() => Definition.Equation.ToString();
    }
}
