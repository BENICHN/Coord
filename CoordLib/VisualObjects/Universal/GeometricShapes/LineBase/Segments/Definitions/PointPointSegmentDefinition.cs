using System.Windows;

namespace Coord
{
    /// <summary>
    /// Détermine l'équation et les extrémités d'un segment délimité par deux <see cref="PointVisualObject"/>
    /// </summary>
    public class PointPointSegmentDefinition : SegmentDefinition
    {
        protected override Freezable CreateInstanceCore() => new PointPointSegmentDefinition();

        public new PointVisualObject Start { get => (PointVisualObject)GetValue(StartProperty); set => SetValue(StartProperty, value); }
        public static readonly DependencyProperty StartProperty = CreateProperty<PointPointSegmentDefinition, PointVisualObject>(true, true, true, "Start");

        public new PointVisualObject End { get => (PointVisualObject)GetValue(EndProperty); set => SetValue(EndProperty, value); }
        public static readonly DependencyProperty EndProperty = CreateProperty<PointPointSegmentDefinition, PointVisualObject>(true, true, true, "End");

        protected override void OnChanged() => (base.Start, base.End) = (Start?.Definition.InPoint ?? new Point(double.NaN, double.NaN), End?.Definition.InPoint ?? new Point(double.NaN, double.NaN));
    }
}
