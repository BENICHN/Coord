using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Coord
{
    /// <summary>
    /// Détermine le point résultant de la transformation de plusieurs <see cref="PointVisualObject"/> par une <see cref="Func{T, TResult}"/>
    /// </summary>
    public class MultiOperationsPointDefinition : PointDefinition
    {
        private NotifyObjectCollection<PointVisualObject> m_points;
        private Func<Point[], Point> m_operations;

        public MultiOperationsPointDefinition(NotifyObjectCollection<PointVisualObject> points, Func<Point[], Point> operations)
        {
            Points = points;
            Operations = operations;
        }
        public MultiOperationsPointDefinition(IEnumerable<PointVisualObject> points, Func<Point[], Point> operations) : this(new NotifyObjectCollection<PointVisualObject>(points), operations) { }

        /// <summary>
        /// Points du plan
        /// </summary>
        public NotifyObjectCollection<PointVisualObject> Points
        {
            get => m_points;
            set
            {
                m_points = value;
                RegisterCompute(m_points);
                ComputeAndNotifyChanged();
            }
        }

        /// <summary>
        /// Transformation des points du plan
        /// </summary>
        public Func<Point[], Point> Operations
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
            if (Points == null || Operations == null) return;
            InPoint = Operations(Points.Select(p => (Point)p).ToArray());
        }
    }
}
