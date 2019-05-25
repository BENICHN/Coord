using BenLib.WPF;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Coord
{
    /// <summary>
    /// Objet visuel représentant un polygone du plan
    /// </summary>
    public class PolygonVisualObject : VisualObject
    {
        public override string Type => Definition switch
        {
            PointsPolygonDefinition _ => "PointsPolygon",
            RegularPolygonDefinition _ => "RegularPolygon",
            _ => "Polygon"
        };

        /// <summary>
        /// Détermine les propriétés fondamentales de ce <see cref="PolygonVisualObject"/>
        /// </summary>
        public PolygonDefinition Definition { get => (PolygonDefinition)GetValue(DefinitionProperty); set => SetValue(DefinitionProperty, value); }
        public static readonly DependencyProperty DefinitionProperty = CreateProperty<PolygonDefinition>(true, true, "Definition", typeof(PolygonVisualObject));

        protected override IReadOnlyCollection<Character> GetCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager) => GeometryHelper.GetCurve(Definition.InPoints.Select(pointVisualObject => coordinatesSystemManager.ComputeOutCoordinates(pointVisualObject)).ToArray(), true, false).ToCharacters(Fill, Stroke).ToArray();
    }
}
