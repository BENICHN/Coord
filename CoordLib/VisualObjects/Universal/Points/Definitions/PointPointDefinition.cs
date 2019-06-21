using System.Windows;

namespace Coord
{
    /// <summary>
    /// Détermine un point du plan grâce à un <see cref="Point"/>
    /// </summary>
    public class PointPointDefinition : PointDefinition
    {
        protected override Freezable CreateInstanceCore() => new PointPointDefinition();

        /// <summary>
        /// Point du plan
        /// </summary>
        public new Point InPoint { get => (Point)GetValue(InPointProperty); set => SetValue(InPointProperty, value); }
        public static readonly DependencyProperty InPointProperty = CreateProperty<PointPointDefinition, Point>(true, true, true, "InPoint");

        protected override void OnChanged() => base.InPoint = InPoint;
    }
}
