using System.Windows;

namespace Coord
{
    /// <summary>
    /// Détermine le centre et le rayon d'un cercle de centre et de rayon donnés
    /// </summary>
    public class CenterRadiusCircleDefinition : CircleDefinition
    {
        protected override Freezable CreateInstanceCore() => new CenterRadiusCircleDefinition();

        /// <summary>
        /// Centre du cercle du plan
        /// </summary>
        public new PointVisualObject Center { get => (PointVisualObject)GetValue(CenterProperty); set => SetValue(CenterProperty, value); }
        public static readonly DependencyProperty CenterProperty = CreateProperty<CenterRadiusCircleDefinition, PointVisualObject>(true, true, true, "Center");

        /// <summary>
        /// Rayon du cercle du plan
        /// </summary>
        public new double Radius { get => (double)GetValue(RadiusProperty); set => SetValue(RadiusProperty, value); }
        public static readonly DependencyProperty RadiusProperty = CreateProperty<CenterRadiusCircleDefinition, double>(true, true, true, "Radius");

        protected override void OnChanged() => (base.Center, base.Radius) = (Center?.Definition.InPoint ?? new Point(double.NaN, double.NaN), Radius);
    }
}
