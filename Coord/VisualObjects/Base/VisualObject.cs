using BenLib;
using BenLib.WPF;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
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

        public static Pen SelectionStroke { get; } = new Pen(FlatBrushes.Asbestos, 3);

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

        public abstract string Type { get; }

        private IntInterval m_selected = IntInterval.EmptySet;
        public IntInterval Selected
        {
            get => m_selected;
            set
            {
                m_selected = value;
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
        /// Indique si l'objet a changé depuis le dernier appel de <see cref="Render(DrawingContext, ReadOnlyCoordinatesSystemManager)"/>
        /// </summary>
        public bool IsChanged { get; private set; }

        protected (IReadOnlyCollection<Character> Characters, ReadOnlyCoordinatesSystemManager ReadOnlyCoordinatesSystemManager) Cache;

        public IEnumerable<HitTestCharacterResult> HitTestCache(Point point)
        {
            if (Cache.Characters != null)
            {
                int i = 0;
                foreach (var character in Cache.Characters)
                {
                    bool fill = character.Fill != null && character.Fill.Opacity > 0 && character.Fill != Brushes.Transparent;
                    bool stroke = character.Stroke != null && character.Stroke.Thickness > 0 && character.Stroke.Brush != null && character.Stroke.Brush.Opacity > 0 && character.Stroke.Brush != Brushes.Transparent;
                    bool transformed = character.Transformed;
                    if (!transformed) character.ApplyTransforms();
                    if (fill && character.Geometry.FillContains(point) || stroke && character.Geometry.StrokeContains(character.Stroke, point)) yield return new HitTestCharacterResult(this, i, character);
                    if (!transformed) character.ReleaseTransforms();
                    i++;
                }
            }
        }

        public IEnumerable<HitTestCharacterResult> HitTestCache(Rect rect)
        {
            if (Cache.Characters != null)
            {
                var rectangle = new RectangleGeometry(rect);
                int i = 0;
                foreach (var character in Cache.Characters)
                {
                    bool transformed = character.Transformed;
                    if (!transformed) character.ApplyTransforms();
                    if (rectangle.FillContainsFigure(character.Geometry) || GeometryHelper.GetIntersectionPoints(rectangle, character.Geometry).Length > 0) yield return new HitTestCharacterResult(this, i, character);
                    if (!transformed) character.ReleaseTransforms();
                    i++;
                }
            }
        }

        /// <summary>
        /// Dessine dans un <see cref="Plane"/> et définit <see cref="IsChanged"/> à <see langword="false"/>
        /// </summary>
        /// <param name="drawingContext">Dessine dans le <see cref="DrawingVisual"/> associé à l'objet</param>
        /// <param name="coordinatesSystemManager">Système de coordonnées du <see cref="Plane"/></param>
        public void Render(DrawingContext drawingContext, ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            RenderCore(drawingContext, coordinatesSystemManager);
            IsChanged = false;
        }

        /// <summary>
        /// Dessine dans un <see cref="Plane"/>
        /// </summary>
        /// <param name="drawingContext">Dessine dans le <see cref="DrawingVisual"/> associé à l'objet</param>
        /// <param name="coordinatesSystemManager">Système de coordonnées du <see cref="Plane"/></param>
        protected virtual void RenderCore(DrawingContext drawingContext, ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            var characters = GetTransformedCharacters(coordinatesSystemManager, false);
            foreach (var character in characters) drawingContext.DrawCharacter(character);
        }

        public abstract IReadOnlyCollection<Character> GetCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager);
        public virtual IReadOnlyCollection<Character> GetTransformedCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager, bool applyTransforms)
        {
            if (Cache.Characters == null || Cache.ReadOnlyCoordinatesSystemManager != coordinatesSystemManager)
            {
                var characters = GetCharacters(coordinatesSystemManager);
                if (!Effects.IsNullOrEmpty()) foreach (var effect in Effects) effect.Apply(characters, coordinatesSystemManager);
                Cache = (characters, coordinatesSystemManager);
            }
            var chars = Cache.Characters.CloneCharacters().ToArray();
            if (applyTransforms) foreach (var character in chars) character.ApplyTransforms();
            foreach (var character in chars.SubCollection(Selected, true)) character.Stroke = SelectionStroke;
            return chars;
        }
    }
}
