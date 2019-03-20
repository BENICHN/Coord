using BenLib;
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
        private IntInterval m_textInterval;
        private VisualObject m_text;
        private RectPoint m_rectPoint;
        private bool m_translateX;
        private bool m_translateY;

        public InsertCharacterEffect(IntInterval interval, IntInterval boundsInterval, VisualObject text, IntInterval textInterval, Progress progress, params SynchronizedProgress[] synchronizedProgresses) : this(interval, boundsInterval, text, textInterval, progress, true, true, true, default, synchronizedProgresses) { }
        public InsertCharacterEffect(IntInterval interval, IntInterval boundsInterval, VisualObject text, IntInterval textInterval, Progress progress, bool withTransforms, params SynchronizedProgress[] synchronizedProgresses) : this(interval, boundsInterval, text, textInterval, progress, withTransforms, true, true, default, synchronizedProgresses) { }
        public InsertCharacterEffect(IntInterval interval, IntInterval boundsInterval, VisualObject text, IntInterval textInterval, Progress progress, bool translateX, bool translateY, RectPoint rectPoint, params SynchronizedProgress[] synchronizedProgresses) : this(interval, boundsInterval, text, textInterval, progress, true, translateX, translateY, rectPoint, synchronizedProgresses) { }
        public InsertCharacterEffect(IntInterval interval, IntInterval boundsInterval, VisualObject text, IntInterval textInterval, Progress progress, bool withTransforms, bool translateX, bool translateY, RectPoint rectPoint, params SynchronizedProgress[] synchronizedProgresses) : base(interval, progress, withTransforms, synchronizedProgresses)
        {
            BoundsInterval = boundsInterval;
            Text = text;
            TextInterval = textInterval;
            TranslateX = translateX;
            TranslateY = translateY;
            RectPoint = rectPoint;
        }

        private IntInterval m_boundsInterval;
        public IntInterval BoundsInterval
        {
            get => m_boundsInterval;
            set
            {
                m_boundsInterval = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// <see cref="VisualObject"/> dans lequel insérer les <see cref="Character"/> de la collection
        /// </summary>
        public VisualObject Text
        {
            get => m_text;
            set
            {
                UnRegister(m_text);
                m_text = value;
                Register(m_text);
                NotifyChanged();
            }
        }

        public IntInterval TextInterval
        {
            get => m_textInterval;
            set
            {
                m_textInterval = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Point du <see cref="System.Windows.Media.GeometryGroup"/> formé par les <see cref="Character"/> la sous-collection de <see cref="Character"/> de <see cref="Text"/>
        /// </summary>
        public RectPoint RectPoint
        {
            get => m_rectPoint;
            set
            {
                m_rectPoint = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Indique si les <see cref="Character"/> sont translatés horizontalement
        /// </summary>
        public bool TranslateX
        {
            get => m_translateX;
            set
            {
                m_translateX = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Indique si les <see cref="Character"/> sont translatés verticalement
        /// </summary>
        public bool TranslateY
        {
            get => m_translateY;
            set
            {
                m_translateY = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Applique l'effet à une collection de <see cref="Character"/>
        /// </summary>
        /// <param name="characters">Collection de <see cref="Character"/> sur qui appliquer l'effet</param>
        protected override void ApplyCore(IReadOnlyCollection<Character> characters, ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            if (Text == null) return;

            bool withTransforms = WithTransforms;
            var chars = characters.SubCollection(BoundsInterval.IsNullOrEmpty() ? Interval : BoundsInterval, true).ToArray();

            var diff = withTransforms ?
                RectPoint.GetPoint(Text.GetTransformedCharacters(coordinatesSystemManager, true).SubCollection(TextInterval, true).Geometry().Bounds) - RectPoint.GetPoint(chars.Geometry().Bounds) :
                RectPoint.GetPoint(Text.GetCharacters(coordinatesSystemManager).SubCollection(TextInterval, true).Geometry().Bounds) - RectPoint.GetPoint(chars.Geometry().Bounds);

            if (!TranslateX) diff.X = 0;
            if (!TranslateY) diff.Y = 0;

            characters.SubCollection(Interval, true).Translate(diff, EasedProgress).Enumerate();
        }

        /// <summary>
        /// Crée un une copie des valeurs de l'instance actuelle de <see cref="InsertCharacterEffect"/>
        /// </summary>
        /// <returns>Copie des valeurs de l'instance actuelle</returns>
        public override CharacterEffect Clone() => new InsertCharacterEffect(Interval, BoundsInterval, Text, TextInterval, Progress, WithTransforms, TranslateX, TranslateY, RectPoint);
    }
}
