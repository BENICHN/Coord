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
        public static readonly DependencyProperty DefinitionProperty = CreateProperty<CircleDefinition>(true, true, "Definition", typeof(CircleVisualObject));

        protected override IReadOnlyCollection<Character> GetCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager) => GetCharacters(Definition.Center, Definition.Radius, Fill, Stroke, coordinatesSystemManager);

        public static Character[] GetCharacters(Point center, double radius, Brush fill, PlanePen stroke, ReadOnlyCoordinatesSystemManager coordinatesSystemManager) => new[] { Character.Ellipse(coordinatesSystemManager.ComputeOutCoordinates(center), radius * coordinatesSystemManager.WidthRatio, radius * coordinatesSystemManager.HeightRatio).Color(fill, stroke) };
    }
}
