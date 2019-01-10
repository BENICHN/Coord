using BenLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using static BenLib.Animating;
using static Coord.MainWindow;

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
                if (!Objects.IsNullOrEmpty()) foreach (var characterEffect in Objects) characterEffect.Progress = characterEffect.Progress.ChangeValue(value);
            }
        }

        public List<IProgressive> Objects { get; set; }

        public Task Animate(string name, TimeSpan duration, IEasingFunction easingFunction = null, double from = 0, double to = 1, RepeatBehavior repeatBehavior = default, bool autoReverse = false, bool isCumulative = false, int? fps = FPS) => AnimateDouble(name, value => Progress = value, from, to, duration, repeatBehavior, autoReverse, isCumulative, easingFunction, fps);
        public Task Animate(string name, double secondsDuration, IEasingFunction easingFunction = null, double from = 0, double to = 1, RepeatBehavior repeatBehavior = default, bool autoReverse = false, bool isCumulative = false, int? fps = FPS) => Animate(name, TimeSpan.FromSeconds(secondsDuration), easingFunction, from, to, repeatBehavior, autoReverse, isCumulative, fps);

        public Task Animate(TimeSpan duration, IEasingFunction easingFunction = null, double from = 0, double to = 1, RepeatBehavior repeatBehavior = default, bool autoReverse = false, bool isCumulative = false, int? fps = FPS) => Animate(null, duration, easingFunction, from, to, repeatBehavior, autoReverse, isCumulative, fps);
        public Task Animate(double secondsDuration, IEasingFunction easingFunction = null, double from = 0, double to = 1, RepeatBehavior repeatBehavior = default, bool autoReverse = false, bool isCumulative = false, int? fps = FPS) => Animate(null, TimeSpan.FromSeconds(secondsDuration), easingFunction, from, to, repeatBehavior, autoReverse, isCumulative, fps);
    }
}
