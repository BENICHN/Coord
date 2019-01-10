using System.Windows;

namespace Coord
{
    /// <summary>
    /// Détermine le milieu d'un segment défini par deux <see cref="PointVisualObject"/>
    /// </summary>
    public class MiddlePointDefinition : PointDefinition
    {
        private PointVisualObject m_pointA;
        private PointVisualObject m_pointB;

        public MiddlePointDefinition(PointVisualObject pointA, PointVisualObject pointB)
        {
            PointA = pointA;
            PointB = pointB;
        }

        /// <summary>
        /// Première extrémité du segment
        /// </summary>
        public PointVisualObject PointA
        {
            get => m_pointA;
            set
            {
                m_pointA = value;
                RegisterCompute(m_pointA);
                ComputeAndNotifyChanged();
            }
        }

        /// <summary>
        /// Seconde extrémité du segment
        /// </summary>
        public PointVisualObject PointB
        {
            get => m_pointB;
            set
            {
                m_pointB = value;
                RegisterCompute(m_pointB);
                ComputeAndNotifyChanged();
            }
        }

        /// <summary>
        /// Calcule et met en cache les propriétés fondamentales du point du plan
        /// </summary>
        protected override void Compute()
        {
            if (PointA == null || PointB == null) return;

            var pointA = (Point)PointA;
            var pointB = (Point)PointB;

            InPoint = new Point((pointA.X + pointB.X) / 2.0, (pointA.Y + pointB.Y) / 2.0);
        }
    }
}
