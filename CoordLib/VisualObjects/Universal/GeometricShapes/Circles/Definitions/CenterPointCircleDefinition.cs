using System.Windows;

namespace Coord
{
    /// <summary>
    /// Détermine le centre et le rayon d'un cercle de centre donné passant par un <see cref="PointVisualObject"/>
    /// </summary>
    public class CenterPointCircleDefinition : CircleDefinition
    {
        protected override Freezable CreateInstanceCore() => new CenterPointCircleDefinition();

        /// <summary>
        /// Centre du cercle du plan
        /// </summary>
        public new PointVisualObject Center { get => (PointVisualObject)GetValue(CenterProperty); set => SetValue(CenterProperty, value); }
        public static readonly DependencyProperty CenterProperty = CreateProperty<CenterPointCircleDefinition, PointVisualObject>(true, true, true, "Center");

        /// <summary>
        /// Point du cercle du plan
        /// </summary>
        public PointVisualObject Point { get => (PointVisualObject)GetValue(PointProperty); set => SetValue(PointProperty, value); }
        public static readonly DependencyProperty PointProperty = CreateProperty<CenterPointCircleDefinition, PointVisualObject>(true, true, true, "Point");

        protected override void OnChanged()
        {
            if (Center == null) base.Center = new Point(double.NaN, double.NaN);
            else if (Point != null)
            {
                base.Center = Center.Definition.InPoint;
                Radius = (Center.Definition.InPoint - Point.Definition.InPoint).Length;
            }
        }
    }
}
