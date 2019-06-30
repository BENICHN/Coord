using BenLib.Framework;
using BenLib.Standard;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Coord
{
    public class WriteCharacterEffect : CharacterEffect
    {
        protected override Freezable CreateInstanceCore() => new WriteCharacterEffect();

        public double StrokeThickness { get => (double)GetValue(StrokeThicknessProperty); set => SetValue(StrokeThicknessProperty, value); }
        public static readonly DependencyProperty StrokeThicknessProperty = CreateProperty<WriteCharacterEffect, double>(true, true, true, "StrokeThickness");

        public bool Reverse { get => (bool)GetValue(ReverseProperty); set => SetValue(ReverseProperty, value); }
        public static readonly DependencyProperty ReverseProperty = CreateProperty<WriteCharacterEffect, bool>(true, true, true, "Reverse");

        protected override void ApplyCore(IReadOnlyCollection<Character> characters, Interval<int> interval, in ReadOnlyCoordinatesSystemManager coordinatesSystemManager) => characters.SubCollection(interval, true).ForEach((character, i) => ApplyOn(character, Reverse, StrokeThickness, EasedProgress.Get(i, RealLength)));

        public static void ApplyOn(Character character, bool reverse, double strokeThickness, double progress)
        {
            var fill = character.Fill;

            if (character.Stroke == null)
            {
                var sample = new Pen(fill, 0);
                character.Stroke = sample.CloneCurrentValue();
            }

            var (strokeProgress, endProgress) = progress.SplitTrimProgress(0.5);
            double colorProgress = 1 - endProgress;

            OpacityCharacterEffect.ApplyOn(character, 0, double.NaN, colorProgress);
            StrokeThicknessCharacterEffect.ApplyOn(character, strokeThickness, colorProgress);

            StrokeCharacterEffect.ApplyOn(character, reverse, strokeProgress.Trim(0, 1));
        }
    }
}
