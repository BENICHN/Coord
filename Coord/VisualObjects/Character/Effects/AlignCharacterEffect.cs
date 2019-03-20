using BenLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Coord
{
    public class AlignCharacterEffect : CharacterEffect
    {
        private VisualObject m_text;
        private RectPoint m_rectPoint;
        private bool m_translateX;
        private bool m_translateY;

        public AlignCharacterEffect(IntInterval interval, VisualObject text, Progress progress, params SynchronizedProgress[] synchronizedProgresses) : this(interval, text, progress, true, true, true, default, synchronizedProgresses) { }
        public AlignCharacterEffect(IntInterval interval, VisualObject text, Progress progress, bool withTransforms, params SynchronizedProgress[] synchronizedProgresses) : this(interval, text, progress, withTransforms, true, true, default, synchronizedProgresses) { }
        public AlignCharacterEffect(IntInterval interval, VisualObject text, Progress progress, bool translateX, bool translateY, RectPoint rectPoint, params SynchronizedProgress[] synchronizedProgresses) : this(interval, text, progress, true, translateX, translateY, rectPoint, synchronizedProgresses) { }
        public AlignCharacterEffect(IntInterval interval, VisualObject text, Progress progress, bool withTransforms, bool translateX, bool translateY, RectPoint rectPoint, params SynchronizedProgress[] synchronizedProgresses) : base(interval, progress, withTransforms, synchronizedProgresses)
        {
            Text = text;
            TranslateX = translateX;
            TranslateY = translateY;
            RectPoint = rectPoint;
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

        public override CharacterEffect Clone() => new AlignCharacterEffect(Interval, Text, Progress, WithTransforms, TranslateX, TranslateY, RectPoint);

        protected override void ApplyCore(IReadOnlyCollection<Character> characters, ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            var chars = characters.SubCollection(Interval, true).ToArray();
            var diff = RectPoint.GetPoint(Text.GetTransformedCharacters(coordinatesSystemManager, true).Geometry().Bounds) - RectPoint.GetPoint(chars.Geometry().Bounds);

            if (!TranslateX) diff.X = 0;
            if (!TranslateY) diff.Y = 0;

            chars.Translate(diff, EasedProgress).Enumerate();
        }
    }
}
