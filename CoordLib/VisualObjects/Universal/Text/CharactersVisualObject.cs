using System.Windows;

namespace Coord
{
    /// <summary>
    /// Définit un <see cref="TextVisualObjectBase"/> à partir d'une collection de <see cref="Character"/>
    /// </summary>
    public class CharactersVisualObject : TextVisualObjectBase
    {
        public override string Type => "Characters";

        /// <summary>
        /// Tableau contenant les <see cref="Character"/> qui composent ce <see cref="CharactersVisualObject"/>
        /// </summary>
        public Character[] Characters { get => (Character[])GetValue(CharactersProperty); set => SetValue(CharactersProperty, value); }
        public static readonly DependencyProperty CharactersProperty = CreateProperty<Character[]>(true, true, "Characters", typeof(CharactersVisualObject));

        /// <summary>
        /// Obtient les <see cref="Character"/> de <see cref="Characters"/>
        /// </summary>
        /// <returns>Les <see cref="Character"/> de <see cref="Characters"/></returns>
        protected override Character[] GetCharactersCore() => Characters;

        protected override void OnChanged()
        {
            InCache = null;
            base.OnChanged();
        }
    }
}
