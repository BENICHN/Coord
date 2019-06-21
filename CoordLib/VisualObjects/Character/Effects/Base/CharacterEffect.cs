using BenLib.Standard;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Animation;

namespace Coord
{
    /// <summary>
    /// Effet à appliquer sur une collection de <see cref="Character"/> permettant de les colorer ou de les transformer individuellement
    /// </summary>
    public abstract class CharacterEffect : NotifyObject, IProgressive
    {
        protected int RealLength { get; private set; } = 0;
        protected Progress EasedProgress { get; private set; }

        /// <summary>
        /// Indique si ce <see cref="CharacterEffect"/> reçoit des <see cref="Character"/> transformés ou non
        /// </summary>
        public bool WithTransforms { get => (bool)GetValue(WithTransformsProperty); set => SetValue(WithTransformsProperty, value); }
        public static readonly DependencyProperty WithTransformsProperty = CreateProperty<CharacterEffect, bool>(true, true, true, "WithTransforms", true);

        /// <summary>
        /// Progression dans le temps, entre 0 et 1, de ce <see cref="ProgressiveObject"/>
        /// </summary>
        public Progress Progress { get => (Progress)GetValue(ProgressProperty); set => SetValue(ProgressProperty, value); }
        public static readonly DependencyProperty ProgressProperty = CreateProperty<CharacterEffect, Progress>(true, true, true, "Progress", (sender, e) => { if (sender is CharacterEffect owner) owner.EasedProgress = ((Progress)e.NewValue).ChangeValue(owner.EasingFunction?.Ease(((Progress)e.NewValue).Value) ?? ((Progress)e.NewValue).Value); });

        public IEasingFunction EasingFunction { get => (IEasingFunction)GetValue(EasingFunctionProperty); set => SetValue(EasingFunctionProperty, value); }
        public static readonly DependencyProperty EasingFunctionProperty = CreateProperty<CharacterEffect, IEasingFunction>(true, true, true, "EasingFunction", (sender, e) => { if (sender is CharacterEffect owner) owner.EasedProgress = ((Progress)e.NewValue).ChangeValue(owner.EasingFunction?.Ease(((Progress)e.NewValue).Value) ?? ((Progress)e.NewValue).Value); });

        /// <summary>
        /// Transforme éventuellement une collection de <see cref="Character"/> puis applique l'effet sur celle-ci
        /// </summary>
        /// <param name="characters">Collection de <see cref="Character"/> sur qui appliquer l'effet</param>
        public virtual void Apply(IReadOnlyCollection<Character> characters, Interval<int> interval, in ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            int realLength = RealLength;
            RealLength = (interval & (0, characters.Count)).Container.Length() ?? -1;

            if (WithTransforms)
            {
                bool[] tr = characters.Select(c =>
                {
                    bool result = c.IsTransformed;
                    c.ApplyTransforms();
                    return result;
                }).ToArray();

                ApplyCore(characters, interval, coordinatesSystemManager);

                characters.ForEach((c, i) => { if (!tr[i]) c.ReleaseTransforms(); });
            }
            else ApplyCore(characters, interval, coordinatesSystemManager);

            RealLength = realLength;
        }

        /// <summary>
        /// Applique l'effet à une collection de <see cref="Character"/>
        /// </summary>
        /// <param name="characters">Collection de <see cref="Character"/> sur qui appliquer l'effet</param>
        protected abstract void ApplyCore(IReadOnlyCollection<Character> characters, Interval<int> interval, in ReadOnlyCoordinatesSystemManager coordinatesSystemManager);
    }
}
