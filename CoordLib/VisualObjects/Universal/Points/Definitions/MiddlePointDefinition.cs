using System.Windows;

namespace Coord
{
    /// <summary>
    /// Détermine le milieu d'un segment défini par deux <see cref="PointVisualObject"/>
    /// </summary>
    public class MiddlePointDefinition : PointDefinition
    {
        protected override Freezable CreateInstanceCore() => new MiddlePointDefinition();

        /// <summary>
        /// Première extrémité du segment
        /// </summary>
        public PointVisualObject PointA { get => (PointVisualObject)GetValue(PointAProperty); set => SetValue(PointAProperty, value); }
        public static readonly DependencyProperty PointAProperty = CreateProperty<MiddlePointDefinition, PointVisualObject>(true, true, true, "PointA");

        /// <summary>
        /// Seconde extrémité du segment
        /// </summary>
        public PointVisualObject PointB { get => (PointVisualObject)GetValue(PointBProperty); set => SetValue(PointBProperty, value); }
        public static readonly DependencyProperty PointBProperty = CreateProperty<MiddlePointDefinition, PointVisualObject>(true, true, true, "PointB");

        protected override void OnChanged()
        {
            if (PointA == null || PointB == null) return;

            var pointA = PointA.Definition.InPoint;
            var pointB = PointB.Definition.InPoint;

            InPoint = new Point((pointA.X + pointB.X) / 2.0, (pointA.Y + pointB.Y) / 2.0);
        }
    }
}
