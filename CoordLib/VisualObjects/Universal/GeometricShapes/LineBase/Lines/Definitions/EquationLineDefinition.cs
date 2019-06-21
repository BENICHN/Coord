using BenLib.Framework;
using System.Windows;

namespace Coord
{
    /// <summary>
    /// Détermine l'équation d'une droite du plan grâce à une <see cref="LinearEquation"/>
    /// </summary>
    public class EquationLineDefinition : LineDefinition
    {
        protected override Freezable CreateInstanceCore() => new EquationLineDefinition();

        /// <summary>
        /// Équation de la droite dun plan
        /// </summary>
        public new LinearEquation Equation { get => (LinearEquation)GetValue(EquationProperty); set => SetValue(EquationProperty, value); }
        public static readonly DependencyProperty EquationProperty = CreateProperty<EquationLineDefinition, LinearEquation>(true, true, true, "Equation");

        protected override void OnChanged() => base.Equation = Equation;
    }
}
