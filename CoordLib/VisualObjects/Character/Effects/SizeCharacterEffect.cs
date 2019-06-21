using BenLib.Standard;
using System.Collections.Generic;
using System.Windows;

namespace Coord
{
    /// <summary>
    /// Met progressivement à l'échelle une sous-collection de <see cref="Character"/> au point spécifié
    /// </summary>
    public class SizeCharacterEffect : CharacterEffect
    {
        protected override Freezable CreateInstanceCore() => new SizeCharacterEffect();

        /// <summary>
        /// Dimensions que le <see cref="System.Windows.Media.GeometryGroup"/> formé par les <see cref="Character"/> de la sous-collection doit avoir
        /// </summary>
        public Size Size { get => (Size)GetValue(SizeProperty); set => SetValue(SizeProperty, value); }
        public static readonly DependencyProperty SizeProperty = CreateProperty<SizeCharacterEffect, Size>(true, true, true, "Size");

        /// <summary>
        /// Point du <see cref="System.Windows.Media.GeometryGroup"/> formé par les <see cref="Character"/> de la sous-collection où appliquer la mise à l'échelle
        /// </summary>
        public RectPoint RectPoint { get => (RectPoint)GetValue(RectPointProperty); set => SetValue(RectPointProperty, value); }
        public static readonly DependencyProperty RectPointProperty = CreateProperty<SizeCharacterEffect, RectPoint>(true, true, true, "RectPoint");

        public bool ScaleX { get => (bool)GetValue(ScaleXProperty); set => SetValue(ScaleXProperty, value); }
        public static readonly DependencyProperty ScaleXProperty = CreateProperty<SizeCharacterEffect, bool>(true, true, true, "ScaleX");

        public bool ScaleY { get => (bool)GetValue(ScaleYProperty); set => SetValue(ScaleYProperty, value); }
        public static readonly DependencyProperty ScaleYProperty = CreateProperty<SizeCharacterEffect, bool>(true, true, true, "ScaleY");

        /// <summary>
        /// Applique l'effet à une collection de <see cref="Character"/>
        /// </summary>
        /// <param name="characters">Collection de <see cref="Character"/> sur qui appliquer l'effet</param>
        protected override void ApplyCore(IReadOnlyCollection<Character> characters, Interval<int> interval, in ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            double progress = EasedProgress.Value;
            var chars = characters.SubCollection(interval, true);
            var bounds = chars.Geometry().Bounds;
            var size = bounds.Size;
            var newSize = new Size(size.Width + (Size.Width - size.Width) * progress, size.Height + (Size.Height - size.Height) * progress);

            var scaleCenter = RectPoint.GetPoint(bounds);
            chars.ScaleAt(ScaleX ? newSize.Width / size.Width : 1, ScaleY ? newSize.Height / size.Height : 1, scaleCenter, 1.0).Enumerate();
        }
    }
}
