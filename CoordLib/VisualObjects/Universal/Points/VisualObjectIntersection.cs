using BenLib.WPF;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using static Coord.VisualObjects;

namespace Coord
{
    public class VisualObjectIntersection : VisualObjectGroupBase
    {
        public override string Type => "VisualObjectIntersection";

        public VisualObject Object1 { get => (VisualObject)GetValue(Object1Property); set => SetValue(Object1Property, value); }
        public static readonly DependencyProperty Object1Property = CreateProperty<VisualObject>(true, true, "Object1", typeof(VisualObjectIntersection));

        public VisualObject Object2 { get => (VisualObject)GetValue(Object2Property); set => SetValue(Object2Property, value); }
        public static readonly DependencyProperty Object2Property = CreateProperty<VisualObject>(true, true, "Object2", typeof(VisualObjectIntersection));

        public double Radius { get => (double)GetValue(RadiusProperty); set => SetValue(RadiusProperty, value); }
        public static readonly DependencyProperty RadiusProperty = CreateProperty<double>(true, true, "Radius", typeof(VisualObjectIntersection), 10);

        private Point[] m_inPoints;
        private readonly List<(PointVisualObject visualObject, int index)> m_generatedPoints = new List<(PointVisualObject visualObject, int index)>();
        private readonly List<NotifyObjectCollection<PointVisualObject>> m_generatedCollections = new List<NotifyObjectCollection<PointVisualObject>>();

        protected override IReadOnlyCollection<Character> GetCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            var g1 = Object1.GetTransformedCharacters(coordinatesSystemManager).Geometry();
            var g2 = Object2.GetTransformedCharacters(coordinatesSystemManager).Geometry();
            var outPoints = GeometryHelper.GetIntersectionPoints(g1, g2).ToArray();
            m_inPoints = outPoints.Select(point => coordinatesSystemManager.ComputeInCoordinates(point)).ToArray();

            foreach (var (point, index) in m_generatedPoints) point.SetInPoint(m_inPoints.Length > index ? m_inPoints[index] : new Point(double.NaN, double.NaN));
            foreach (var collection in m_generatedCollections) UpdatePointsCollection(collection);
            UpdateCollection(Children);

            return base.GetCharacters(coordinatesSystemManager);

            void UpdateCollection(ICollection<VisualObject> collection)
            {
                var vobjs = collection.GetEnumerator();

                foreach (var inPoint in m_inPoints)
                {
                    if (vobjs.MoveNext()) ((PointVisualObject)vobjs.Current).Style(Fill, Stroke).Extend(Radius).SetInPoint(inPoint);
                    else collection.Add(CreatePoint(inPoint));
                }

                while (vobjs.MoveNext()) ((PointVisualObject)vobjs.Current).SetInPoint(new Point(double.NaN, double.NaN));
            }

            void UpdatePointsCollection(ICollection<PointVisualObject> collection)
            {
                var vobjs = collection.GetEnumerator();

                foreach (var inPoint in m_inPoints)
                {
                    if (vobjs.MoveNext()) vobjs.Current.Style(Fill, Stroke).Extend(Radius).SetInPoint(inPoint);
                    else collection.Add(CreatePoint(inPoint));
                }

                while (vobjs.MoveNext()) vobjs.Current.SetInPoint(new Point(double.NaN, double.NaN));
            }
        }

        public PointVisualObject GetIntersectionPoint(int index)
        {
            var point = Point(m_inPoints.Length > index ? m_inPoints[index] : new Point(double.NaN, double.NaN));
            m_generatedPoints.Add((point, index));
            return point;
        }

        public NotifyObjectCollection<PointVisualObject> GetIntersectionPoints()
        {
            var collection = new NotifyObjectCollection<PointVisualObject>(m_inPoints.Select(inPoint => CreatePoint(inPoint)));
            m_generatedCollections.Add(collection);
            return collection;
        }

        private PointVisualObject CreatePoint(Point inPoint) => Point(inPoint).Extend(Radius).Style(Fill, Stroke);
    }
}
