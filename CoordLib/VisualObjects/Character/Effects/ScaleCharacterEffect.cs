using BenLib.Standard;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Coord
{
    /// <summary>
    /// Met progressivement à l'échelle une sous-collection de <see cref="Character"/> par un coefficient spécifié
    /// </summary>
    public class ScaleCharacterEffect : CharacterEffect
    {
        public double ScaleX { get => (double)GetValue(ScaleXProperty); set => SetValue(ScaleXProperty, value); }
        public static readonly DependencyProperty ScaleXProperty = CreateProperty<double>(true, true, "ScaleX", typeof(ScaleCharacterEffect));

        public double ScaleY { get => (double)GetValue(ScaleYProperty); set => SetValue(ScaleYProperty, value); }
        public static readonly DependencyProperty ScaleYProperty = CreateProperty<double>(true, true, "ScaleY", typeof(ScaleCharacterEffect));

        public RectPoint RectPoint { get => (RectPoint)GetValue(RectPointProperty); set => SetValue(RectPointProperty, value); }
        public static readonly DependencyProperty RectPointProperty = CreateProperty<RectPoint>(true, true, "RectPoint", typeof(ScaleCharacterEffect));

        public Point Center { get => (Point)GetValue(CenterProperty); set => SetValue(CenterProperty, value); }
        public static readonly DependencyProperty CenterProperty = CreateProperty(true, true, "Center", typeof(ScaleCharacterEffect), new Point(double.NaN, double.NaN));

        public bool In { get => (bool)GetValue(InProperty); set => SetValue(InProperty, value); }
        public static readonly DependencyProperty InProperty = CreateProperty<bool>(true, true, "In", typeof(ScaleCharacterEffect));

        /// <summary>
        /// Applique l'effet à une collection de <see cref="Character"/>
        /// </summary>
        /// <param name="characters">Collection de <see cref="Character"/> sur qui appliquer l'effet</param>
        protected override void ApplyCore(IReadOnlyCollection<Character> characters, Interval<int> interval, in ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            var center = Center;
            var chars = characters.SubCollection(interval, true).ToArray();
            chars.ScaleAt(ScaleX, ScaleY, double.IsNaN(center.X + center.Y) ? RectPoint.GetPoint(chars.Geometry().Bounds) : In ? coordinatesSystemManager.ComputeOutCoordinates(Center) : Center, EasedProgress).Enumerate();
        }
    }
}
