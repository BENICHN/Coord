using System.Collections.Generic;

namespace Coord
{
    /// <summary>
    /// Détermine les points d'un polygone passant par des <see cref="PointVisualObject"/>
    /// </summary>
    public class PointsPolygonDefinition : PolygonDefinition
    {
        public PointsPolygonDefinition(params PointVisualObject[] inPoints) => InPoints = new NotifyObjectCollection<PointVisualObject>(inPoints);
        public PointsPolygonDefinition(IEnumerable<PointVisualObject> inPoints) => InPoints = new NotifyObjectCollection<PointVisualObject>(inPoints);
        public PointsPolygonDefinition(NotifyObjectCollection<PointVisualObject> inPoints) => InPoints = inPoints;

        /// <summary>
        /// Points du polygone
        /// </summary>
        public new NotifyObjectCollection<PointVisualObject> InPoints
        {
            get => base.InPoints;
            set
            {
                base.InPoints = value;
                RegisterCompute(base.InPoints);
                ComputeAndNotifyChanged();
            }
        }

        /// <summary>
        /// Calcule et met en cache les propriétés fondamentales du polygone du plan
        /// </summary>
        protected override void Compute() { }
    }
}
