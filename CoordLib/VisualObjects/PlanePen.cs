using BenLib.WPF;
using System;
using System.Windows.Media;

namespace Coord
{
    public class PlanePen
    {
        public PlanePen(Pen pen, bool inPen)
        {
            Pen = pen;
            In = inPen;
        }
        public PlanePen(Brush brush, double thickness, bool inPen = false)
        {
            Pen = new Pen(brush, thickness);
            In = inPen;
        }

        public bool In { get; set; }
        public Pen Pen { get; set; }

        public PenLineJoin LineJoin { get => Pen.LineJoin; set => Pen.LineJoin = value; }
        public PenLineCap DashCap { get => Pen.DashCap; set => Pen.DashCap = value; }
        public PenLineCap EndLineCap { get => Pen.EndLineCap; set => Pen.EndLineCap = value; }
        public PenLineCap StartLineCap { get => Pen.StartLineCap; set => Pen.StartLineCap = value; }
        public DashStyle DashStyle { get => Pen.DashStyle; set => Pen.DashStyle = value; }
        public Brush Brush { get => Pen.Brush; set => Pen.Brush = value; }
        public double MiterLimit { get => Pen.MiterLimit; set => Pen.MiterLimit = value; }
        public double Thickness { get => Pen.Thickness; set => Pen.Thickness = value; }

        public bool CanFreeze => Pen.CanFreeze;

        public Pen GetOutPen(ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            if (Pen?.CloneCurrentValue() is Pen pen)
            {
                if (In) pen.Thickness *= coordinatesSystemManager.WidthRatio;
                return pen;
            }
            else return null;
        }
        public PlanePen CloneCurrentValue() => new PlanePen(Pen?.CloneCurrentValue(), In);
        public void Freeze() => Pen?.Freeze();
        public PlanePen EditFreezable(Action<Pen> edition) => new PlanePen(Pen?.EditFreezable(edition), In);

        public static implicit operator Pen(PlanePen planePen) => planePen?.Pen;
        public static implicit operator PlanePen(Pen pen) => pen == null ? null : new PlanePen(pen, false);
    }

    public static partial class Extensions
    {
        public static bool IsNull(this PlanePen planePen) => planePen == null || planePen.Pen == null;
    }
}
