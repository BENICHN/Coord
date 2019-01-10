using BenLib.WPF;
using System;
using System.Windows;
using System.Windows.Media;

namespace Coord
{
    public abstract class Arrow : NotifyObject
    {
        private double m_length;
        private double m_width;
        private bool m_in;

        protected Arrow(double length, double width, bool inArrow)
        {
            Length = length;
            Width = width;
            In = inArrow;
        }

        public double Length
        {
            get => m_length;
            set
            {
                m_length = value;
                NotifyChanged();
            }
        }

        public double Width
        {
            get => m_width;
            set
            {
                m_width = value;
                NotifyChanged();
            }
        }

        public bool In
        {
            get => m_in;
            set
            {
                m_in = value;
                NotifyChanged();
            }
        }

        public abstract Geometry GetOutGeometry(Point outAnchorPoint, Vector arrow, ArrowEnd arrowEnd);
        public abstract Geometry GetInGeometry(Point outAnchorPoint, Vector arrow, ArrowEnd arrowEnd, CoordinatesSystemManager coordinatesSystemManager);

        /// <summary>
        /// Calcule deux vecteurs orthogonaux définissant une flèche
        /// </summary>
        /// <param name="outAnchorPoint">Origine du vecteur représenté</param>
        /// <param name="arrow">Vecteur représenté</param>
        /// <param name="arrowLength">Longueur à l'écran de la flèche</param>
        /// <param name="arrowWidth">Largeur à l'écran de la flèche</param>
        /// <returns></returns>
        public static (Vector Collinear, Vector Orthogonal) GetArrowEndVectors(Point outAnchorPoint, Vector arrow, double arrowLength, double arrowWidth)
        {
            var unitVector = arrow / arrow.Length;
            var scaledVector = unitVector * arrowLength;
            var orthogonalVector = new Vector(-unitVector.Y, unitVector.X) * arrowWidth;

            return (scaledVector, orthogonalVector);
        }
    }

    public class TriangleArrow : Arrow
    {
        private bool m_smooth;
        private bool m_closed;
        private double m_smoothValue = 0.75;

        public TriangleArrow(bool closed, bool inArrow, double length = 30, double width = 10) : base(length, width, inArrow) => Closed = closed;
        public TriangleArrow(bool closed, bool smooth, bool inArrow, double length = 30, double width = 10) : base(length, width, inArrow)
        {
            Closed = closed;
            Smooth = smooth;
        }
        public TriangleArrow(bool closed, bool smooth, double smoothValue, bool inArrow, double length = 30, double width = 10) : base(length, width, inArrow)
        {
            Closed = closed;
            Smooth = smooth;
            SmoothValue = smoothValue;
        }

        /// <summary>
        /// Indique si la flèche est lissée
        /// </summary>
        public bool Smooth
        {
            get => m_smooth;
            set
            {
                m_smooth = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Coefficient de lissage de la flèche dans le cas où <see cref="Smooth"/> est <see langword="true"/>
        /// </summary>
        public double SmoothValue
        {
            get => m_smoothValue;
            set
            {
                m_smoothValue = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Indique si la flèche est fermée
        /// </summary>
        public bool Closed
        {
            get => m_closed;
            set
            {
                m_closed = value;
                NotifyChanged();
            }
        }

        public override Geometry GetInGeometry(Point inAnchorPoint, Vector inArrow, ArrowEnd arrowEnd, CoordinatesSystemManager coordinatesSystemManager)
        {
            if (arrowEnd == ArrowEnd.None) return Geometry.Empty;

            var (collinear, orthogonal) = GetArrowEndVectors(inAnchorPoint, inArrow, Length, Width);

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
                return GeometryHelper.GetCurve(new[] { coordinatesSystemManager.ComputeOutCoordinates(inAnchorPoint), coordinatesSystemManager.ComputeOutCoordinates(startArrowPoint1), coordinatesSystemManager.ComputeOutCoordinates(startArrowPoint2) }, Closed, Smooth, SmoothValue);
            }

            Geometry DrawEndArrow()
            {
                var endPoint = inAnchorPoint + inArrow;
                var endArrowPoint = endPoint - collinear;
                var endArrowPoint1 = endArrowPoint + orthogonal;
                var endArrowPoint2 = endArrowPoint - orthogonal;
                return GeometryHelper.GetCurve(new[] { coordinatesSystemManager.ComputeOutCoordinates(endPoint), coordinatesSystemManager.ComputeOutCoordinates(endArrowPoint1), coordinatesSystemManager.ComputeOutCoordinates(endArrowPoint2) }, Closed, Smooth, SmoothValue);
            }
        }

        public override Geometry GetOutGeometry(Point outAnchorPoint, Vector outArrow, ArrowEnd arrowEnd)
        {
            if (arrowEnd == ArrowEnd.None) return Geometry.Empty;

            var (collinear, orthogonal) = GetArrowEndVectors(outAnchorPoint, outArrow, Length, Width);

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
                return GeometryHelper.GetCurve(new[] { outAnchorPoint, startArrowPoint1, startArrowPoint2 }, Closed, Smooth, SmoothValue);
            }

            Geometry DrawEndArrow()
            {
                var endPoint = outAnchorPoint + outArrow;
                var endArrowPoint = endPoint - collinear;
                var endArrowPoint1 = endArrowPoint + orthogonal;
                var endArrowPoint2 = endArrowPoint - orthogonal;
                return GeometryHelper.GetCurve(new[] { endPoint, endArrowPoint1, endArrowPoint2 }, Closed, Smooth, SmoothValue);
            }
        }
    }
}
