using BenLib;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Coord
{
    /// <summary>
    /// Rétrécit progressivement une sous-collection de <see cref="Character"/> au point spécifié puis translate progressivement les <see cref="Character"/> après cette sous-collection par la différence entre <see cref="BigSize"/> et <see cref="SmallSize"/>
    /// </summary>
    public class MaskCharacterEffect : CharacterEffect
    {
        private Size m_smallSize;
        private Size m_bigSize;
        private Vector m_sizeDiff;
        private RectPoint m_rectPoint;

        public MaskCharacterEffect(IntInterval interval, Size smallSize, Size bigSize, RectPoint rectPoint, Progress progress, params SynchronizedProgress[] synchronizedProgresses) : this(interval, smallSize, bigSize, rectPoint, progress, true, synchronizedProgresses) { }
        public MaskCharacterEffect(IntInterval interval, Size smallSize, Size bigSize, RectPoint rectPoint, Progress progress, bool withTransforms, params SynchronizedProgress[] synchronizedProgresses) : base(interval, progress, withTransforms, synchronizedProgresses)
        {
            BigSize = bigSize;
            SmallSize = smallSize;
            RectPoint = rectPoint;
        }

        private void ComputeSizeDiff() => m_sizeDiff = new Vector(BigSize.Width - SmallSize.Width, BigSize.Height - SmallSize.Height);

        /// <summary>
        /// Dimensions du <see cref="System.Windows.Media.GeometryGroup"/> formé par les <see cref="Character"/> de la sous-collection après le masquage
        /// </summary>
        public Size SmallSize
        {
            get => m_smallSize;
            set
            {
                m_smallSize = value;
                ComputeSizeDiff();
                NotifyChanged();
            }
        }

        /// <summary>
        /// Dimensions du <see cref="System.Windows.Media.GeometryGroup"/> formé par les <see cref="Character"/> de la sous-collection avant le masquage
        /// </summary>
        public Size BigSize
        {
            get => m_bigSize;
            set
            {
                m_bigSize = value;
                ComputeSizeDiff();
                NotifyChanged();
            }
        }

        /// <summary>
        /// Point du <see cref="System.Windows.Media.GeometryGroup"/> formé par les <see cref="Character"/> de la sous-collection où appliquer la mise à l'échelle
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
        /// Applique l'effet à une collection de <see cref="Character"/>
        /// </summary>
        /// <param name="characters">Collection de <see cref="Character"/> sur qui appliquer l'effet</param>
        protected override void ApplyCore(IReadOnlyCollection<Character> characters, ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            var progress = EasedProgress;

            if (RealLength > 0)
            {
                var chars = characters.SubCollection(Interval, true);
                var bounds = chars.Geometry().Bounds;

                var scaleCenter = RectPoint.GetPoint(bounds);
                chars.ScaleAt(1, 1, scaleCenter, progress.ChangeValue(1.0 - progress.Value)).Enumerate();
            }

            characters.Skip(Interval.LastIndex).Translate(-m_sizeDiff, progress).Enumerate();
        }

        /// <summary>
        /// Crée un une copie des valeurs de l'instance actuelle de <see cref="MaskCharacterEffect"/>
        /// </summary>
        /// <returns>Copie des valeurs de l'instance actuelle</returns>
        public override CharacterEffect Clone() => new MaskCharacterEffect(Interval, SmallSize, BigSize, RectPoint, Progress);
    }
}
