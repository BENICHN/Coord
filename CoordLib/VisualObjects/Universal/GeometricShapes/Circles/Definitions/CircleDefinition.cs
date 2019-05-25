using System.Windows;

namespace Coord
{
    /// <summary>
    /// Détermine les propriétés fondamentales d'un <see cref="CircleVisualObject"/> à l'aide d'autres propriétés
    /// </summary>
    public abstract class CircleDefinition : NotifyObject
    {
        /// <summary>
        /// Centre du cercle du plan
        /// </summary>
        public Point Center { get; protected set; }

        /// <summary>
        /// Rayon du cercle du plan
        /// </summary>
        public double Radius { get; protected set; } = double.NaN;

        public override string ToString() => $"({Center.ToString()} ; {Radius})";
    }
}
