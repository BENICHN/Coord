using BenLib;
using BenLib.WPF;
using System.Collections.Generic;

namespace Coord
{
    public class StrokeThicknessCharacterEffect : CharacterEffect
    {
        private double m_strokeThickness;

        public StrokeThicknessCharacterEffect(IntInterval interval, double strokeThickness, Progress progress, params SynchronizedProgress[] synchronizedProgresses) : this(interval, strokeThickness, progress, false, synchronizedProgresses) { }
        public StrokeThicknessCharacterEffect(IntInterval interval, double strokeThickness, Progress progress, bool withTransforms, params SynchronizedProgress[] synchronizedProgresses) : base(interval, progress, withTransforms, synchronizedProgresses) => StrokeThickness = strokeThickness;

        public double StrokeThickness
        {
            get => m_strokeThickness;
            set
            {
                m_strokeThickness = value;
                NotifyChanged();
            }
        }

        protected override void ApplyCore(IReadOnlyCollection<Character> characters, CoordinatesSystemManager coordinatesSystemManager) => characters.SubCollection(Interval).ForEach((i, character) => ApplyOn(character, StrokeThickness, EasedProgress.Get(i, RealLength)));

        public static void ApplyOn(Character character, double strokeThickness, double progress)
        {
            var stroke = character.Stroke;
            if (stroke != null) character.Stroke = stroke.Edit(pen => pen.Thickness = Num.Interpolate(stroke.Thickness, strokeThickness, progress));
        }

        public override CharacterEffect Clone() => new StrokeThicknessCharacterEffect(Interval, StrokeThickness, Progress, WithTransforms);
    }
}
