using BenLib;

namespace Coord
{
    /// <summary>
    /// Détermine l'équation d'une droite passant par deux <see cref="PointVisualObject"/>
    /// </summary>
    public class PointPointLineDefinition : LineDefinition
    {
        private PointVisualObject m_pointA;
        private PointVisualObject m_pointB;

        public PointPointLineDefinition(PointVisualObject pointA, PointVisualObject pointB)
        {
            PointA = pointA;
            PointB = pointB;
        }

        /// <summary>
        /// Premier point de la droite
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
        /// Second point de la droite
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
        /// Calcule et met en cache les propriétés fondamentales de la droite du plan
        /// </summary>
        protected override void Compute()
        {
            if (PointA == null || PointB == null) return;
            Equation = LinearEquation.FromPoints(PointA, PointB);
        }
    }
}
