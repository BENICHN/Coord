using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Coord
{
    /// <summary>
    /// Détermine les coordonnées à l'écran de plusieurs <see cref="PointVisualObject"/>
    /// </summary>
    public class PointSeries : Series
    {
        private NotifyObjectCollection<PointVisualObject> m_points;

        public PointSeries() => Points = new NotifyObjectCollection<PointVisualObject>();
        public PointSeries(params PointVisualObject[] points) => Points = new NotifyObjectCollection<PointVisualObject>(points);
        public PointSeries(IEnumerable<PointVisualObject> points) => Points = new NotifyObjectCollection<PointVisualObject>(points);
        public PointSeries(NotifyObjectCollection<PointVisualObject> points) => Points = new NotifyObjectCollection<PointVisualObject>(points);

        /// <summary>
        /// Points de cette <see cref="Series"/>
        /// </summary>
        public NotifyObjectCollection<PointVisualObject> Points
        {
            get => m_points;
            set
            {
                UnRegister(m_points);
                m_points = value;
                Register(m_points);
                NotifyChanged();
            }
        }

        /// <summary>
        /// Calcule les coordonnées à l'écran des <see cref="PointVisualObject"/> de cette <see cref="Series"/>
        /// </summary>
        /// <param name="coordinatesSystemManager">Système de coordonnées du <see cref="Plane"/></param>
        /// <returns>Coordonnées à l'écran des <see cref="PointVisualObject"/> de cette <see cref="Series"/></returns>
        public override Point[] GetOutPoints(CoordinatesSystemManager coordinatesSystemManager) => Points.Select(pointSourceBase => coordinatesSystemManager.ComputeOutCoordinates(pointSourceBase)).ToArray();
    }
}
