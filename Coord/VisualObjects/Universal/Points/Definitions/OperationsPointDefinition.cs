using System;
using System.Windows;

namespace Coord
{
    /// <summary>
    /// Détermine la transformation d'un <see cref="PointVisualObject"/> par une <see cref="Func{T, TResult}"/>
    /// </summary>
    public class OperationsPointDefinition : PointDefinition
    {
        private PointVisualObject m_point;
        private Func<Point, Point> m_operations;

        public OperationsPointDefinition(PointVisualObject point, Func<Point, Point> operations)
        {
            Point = point;
            Operations = operations;
        }

        /// <summary>
        /// Point du plan
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
        /// Transformation du point du plan
        /// </summary>
        public Func<Point, Point> Operations
        {
            get => m_operations;
            set
            {
                m_operations = value;
                ComputeAndNotifyChanged();
            }
        }

        /// <summary>
        /// Calcule et met en cache les propriétés fondamentales du point du plan
        /// </summary>
        protected override void Compute()
        {
            if (Point == null || Operations == null) return;
            InPoint = Operations(Point);
        }
    }
}
