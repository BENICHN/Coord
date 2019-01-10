using BenLib;

namespace Coord
{
    /// <summary>
    /// Détermine les propriétés fondamentales d'un <see cref="SegmentVisualObject"/> à l'aide d'autres propriétés
    /// </summary>
    public abstract class SegmentDefinition : VisualObjectDefinition
    {
        /// <summary>
        /// Équation de la droite du plan qui prolonge le segment
        /// </summary>
        public LinearEquation Equation { get; protected set; }

        /// <summary>
        /// Première extrémité du segment
        /// </summary>
        public PointVisualObject Start { get; protected set; }

        /// <summary>
        /// Seconde extrémité du segment
        /// </summary>
        public PointVisualObject End { get; protected set; }

        public override string ToString() => Equation.ToString();
    }
}
