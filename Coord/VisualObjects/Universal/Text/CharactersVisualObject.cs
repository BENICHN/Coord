using System.Collections.Generic;
using System.Linq;

namespace Coord
{
    /// <summary>
    /// Définit un <see cref="TextVisualObjectBase"/> à partir d'une collection de <see cref="Character"/>
    /// </summary>
    public class CharactersVisualObject : TextVisualObjectBase
    {
        private Character[] m_characters;

        /// <summary>
        /// Tableau contenant les <see cref="Character"/> qui composent ce <see cref="CharactersVisualObject"/>
        /// </summary>
        public Character[] Characters
        {
            get => m_characters;
            set
            {
                m_characters = value;
                InCache = null;
                NotifyChanged();
            }
        }

        public CharactersVisualObject(PointVisualObject inAnchorPoint, RectPoint rectPoint, bool inText, IEnumerable<Character> characters) : base(inAnchorPoint, rectPoint, inText) => Characters = characters.ToArray();

        /// <summary>
        /// Obtient les <see cref="Character"/> de <see cref="Characters"/>
        /// </summary>
        /// <returns>Les <see cref="Character"/> de <see cref="Characters"/></returns>
        protected override Character[] GetCharactersCore() => Characters;
    }
}
