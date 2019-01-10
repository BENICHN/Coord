using System;
using System.Linq;
using System.Windows;

namespace Coord
{
    /// <summary>
    /// Détermine les points d'un polygone régulier
    /// </summary>
    public class RegularPolygonDefinition : PolygonDefinition
    {
        private double m_sideLength;
        private int m_sideCount;

        public RegularPolygonDefinition(double sideLength, int sideCount)
        {
            SideLength = sideLength;
            SideCount = sideCount;
        }

        /// <summary>
        /// Longueur dans le plan des côtés du polygone
        /// </summary>
        public double SideLength
        {
            get => m_sideLength;
            set
            {
                m_sideLength = value;
                ComputeAndNotifyChanged();
            }
        }

        /// <summary>
        /// Nombre de côtés du polygone
        /// </summary>
        public int SideCount
        {
            get => m_sideCount;
            set
            {
                m_sideCount = value;
                ComputeAndNotifyChanged();
            }
        }

        /// <summary>
        /// Calcule et met en cache les propriétés fondamentales du polygone du plan
        /// </summary>
        protected override void Compute()
        {
            var length = SideLength;
            var count = SideCount;

            var a = 2.0 * Math.PI / count;

            var angle = 0.0;
            var point = new Point(0.0, 0.0);
            var points = new Point[count];

            for (int i = 0; i < count; i++)
            {
                point += new Vector(Math.Cos(angle) * length, Math.Sin(angle) * length);
                points[i] = point;
                angle += a;
            }

            InPoints = new NotifyObjectCollection<PointVisualObject>(points.Select(p => new PointVisualObject(p)));
        }
    }
}
