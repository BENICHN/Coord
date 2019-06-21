using BenLib.WPF;
using System.Windows;
using System.Windows.Media;

namespace Coord
{
    public abstract class Arrow : NotifyObject
    {
        public double Length { get => (double)GetValue(LengthProperty); set => SetValue(LengthProperty, value); }
        public static readonly DependencyProperty LengthProperty = CreateProperty<Arrow, double>(true, true, true, "Length", 30);

        public double Width { get => (double)GetValue(WidthProperty); set => SetValue(WidthProperty, value); }
        public static readonly DependencyProperty WidthProperty = CreateProperty<Arrow, double>(true, true, true, "Width", 10);

        public bool In { get => (bool)GetValue(InProperty); set => SetValue(InProperty, value); }
        public static readonly DependencyProperty InProperty = CreateProperty<Arrow, bool>(true, true, true, "In");

        public abstract Geometry GetOutGeometry(Point outAnchorPoint, Vector arrow, ArrowEnd arrowEnd);
        public abstract Geometry GetInGeometry(Point outAnchorPoint, Vector arrow, ArrowEnd arrowEnd, ReadOnlyCoordinatesSystemManager coordinatesSystemManager);

        /// <summary>
        /// Calcule deux vecteurs orthogonaux définissant une flèche
        /// </summary>
        /// <param name="arrow">Vecteur représenté</param>
        /// <param name="arrowLength">Longueur à l'écran de la flèche</param>
        /// <param name="arrowWidth">Largeur à l'écran de la flèche</param>
        public static (Vector Collinear, Vector Orthogonal) GetArrowEndVectors(Vector arrow, double arrowLength, double arrowWidth)
        {
            var unitVector = arrow / arrow.Length;
            var scaledVector = unitVector * arrowLength;
            var orthogonalVector = new Vector(-unitVector.Y, unitVector.X) * arrowWidth;

            return (scaledVector, orthogonalVector);
        }
    }

    public class TriangleArrow : Arrow
    {
        protected override Freezable CreateInstanceCore() => new TriangleArrow();

        public bool Closed { get => (bool)GetValue(ClosedProperty); set => SetValue(ClosedProperty, value); }
        public static readonly DependencyProperty ClosedProperty = CreateProperty<TriangleArrow, bool>(true, true, true, "Closed", true);

        public override Geometry GetInGeometry(Point inAnchorPoint, Vector inArrow, ArrowEnd arrowEnd, ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            if (arrowEnd == ArrowEnd.None) return Geometry.Empty;

            var (collinear, orthogonal) = GetArrowEndVectors(inArrow, Length, Width);

            switch (arrowEnd)
            {
                case ArrowEnd.Start: return DrawStartArrow();
                case ArrowEnd.End: return DrawEndArrow();
                case ArrowEnd.Both: return GeometryHelper.Group(DrawStartArrow(), DrawEndArrow());
                default: return Geometry.Empty;
            }

            Geometry DrawStartArrow()
            {
                var startArrowPoint = inAnchorPoint + collinear;
                var startArrowPoint1 = startArrowPoint + orthogonal;
                var startArrowPoint2 = startArrowPoint - orthogonal;
                return GeometryHelper.GetCurve(new[] { coordinatesSystemManager.ComputeOutCoordinates(inAnchorPoint), coordinatesSystemManager.ComputeOutCoordinates(startArrowPoint1), coordinatesSystemManager.ComputeOutCoordinates(startArrowPoint2) }, Closed, false);
            }

            Geometry DrawEndArrow()
            {
                var endPoint = inAnchorPoint + inArrow;
                var endArrowPoint = endPoint - collinear;
                var endArrowPoint1 = endArrowPoint + orthogonal;
                var endArrowPoint2 = endArrowPoint - orthogonal;
                return GeometryHelper.GetCurve(new[] { coordinatesSystemManager.ComputeOutCoordinates(endPoint), coordinatesSystemManager.ComputeOutCoordinates(endArrowPoint1), coordinatesSystemManager.ComputeOutCoordinates(endArrowPoint2) }, Closed, false);
            }
        }

        public override Geometry GetOutGeometry(Point outAnchorPoint, Vector outArrow, ArrowEnd arrowEnd)
        {
            if (arrowEnd == ArrowEnd.None) return Geometry.Empty;

            var (collinear, orthogonal) = GetArrowEndVectors(outArrow, Length, Width);

            switch (arrowEnd)
            {
                case ArrowEnd.Start: return DrawStartArrow();
                case ArrowEnd.End: return DrawEndArrow();
                case ArrowEnd.Both: return GeometryHelper.Group(DrawStartArrow(), DrawEndArrow());
                default: return Geometry.Empty;
            }

            Geometry DrawStartArrow()
            {
                var startArrowPoint = outAnchorPoint + collinear;
                var startArrowPoint1 = startArrowPoint + orthogonal;
                var startArrowPoint2 = startArrowPoint - orthogonal;
                return GeometryHelper.GetCurve(new[] { outAnchorPoint, startArrowPoint1, startArrowPoint2 }, Closed, false);
            }

            Geometry DrawEndArrow()
            {
                var endPoint = outAnchorPoint + outArrow;
                var endArrowPoint = endPoint - collinear;
                var endArrowPoint1 = endArrowPoint + orthogonal;
                var endArrowPoint2 = endArrowPoint - orthogonal;
                return GeometryHelper.GetCurve(new[] { endPoint, endArrowPoint1, endArrowPoint2 }, Closed, false);
            }
        }
    }
}
