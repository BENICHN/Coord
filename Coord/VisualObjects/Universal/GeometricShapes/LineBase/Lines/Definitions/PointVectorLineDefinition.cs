using BenLib;
using System.Windows;

namespace Coord
{
    /// <summary>
    /// Détermine l'équation d'une droite passant par un <see cref="PointVisualObject"/> et dirigée par un <see cref="VectorVisualObject"/>
    /// </summary>
    public class PointVectorLineDefinition : LineDefinition
    {
        private PointVisualObject m_point;
        private VectorVisualObject m_vector;

        public PointVectorLineDefinition(PointVisualObject point, VectorVisualObject vector)
        {
            Point = point;
            Vector = vector;
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
        /// Vecteur directeur de la droite
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
        /// Calcule et met en cache les propriétés fondamentales de la droite du plan
        /// </summary>
        protected override void Compute()
        {
            if (Point == null || Vector == null) return;

            var point = (Point)Point;
            var vector = (Vector)Vector;

            if (vector.X == 0) Equation = new LinearEquation(point.X);

            double a = vector.Y / vector.X;
            double b = point.Y - a * point.X;

            Equation = new LinearEquation(a, b);
        }
    }
}
