using BenLib.Standard;
using System.Collections.Generic;
using System.Windows;

namespace Coord
{
    /// <summary>
    /// Translate progressivement une sous-collection de <see cref="Character"/> par un vecteur spécifié
    /// </summary>
    public class TranslateCharacterEffect : CharacterEffect
    {
        protected override Freezable CreateInstanceCore() => new TranslateCharacterEffect();

        /// <summary>
        /// Vecteur associé à la translation
        /// </summary>
        public Vector Vector { get => (Vector)GetValue(VectorProperty); set => SetValue(VectorProperty, value); }
        public static readonly DependencyProperty VectorProperty = CreateProperty<TranslateCharacterEffect, Vector>(true, true, true, "Vector");

        public bool In { get => (bool)GetValue(InProperty); set => SetValue(InProperty, value); }

        public static readonly DependencyProperty InProperty = CreateProperty<TranslateCharacterEffect, bool>(true, true, true, "In");

        /// <summary>
        /// Applique l'effet à une collection de <see cref="Character"/>
        /// </summary>
        /// <param name="characters">Collection de <see cref="Character"/> sur qui appliquer l'effet</param>
        protected override void ApplyCore(IReadOnlyCollection<Character> characters, Interval<int> interval, in ReadOnlyCoordinatesSystemManager coordinatesSystemManager) => characters.SubCollection(interval, true).Translate(In ? coordinatesSystemManager.ComputeOutCoordinates(Vector) : Vector, EasedProgress).Enumerate();
    }
}
