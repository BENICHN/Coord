using BenLib;
using BenLib.WPF;
using System.Collections.Generic;

namespace Coord
{
    public class OpacityCharacterEffect : CharacterEffect
    {
        private double m_fillOpacity;
        private double m_strokeOpacity;

        public OpacityCharacterEffect(IntInterval interval, double fillOpacity, double strokeOpacity, Progress progress, params SynchronizedProgress[] synchronizedProgresses) : this(interval, fillOpacity, strokeOpacity, progress, false, synchronizedProgresses) { }
        public OpacityCharacterEffect(IntInterval interval, double fillOpacity, double strokeOpacity, Progress progress, bool withTransforms, params SynchronizedProgress[] synchronizedProgresses) : base(interval, progress, withTransforms, synchronizedProgresses)
        {
            FillOpacity = fillOpacity;
            StrokeOpacity = strokeOpacity;
        }

        public double FillOpacity
        {
            get => m_fillOpacity;
            set
            {
                m_fillOpacity = value;
                NotifyChanged();
            }
        }
        public double StrokeOpacity
        {
            get => m_strokeOpacity;
            set
            {
                m_strokeOpacity = value;
                NotifyChanged();
            }
        }

        protected override void ApplyCore(IReadOnlyCollection<Character> characters, CoordinatesSystemManager coordinatesSystemManager) => characters.SubCollection(Interval).ForEach((i, character) => ApplyOn(character, FillOpacity, StrokeOpacity, EasedProgress.Get(i, RealLength)));

        public static void ApplyOn(Character character, double fillOpacity, double strokeOpacity, double progress)
        {
            var fill = character.Fill;
            var stroke = character.Stroke;

            if (!double.IsNaN(fillOpacity) && fill != null) character.Fill = fill.Edit(brush => brush.Opacity = Num.Interpolate(fill.Opacity, fillOpacity, progress));
            if (!double.IsNaN(strokeOpacity) && stroke != null && stroke.Brush != null) character.Stroke = stroke.Edit(pen => pen.Brush = pen.Brush.Edit(brush => brush.Opacity = Num.Interpolate(stroke.Brush.Opacity, strokeOpacity, progress)));
        }

        public override CharacterEffect Clone() => new OpacityCharacterEffect(Interval, FillOpacity, StrokeOpacity, Progress, WithTransforms);
    }
}
