using BenLib.Standard;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Coord
{
    public class FitCharacterEffect : CharacterEffect
    {
        protected override Freezable CreateInstanceCore() => new FitCharacterEffect();

        public Interval<int> BoundsInterval { get => (Interval<int>)GetValue(BoundsIntervalProperty); set => SetValue(BoundsIntervalProperty, value); }
        public static readonly DependencyProperty BoundsIntervalProperty = CreateProperty<FitCharacterEffect, Interval<int>>(true, true, true, "BoundsInterval");

        public VisualObject VisualObject { get => (VisualObject)GetValue(VisualObjectProperty); set => SetValue(VisualObjectProperty, value); }
        public static readonly DependencyProperty VisualObjectProperty = CreateProperty<FitCharacterEffect, VisualObject>(true, true, true, "VisualObject");

        public bool ScaleX { get => (bool)GetValue(ScaleXProperty); set => SetValue(ScaleXProperty, value); }
        public static readonly DependencyProperty ScaleXProperty = CreateProperty<FitCharacterEffect, bool>(true, true, true, "ScaleX");

        public bool ScaleY { get => (bool)GetValue(ScaleYProperty); set => SetValue(ScaleYProperty, value); }
        public static readonly DependencyProperty ScaleYProperty = CreateProperty<FitCharacterEffect, bool>(true, true, true, "ScaleY");

        protected override void ApplyCore(IReadOnlyCollection<Character> characters, Interval<int> interval, in ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            if (VisualObject == null) return;

            var chars = characters.SubCollection(BoundsInterval.IsNullOrEmpty() ? interval : BoundsInterval, true).ToArray();

            var (from, to) = (chars.Geometry().Bounds.Size, VisualObject.GetTransformedCharacters(coordinatesSystemManager).Geometry().Bounds.Size);

            double scaleX = ScaleX ? to.Width / from.Width : 0;
            double scaleY = ScaleY ? to.Height / from.Height : 0;

            characters.SubCollection(interval, true).Scale(scaleX, scaleY, EasedProgress).Enumerate();
        }
    }
}
