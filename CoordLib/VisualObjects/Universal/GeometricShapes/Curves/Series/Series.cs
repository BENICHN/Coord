using System.Windows;

namespace Coord
{
    /// <summary>
    /// Série de points de l'écran
    /// </summary>
    public abstract class Series : NotifyObject
    {
        /// <summary>
        /// Calcule les coordonnées à l'écran des points de cette <see cref="Series"/>
        /// </summary>
        /// <param name="coordinatesSystemManager">Système de coordonnées du <see cref="Plane"/></param>
        /// <returns>Coordonnées à l'écran des points de cette <see cref="Series"/></returns>
        public abstract Point[] GetOutPoints(ReadOnlyCoordinatesSystemManager coordinatesSystemManager);
    }
}
