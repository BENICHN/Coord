using BenLib;
using System.Collections.Generic;
using System.Linq;

namespace Coord
{
    public class FitCharacterEffect : CharacterEffect
    {
        private IntInterval m_textInterval;
        private VisualObject m_text;
        private bool m_scaleX;
        private bool m_scaleY;

        public FitCharacterEffect(IntInterval interval, IntInterval boundsInterval, VisualObject text, IntInterval textInterval, Progress progress, params SynchronizedProgress[] synchronizedProgresses) : this(interval, boundsInterval, text, textInterval, progress, true, true, true, synchronizedProgresses) { }
        public FitCharacterEffect(IntInterval interval, IntInterval boundsInterval, VisualObject text, IntInterval textInterval, Progress progress, bool withTransforms, params SynchronizedProgress[] synchronizedProgresses) : this(interval, boundsInterval, text, textInterval, progress, withTransforms, true, true, synchronizedProgresses) { }
        public FitCharacterEffect(IntInterval interval, IntInterval boundsInterval, VisualObject text, IntInterval textInterval, Progress progress, bool scaleX, bool scaleY, params SynchronizedProgress[] synchronizedProgresses) : this(interval, boundsInterval, text, textInterval, progress, true, scaleX, scaleY, synchronizedProgresses) { }
        public FitCharacterEffect(IntInterval interval, IntInterval boundsInterval, VisualObject text, IntInterval textInterval, Progress progress, bool withTransforms, bool scaleX, bool scaleY, params SynchronizedProgress[] synchronizedProgresses) : base(interval, progress, withTransforms, synchronizedProgresses)
        {
            BoundsInterval = boundsInterval;
            Text = text;
            TextInterval = textInterval;
            ScaleX = scaleX;
            ScaleY = scaleY;
        }

        private IntInterval m_boundsInterval;
        public IntInterval BoundsInterval
        {
            get => m_boundsInterval;
            set
            {
                m_boundsInterval = value;
                NotifyChanged();
            }
        }

        public VisualObject Text
        {
            get => m_text;
            set
            {
                UnRegister(m_text);
                m_text = value;
                Register(m_text);
                NotifyChanged();
            }
        }

        public IntInterval TextInterval
        {
            get => m_textInterval;
            set
            {
                m_textInterval = value;
                NotifyChanged();
            }
        }

        public bool ScaleX
        {
            get => m_scaleX;
            set
            {
                m_scaleX = value;
                NotifyChanged();
            }
        }

        public bool ScaleY
        {
            get => m_scaleY;
            set
            {
                m_scaleY = value;
                NotifyChanged();
            }
        }

        protected override void ApplyCore(IReadOnlyCollection<Character> characters, ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            if (Text == null) return;

            bool withTransforms = WithTransforms;
            var chars = characters.SubCollection(BoundsInterval.IsNullOrEmpty() ? Interval : BoundsInterval, true).ToArray();

            var (from, to) = withTransforms ?
                (chars.Geometry().Bounds.Size, Text.GetTransformedCharacters(coordinatesSystemManager, true).SubCollection(TextInterval, true).Geometry().Bounds.Size) :
                (chars.Geometry().Bounds.Size, Text.GetCharacters(coordinatesSystemManager).SubCollection(TextInterval, true).Geometry().Bounds.Size);

            double scaleX = ScaleX ? to.Width / from.Width : 0;
            double scaleY = ScaleY ? to.Height / from.Height : 0;

            characters.SubCollection(Interval, true).Scale(scaleX, scaleY, EasedProgress).Enumerate();
        }

        public override CharacterEffect Clone() => new FitCharacterEffect(Interval, BoundsInterval, Text, TextInterval, Progress, WithTransforms, ScaleX, ScaleY);
    }
}
