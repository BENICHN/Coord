using BenLib;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace Coord
{
    /// <summary>
    /// Objet visuel pouvant dessiner dans un <see cref="Plane"/>
    /// </summary>
    public abstract class VisualObject : NotifyObject
    {
        private Pen m_stroke;
        private Brush m_fill;
        private NotifyObjectCollection<CharacterEffect> m_effects;

        public VisualObject()
        {
            Effects = new NotifyObjectCollection<CharacterEffect>();
            Changed += (sender, e) =>
            {
                IsChanged = true;
                Cache = default;
            };
        }

        /// <summary>
        /// Remplissage de l'objet
        /// </summary>
        public Brush Fill
        {
            get => m_fill;
            set
            {
                m_fill = value;
                if (m_fill != null && m_fill.CanFreeze) m_fill.Freeze();
                NotifyChanged();
            }
        }

        /// <summary>
        /// Contour de l'objet
        /// </summary>
        public Pen Stroke
        {
            get => m_stroke;
            set
            {
                m_stroke = value;
                if (m_stroke != null && m_stroke.CanFreeze) m_stroke.Freeze();
                NotifyChanged();
            }
        }

        /// <summary>
        /// Effets à appliquer sur les <see cref="Character"/>
        /// </summary>
        public NotifyObjectCollection<CharacterEffect> Effects
        {
            get => m_effects;
            set
            {
                UnRegister(m_effects);
                m_effects = value;
                Register(m_effects);
                NotifyChanged();
            }
        }

        /// <summary>
        /// Indique si l'objet a changé depuis le dernier appel de <see cref="Render(DrawingContext, CoordinatesSystemManager)"/>
        /// </summary>
        public bool IsChanged { get; private set; }

        protected (IReadOnlyCollection<Character> Characters, CoordinatesSystemManager CoordinatesSystemManager) Cache;

        /// <summary>
        /// Dessine dans un <see cref="Plane"/> et définit <see cref="IsChanged"/> à <see langword="false"/>
        /// </summary>
        /// <param name="drawingContext">Dessine dans le <see cref="DrawingVisual"/> associé à l'objet</param>
        /// <param name="coordinatesSystemManager">Système de coordonnées du <see cref="Plane"/></param>
        public void Render(DrawingContext drawingContext, CoordinatesSystemManager coordinatesSystemManager)
        {
            RenderCore(drawingContext, coordinatesSystemManager);
            IsChanged = false;
        }

        /// <summary>
        /// Dessine dans un <see cref="Plane"/>
        /// </summary>
        /// <param name="drawingContext">Dessine dans le <see cref="DrawingVisual"/> associé à l'objet</param>
        /// <param name="coordinatesSystemManager">Système de coordonnées du <see cref="Plane"/></param>
        protected virtual void RenderCore(DrawingContext drawingContext, CoordinatesSystemManager coordinatesSystemManager)
        {
            var characters = GetTransformedCharacters(coordinatesSystemManager, false);
            foreach (var character in characters) drawingContext.DrawCharacter(character);
        }

        public abstract IReadOnlyCollection<Character> GetCharacters(CoordinatesSystemManager coordinatesSystemManager);
        public virtual IReadOnlyCollection<Character> GetTransformedCharacters(CoordinatesSystemManager coordinatesSystemManager, bool applyTransforms)
        {
            if (Cache.Characters == null || Cache.CoordinatesSystemManager != coordinatesSystemManager)
            {
                var characters = GetCharacters(coordinatesSystemManager);
                if (!Effects.IsNullOrEmpty()) foreach (var effect in Effects) effect.Apply(characters, coordinatesSystemManager);
                Cache = (characters, coordinatesSystemManager);
                coordinatesSystemManager.Changed += (sender, e) => Cache = default;
            }
            var chars = Cache.Characters.CloneCharacters().ToArray();
            if (applyTransforms) foreach (var character in chars) character.ApplyTransforms();
            return chars;
        }
    }
}
