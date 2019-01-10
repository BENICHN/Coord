using BenLib;
using BenLib.WPF;
using System.Collections.Generic;
using System.Windows.Media;

namespace Coord
{
    public class WriteCharacterEffect : CharacterEffect
    {
        public WriteCharacterEffect(IntInterval interval, bool reverse, double strokeThickness, Progress progress, params SynchronizedProgress[] synchronizedProgresses) : this(interval, reverse, strokeThickness, progress, true, synchronizedProgresses) { }
        public WriteCharacterEffect(IntInterval interval, bool reverse, double strokeThickness, Progress progress, bool withTransforms, params SynchronizedProgress[] synchronizedProgresses) : base(interval, progress, withTransforms, synchronizedProgresses) => StrokeThickness = strokeThickness;

        private double m_strokeThickness;
        public double StrokeThickness
        {
            get => m_strokeThickness;
            set
            {
                m_strokeThickness = value;
                NotifyChanged();
            }
        }

        private bool m_reverse;
        public bool Reverse
        {
            get => m_reverse;
            set
            {
                m_reverse = value;
                NotifyChanged();
            }
        }

        protected override void ApplyCore(IReadOnlyCollection<Character> characters, CoordinatesSystemManager coordinatesSystemManager) => characters.SubCollection(Interval).ForEach((i, character) => ApplyOn(character, Reverse, StrokeThickness, EasedProgress.Get(i, RealLength)));

        public static void ApplyOn(Character character, bool reverse, double strokeThickness, double progress)
        {
            var fill = character.Fill;

            if (character.Stroke == null)
            {
                var sample = new Pen(null, 0) { Brush = fill };
                character.Stroke = sample.CloneCurrentValue();
            }

            double baseStrokeThickness = character.Stroke.Thickness;

            var (strokeProgress, endProgress) = progress.SplitTrimProgress(0.5);
            double colorProgress = 1 - endProgress;

            var stroke = character.Stroke;
            OpacityCharacterEffect.ApplyOn(character, 0, double.NaN, colorProgress);
            StrokeThicknessCharacterEffect.ApplyOn(character, strokeThickness, colorProgress);

            StrokeCharacterEffect.ApplyOn(character, reverse, strokeProgress.Trim());
        }

        public override CharacterEffect Clone() => new WriteCharacterEffect(Interval, Reverse, StrokeThickness, Progress, WithTransforms);
    }
}
