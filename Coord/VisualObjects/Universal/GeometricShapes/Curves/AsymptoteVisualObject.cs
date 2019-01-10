using BenLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Coord
{
    public class AsymptoteVisualObject : VisualObject
    {
        private Func<double, double> m_function;
        public Func<double, double> Function
        {
            get => m_function;
            set
            {
                m_function = value;
                NotifyChanged();
            }
        }

        private double m_x;
        public double X
        {
            get => m_x;
            set
            {
                m_x = value;
                NotifyChanged();
            }
        }

        private double m_length;
        public double Length
        {
            get => m_length;
            set
            {
                m_length = value;
                NotifyChanged();
            }
        }

        private Pen m_diffStroke;

        public AsymptoteVisualObject(Func<double, double> function, double x, double length)
        {
            Function = function;
            X = x;
            Length = length;
        }

        public Pen DiffStroke
        {
            get => m_diffStroke;
            set
            {
                m_diffStroke = value;
                Register(value);
                NotifyChanged();
            }
        }

        private double m_pointsRadius;
        public double PointsRadius
        {
            get => m_pointsRadius;
            set
            {
                m_pointsRadius = value;
                NotifyChanged();
            }
        }

        private Brush m_pointsFill;
        public Brush PointsFill
        {
            get => m_pointsFill;
            set
            {
                m_pointsFill = value;
                Register(value);
                NotifyChanged();
            }
        }

        private Pen m_pointsStroke;
        public Pen PointsStroke
        {
            get => m_pointsStroke;
            set
            {
                m_pointsStroke = value;
                Register(value);
                NotifyChanged();
            }
        }

        public static Character[] GetCharacters(CoordinatesSystemManager coordinatesSystemManager, Pen stroke, Pen diffStroke, (Brush fill, Pen stroke, double radius) points, Func<double, double> function, double x, double length)
        {
            var point1i = new Point(x, function(x));
            var point1 = coordinatesSystemManager.ComputeOutCoordinates(point1i);
            var point2i = new Point(x + length, function(x + length));
            var point2 = coordinatesSystemManager.ComputeOutCoordinates(point2i);
            var line = LinearEquation.FromPoints(point1i, point2i);
            var (start, end) = LineVisualObject.GetEndpoints(line, coordinatesSystemManager);
            var point3 = -line.A / line.B > 0 ? new Point(point2.X, point1.Y) : new Point(point1.X, point2.Y);

            return new[]
            {
                Character.Line(coordinatesSystemManager.ComputeOutCoordinates(start), coordinatesSystemManager.ComputeOutCoordinates(end), null, stroke),
                Character.Line(point1, point3, null, diffStroke),
                Character.Line(point3, point2, null, diffStroke),
                Character.Ellipse(point1, points.radius, points.radius, points.fill, points.stroke),
                Character.Ellipse(point2, points.radius, points.radius, points.fill, points.stroke)
            };
        }

        public override IReadOnlyCollection<Character> GetCharacters(CoordinatesSystemManager coordinatesSystemManager) => GetCharacters(coordinatesSystemManager, Stroke, DiffStroke, (PointsFill, PointsStroke, PointsRadius), Function, X, Length);
    }
}
