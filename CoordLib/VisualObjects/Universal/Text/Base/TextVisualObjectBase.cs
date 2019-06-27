using BenLib.Standard;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Coord
{
    /// <summary>
    /// Objet visuel effectuant le rendu d'une collection de <see cref="Character"/> sur lesquels des <see cref="CharacterEffect"/> peuvent être appliqués
    /// </summary>
    public abstract class TextVisualObjectBase : VisualObject
    {
        /// <summary>
        /// Tableau contanant les <see cref="Character"/> mis en cache par <see cref="GetCharacters"/>. Pour que le cache soit recalculé, définissez le à <see langword="null"/>
        /// </summary>
        protected Character[] InCache;

        /// <summary>
        /// Coin supérieur gauche
        /// </summary>
        public PointVisualObject InAnchorPoint { get => (PointVisualObject)GetValue(InAnchorPointProperty); set => SetValue(InAnchorPointProperty, value); }
        public static readonly DependencyProperty InAnchorPointProperty = CreateProperty<TextVisualObjectBase, PointVisualObject>(true, true, true, "InAnchorPoint");

        public bool In { get => (bool)GetValue(InProperty); set => SetValue(InProperty, value); }
        public static readonly DependencyProperty InProperty = CreateProperty<TextVisualObjectBase, bool>(true, true, true, "In");

        public RectPoint RectPoint { get => (RectPoint)GetValue(RectPointProperty); set => SetValue(RectPointProperty, value); }
        public static readonly DependencyProperty RectPointProperty = CreateProperty<TextVisualObjectBase, RectPoint>(true, true, true, "RectPoint");

        /// <summary>
        /// Obtient le nombre de <see cref="Character"/> qui composent ce <see cref="TextVisualObjectBase"/>
        /// </summary>
        public int CharactersCount => GetCharacters().Length;

        /// <summary>
        /// Obtient et met en cache un tableau contenant les <see cref="Character"/> qui composent ce <see cref="TextVisualObjectBase"/>
        /// </summary>
        /// <returns>Tableau contenant les <see cref="Character"/> qui composent ce <see cref="TextVisualObjectBase"/></returns>
        public Character[] GetCharacters()
        {
            if (InCache == null) InCache = GetCharactersCore();
            return InCache.CloneCharacters().ToArray();
        }

        protected override IReadOnlyCollection<Character> GetCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            var characters = GetCharacters().Color(Fill, Stroke);
            var outAnchorPoint = coordinatesSystemManager.ComputeOutCoordinates(InAnchorPoint);
            var outVector = RectPoint.IsNaN ? (Vector)outAnchorPoint : outAnchorPoint - RectPoint.GetPoint(characters.Bounds());
            characters.Translate(outVector, 1).Enumerate();
            return In ? characters.ScaleAt(coordinatesSystemManager.WidthRatio, coordinatesSystemManager.HeightRatio, outAnchorPoint, 1).ToArray() : characters.ToArray();
        }

        /// <summary>
        /// Obtient un tableau contenant les <see cref="Character"/> qui composent ce <see cref="TextVisualObjectBase"/>
        /// </summary>
        /// <returns>Tableau contenant les <see cref="Character"/> qui composent ce <see cref="TextVisualObjectBase"/></returns>
        protected abstract Character[] GetCharactersCore();

        /// <summary>
        /// Obtient les dimensions du <see cref="GeometryGroup"/> composé de toutes les <see cref="Character.Geometry"/> de ce <see cref="TextVisualObjectBase"/>
        /// </summary>
        /// <returns>Dimensions du <see cref="GeometryGroup"/> composé de toutes les <see cref="Character.Geometry"/> de ce <see cref="TextVisualObjectBase"/></returns>
        public Size Size() => GetCharacters().Bounds().Size;
        public Size SizeX() => new Size(Size().Width, 0);
        public Size SizeY() => new Size(0, Size().Height);

        /// <summary>
        /// Obtient les dimensions du <see cref="GeometryGroup"/> composé de toutes les <see cref="Character.Geometry"/> transformées par les <see cref="CharacterEffect"/> de <see cref="Effects"/> de ce <see cref="TextVisualObjectBase"/>
        /// </summary>
        /// <returns>Dimensions du <see cref="GeometryGroup"/> composé de toutes les <see cref="Character.Geometry"/> transformées par les <see cref="CharacterEffect"/> de <see cref="Effects"/> de ce <see cref="TextVisualObjectBase"/></returns>
        public Size TransformedSize()
        {
            var characters = GetCharacters();
            foreach (var character in characters) character.ApplyTransforms();
            return characters.Bounds().Size;
        }
    }
}
