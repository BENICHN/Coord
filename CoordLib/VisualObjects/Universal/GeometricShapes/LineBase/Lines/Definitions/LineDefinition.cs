using BenLib.Framework;

namespace Coord
{
    /// <summary>
    /// Détermine les propriétés fondamentales d'un <see cref="LineVisualObject"/> à l'aide d'autres propriétés
    /// </summary>
    public abstract class LineDefinition : NotifyObject
    {
        /// <summary>
        /// Équation de la droite du plan
        /// </summary>
        public LinearEquation Equation { get; protected set; }

        public override string ToString() => Equation.ToString();
    }
}
