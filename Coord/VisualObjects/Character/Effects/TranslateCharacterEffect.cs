using BenLib;
using System.Collections.Generic;
using System.Windows;

namespace Coord
{
    /// <summary>
    /// Translate progressivement une sous-collection de <see cref="Character"/> par un vecteur spécifié
    /// </summary>
    public class TranslateCharacterEffect : CharacterEffect
    {
        private Vector m_outVector;
        private bool m_in;

        public TranslateCharacterEffect(IntInterval interval, Vector vector, bool inTranslation, Progress progress, params SynchronizedProgress[] synchronizedProgresses) : this(interval, vector, inTranslation, progress, true, synchronizedProgresses) { }
        public TranslateCharacterEffect(IntInterval interval, Vector vector, bool inTranslation, Progress progress, bool withTransforms, params SynchronizedProgress[] synchronizedProgresses) : base(interval, progress, withTransforms, synchronizedProgresses)
        {
            Vector = vector;
            In = inTranslation;
        }

        /// <summary>
        /// Vecteur associé à la translation
        /// </summary>
        public Vector Vector
        {
            get => m_outVector;
            set
            {
                m_outVector = value;
                NotifyChanged();
            }
        }
        public bool In
        {
            get => m_in;
            set
            {
                m_in = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Applique l'effet à une collection de <see cref="Character"/>
        /// </summary>
        /// <param name="characters">Collection de <see cref="Character"/> sur qui appliquer l'effet</param>
        protected override void ApplyCore(IReadOnlyCollection<Character> characters, ReadOnlyCoordinatesSystemManager coordinatesSystemManager) => characters.SubCollection(Interval, true).Translate(In ? coordinatesSystemManager.ComputeOutCoordinates(Vector) : Vector, EasedProgress).Enumerate();

        /// <summary>
        /// Crée un une copie des valeurs de l'instance actuelle de <see cref="TranslateCharacterEffect"/>
        /// </summary>
        /// <returns>Copie des valeurs de l'instance actuelle</returns>
        public override CharacterEffect Clone() => new TranslateCharacterEffect(Interval, Vector, In, Progress, WithTransforms);
    }
}
