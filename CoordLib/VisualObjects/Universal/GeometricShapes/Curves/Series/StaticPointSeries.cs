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
        protected override Freezable CreateInstanceCore() => new StaticPointSeries();

        public StaticPointSeries() => Points = new ObservableRangeCollection<Point>();

        /// <summary>
        /// Points de cette <see cref="Series"/>
        /// </summary>
        public ObservableRangeCollection<Point> Points { get => (ObservableRangeCollection<Point>)GetValue(PointsProperty); set => SetValue(PointsProperty, value); }
        public static readonly DependencyProperty PointsProperty = CreateProperty<StaticPointSeries, ObservableRangeCollection<Point>>(true, true, true, "Points");

        /// <summary>
        /// Calcule les coordonnées à l'écran des <see cref="PointVisualObject"/> de cette <see cref="Series"/>
        /// </summary>
        /// <param name="coordinatesSystemManager">Système de coordonnées du <see cref="Plane"/></param>
        /// <returns>Coordonnées à l'écran des <see cref="PointVisualObject"/> de cette <see cref="Series"/></returns>
        public override IEnumerable<Point> GetOutPoints(ReadOnlyCoordinatesSystemManager coordinatesSystemManager) => Points.Select(point => coordinatesSystemManager.ComputeOutCoordinates(point));
    }
}
