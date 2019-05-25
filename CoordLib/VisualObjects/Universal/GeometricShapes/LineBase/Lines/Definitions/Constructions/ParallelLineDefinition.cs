using BenLib.Framework;
using System.Windows;

namespace Coord
{
    /// <summary>
    /// Détermine l'équation d'une droite passant par un <see cref="PointVisualObject"/> et parallèle à une <see cref="LineVisualObjectBase"/>
    /// </summary>
    public class ParallelLineDefinition : LineDefinition
    {
        /// <summary>
        /// Point de la droite
        /// </summary>
        public PointVisualObject Point { get => (PointVisualObject)GetValue(PointProperty); set => SetValue(PointProperty, value); }
        public static readonly DependencyProperty PointProperty = CreateProperty<PointVisualObject>(true, true, "Point", typeof(ParallelLineDefinition));

        /// <summary>
        /// Droite parallèle à la droite
        /// </summary>
        public LineVisualObjectBase Line { get => (LineVisualObjectBase)GetValue(LineProperty); set => SetValue(LineProperty, value); }
        public static readonly DependencyProperty LineProperty = CreateProperty<LineVisualObjectBase>(true, true, "Line", typeof(ParallelLineDefinition));

        protected override void OnChanged()
        {
            if (Point == null || Line == null) return;
            Equation = LinearEquation.Parallel(Line.Equation, Point);
        }
    }
}
