using BenLib;
using System;
using System.Windows;

namespace Coord
{
    /// <summary>
    /// Détermine le centre et le rayon d'un cercle de centre donné passant par un <see cref="PointVisualObject"/>
    /// </summary>
    public class CenterPointCircleDefinition : CircleDefinition
    {
        private PointVisualObject m_point;

        public CenterPointCircleDefinition(PointVisualObject center, PointVisualObject point)
        {
            Center = center;
            Point = point;
        }

        /// <summary>
        /// Centre du cercle du plan
        /// </summary>
        public new PointVisualObject Center
        {
            get => base.Center;
            set
            {
                base.Center = value;
                RegisterCompute(base.Center);
                ComputeAndNotifyChanged();
            }
        }

        /// <summary>
        /// Point du cercle du plan
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
        /// Calcule et met en cache les propriétés fondamentales du cercle du plan
        /// </summary>
        protected override void Compute()
        {
            if (Center == null || Point == null) return;

            var center = (Point)Center;
            var point = (Point)Point;

            Radius = Math.Sqrt((center.X - point.X).Pow(2) + (center.Y - point.Y).Pow(2));
        }
    }
}
