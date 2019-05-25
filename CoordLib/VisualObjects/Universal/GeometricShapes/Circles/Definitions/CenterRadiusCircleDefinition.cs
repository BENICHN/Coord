using System.Windows;

namespace Coord
{
    /// <summary>
    /// Détermine le centre et le rayon d'un cercle de centre et de rayon donnés
    /// </summary>
    public class CenterRadiusCircleDefinition : CircleDefinition
    {
        /// <summary>
        /// Centre du cercle du plan
        /// </summary>
        public new PointVisualObject Center { get => (PointVisualObject)GetValue(CenterProperty); set => SetValue(CenterProperty, value); }
        public static readonly DependencyProperty CenterProperty = CreateProperty<PointVisualObject>(true, true, "Center", typeof(CenterRadiusCircleDefinition));

        /// <summary>
        /// Rayon du cercle du plan
        /// </summary>
        public new double Radius { get => (double)GetValue(RadiusProperty); set => SetValue(RadiusProperty, value); }
        public static readonly DependencyProperty RadiusProperty = CreateProperty<double>(true, true, "Radius", typeof(CenterRadiusCircleDefinition));

        protected override void OnChanged() => (base.Center, base.Radius) = (Center?.Definition.InPoint ?? new Point(double.NaN, double.NaN), Radius);
    }
}
