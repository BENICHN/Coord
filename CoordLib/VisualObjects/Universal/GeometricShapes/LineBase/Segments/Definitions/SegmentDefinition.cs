using BenLib.Framework;
using System.Windows;

namespace Coord
{
    /// <summary>
    /// Détermine les propriétés fondamentales d'un <see cref="SegmentVisualObject"/> à l'aide d'autres propriétés
    /// </summary>
    public abstract class SegmentDefinition : NotifyObject
    {
        /// <summary>
        /// Équation de la droite du plan qui prolonge le segment
        /// </summary>
        public LinearEquation Equation => LinearEquation.FromPoints(Start, End);

        /// <summary>
        /// Première extrémité du segment
        /// </summary>
        public Point Start { get; protected set; }

        /// <summary>
        /// Seconde extrémité du segment
        /// </summary>
        public Point End { get; protected set; }

        public override string ToString() => $"[{Start} ; {End}]";
    }
}
