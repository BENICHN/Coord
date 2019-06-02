using System;
using System.Linq;
using System.Windows;

namespace Coord
{
    /// <summary>
    /// Détermine le point résultant de la transformation de plusieurs <see cref="PointVisualObject"/> par une <see cref="Func{T, TResult}"/>
    /// </summary>
    public class MultiOperationsPointDefinition : PointDefinition
    {
        public override string Type => "MultiOperationsPointDefinition";

        /// <summary>
        /// Points du plan
        /// </summary>
        public NotifyObjectCollection<PointVisualObject> Points { get => (NotifyObjectCollection<PointVisualObject>)GetValue(PointsProperty); set => SetValue(PointsProperty, value); }
        public static readonly DependencyProperty PointsProperty = CreateProperty<NotifyObjectCollection<PointVisualObject>>(true, true, "Points", typeof(MultiOperationsPointDefinition));

        /// <summary>
        /// Transformation des points du plan
        /// </summary>
        public Func<Point[], Point> Operations { get => (Func<Point[], Point>)GetValue(OperationsProperty); set => SetValue(OperationsProperty, value); }
        public static readonly DependencyProperty OperationsProperty = CreateProperty<Func<Point[], Point>>(true, true, "Operations", typeof(MultiOperationsPointDefinition));

        protected override void OnChanged()
        {
            if (Points == null || Operations == null) return;
            InPoint = Operations(Points.Select(p => p.Definition.InPoint).ToArray());
        }
    }
}
