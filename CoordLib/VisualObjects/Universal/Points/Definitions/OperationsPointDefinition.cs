using System;
using System.Windows;

namespace Coord
{
    /// <summary>
    /// Détermine la transformation d'un <see cref="PointVisualObject"/> par une <see cref="Func{T, TResult}"/>
    /// </summary>
    public class OperationsPointDefinition : PointDefinition
    {
        protected override Freezable CreateInstanceCore() => new OperationsPointDefinition();

        /// <summary>
        /// Point du plan
        /// </summary>
        public PointVisualObject Point { get => (PointVisualObject)GetValue(PointProperty); set => SetValue(PointProperty, value); }
        public static readonly DependencyProperty PointProperty = CreateProperty<OperationsPointDefinition, PointVisualObject>(true, true, true, "Point");

        /// <summary>
        /// Transformation du point du plan
        /// </summary>
        public Func<Point, Point> Operations { get => (Func<Point, Point>)GetValue(OperationsProperty); set => SetValue(OperationsProperty, value); }
        public static readonly DependencyProperty OperationsProperty = CreateProperty<OperationsPointDefinition, Func<Point, Point>>(true, true, true, "Operations");

        protected override void OnChanged()
        {
            if (Point == null || Operations == null) return;
            InPoint = Operations(Point);
        }
    }
}
