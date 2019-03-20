using System.Windows;

namespace Coord
{
    /// <summary>
    /// Point d'un rectangle
    /// </summary>
    public struct RectPoint
    {
        public RectPoint(double xProgress, double yProgress) : this()
        {
            XProgress = xProgress;
            YProgress = yProgress;
        }

        public double XProgress { get; }
        public double YProgress { get; }

        public bool IsNaN => double.IsNaN(XProgress) || double.IsNaN(YProgress);

        public Point GetPoint(Rect rect) => new Point(rect.Left + (rect.Right - rect.Left) * XProgress, rect.Top + (rect.Bottom - rect.Top) * YProgress);

        public static RectPoint TopLeft { get; } = new RectPoint(0, 0);
        public static RectPoint TopRight { get; } = new RectPoint(1, 0);
        public static RectPoint BottomRight { get; } = new RectPoint(1, 1);
        public static RectPoint BottomLeft { get; } = new RectPoint(0, 1);
        public static RectPoint Left { get; } = new RectPoint(0, 0.5);
        public static RectPoint Top { get; } = new RectPoint(0.5, 1);
        public static RectPoint Right { get; } = new RectPoint(1, 0.5);
        public static RectPoint Bottom { get; } = new RectPoint(0.5, 0);
        public static RectPoint Center { get; } = new RectPoint(0.5, 0.5);
        public static RectPoint NaN { get; } = new RectPoint(double.NaN, double.NaN);

        public override string ToString() => (XProgress, YProgress) switch
        {
            (0, 0) => "TopLeft",
            (1, 0) => "TopRight",
            (1, 1) => "BottomRight",
            (0, 1) => "BottomLeft",
            (0, 0.5) => "Left",
            (0.5, 1) => "Top",
            (1, 0.5) => "Right",
            (0.5, 0) => "Bottom",
            (0.5, 0.5) => "Center",
            _ => IsNaN ? "NaN" : $"{XProgress};{YProgress}",
        };
    }
}
