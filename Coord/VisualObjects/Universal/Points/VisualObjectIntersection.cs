using BenLib.WPF;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Coord
{
    public class VisualObjectIntersection : VisualObject
    {
        public override string Type => "VisualObjectIntersection";

        private VisualObject m_object1;
        public VisualObject Object1
        {
            get => m_object1;
            set
            {
                UnRegister(m_object1);
                m_object1 = value;
                Register(value);
                NotifyChanged();
            }
        }

        private VisualObject m_object2;
        public VisualObject Object2
        {
            get => m_object2;
            set
            {
                UnRegister(m_object2);
                m_object2 = value;
                Register(value);
                NotifyChanged();
            }
        }

        private Point[] m_inPoints;
        private readonly List<(PointVisualObject visualObject, int index)> m_generatedPoints = new List<(PointVisualObject visualObject, int index)>();
        private readonly List<NotifyObjectCollection<PointVisualObject>> m_generatedCollections = new List<NotifyObjectCollection<PointVisualObject>>();

        private double m_radius;
        public double Radius
        {
            get => m_radius;
            set
            {
                m_radius = value;
                NotifyChanged();
            }
        }

        public VisualObjectIntersection(VisualObject object1, VisualObject object2, double radius = 10)
        {
            Object1 = object1;
            Object2 = object2;
            Radius = radius;
        }

        public override IReadOnlyCollection<Character> GetCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            var g1 = Object1.GetTransformedCharacters(coordinatesSystemManager, true).Geometry();
            var g2 = Object2.GetTransformedCharacters(coordinatesSystemManager, true).Geometry();
            var outPoints = GeometryHelper.GetIntersectionPoints(g1, g2).ToArray();
            m_inPoints = outPoints.Select(point => coordinatesSystemManager.ComputeInCoordinates(point)).ToArray();

            foreach (var (point, index) in m_generatedPoints) point.SetInPoint(m_inPoints.Length > index ? m_inPoints[index] : new Point(double.NaN, double.NaN));
            foreach (var collection in m_generatedCollections) collection.ReplaceRange(m_inPoints.Select(point => new PointVisualObject(point)));

            double radius = Radius;
            return outPoints.Select(point => Character.Ellipse(point, radius, radius, Fill, Stroke)).ToArray();
        }

        public PointVisualObject GetIntersectionPoint(int index)
        {
            var point = new PointVisualObject(m_inPoints.Length > index ? m_inPoints[index] : new Point(double.NaN, double.NaN));
            m_generatedPoints.Add((point, index));
            return point;
        }

        public NotifyObjectCollection<PointVisualObject> GetIntersectionPoints()
        {
            var collection = new NotifyObjectCollection<PointVisualObject>(m_inPoints.Select(point => new PointVisualObject(point)));
            m_generatedCollections.Add(collection);
            return collection;
        }
    }
}
