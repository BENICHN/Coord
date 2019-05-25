using BenLib.Framework;
using System.Windows;

namespace Coord
{
    /// <summary>
    /// Détermine la médiatrice d'un <see cref="SegmentVisualObject"/>
    /// </summary>
    public class PerpendicularBisectorLineDefinition : LineDefinition
    {
        /// <summary>
        /// Segment du plan
        /// </summary>
        public SegmentVisualObject Segment { get => (SegmentVisualObject)GetValue(SegmentProperty); set => SetValue(SegmentProperty, value); }
        public static readonly DependencyProperty SegmentProperty = CreateProperty<SegmentVisualObject>(true, true, "Segment", typeof(PerpendicularBisectorLineDefinition));

        protected override void OnChanged()
        {
            if (Segment == null) return;

            var start = Segment.Definition.Start;
            var end = Segment.Definition.End;

            var middle = new Point((start.X + end.X) / 2.0, (start.Y + end.Y) / 2.0);
            var equation = LinearEquation.FromPoints(start, end);

            Equation = LinearEquation.Perpendicular(equation, middle);
        }
    }
}
