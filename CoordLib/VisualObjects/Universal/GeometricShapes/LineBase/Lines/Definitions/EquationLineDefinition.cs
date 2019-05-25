using BenLib.Framework;
using System.Windows;

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
        public new LinearEquation Equation { get => (LinearEquation)GetValue(EquationProperty); set => SetValue(EquationProperty, value); }
        public static readonly DependencyProperty EquationProperty = CreateProperty<LinearEquation>(true, true, "Equation", typeof(EquationLineDefinition));

        protected override void OnChanged() => base.Equation = Equation;
    }
}
