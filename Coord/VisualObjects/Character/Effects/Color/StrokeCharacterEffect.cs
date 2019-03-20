using BenLib;
using BenLib.WPF;
using System.Collections.Generic;
using System.Windows.Media;

namespace Coord
{
    /// <summary>
    /// Dessine progressivement le contour des <see cref="Character"/> d'une sous-collection
    /// </summary>
    public class StrokeCharacterEffect : CharacterEffect
    {
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

        public StrokeCharacterEffect(IntInterval interval, bool reverse, Progress progress, params SynchronizedProgress[] synchronizedProgresses) : this(interval, reverse, progress, true, synchronizedProgresses) { }
        public StrokeCharacterEffect(IntInterval interval, bool reverse, Progress progress, bool withTransforms, params SynchronizedProgress[] synchronizedProgresses) : base(interval, progress, withTransforms, synchronizedProgresses) => Reverse = reverse;

        /// <summary>
        /// Applique l'effet à une collection de <see cref="Character"/>
        /// </summary>
        /// <param name="characters">Collection de <see cref="Character"/> sur qui appliquer l'effet</param>
        protected override void ApplyCore(IReadOnlyCollection<Character> characters, ReadOnlyCoordinatesSystemManager coordinatesSystemManager) => characters.SubCollection(Interval, true).ForEach((i, character) => ApplyOn(character, Reverse, EasedProgress.Get(i, RealLength)));

        public static void ApplyOn(Character character, bool reverse, double progress)
        {
            if (progress == 0)
            {
                StrokeThicknessCharacterEffect.ApplyOn(character, 0, 1);
                return;
            }
            if (progress == 1) return;

            var stroke = character.Stroke;
            if (stroke == null) return;

            var length = character.Geometry.StrokeLength() / stroke.Thickness;
            character.Stroke = stroke.Edit(pen => pen.DashStyle = new DashStyle(new[] { 0, length, length, 0 }, (reverse ? -length : length) * progress));
        }

        /// <summary>
        /// Crée un une copie des valeurs de l'instance actuelle de <see cref="StrokeCharacterEffect"/>
        /// </summary>
        /// <returns>Copie des valeurs de l'instance actuelle</returns>
        public override CharacterEffect Clone() => new StrokeCharacterEffect(Interval, Reverse, Progress);
    }
}
