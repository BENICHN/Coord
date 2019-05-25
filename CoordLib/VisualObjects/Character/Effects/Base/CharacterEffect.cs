using BenLib.Standard;
using System;
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
        public static readonly DependencyProperty WithTransformsProperty = CreateProperty<bool>(true, true, "WithTransforms", typeof(CharacterEffect));

        /// <summary>
        /// Progression dans le temps, entre 0 et 1, de ce <see cref="ProgressiveObject"/>
        /// </summary>
        public Progress Progress { get => (Progress)GetValue(ProgressProperty); set => SetValue(ProgressProperty, value); }
        public static readonly DependencyProperty ProgressProperty = CreateProperty<Progress>(true, true, "Progress", typeof(CharacterEffect), (sender, e) => { if (sender is CharacterEffect owner) owner.EasedProgress = ((Progress)e.NewValue).ChangeValue(owner.EasingFunction?.Ease(((Progress)e.NewValue).Value) ?? ((Progress)e.NewValue).Value); });

        public IEasingFunction EasingFunction { get => (IEasingFunction)GetValue(EasingFunctionProperty); set => SetValue(EasingFunctionProperty, value); }
        public static readonly DependencyProperty EasingFunctionProperty = CreateProperty<IEasingFunction>(true, true, "EasingFunction", typeof(CharacterEffect), (sender, e) => { if (sender is CharacterEffect owner) owner.EasedProgress = ((Progress)e.NewValue).ChangeValue(owner.EasingFunction?.Ease(((Progress)e.NewValue).Value) ?? ((Progress)e.NewValue).Value); });

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
                    bool result = c.Transformed;
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

    /*public static partial class Extensions
    {
        /// <summary>
        /// Sélectionne et tronque les <see cref="CharacterEffect"/> qui s'appliquent à une sous-collection d'une collection de <see cref="Character"/>
        /// </summary>
        /// <param name="characterEffects">Collection de <see cref="CharacterEffect"/> à tronquer et vérifier</param>
        /// <param name="index">Index de départ de la sous-collection</param>
        /// <param name="length">Longueur de la sous-collection</param>
        /// <returns>Collection de <see cref="CharacterEffect"/> tronqués et vérifiés</returns>
        public static IEnumerable<CharacterEffect> RangeEffects(this IEnumerable<CharacterEffect> characterEffects, Interval<int> interval)
        {
            foreach (var characterEffect in characterEffects)
            {
                var result = characterEffect.Clone();
                result.Interval *= interval;
                yield return result;
            }
        }

        /// <summary>
        /// Sélectionne et tronque les <see cref="CharacterEffect"/> qui s'appliquent à une sous-collection d'une collection de <see cref="Character"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="characterEffects">Collection de <see cref="CharacterEffect"/> à tronquer et vérifier</param>
        /// <param name="index">Index de départ de la sous-collection</param>
        /// <param name="length">Longueur de la sous-collection</param>
        /// <returns>Collection de <see cref="CharacterEffect"/> tronqués et vérifiés</returns>
        public static IEnumerable<T> RangeEffects<T>(this IEnumerable<T> characterEffects, Interval<int> interval) where T : CharacterEffect
        {
            foreach (var characterEffect in characterEffects)
            {
                var result = (T)characterEffect.Clone();
                result.Interval *= interval;
                yield return result;
            }
        }
    }*/
}
