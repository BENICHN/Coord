using System;
using System.Windows;

namespace Coord
{
    /// <summary>
    /// Détermine les points d'un polygone régulier
    /// </summary>
    public class RegularPolygonDefinition : PolygonDefinition
    {
        /// <summary>
        /// Longueur dans le plan des côtés du polygone
        /// </summary>
        public double SideLength { get => (double)GetValue(SideLengthProperty); set => SetValue(SideLengthProperty, value); }
        public static readonly DependencyProperty SideLengthProperty = CreateProperty<double>(true, true, "SideLength", typeof(RegularPolygonDefinition));

        /// <summary>
        /// Nombre de côtés du polygone
        /// </summary>
        public int SideCount { get => (int)GetValue(SideCountProperty); set => SetValue(SideCountProperty, value); }
        public static readonly DependencyProperty SideCountProperty = CreateProperty<int>(true, true, "SideCount", typeof(RegularPolygonDefinition));

        protected override void OnChanged()
        {
            double length = SideLength;
            int count = SideCount;

            double a = 2.0 * Math.PI / count;

            double angle = 0.0;
            var point = new Point(0.0, 0.0);
            var points = new Point[count];

            for (int i = 0; i < count; i++)
            {
                point += new Vector(Math.Cos(angle) * length, Math.Sin(angle) * length);
                points[i] = point;
                angle += a;
            }

            InPoints = points;
        }
    }
}
