using BenLib;
using System;
using System.Linq;
using System.Windows;

namespace Coord
{
    /// <summary>
    /// Détermine les coordonnées à l'écran de points d'une fonction
    /// </summary>
    public class FunctionSeries : Series
    {
        private Func<double, double> m_function;
        private SeriesType m_type;
        private decimal m_density;

        public FunctionSeries(Func<double, double> function, DecimalInterval interval, SeriesType type)
        {
            Function = function;
            Interval = interval;
            Type = type;
        }
        public FunctionSeries(Func<double, double> function, DecimalInterval interval, SeriesType type, decimal density)
        {
            Function = function;
            Type = type;
            Interval = interval;
            Density = density;
        }

        /// <summary>
        /// Fonction de cette <see cref="Series"/>
        /// </summary>
        public Func<double, double> Function
        {
            get => m_function;
            set
            {
                m_function = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Type de la fonction (x->y ou y->x)
        /// </summary>
        public SeriesType Type
        {
            get => m_type;
            set
            {
                m_type = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Nombre de points par cellule de grille
        /// </summary>
        public decimal Density
        {
            get => m_density;
            set
            {
                m_density = value;
                NotifyChanged();
            }
        }

        private DecimalInterval m_interval;
        public DecimalInterval Interval
        {
            get => m_interval;
            set
            {
                m_interval = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Calcule les coordonnées à l'écran des points de <see cref="Function"/> de cette <see cref="Series"/>
        /// </summary>
        /// <param name="coordinatesSystemManager">Système de coordonnées du <see cref="Plane"/></param>
        /// <returns>Coordonnées à l'écran des points de <see cref="Function"/> de cette <see cref="Series"/></returns>
        public override Point[] GetOutPoints(ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            decimal step = coordinatesSystemManager.GetHorizontalStep() / (Density > 0 ? Density : 100);

            decimal start = coordinatesSystemManager.GetHorizontalStart(step);
            decimal end = coordinatesSystemManager.GetHorizontalEnd(step);
            decimal length = end - start + step;

            return Collections.DecimalRange(start, length, step).Select(i =>
            {
                if (Interval.Contains(i))
                {
                    double x = (double)i;
                    return Type == SeriesType.X ? new Point(Function(x), x) : new Point(x, Function(x));
                }
                else return new Point(double.NaN, double.NaN);
            }).Where(point => !double.IsNaN(point.X) && !double.IsNaN(point.Y)).Select(point => coordinatesSystemManager.ComputeOutCoordinates(point)).ToArray();
        }
    }

    public enum SeriesType { X, Y }
}
