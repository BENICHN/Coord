using BenLib.Standard;
using BenLib.WPF;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Coord
{
    /// <summary>
    /// Dessine progressivement le contour des <see cref="Character"/> d'une sous-collection
    /// </summary>
    public class StrokeCharacterEffect : CharacterEffect
    {
        protected override Freezable CreateInstanceCore() => new StrokeCharacterEffect();

        public bool Reverse { get => (bool)GetValue(ReverseProperty); set => SetValue(ReverseProperty, value); }
        public static readonly DependencyProperty ReverseProperty = CreateProperty<StrokeCharacterEffect, bool>(true, true, true, "Reverse");

        /// <summary>
        /// Applique l'effet à une collection de <see cref="Character"/>
        /// </summary>
        /// <param name="characters">Collection de <see cref="Character"/> sur qui appliquer l'effet</param>
        protected override void ApplyCore(IReadOnlyCollection<Character> characters, Interval<int> interval, in ReadOnlyCoordinatesSystemManager coordinatesSystemManager) => characters.SubCollection(interval, true).ForEach((character, i) => ApplyOn(character, Reverse, EasedProgress.Get(i, RealLength)));

        public static void ApplyOn(Character character, bool reverse, double progress)
        {
            if (progress == 0)
            {
                StrokeThicknessCharacterEffect.ApplyOn(character, 0, 1);
                return;
            }
            if (progress == 1) return;

            var stroke = character.Stroke;
            if (stroke == null) return;

            double length = character.Geometry.StrokeLength() / stroke.Thickness;
            character.Stroke = stroke.EditFreezable(pen => pen.DashStyle = new DashStyle(new[] { 0, length, length, 0 }, (reverse ? -length : length) * progress));
        }
    }
}
