using BenLib.WPF;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using static System.Math;

namespace Coord
{
    public class Polygon
    {
        public Polygon(params Point[] points) => Points = points;

        public Point[] Points { get; private set; }

        public Vector VectorBetween(int point1, int point2) => Points[point1] - Points[point2];

        public void RotateAt(double angle, Point center)
        {
            double c = Cos(angle);
            double s = Sin(angle);
            Points = Points.Select(point =>
            {
                var vector = point - center;
                double x = vector.X;
                double y = vector.Y;
                return point + new Vector(x * (c - 1) - y * s, x * s + y * (c - 1));
            }).ToArray();
        }

        public void Translate(Vector vector) => Points = Points.Select(point => point + vector).ToArray();

        public Polygon ComputeOutCoordinates(ReadOnlyCoordinatesSystemManager coordinatesSystemManager) => new Polygon(Points.Select(point => coordinatesSystemManager.ComputeOutCoordinates(point)).ToArray());

        public PathGeometry ToGeometry() => GeometryHelper.GetCurve(Points, true, false);

        public Point this[int index] => Points[index];
    }
}
