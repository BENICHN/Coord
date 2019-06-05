using BenLib.Standard;
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
        /// <summary>
        /// Fonction de cette <see cref="Series"/>
        /// </summary>
        public Func<double, double> Function { get => (Func<double, double>)GetValue(FunctionProperty); set => SetValue(FunctionProperty, value); }
        public static readonly DependencyProperty FunctionProperty = CreateProperty<Func<double, double>>(true, true, "Function", typeof(FunctionSeries));

        /// <summary>
        /// Type de la fonction (x->y ou y->x)
        /// </summary>
        public SeriesType Type { get => (SeriesType)GetValue(TypeProperty); set => SetValue(TypeProperty, value); }
        public static readonly DependencyProperty TypeProperty = CreateProperty<SeriesType>(true, true, "Type", typeof(FunctionSeries));

        /// <summary>
        /// Nombre de points par cellule de grille
        /// </summary>
        public double Density { get => (double)GetValue(DensityProperty); set => SetValue(DensityProperty, value); }
        public static readonly DependencyProperty DensityProperty = CreateProperty<double>(true, true, "Density", typeof(FunctionSeries));

        public Interval<double> Interval { get => (Interval<double>)GetValue(IntervalProperty); set => SetValue(IntervalProperty, value); }
        public static readonly DependencyProperty IntervalProperty = CreateProperty<Interval<double>>(true, true, "Interval", typeof(FunctionSeries));

        /// <summary>
        /// Calcule les coordonnées à l'écran des points de <see cref="Function"/> de cette <see cref="Series"/>
        /// </summary>
        /// <param name="coordinatesSystemManager">Système de coordonnées du <see cref="Plane"/></param>
        /// <returns>Coordonnées à l'écran des points de <see cref="Function"/> de cette <see cref="Series"/></returns>
        public override Point[] GetOutPoints(ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            double step = coordinatesSystemManager.GetHorizontalStep() / (Density > 0 ? Density : 100);

            double start = coordinatesSystemManager.GetHorizontalStart(step);
            double end = coordinatesSystemManager.GetHorizontalEnd(step);

            return Interval<double>.CC(start, end).Numbers(step).Select(i =>
            {
                if (Interval >= i)
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
