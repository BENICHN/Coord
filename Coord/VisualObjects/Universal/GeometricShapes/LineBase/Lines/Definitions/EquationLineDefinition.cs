using BenLib;

namespace Coord
{
    /// <summary>
    /// Détermine l'équation d'une droite du plan grâce à une <see cref="LinearEquation"/>
    /// </summary>
    public class EquationLineDefinition : LineDefinition
    {
        /// <summary>
        /// Équation de la droite dun plan
        /// </summary>
        public new LinearEquation Equation
        {
            get => base.Equation;
            set
            {
                base.Equation = value;
                NotifyChanged();
            }
        }

        public EquationLineDefinition(LinearEquation equation) => Equation = equation;

        /// <summary>
        /// Calcule et met en cache les propriétés fondamentales de la droite du plan
        /// </summary>
        protected override void Compute() { }
    }
}
