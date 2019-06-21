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
        protected override Freezable CreateInstanceCore() => new CircleVisualObject();

        public override string Type => Definition switch
        {
            CenterRadiusCircleDefinition _ => "CenterRadiusCircle",
            CenterPointCircleDefinition _ => "CenterPointCircle",
            _ => "Circle"
        };

        /// <summary>
        /// Détermine les propriétés fondamentales de ce <see cref="CircleVisualObject"/>
        /// </summary>
        public CircleDefinition Definition { get => (CircleDefinition)GetValue(DefinitionProperty); set => SetValue(DefinitionProperty, value); }
        public static readonly DependencyProperty DefinitionProperty = CreateProperty<CircleVisualObject, CircleDefinition>(true, true, true, "Definition");

        protected override IReadOnlyCollection<Character> GetCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager) => new[] { Character.Ellipse(coordinatesSystemManager.ComputeOutCoordinates(Definition.Center), Definition.Radius * coordinatesSystemManager.WidthRatio, Definition.Radius * coordinatesSystemManager.HeightRatio).Color(Fill, Stroke) };
    }
}
