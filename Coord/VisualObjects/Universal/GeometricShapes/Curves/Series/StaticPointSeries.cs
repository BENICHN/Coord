using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace Coord
{
    /// <summary>
    /// Détermine les coordonnées à l'écran de plusieurs <see cref="PointVisualObject"/>
    /// </summary>
    public class StaticPointSeries : Series
    {
        private ObservableCollection<Point> m_points;

        public StaticPointSeries() => Points = new ObservableCollection<Point>();
        public StaticPointSeries(params Point[] points) => Points = new ObservableCollection<Point>(points);
        public StaticPointSeries(IEnumerable<Point> points) => Points = new ObservableCollection<Point>(points);

        /// <summary>
        /// Points de cette <see cref="Series"/>
        /// </summary>
        public ObservableCollection<Point> Points
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
        public override Point[] GetOutPoints(CoordinatesSystemManager coordinatesSystemManager) => Points.Select(point => coordinatesSystemManager.ComputeOutCoordinates(point)).ToArray();
    }
}
