using BenLib;
using System.Collections.Generic;
using System.Windows.Media;

namespace Coord
{
    public class AutoPenCharacterEffect : CharacterEffect
    {
        private Pen m_sample;

        public AutoPenCharacterEffect(IntInterval interval, Pen sample, Progress progress, bool withTransforms = false, params SynchronizedProgress[] synchronizedProgresses) : base(interval, progress, withTransforms, synchronizedProgresses) => Sample = sample;

        public Pen Sample
        {
            get => m_sample;
            set
            {
                UnRegister(m_sample);
                m_sample = value;
                Register(m_sample);
                NotifyChanged();
            }
        }

        public override CharacterEffect Clone() => new AutoPenCharacterEffect(Interval, Sample, Progress, WithTransforms);

        protected override void ApplyCore(IReadOnlyCollection<Character> characters, CoordinatesSystemManager coordinatesSystemManager)
        {
            var sample = Sample?.CloneCurrentValue();
            if (sample != null)
            {
                foreach (var character in characters.SubCollection(Interval))
                {
                    if (character.Stroke == null)
                    {
                        sample.Brush = character.Fill;
                        character.Stroke = sample.CloneCurrentValue();
                    }
                }
            }
        }
    }
}
