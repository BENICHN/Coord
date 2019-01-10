using BenLib;

namespace Coord
{
    /// <summary>
    /// Détermine l'équation et les extrémités d'un segment délimité par deux <see cref="PointVisualObject"/>
    /// </summary>
    public class PointPointSegmentDefinition : SegmentDefinition
    {
        public PointPointSegmentDefinition(PointVisualObject start, PointVisualObject end)
        {
            Start = start;
            End = end;
        }

        /// <summary>
        /// Première extrémité du segment
        /// </summary>
        public new PointVisualObject Start
        {
            get => base.Start;
            set
            {
                base.Start = value;
                RegisterCompute(base.Start);
                ComputeAndNotifyChanged();
            }
        }

        /// <summary>
        /// Seconde extrémité du segment
        /// </summary>
        public new PointVisualObject End
        {
            get => base.End;
            set
            {
                base.End = value;
                RegisterCompute(base.End);
                ComputeAndNotifyChanged();
            }
        }

        /// <summary>
        /// Calcule et met en cache les propriétés fondamentales du segment du plan
        /// </summary>
        protected override void Compute()
        {
            if (Start == null || End == null) return;
            Equation = LinearEquation.FromPoints(Start, End);
        }
    }
}
