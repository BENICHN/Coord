using BenLib;

namespace Coord
{
    /// <summary>
    /// Détermine l'intersection de deux droites du plan
    /// </summary>
    public class LineIntersectionPointDefinition : PointDefinition
    {
        private LineVisualObject m_lineA;
        private LineVisualObject m_lineB;

        public LineIntersectionPointDefinition(LineVisualObject lineA, LineVisualObject lineB)
        {
            LineA = lineA;
            LineB = lineB;
        }

        /// <summary>
        /// Première droite
        /// </summary>
        public LineVisualObject LineA
        {
            get => m_lineA;
            set
            {
                m_lineA = value;
                RegisterCompute(m_lineA);
                ComputeAndNotifyChanged();
            }
        }

        /// <summary>
        /// Seconde droite
        /// </summary>
        public LineVisualObject LineB
        {
            get => m_lineB;
            set
            {
                m_lineB = value;
                RegisterCompute(m_lineB);
                ComputeAndNotifyChanged();
            }
        }

        /// <summary>
        /// Calcule et met en cache les propriétés fondamentales du point du plan
        /// </summary>
        protected override void Compute()
        {
            if (LineA == null || LineB == null) return;
            InPoint = LinearEquation.Intersection(LineA.Equation, LineB.Equation);
        }
    }
}
