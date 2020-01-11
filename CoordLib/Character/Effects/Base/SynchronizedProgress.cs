using BenLib.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Animation;
using static BenLib.Framework.Animating;
using static Coord.VisualObjects;

namespace Coord
{
    public class SynchronizedProgress
    {
        private double m_progress;

        public SynchronizedProgress(double progress, IEnumerable<IProgressive> effects) : this(progress, effects.ToArray()) { }
        public SynchronizedProgress(double progress, params IProgressive[] effects) : this(progress, effectsList: effects) { }
        public SynchronizedProgress(double progress, IList<IProgressive> effectsList)
        {
            Progress = progress;
            Objects = effectsList.ToList();
        }

        public double Progress
        {
            get => m_progress;
            set
            {
                m_progress = value;
                if (Objects != null) foreach (var characterEffect in Objects) characterEffect.Progress = characterEffect.Progress.ChangeValue(value);
            }
        }

        public List<IProgressive> Objects { get; set; }

        public StaticAnimation Animate(TimeSpan duration, IEasingFunction easingFunction = null, double from = 0, double to = 1, RepeatBehavior repeatBehavior = default, bool autoReverse = false, bool isCumulative = false, int? fps = FPS) => Animate<double>(value => Progress = value, from, to, duration, repeatBehavior, autoReverse, isCumulative, easingFunction, fps);
        public StaticAnimation Animate(double secondsDuration, IEasingFunction easingFunction = null, double from = 0, double to = 1, RepeatBehavior repeatBehavior = default, bool autoReverse = false, bool isCumulative = false, int? fps = FPS) => Animate(TimeSpan.FromSeconds(secondsDuration), easingFunction, from, to, repeatBehavior, autoReverse, isCumulative, fps);
    }
}
