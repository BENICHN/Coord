using BenLib.Framework;
using BenLib.Standard;
using System.Windows;
using System.Windows.Media;
using static System.Math;

namespace Coord
{
    public abstract class Transform : NotifyObject { public abstract Matrix GetValue(Rect bounds, in ReadOnlyCoordinatesSystemManager coordinatesSystemManager); }

    public class TranslateTransform : Transform
    {
        protected override Freezable CreateInstanceCore() => new TranslateTransform();

        public Vector Offset { get => (Vector)GetValue(OffsetProperty); set => SetValue(OffsetProperty, value); }
        public static readonly DependencyProperty OffsetProperty = CreateProperty<TranslateTransform, Vector>(true, true, true, "Offset");

        public bool In { get => (bool)GetValue(InProperty); set => SetValue(InProperty, value); }
        public static readonly DependencyProperty InProperty = CreateProperty<TranslateTransform, bool>(true, true, true, "In");

        public override Matrix GetValue(Rect bounds, in ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            var offset = In ? coordinatesSystemManager.ComputeOutCoordinates(Offset) : Offset;
            return new Matrix(1, 0, 0, 1, offset.X, offset.Y);
        }
    }

    public class ScaleTransform : Transform
    {
        protected override Freezable CreateInstanceCore() => new ScaleTransform();

        public double ScaleX { get => (double)GetValue(ScaleXProperty); set => SetValue(ScaleXProperty, value); }
        public static readonly DependencyProperty ScaleXProperty = CreateProperty<ScaleTransform, double>(true, true, true, "ScaleX");

        public double ScaleY { get => (double)GetValue(ScaleYProperty); set => SetValue(ScaleYProperty, value); }
        public static readonly DependencyProperty ScaleYProperty = CreateProperty<ScaleTransform, double>(true, true, true, "ScaleY");

        public RectPoint RectPoint { get => (RectPoint)GetValue(RectPointProperty); set => SetValue(RectPointProperty, value); }
        public static readonly DependencyProperty RectPointProperty = CreateProperty<ScaleTransform, RectPoint>(true, true, true, "RectPoint", RectPoint.Center);

        public Point Center { get => (Point)GetValue(CenterProperty); set => SetValue(CenterProperty, value); }
        public static readonly DependencyProperty CenterProperty = CreateProperty<ScaleTransform, Point>(true, true, true, "Center", new Point(double.NaN, double.NaN));

        public bool In { get => (bool)GetValue(InProperty); set => SetValue(InProperty, value); }
        public static readonly DependencyProperty InProperty = CreateProperty<ScaleTransform, bool>(true, true, true, "In");

        public override Matrix GetValue(Rect bounds, in ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            var center = In ? coordinatesSystemManager.ComputeOutCoordinates(Center) : Center;
            if (center.IsNaN()) center = RectPoint.GetPoint(bounds);
            double sX = ScaleX;
            double sY = ScaleY;

            return new Matrix(sX, 0, 0, sY, center.X - sX * center.X, center.Y - sY * center.Y);
        }
    }

    public class RotateTransform : Transform
    {
        protected override Freezable CreateInstanceCore() => new RotateTransform();

        public double Angle { get => (double)GetValue(AngleProperty); set => SetValue(AngleProperty, value); }
        public static readonly DependencyProperty AngleProperty = CreateProperty<RotateTransform, double>(true, true, true, "Angle");

        public RectPoint RectPoint { get => (RectPoint)GetValue(RectPointProperty); set => SetValue(RectPointProperty, value); }
        public static readonly DependencyProperty RectPointProperty = CreateProperty<RotateTransform, RectPoint>(true, true, true, "RectPoint", RectPoint.Center);

        public Point Center { get => (Point)GetValue(CenterProperty); set => SetValue(CenterProperty, value); }
        public static readonly DependencyProperty CenterProperty = CreateProperty<RotateTransform, Point>(true, true, true, "Center", new Point(double.NaN, double.NaN));

        public bool In { get => (bool)GetValue(InProperty); set => SetValue(InProperty, value); }
        public static readonly DependencyProperty InProperty = CreateProperty<RotateTransform, bool>(true, true, true, "In");

        public override Matrix GetValue(Rect bounds, in ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            var center = In ? coordinatesSystemManager.ComputeOutCoordinates(Center) : Center;
            if (center.IsNaN()) center = RectPoint.GetPoint(bounds);

            double angle = Angle % 2 * PI;
            double sin = Sin(angle);
            double cos = Cos(angle);
            double dx = center.X * (1 - cos) + center.Y * sin;
            double dy = center.Y * (1 - cos) - center.X * sin;

            return new Matrix(cos, sin, -sin, cos, dx, dy);
        }
    }

    public class SkewTransform : Transform
    {
        protected override Freezable CreateInstanceCore() => new SkewTransform();

        public double SkewX { get => (double)GetValue(SkewXProperty); set => SetValue(SkewXProperty, value); }
        public static readonly DependencyProperty SkewXProperty = CreateProperty<SkewTransform, double>(true, true, true, "SkewX");

        public double SkewY { get => (double)GetValue(SkewYProperty); set => SetValue(SkewYProperty, value); }
        public static readonly DependencyProperty SkewYProperty = CreateProperty<SkewTransform, double>(true, true, true, "SkewY");

        public override Matrix GetValue(Rect bounds, in ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            double sX = SkewX % 2 * PI;
            double sY = SkewY % 2 * PI;

            return new Matrix(1, Tan(sY), Tan(sX), 1, 0, 0);
        }
    }

    public class MatrixTransform : Transform
    {
        protected override Freezable CreateInstanceCore() => new MatrixTransform();

        public Matrix Matrix { get => (Matrix)GetValue(MatrixProperty); set => SetValue(MatrixProperty, value); }
        public static readonly DependencyProperty MatrixProperty = CreateProperty<MatrixTransform, Matrix>(true, true, true, "Matrix");

        public bool In { get => (bool)GetValue(InProperty); set => SetValue(InProperty, value); }
        public static readonly DependencyProperty InProperty = CreateProperty<MatrixTransform, bool>(true, true, true, "In");

        public override Matrix GetValue(Rect bounds, in ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            var matrix = Matrix;
            if (In)
            {
                var inOffset = new Vector(matrix.OffsetX, matrix.OffsetY);
                var outOffset = coordinatesSystemManager.ComputeOutCoordinates(inOffset);
                (matrix.OffsetX, matrix.OffsetY) = outOffset.Deconstruct();
            }
            return matrix;
        }
    }
}
