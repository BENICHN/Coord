using System.Windows.Media.Animation;

namespace Coord
{
    /*/// <summary>
    /// <see cref="CharacterEffect"/> dont l'effet est progressif dans le temps
    /// </summary>
    public abstract class CharacterEffect : CharacterEffect
    {
        private Progress m_progress;
        private IEasingFunction m_easingFunction;

        public CharacterEffect(int index, int length, Progress progress, bool withTransforms, params SynchronizedProgress[] synchronizedProgresses) : base(index, length, withTransforms)
        {
            Progress = progress;
            foreach (var synchronizedProgress in synchronizedProgresses) synchronizedProgress.Objects.Add(this);
        }

        /// <summary>
        /// Progression dans le temps, entre 0 et 1, de l'effet du <see cref="CharacterEffect"/>
        /// </summary>
        public Progress Progress
        {
            get => m_progress;
            set
            {
                m_progress = value;
                EasedProgress = value.ChangeValue(EasingFunction?.Ease(value.Value) ?? value.Value);
                NotifyChanged();
            }
        }

        protected Progress EasedProgress { get; private set; }

        public IEasingFunction EasingFunction
        {
            get => m_easingFunction;
            set
            {
                m_easingFunction = value;
                EasedProgress = Progress.ChangeValue(value?.Ease(Progress.Value) ?? Progress.Value);
                NotifyChanged();
            }
        }
    }*/
}
