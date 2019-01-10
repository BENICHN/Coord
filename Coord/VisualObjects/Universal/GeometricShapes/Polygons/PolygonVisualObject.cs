using BenLib.WPF;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace Coord
{
    /// <summary>
    /// Objet visuel représentant un polygone du plan
    /// </summary>
    public class PolygonVisualObject : VisualObject
    {
        private PolygonDefinition m_definition;

        public PolygonVisualObject(PolygonDefinition definition) => Definition = definition;

        /// <summary>
        /// Détermine les propriétés fondamentales de ce <see cref="PolygonVisualObject"/>
        /// </summary>
        public PolygonDefinition Definition
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

        public override IReadOnlyCollection<Character> GetCharacters(CoordinatesSystemManager coordinatesSystemManager) => new[] { new Character(GeometryHelper.GetCurve(Definition.InPoints.Select(pointVisualObject => coordinatesSystemManager.ComputeOutCoordinates(pointVisualObject)).ToArray(), true, false), Fill, Stroke) };
    }
}
