using BenLib.Standard;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Coord
{
    /// <summary>
    /// <para>Translate progressivement une sous-collection de <see cref="Character"/> à l'index spécifié des <see cref="Character"/> d'un <see cref="VisualObject"/></para>
    /// <para>Idéalement, la sous-collection de <see cref="Character"/> à translater, décrite par les propriétés <see cref="CharacterEffect.Index"/> et <see cref="CharacterEffect.Length"/> de ce <see cref="InsertCharacterEffect"/> doit être équivalente à celle décrite par les propriétés <see cref="TextIndex"/> et <see cref="TextLength"/></para>
    /// </summary>
    public class InsertCharacterEffect : CharacterEffect
    {
        protected override Freezable CreateInstanceCore() => new InsertCharacterEffect();

        public Interval<int> BoundsInterval { get => (Interval<int>)GetValue(BoundsIntervalProperty); set => SetValue(BoundsIntervalProperty, value); }
        public static readonly DependencyProperty BoundsIntervalProperty = CreateProperty<InsertCharacterEffect, Interval<int>>(true, true, true, "BoundsInterval");

        /// <summary>
        /// <see cref="VisualObject"/> dans lequel insérer les <see cref="Character"/> de la collection
        /// </summary>
        public VisualObject VisualObject { get => (VisualObject)GetValue(VisualObjectProperty); set => SetValue(VisualObjectProperty, value); }
        public static readonly DependencyProperty VisualObjectProperty = CreateProperty<InsertCharacterEffect, VisualObject>(true, true, true, "VisualObject");

        /// <summary>
        /// Point du <see cref="System.Windows.Media.GeometryGroup"/> formé par les <see cref="Character"/> la sous-collection de <see cref="Character"/> de <see cref="Text"/>
        /// </summary>
        public RectPoint RectPoint { get => (RectPoint)GetValue(RectPointProperty); set => SetValue(RectPointProperty, value); }
        public static readonly DependencyProperty RectPointProperty = CreateProperty<InsertCharacterEffect, RectPoint>(true, true, true, "RectPoint");

        /// <summary>
        /// Indique si les <see cref="Character"/> sont translatés horizontalement
        /// </summary>
        public bool TranslateX { get => (bool)GetValue(TranslateXProperty); set => SetValue(TranslateXProperty, value); }
        public static readonly DependencyProperty TranslateXProperty = CreateProperty<InsertCharacterEffect, bool>(true, true, true, "TranslateX");

        /// <summary>
        /// Indique si les <see cref="Character"/> sont translatés verticalement
        /// </summary>
        public bool TranslateY { get => (bool)GetValue(TranslateYProperty); set => SetValue(TranslateYProperty, value); }
        public static readonly DependencyProperty TranslateYProperty = CreateProperty<InsertCharacterEffect, bool>(true, true, true, "TranslateY");

        /// <summary>
        /// Applique l'effet à une collection de <see cref="Character"/>
        /// </summary>
        /// <param name="characters">Collection de <see cref="Character"/> sur qui appliquer l'effet</param>
        protected override void ApplyCore(IReadOnlyCollection<Character> characters, Interval<int> interval, in ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            var chars = characters.SubCollection(BoundsInterval.IsNullOrEmpty() ? interval : BoundsInterval, true).ToArray();

            var diff = RectPoint.GetPoint(VisualObject.GetTransformedCharacters(coordinatesSystemManager).Geometry().Bounds) - RectPoint.GetPoint(chars.Geometry().Bounds);

            if (!TranslateX) diff.X = 0;
            if (!TranslateY) diff.Y = 0;

            characters.SubCollection(interval, true).Translate(diff, EasedProgress).Enumerate();
        }
    }
}
