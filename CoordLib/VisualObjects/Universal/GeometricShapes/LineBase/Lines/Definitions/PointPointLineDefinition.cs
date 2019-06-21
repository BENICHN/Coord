using BenLib.Framework;
using BenLib.Standard;
using System.Windows;

namespace Coord
{
    /// <summary>
    /// Détermine l'équation d'une droite passant par deux <see cref="PointVisualObject"/>
    /// </summary>
    public class PointPointLineDefinition : LineDefinition
    {
        protected override Freezable CreateInstanceCore() => new PointPointLineDefinition();

        /// <summary>
        /// Premier point de la droite
        /// </summary>
        public PointVisualObject PointA { get => (PointVisualObject)GetValue(PointAProperty); set => SetValue(PointAProperty, value); }
        public static readonly DependencyProperty PointAProperty = CreateProperty<PointPointLineDefinition, PointVisualObject>(true, true, true, "PointA");

        /// <summary>
        /// Second point de la droite
        /// </summary>
        public PointVisualObject PointB { get => (PointVisualObject)GetValue(PointBProperty); set => SetValue(PointBProperty, value); }
        public static readonly DependencyProperty PointBProperty = CreateProperty<PointPointLineDefinition, PointVisualObject>(true, true, true, "PointB");

        protected override void OnChanged()
        {
            if (PointA == null || PointB == null) return;
            Equation = LinearEquation.FromPoints(PointA, PointB);
        }
    }
}
