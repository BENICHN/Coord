using BenLib.Standard;
using BenLib.WPF;
using System.Collections.Generic;
using System.Windows;

namespace Coord
{
    public class OpacityCharacterEffect : CharacterEffect
    {
        protected override Freezable CreateInstanceCore() => new OpacityCharacterEffect();

        public double FillOpacity { get => (double)GetValue(FillOpacityProperty); set => SetValue(FillOpacityProperty, value); }
        public static readonly DependencyProperty FillOpacityProperty = CreateProperty<OpacityCharacterEffect, double>(true, true, true, "FillOpacity");

        public double StrokeOpacity { get => (double)GetValue(StrokeOpacityProperty); set => SetValue(StrokeOpacityProperty, value); }
        public static readonly DependencyProperty StrokeOpacityProperty = CreateProperty<OpacityCharacterEffect, double>(true, true, true, "StrokeOpacity");

        protected override void ApplyCore(IReadOnlyCollection<Character> characters, Interval<int> interval, in ReadOnlyCoordinatesSystemManager coordinatesSystemManager) => characters.SubCollection(interval, true).ForEach((character, i) => ApplyOn(character, FillOpacity, StrokeOpacity, EasedProgress.Get(i, RealLength)));

        public static void ApplyOn(Character character, double fillOpacity, double strokeOpacity, double progress)
        {
            var fill = character.Fill;
            var stroke = character.Stroke;

            if (!double.IsNaN(fillOpacity) && fill != null) character.Fill = fill.EditFreezable(brush => brush.Opacity = Num.Interpolate(fill.Opacity, fillOpacity, progress));
            if (!double.IsNaN(strokeOpacity) && stroke != null && stroke.Brush != null) character.Stroke = stroke.EditFreezable(pen => pen.Brush = pen.Brush.EditFreezable(brush => brush.Opacity = Num.Interpolate(stroke.Brush.Opacity, strokeOpacity, progress)));
        }
    }
}
