using BenLib.Framework;
using BenLib.Standard;
using System.Collections.Generic;
using System.Windows;

namespace Coord
{
    public class WriteCharacterEffect : CharacterEffect, ICoordEditable
    {
        IEnumerable<(string Description, DependencyProperty Property)> ICoordEditable.Properties
        {
            get
            {
                yield return ("StrokeThickness", StrokeThicknessProperty);
                yield return ("Reverse", ReverseProperty);
                yield return ("Progress", ProgressProperty);
                yield return ("WithTransforms", WithTransformsProperty);
            }
        }

        public double StrokeThickness { get => (double)GetValue(StrokeThicknessProperty); set => SetValue(StrokeThicknessProperty, value); }
        public static readonly DependencyProperty StrokeThicknessProperty = CreateProperty<double>(true, true, "StrokeThickness", typeof(WriteCharacterEffect));

        public bool Reverse { get => (bool)GetValue(ReverseProperty); set => SetValue(ReverseProperty, value); }
        public static readonly DependencyProperty ReverseProperty = CreateProperty<bool>(true, true, "Reverse", typeof(WriteCharacterEffect));

        protected override void ApplyCore(IReadOnlyCollection<Character> characters, Interval<int> interval, in ReadOnlyCoordinatesSystemManager coordinatesSystemManager) => characters.SubCollection(interval, true).ForEach((character, i) => ApplyOn(character, Reverse, StrokeThickness, EasedProgress.Get(i, RealLength)));

        public static void ApplyOn(Character character, bool reverse, double strokeThickness, double progress)
        {
            var fill = character.Fill;

            if (character.Stroke == null)
            {
                var sample = new PlanePen(fill, 0);
                character.Stroke = sample.CloneCurrentValue();
            }

            var (strokeProgress, endProgress) = progress.SplitTrimProgress(0.5);
            double colorProgress = 1 - endProgress;

            OpacityCharacterEffect.ApplyOn(character, 0, double.NaN, colorProgress);
            StrokeThicknessCharacterEffect.ApplyOn(character, strokeThickness, colorProgress);

            StrokeCharacterEffect.ApplyOn(character, reverse, strokeProgress.Trim());
        }
    }
}
