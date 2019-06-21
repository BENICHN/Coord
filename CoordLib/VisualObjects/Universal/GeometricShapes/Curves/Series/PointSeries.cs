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
        protected override Freezable CreateInstanceCore() => new PointSeries();

        public PointSeries() => Points = new NotifyObjectCollection<PointVisualObject>();

        /// <summary>
        /// Points de cette <see cref="Series"/>
        /// </summary>
        public NotifyObjectCollection<PointVisualObject> Points { get => (NotifyObjectCollection<PointVisualObject>)GetValue(PointsProperty); set => SetValue(PointsProperty, value); }
        public static readonly DependencyProperty PointsProperty = CreateProperty<PointSeries, NotifyObjectCollection<PointVisualObject>>(true, true, true, "Points");

        /// <summary>
        /// Calcule les coordonnées à l'écran des <see cref="PointVisualObject"/> de cette <see cref="Series"/>
        /// </summary>
        /// <param name="coordinatesSystemManager">Système de coordonnées du <see cref="Plane"/></param>
        /// <returns>Coordonnées à l'écran des <see cref="PointVisualObject"/> de cette <see cref="Series"/></returns>
        public override IEnumerable<Point> GetOutPoints(ReadOnlyCoordinatesSystemManager coordinatesSystemManager) => Points.Select(pointSourceBase => coordinatesSystemManager.ComputeOutCoordinates(pointSourceBase));
    }
}
