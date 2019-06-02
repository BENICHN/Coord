using BenLib.Standard;
using System.Collections.Generic;
using System.Windows;

namespace Coord
{
    public class StrokeThicknessCharacterEffect : CharacterEffect, ICoordEditable
    {
        IEnumerable<(string Description, DependencyProperty Property)> ICoordEditable.Properties
        {
            get
            {
                yield return ("StrokeThickness", StrokeThicknessProperty);
                yield return ("Progress", ProgressProperty);
                yield return ("WithTransforms", WithTransformsProperty);
            }
        }

        public double StrokeThickness { get => (double)GetValue(StrokeThicknessProperty); set => SetValue(StrokeThicknessProperty, value); }
        public static readonly DependencyProperty StrokeThicknessProperty = CreateProperty<double>(true, true, "StrokeThickness", typeof(StrokeThicknessCharacterEffect));

        protected override void ApplyCore(IReadOnlyCollection<Character> characters, Interval<int> interval, in ReadOnlyCoordinatesSystemManager coordinatesSystemManager) => characters.SubCollection(interval, true).ForEach((character, i) => ApplyOn(character, StrokeThickness, EasedProgress.Get(i, RealLength)));

        public static void ApplyOn(Character character, double strokeThickness, double progress)
        {
            var stroke = character.Stroke;
            if (stroke != null) character.Stroke = stroke.EditFreezable(pen => pen.Thickness = Num.Interpolate(stroke.Thickness, strokeThickness, progress));
        }
    }
}
