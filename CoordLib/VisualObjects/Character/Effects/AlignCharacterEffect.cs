using BenLib.Standard;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Coord
{
    public class AlignCharacterEffect : CharacterEffect, ICoordEditable
    {
        IEnumerable<(string Description, DependencyProperty Property)> ICoordEditable.Properties
        {
            get
            {
                yield return ("VisualObject", VisualObjectProperty);
                yield return ("RectPoint", RectPointProperty);
                yield return ("TranslateX", TranslateXProperty);
                yield return ("TranslateY", TranslateYProperty);
                yield return ("Progress", ProgressProperty);
                yield return ("WithTransforms", WithTransformsProperty);
            }
        }

        /// <summary>
        /// <see cref="VisualObject"/> dans lequel insérer les <see cref="Character"/> de la collection
        /// </summary>
        public VisualObject VisualObject { get => (VisualObject)GetValue(VisualObjectProperty); set => SetValue(VisualObjectProperty, value); }
        public static readonly DependencyProperty VisualObjectProperty = CreateProperty<VisualObject>(true, true, "VisualObject", typeof(AlignCharacterEffect));

        /// <summary>
        /// Point du <see cref="System.Windows.Media.GeometryGroup"/> formé par les <see cref="Character"/> la sous-collection de <see cref="Character"/> de <see cref="Text"/>
        /// </summary>
        public RectPoint RectPoint { get => (RectPoint)GetValue(RectPointProperty); set => SetValue(RectPointProperty, value); }
        public static readonly DependencyProperty RectPointProperty = CreateProperty<RectPoint>(true, true, "RectPoint", typeof(AlignCharacterEffect));

        /// <summary>
        /// Indique si les <see cref="Character"/> sont translatés horizontalement
        /// </summary>
        public bool TranslateX { get => (bool)GetValue(TranslateXProperty); set => SetValue(TranslateXProperty, value); }
        public static readonly DependencyProperty TranslateXProperty = CreateProperty<bool>(true, true, "TranslateX", typeof(AlignCharacterEffect));

        /// <summary>
        /// Indique si les <see cref="Character"/> sont translatés verticalement
        /// </summary>
        public bool TranslateY { get => (bool)GetValue(TranslateYProperty); set => SetValue(TranslateYProperty, value); }
        public static readonly DependencyProperty TranslateYProperty = CreateProperty<bool>(true, true, "TranslateY", typeof(AlignCharacterEffect));

        protected override void ApplyCore(IReadOnlyCollection<Character> characters, Interval<int> interval, in ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            var chars = characters.SubCollection(interval, true).ToArray();
            var diff = RectPoint.GetPoint(VisualObject.GetTransformedCharacters(coordinatesSystemManager).Geometry().Bounds) - RectPoint.GetPoint(chars.Geometry().Bounds);

            if (!TranslateX) diff.X = 0;
            if (!TranslateY) diff.Y = 0;

            chars.Translate(diff, EasedProgress).Enumerate();
        }
    }
}
