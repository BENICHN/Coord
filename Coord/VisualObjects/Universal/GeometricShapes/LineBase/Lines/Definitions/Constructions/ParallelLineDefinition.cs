using BenLib;

namespace Coord
{
    /// <summary>
    /// Détermine l'équation d'une droite passant par un <see cref="PointVisualObject"/> et parallèle à une <see cref="LineVisualObjectBase"/>
    /// </summary>
    public class ParallelLineDefinition : LineDefinition
    {
        private PointVisualObject m_point;
        private LineVisualObjectBase m_line;

        public ParallelLineDefinition(PointVisualObject point, LineVisualObjectBase line)
        {
            Point = point;
            Line = line;
        }

        /// <summary>
        /// Point de la droite
        /// </summary>
        public PointVisualObject Point
        {
            get => m_point;
            set
            {
                m_point = value;
                RegisterCompute(m_point);
                ComputeAndNotifyChanged();
            }
        }

        /// <summary>
        /// Droite parallèle à la droite
        /// </summary>
        public LineVisualObjectBase Line
        {
            get => m_line;
            set
            {
                m_line = value;
                RegisterCompute(m_line);
                ComputeAndNotifyChanged();
            }
        }

        /// <summary>
        /// Calcule et met en cache les propriétés fondamentales de la droite du plan
        /// </summary>
        protected override void Compute()
        {
            if (Point == null || Line == null) return;
            Equation = LinearEquation.Parallel(Line.Equation, Point);
        }
    }
}
