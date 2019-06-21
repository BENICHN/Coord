using BenLib.Standard;
using System.Collections.Generic;
using System.Windows;

namespace Coord
{
    /// <summary>
    /// Rétrécit progressivement une sous-collection de <see cref="Character"/> au point spécifié puis translate progressivement les <see cref="Character"/> après cette sous-collection par la différence entre <see cref="BigSize"/> et <see cref="SmallSize"/>
    /// </summary>
    public class MaskCharacterEffect : CharacterEffect
    {
        protected override Freezable CreateInstanceCore() => new MaskCharacterEffect();

        private Vector m_sizeDiff;

        /// <summary>
        /// Dimensions du <see cref="System.Windows.Media.GeometryGroup"/> formé par les <see cref="Character"/> de la sous-collection après le masquage
        /// </summary>
        public Size SmallSize { get => (Size)GetValue(SmallSizeProperty); set => SetValue(SmallSizeProperty, value); }
        public static readonly DependencyProperty SmallSizeProperty = CreateProperty<MaskCharacterEffect, Size>(true, true, true, "SmallSize", (sender, e) => { if (sender is MaskCharacterEffect owner) owner.ComputeSizeDiff(); });

        /// <summary>
        /// Dimensions du <see cref="System.Windows.Media.GeometryGroup"/> formé par les <see cref="Character"/> de la sous-collection avant le masquage
        /// </summary>
        public Size BigSize { get => (Size)GetValue(BigSizeProperty); set => SetValue(BigSizeProperty, value); }
        public static readonly DependencyProperty BigSizeProperty = CreateProperty<MaskCharacterEffect, Size>(true, true, true, "BigSize", (sender, e) => { if (sender is MaskCharacterEffect owner) owner.ComputeSizeDiff(); });

        /// <summary>
        /// Point du <see cref="System.Windows.Media.GeometryGroup"/> formé par les <see cref="Character"/> de la sous-collection où appliquer la mise à l'échelle
        /// </summary>
        public RectPoint RectPoint { get => (RectPoint)GetValue(RectPointProperty); set => SetValue(RectPointProperty, value); }
        public static readonly DependencyProperty RectPointProperty = CreateProperty<MaskCharacterEffect, RectPoint>(true, true, true, "RectPoint");

        private void ComputeSizeDiff() => m_sizeDiff = new Vector(BigSize.Width - SmallSize.Width, BigSize.Height - SmallSize.Height);

        /// <summary>
        /// Applique l'effet à une collection de <see cref="Character"/>
        /// </summary>
        /// <param name="characters">Collection de <see cref="Character"/> sur qui appliquer l'effet</param>
        protected override void ApplyCore(IReadOnlyCollection<Character> characters, Interval<int> interval, in ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            var progress = EasedProgress;

            if (RealLength > 0)
            {
                var chars = characters.SubCollection(interval, true);
                var bounds = chars.Geometry().Bounds;

                var scaleCenter = RectPoint.GetPoint(bounds);
                chars.ScaleAt(1, 1, scaleCenter, progress.ChangeValue(1.0 - progress.Value)).Enumerate();
            }

            characters.SubCollection((interval.Container.End.Next, null), true).Translate(-m_sizeDiff, progress).Enumerate();
        }
    }
}
