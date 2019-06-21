using BenLib.Framework;
using System;
using System.Windows;
using System.Windows.Media;

namespace Coord
{
    internal class MathRectValueInterpolationHelper : ValueInterpolationHelper<MathRect> { protected override MathRect InterpolateCore(MathRect start, MathRect end, double progress) => base.InterpolateCore(start, end, progress); }
    
    /// <summary>
    /// Décrit la largeur, la hauteur et l'emplacement d'un rectangle dans un repère orthonormé standard.
    /// </summary>
    public readonly struct MathRect : IEquatable<MathRect>
    {
        static MathRect() => ValueInterpolationHelper<MathRect>.Default = new MathRectValueInterpolationHelper();

        public MathRect(Size size)
        {
            X = 0;
            Y = 0;
            Width = size.Width;
            Height = size.Height;
        }

        public MathRect(Point location, Size size)
        {
            X = location.X;
            Y = location.Y;
            Width = size.Width;
            Height = size.Height;
        }
        public MathRect(Point bottomLeft, Point topRight)
        {
            X = bottomLeft.X;
            Y = bottomLeft.Y;
            Width = topRight.X - X;
            Height = topRight.Y - Y;
        }
        public MathRect(Point point, Vector vector)
        {
            X = point.X;
            Y = point.Y;
            Width = vector.X;
            Height = vector.Y;
        }

        public MathRect(double x, double y, double width, double height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public double X { get; }
        public double Y { get; }
        public double Width { get; }
        public double Height { get; }

        public double Left => X;
        public double Bottom => Y;
        public double Right => X + Width;
        public double Top => Y + Height;

        public Point BottomLeft => new Point(X, Y);
        public Point TopLeft => new Point(X, Top);
        public Point TopRight => new Point(Right, Top);
        public Point BottomRight => new Point(Right, Y);

        public Size Size => new Size(Width, Height);
        public Point Location => new Point(X, Y);

        public MathRect Move(Vector vector) => new MathRect(X - vector.X, Y + vector.Y, Width, Height);

        public bool WidthContains(double value) => Left <= value && value <= Right;
        public bool HeightContains(double value) => Bottom <= value && value <= Top;

        public bool WidthContainsRange(double rangeBeginning, double rangeEnd, bool allRange) => allRange ? Left <= rangeBeginning && rangeEnd <= Right : Right > rangeBeginning && Left < rangeEnd;
        public bool HeightContainsRange(double rangeBeginning, double rangeEnd, bool allRange) => allRange ? Bottom <= rangeBeginning && rangeEnd <= Top : Bottom > rangeBeginning && Top < rangeEnd;

        public override string ToString() => $"{X};{Y};{Width};{Height}";
        public override bool Equals(object obj) => obj is MathRect rect && Equals(rect);
        public bool Equals(MathRect other) => X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;

        public override int GetHashCode()
        {
            int hashCode = 466501756;
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            hashCode = hashCode * -1521134295 + Width.GetHashCode();
            hashCode = hashCode * -1521134295 + Height.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(MathRect left, MathRect right) => left.Equals(right);
        public static bool operator !=(MathRect left, MathRect right) => !(left == right);
    }

    public static partial class Extensions
    {
        public static bool WidthContains(this Rect rect, double value) => rect.Left <= value && value <= rect.Right;
        public static bool HeightContains(this Rect rect, double value) => rect.Top <= value && value <= rect.Bottom;
        public static Point Center(this Rect rect) => new Point((rect.Left + rect.Right) / 2, (rect.Top + rect.Bottom) / 2);
        public static Matrix To(this Rect source, Rect target)
        {
            var offset = target.TopLeft - source.TopLeft;
            double scaleX = target.Width / source.Width;
            double scaleY = target.Height / source.Height;
            var result = new Matrix();
            result.Translate(offset.X, offset.Y);
            result.ScaleAt(scaleX, scaleY, source.Left + offset.X, source.Top + offset.Y);
            return result;
        }

        public static bool WidthContainsRange(this Rect rect, double rangeBeginning, double rangeEnd, bool allRange) => allRange ? rect.Left <= rangeBeginning && rangeEnd <= rect.Right : rect.Right > rangeBeginning && rect.Left < rangeEnd;
        public static bool HeightContainsRange(this Rect rect, double rangeBeginning, double rangeEnd, bool allRange) => allRange ? rect.Top <= rangeBeginning && rangeEnd <= rect.Top : rect.Bottom > rangeBeginning && rect.Top < rangeEnd;
        public static Point Center(this MathRect rect) => new Point((rect.Left + rect.Right) / 2, (rect.Top + rect.Bottom) / 2);
        public static Matrix To(this MathRect source, MathRect target)
        {
            var offset = target.BottomLeft - source.BottomLeft;
            double scaleX = target.Width / source.Width;
            double scaleY = target.Height / source.Height;
            var result = new Matrix();
            result.Translate(offset.X, offset.Y);
            result.ScaleAt(scaleX, scaleY, source.Left + offset.X, source.Bottom + offset.Y);
            return result;
        }
    }
}
