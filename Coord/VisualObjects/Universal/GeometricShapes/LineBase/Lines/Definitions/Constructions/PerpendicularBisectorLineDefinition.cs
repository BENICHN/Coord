using BenLib;
using System.Windows;

namespace Coord
{
    /// <summary>
    /// Détermine la médiatrice d'un <see cref="SegmentVisualObject"/>
    /// </summary>
    public class PerpendicularBisectorLineDefinition : LineDefinition
    {
        private SegmentVisualObject m_segment;

        public PerpendicularBisectorLineDefinition(SegmentVisualObject segment) => Segment = segment;
        public PerpendicularBisectorLineDefinition(PointVisualObject start, PointVisualObject end) => Segment = new SegmentVisualObject(new PointPointSegmentDefinition(start, end));

        /// <summary>
        /// Segment du plan
        /// </summary>
        public SegmentVisualObject Segment
        {
            get => m_segment;
            set
            {
                m_segment = value;
                RegisterCompute(m_segment);
                ComputeAndNotifyChanged();
            }
        }

        /// <summary>
        /// Calcule et met en cache les propriétés fondamentales de la droite du plan
        /// </summary>
        protected override void Compute()
        {
            if (Segment == null) return;

            var start = (Point)Segment.Definition.Start;
            var end = (Point)Segment.Definition.End;

            var middle = new Point((start.X + end.X) / 2.0, (start.Y + end.Y) / 2.0);
            var equation = LinearEquation.FromPoints(start, end);

            Equation = LinearEquation.Perpendicular(equation, middle);
        }
    }
}
