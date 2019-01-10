using System.Windows;

namespace Coord
{
    /// <summary>
    /// Détermine la translation d'un <see cref="PointVisualObject"/> par un <see cref="VectorVisualObject"/>
    /// </summary>
    public class TranslationPointDefinition : PointDefinition
    {
        private PointVisualObject m_point;
        private VectorVisualObject m_vector;

        public TranslationPointDefinition(PointVisualObject point, VectorVisualObject vector)
        {
            Point = point;
            Vector = vector;
        }

        /// <summary>
        /// Point du plan à translater
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
        /// Vecteur du plan associé à la translation
        /// </summary>
        public VectorVisualObject Vector
        {
            get => m_vector;
            set
            {
                m_vector = value;
                RegisterCompute(m_vector);
                ComputeAndNotifyChanged();
            }
        }

        /// <summary>
        /// Calcule et met en cache les propriétés fondamentales du point du plan
        /// </summary>
        protected override void Compute()
        {
            if (Point == null || Vector == null) return;
            InPoint = (Point)Point + Vector;
        }
    }
}
