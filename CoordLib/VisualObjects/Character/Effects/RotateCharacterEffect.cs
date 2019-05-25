using BenLib.Standard;
using System.Collections.Generic;
using System.Windows;

namespace Coord
{
    public class RotateCharacterEffect : CharacterEffect
    {
        public Point Center { get => (Point)GetValue(CenterProperty); set => SetValue(CenterProperty, value); }
        public static readonly DependencyProperty CenterProperty = CreateProperty<Point>(true, true, "Center", typeof(RotateCharacterEffect));

        public bool In { get => (bool)GetValue(InProperty); set => SetValue(InProperty, value); }
        public static readonly DependencyProperty InProperty = CreateProperty<bool>(true, true, "In", typeof(RotateCharacterEffect));

        public double Angle { get => (double)GetValue(AngleProperty); set => SetValue(AngleProperty, value); }
        public static readonly DependencyProperty AngleProperty = CreateProperty<double>(true, true, "Angle", typeof(RotateCharacterEffect));

        /// <summary>
        /// Applique l'effet à une collection de <see cref="Character"/>
        /// </summary>
        /// <param name="characters">Collection de <see cref="Character"/> sur qui appliquer l'effet</param>
        protected override void ApplyCore(IReadOnlyCollection<Character> characters, Interval<int> interval, in ReadOnlyCoordinatesSystemManager coordinatesSystemManager) => characters.SubCollection(interval, true).RotateAt(Angle, In ? coordinatesSystemManager.ComputeOutCoordinates(Center) : Center, EasedProgress).Enumerate();
    }
}
