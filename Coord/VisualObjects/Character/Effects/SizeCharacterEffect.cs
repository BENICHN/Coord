using BenLib;
using System.Collections.Generic;
using System.Windows;

namespace Coord
{
    /// <summary>
    /// Met progressivement à l'échelle une sous-collection de <see cref="Character"/> au point spécifié
    /// </summary>
    public class SizeCharacterEffect : CharacterEffect
    {
        private Size m_newSize;
        private RectPoint m_rectPoint;
        private bool m_scaleX;
        private bool m_scaleY;

        public SizeCharacterEffect(IntInterval interval, Size newSize, RectPoint rectPoint, Progress progress, params SynchronizedProgress[] synchronizedProgresses) : this(interval, newSize, rectPoint, progress, true, true, true, synchronizedProgresses) { }
        public SizeCharacterEffect(IntInterval interval, Size newSize, RectPoint rectPoint, Progress progress, bool scaleX, bool scaleY, params SynchronizedProgress[] synchronizedProgresses) : this(interval, newSize, rectPoint, progress, scaleX, scaleY, true, synchronizedProgresses) { }
        public SizeCharacterEffect(IntInterval interval, Size newSize, RectPoint rectPoint, Progress progress, bool scaleX, bool scaleY, bool withTransforms, params SynchronizedProgress[] synchronizedProgresses) : base(interval, progress, withTransforms, synchronizedProgresses)
        {
            NewSize = newSize;
            RectPoint = rectPoint;
            ScaleX = scaleX;
            ScaleY = scaleY;
        }

        /// <summary>
        /// Dimensions que le <see cref="System.Windows.Media.GeometryGroup"/> formé par les <see cref="Character"/> de la sous-collection doit avoir
        /// </summary>
        public Size NewSize
        {
            get => m_newSize;
            set
            {
                m_newSize = value;
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

        public bool ScaleX
        {
            get => m_scaleX;
            set
            {
                m_scaleX = value;
                NotifyChanged();
            }
        }

        public bool ScaleY
        {
            get => m_scaleY;
            set
            {
                m_scaleY = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Applique l'effet à une collection de <see cref="Character"/>
        /// </summary>
        /// <param name="characters">Collection de <see cref="Character"/> sur qui appliquer l'effet</param>
        protected override void ApplyCore(IReadOnlyCollection<Character> characters, CoordinatesSystemManager coordinatesSystemManager)
        {
            var progress = EasedProgress.Value;
            var chars = characters.SubCollection(Interval);
            var bounds = chars.Geometry().Bounds;
            var size = bounds.Size;
            var newSize = new Size(size.Width + (NewSize.Width - size.Width) * progress, size.Height + (NewSize.Height - size.Height) * progress);

            var scaleCenter = RectPoint.GetPoint(bounds);
            chars.ScaleAt(ScaleX ? newSize.Width / size.Width : 1, ScaleY ? newSize.Height / size.Height : 1, scaleCenter, 1.0).Enumerate();
        }

        /// <summary>
        /// Crée un une copie des valeurs de l'instance actuelle de <see cref="SizeCharacterEffect"/>
        /// </summary>
        /// <returns>Copie des valeurs de l'instance actuelle</returns>
        public override CharacterEffect Clone() => new SizeCharacterEffect(Interval, NewSize, RectPoint, Progress);
    }
}
