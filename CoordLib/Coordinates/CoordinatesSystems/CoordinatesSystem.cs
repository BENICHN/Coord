using System.Windows;
using static BenLib.Framework.NumFramework;

namespace Coord
{
    /// <summary>
    /// Transforme tout point ou vecteur du plan
    /// </summary>
    public abstract class CoordinatesSystem : NotifyObject
    {
        /// <summary>
        /// Progression de la translation d'un point du plan à son correspondant transformé
        /// </summary>
        public double Progress { get => (double)GetValue(ProgressProperty); set => SetValue(ProgressProperty, value); }
        public static readonly DependencyProperty ProgressProperty = CreateProperty<CoordinatesSystem, double>(true, true, true, "Progress", 1);

        protected abstract Point ComputeOutCoordinatesCore(Point point);
        protected abstract Point ComputeInCoordinatesCore(Point point);

        /// <summary>
        /// Transforme un point du plan
        /// </summary>
        /// <param name="point">Point du plan à transformer</param>
        /// <returns>Point du plan transformé</returns>
        public Point ComputeOutCoordinates(Point point)
        {
            double progress = Progress;
            var coordinates = ComputeOutCoordinatesCore(point);
            return Interpolate(point, coordinates, progress);
        }

        /// <summary>
        /// Retrouve le point original à partir d'un point transformé du plan
        /// </summary>
        /// <param name="point">Point transformé du plan</param>
        /// <returns>Point original du plan</returns>
        public Point ComputeInCoordinates(Point point)
        {
            double progress = Progress;
            var coordinates = ComputeInCoordinatesCore(point);
            return Interpolate(point, coordinates, progress);
        }

        protected abstract Vector ComputeOutCoordinatesCore(Vector vector);
        protected abstract Vector ComputeInCoordinatesCore(Vector vector);

        /// <summary>
        /// Transforme un vecteur du plan
        /// </summary>
        /// <param name="vector">Vecteur du plan à transformer</param>
        /// <returns>Vecteur du plan transformé</returns>
        public Vector ComputeOutCoordinates(Vector vector)
        {
            double progress = Progress;
            var coordinates = ComputeOutCoordinatesCore(vector);
            return Interpolate(vector, coordinates, progress);
        }

        /// <summary>
        /// Transforme le vecteur du plan
        /// </summary>
        /// <param name="vector">Vecteur du plan à transformer</param>
        /// <returns>Vecteur du plan transformé</returns>
        public Vector ComputeInCoordinates(Vector vector)
        {
            double progress = Progress;
            var coordinates = ComputeInCoordinates(vector);
            return Interpolate(vector, coordinates, progress);
        }
    }
}
