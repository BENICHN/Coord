using BenLib.Framework;
using System.Windows;

namespace Coord
{
    /// <summary>
    /// Détermine l'équation d'une droite passant par un <see cref="PointVisualObject"/> et perpendiculaire à une <see cref="LineVisualObjectBase"/>
    /// </summary>
    public class PerpendicularLineDefinition : LineDefinition
    {
        protected override Freezable CreateInstanceCore() => new PerpendicularLineDefinition();

        /// <summary>
        /// Point de la droite
        /// </summary>
        public PointVisualObject Point { get => (PointVisualObject)GetValue(PointProperty); set => SetValue(PointProperty, value); }
        public static readonly DependencyProperty PointProperty = CreateProperty<PerpendicularLineDefinition, PointVisualObject>(true, true, true, "Point");

        /// <summary>
        /// Droite perpendiculaire à la droite
        /// </summary>
        public LineVisualObjectBase Line { get => (LineVisualObjectBase)GetValue(LineProperty); set => SetValue(LineProperty, value); }
        public static readonly DependencyProperty LineProperty = CreateProperty<PerpendicularLineDefinition, LineVisualObjectBase>(true, true, true, "Line");

        protected override void OnChanged()
        {
            if (Point == null || Line == null) return;
            Equation = LinearEquation.Perpendicular(Line.Equation, Point);
        }
    }
}
