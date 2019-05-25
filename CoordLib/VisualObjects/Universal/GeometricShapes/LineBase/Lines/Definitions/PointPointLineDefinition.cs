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
        /// <summary>
        /// Premier point de la droite
        /// </summary>
        public PointVisualObject PointA { get => (PointVisualObject)GetValue(PointAProperty); set => SetValue(PointAProperty, value); }
        public static readonly DependencyProperty PointAProperty = CreateProperty<PointVisualObject>(true, true, "PointA", typeof(PointPointLineDefinition));

        /// <summary>
        /// Second point de la droite
        /// </summary>
        public PointVisualObject PointB { get => (PointVisualObject)GetValue(PointBProperty); set => SetValue(PointBProperty, value); }
        public static readonly DependencyProperty PointBProperty = CreateProperty<PointVisualObject>(true, true, "PointB", typeof(PointPointLineDefinition));

        protected override void OnChanged()
        {
            if (PointA == null || PointB == null) return;
            Equation = LinearEquation.FromPoints(PointA, PointB);
        }
    }
}
