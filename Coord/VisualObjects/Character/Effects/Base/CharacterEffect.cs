using BenLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Animation;

namespace Coord
{
    /// <summary>
    /// Effet à appliquer sur une collection de <see cref="Character"/> permettant de les colorer ou de les transformer individuellement
    /// </summary>
    public abstract class CharacterEffect : NotifyObject, IProgressive, ICloneable<CharacterEffect>
    {
        private IntInterval m_interval;
        private bool m_withTransforms;
        private Progress m_progress;
        private IEasingFunction m_easingFunction;

        protected int RealLength { get; private set; } = 0;

        public CharacterEffect(IntInterval interval, Progress progress, bool withTransforms, params SynchronizedProgress[] synchronizedProgresses)
        {
            Interval = interval;
            WithTransforms = withTransforms;
            Progress = progress;
            foreach (var synchronizedProgress in synchronizedProgresses) synchronizedProgress.Objects.Add(this);
        }

        public IntInterval Interval
        {
            get => m_interval;
            set
            {
                m_interval = value;
                NotifyChanged();
            }
        }
        
        /// <summary>
        /// Indique si ce <see cref="CharacterEffect"/> reçoit des <see cref="Character"/> transformés ou non
        /// </summary>
        public bool WithTransforms
        {
            get => m_withTransforms;
            set
            {
                m_withTransforms = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Progression dans le temps, entre 0 et 1, de ce <see cref="ProgressiveObject"/>
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

        /// <summary>
        /// Transforme éventuellement une collection de <see cref="Character"/> puis applique l'effet sur celle-ci
        /// </summary>
        /// <param name="characters">Collection de <see cref="Character"/> sur qui appliquer l'effet</param>
        public virtual void Apply(IReadOnlyCollection<Character> characters, CoordinatesSystemManager coordinatesSystemManager)
        {
            int realLength = RealLength;
            RealLength = (Interval * (0, characters.Count)).Length;

            if (WithTransforms)
            {
                var tr = characters.Select(c =>
                {
                    var result = c.Transformed;
                    c.ApplyTransforms();
                    return result;
                }).ToArray();

                ApplyCore(characters, coordinatesSystemManager);

                characters.ForEach((i, c) => { if (!tr[i]) c.ReleaseTransforms(); });
            }
            else ApplyCore(characters, coordinatesSystemManager);

            RealLength = realLength;
        }

        /// <summary>
        /// Applique l'effet à une collection de <see cref="Character"/>
        /// </summary>
        /// <param name="characters">Collection de <see cref="Character"/> sur qui appliquer l'effet</param>
        protected abstract void ApplyCore(IReadOnlyCollection<Character> characters, CoordinatesSystemManager coordinatesSystemManager);

        /// <summary>
        /// Crée un une copie des valeurs de l'instance actuelle de <see cref="CharacterEffect"/>
        /// </summary>
        /// <returns>Copie des valeurs de l'instance actuelle</returns>
        public abstract CharacterEffect Clone();
    }

    public static partial class Extensions
    {
        /// <summary>
        /// Sélectionne et tronque les <see cref="CharacterEffect"/> qui s'appliquent à une sous-collection d'une collection de <see cref="Character"/>
        /// </summary>
        /// <param name="characterEffects">Collection de <see cref="CharacterEffect"/> à tronquer et vérifier</param>
        /// <param name="index">Index de départ de la sous-collection</param>
        /// <param name="length">Longueur de la sous-collection</param>
        /// <returns>Collection de <see cref="CharacterEffect"/> tronqués et vérifiés</returns>
        public static IEnumerable<CharacterEffect> RangeEffects(this IEnumerable<CharacterEffect> characterEffects, IntInterval interval)
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
        public static IEnumerable<T> RangeEffects<T>(this IEnumerable<T> characterEffects, IntInterval interval) where T : CharacterEffect
        {
            foreach (var characterEffect in characterEffects)
            {
                var result = (T)characterEffect.Clone();
                result.Interval *= interval;
                yield return result;
            }
        }
    }
}
