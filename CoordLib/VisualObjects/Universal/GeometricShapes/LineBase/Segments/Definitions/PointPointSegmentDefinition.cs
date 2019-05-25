using System.Windows;

namespace Coord
{
    /// <summary>
    /// Détermine l'équation et les extrémités d'un segment délimité par deux <see cref="PointVisualObject"/>
    /// </summary>
    public class PointPointSegmentDefinition : SegmentDefinition
    {
        public new PointVisualObject Start { get => (PointVisualObject)GetValue(StartProperty); set => SetValue(StartProperty, value); }
        public static readonly DependencyProperty StartProperty = CreateProperty<PointVisualObject>(true, true, "Start", typeof(PointPointSegmentDefinition));

        public new PointVisualObject End { get => (PointVisualObject)GetValue(EndProperty); set => SetValue(EndProperty, value); }
        public static readonly DependencyProperty EndProperty = CreateProperty<PointVisualObject>(true, true, "End", typeof(PointPointSegmentDefinition));

        protected override void OnChanged() => (base.Start, base.End) = (Start?.Definition.InPoint ?? new Point(double.NaN, double.NaN), End?.Definition.InPoint ?? new Point(double.NaN, double.NaN));
    }
}
