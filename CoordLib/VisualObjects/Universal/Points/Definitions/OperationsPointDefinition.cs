using System;
using System.Windows;

namespace Coord
{
    /// <summary>
    /// Détermine la transformation d'un <see cref="PointVisualObject"/> par une <see cref="Func{T, TResult}"/>
    /// </summary>
    public class OperationsPointDefinition : PointDefinition
    {
        public override string Type => "OperationsPointDefinition";

        /// <summary>
        /// Point du plan
        /// </summary>
        public PointVisualObject Point { get => (PointVisualObject)GetValue(PointProperty); set => SetValue(PointProperty, value); }
        public static readonly DependencyProperty PointProperty = CreateProperty<PointVisualObject>(true, true, "Point", typeof(OperationsPointDefinition));

        /// <summary>
        /// Transformation du point du plan
        /// </summary>
        public Func<Point, Point> Operations { get => (Func<Point, Point>)GetValue(OperationsProperty); set => SetValue(OperationsProperty, value); }
        public static readonly DependencyProperty OperationsProperty = CreateProperty<Func<Point, Point>>(true, true, "Operations", typeof(OperationsPointDefinition));

        protected override void OnChanged()
        {
            if (Point == null || Operations == null) return;
            InPoint = Operations(Point);
        }
    }
}
