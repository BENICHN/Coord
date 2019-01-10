using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Coord
{
    public class AxesVisualObject : VisualObject
    {
        private AxesNumbers m_numbers;
        private AxesDirection m_direction;

        public AxesVisualObject(AxesDirection direction, AxesNumbers numbers)
        {
            Direction = direction;
            Numbers = numbers;
        }

        public AxesNumbers Numbers
        {
            get => m_numbers;
            set
            {
                UnRegister(m_numbers);
                m_numbers = value;
                Register(m_numbers);
                NotifyChanged();
            }
        }

        public AxesDirection Direction
        {
            get => m_direction;
            set
            {
                m_direction = value;
                NotifyChanged();
            }
        }

        public override IReadOnlyCollection<Character> GetCharacters(CoordinatesSystemManager coordinatesSystemManager)
        {
            return GetCharactersCore().ToArray();
            IEnumerable<Character> GetCharactersCore()
            {
                var direction = Direction;
                var outRange = coordinatesSystemManager.OutputRange;
                var inRange = coordinatesSystemManager.InputRange;

                var center = coordinatesSystemManager.OrthonormalOrigin;
                var demiThickness = Stroke == null ? 0 : Stroke.Thickness / 2;

                if ((direction == AxesDirection.Horizontal || direction == AxesDirection.Both) && outRange.HeightContainsRange(center.Y - demiThickness, center.Y + demiThickness, false))
                    yield return Character.Line(coordinatesSystemManager.ComputeOutCoordinates(new Point(inRange.Left, 0)), coordinatesSystemManager.ComputeOutCoordinates(new Point(inRange.Right, 0)), Fill, Stroke);

                if ((direction == AxesDirection.Vertical || direction == AxesDirection.Both) && outRange.WidthContainsRange(center.X - demiThickness, center.X + demiThickness, false))
                    yield return Character.Line(coordinatesSystemManager.ComputeOutCoordinates(new Point(0, inRange.Bottom)), coordinatesSystemManager.ComputeOutCoordinates(new Point(0, inRange.Top)), Fill, Stroke);
            }
        }

        //public override Geometry GetOutGeometry(CoordinatesSystemManager coordinatesSystemManager)
        //{
        //    var inRange = coordinatesSystemManager.InputRange;
        //    return new GeometryGroup
        //    {
        //        Children = new GeometryCollection(new[]
        //        {
        //            new LineGeometry(coordinatesSystemManager.ComputeOutCoordinates(new Point(inRange.Left, 0)), coordinatesSystemManager.ComputeOutCoordinates(new Point(inRange.Right, 0))),
        //            new LineGeometry(coordinatesSystemManager.ComputeOutCoordinates(new Point(inRange.Bottom, 0)), coordinatesSystemManager.ComputeOutCoordinates(new Point(inRange.Top, 0)))
        //        })
        //    };
        //}

        /*public override Point[] Intersection(VisualObject visualObject, CoordinatesSystemManager coordinatesSystemManager)
        {
            if (visualObject is CurveVisualObject curveVisualObject && curveVisualObject.Function != null)
            {
                return null; //////////////////////////////////////
            }
            else if (visualObject.GetAsGeometry(coordinatesSystemManager) is Geometry geometry)
            {
                var bounds = geometry.Bounds;
                var xAxis = new LineGeometry(new Point(bounds.Left, 0.0), new Point(bounds.Right, 0.0));
                var yAxis = new LineGeometry(new Point(0.0, bounds.Top), new Point(0.0, bounds.Bottom));
                var geometryGroup = new GeometryGroup() { Children = new GeometryCollection(new[] { xAxis, yAxis }) };
                return GeometryHelper.GetIntersectionPoints(geometryGroup, geometry);
            }
            else return null;
        }*/
    }

    public enum AxesDirection { None, Horizontal, Vertical, Both }
}
