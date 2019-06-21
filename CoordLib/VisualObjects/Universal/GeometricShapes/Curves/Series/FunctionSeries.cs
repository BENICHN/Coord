using BenLib.Standard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Coord
{
    /// <summary>
    /// Détermine les coordonnées à l'écran de points d'une fonction
    /// </summary>
    public class FunctionSeries : Series
    {
        protected override Freezable CreateInstanceCore() => new FunctionSeries();

        /// <summary>
        /// Fonction de cette <see cref="Series"/>
        /// </summary>
        public Func<double, double> Function { get => (Func<double, double>)GetValue(FunctionProperty); set => SetValue(FunctionProperty, value); }
        public static readonly DependencyProperty FunctionProperty = CreateProperty<FunctionSeries, Func<double, double>>(true, true, true, "Function");

        /// <summary>
        /// Type de la fonction (x->y ou y->x)
        /// </summary>
        public SeriesType Type { get => (SeriesType)GetValue(TypeProperty); set => SetValue(TypeProperty, value); }
        public static readonly DependencyProperty TypeProperty = CreateProperty<FunctionSeries, SeriesType>(true, true, true, "Type", SeriesType.Y);

        /// <summary>
        /// Nombre de points par cellule de grille
        /// </summary>
        public double Density { get => (double)GetValue(DensityProperty); set => SetValue(DensityProperty, value); }
        public static readonly DependencyProperty DensityProperty = CreateProperty<FunctionSeries, double>(true, true, true, "Density");

        public Interval<double> Interval { get => (Interval<double>)GetValue(IntervalProperty); set => SetValue(IntervalProperty, value); }
        public static readonly DependencyProperty IntervalProperty = CreateProperty<FunctionSeries, Interval<double>>(true, true, true, "Interval", Interval<double>.Reals);

        /// <summary>
        /// Calcule les coordonnées à l'écran des points de <see cref="Function"/> de cette <see cref="Series"/>
        /// </summary>
        /// <param name="coordinatesSystemManager">Système de coordonnées du <see cref="Plane"/></param>
        /// <returns>Coordonnées à l'écran des points de <see cref="Function"/> de cette <see cref="Series"/></returns>
        public override IEnumerable<Point> GetOutPoints(ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            double step = coordinatesSystemManager.GetHorizontalStep() / (Density > 0 ? Density : 100);

            double start = coordinatesSystemManager.GetHorizontalStart(step);
            double end = coordinatesSystemManager.GetHorizontalEnd(step);

            return Interval<double>.CC(start, end).Numbers(step).Select(x => Interval >= x ? Type == SeriesType.X ? new Point(Function(x), x) : new Point(x, Function(x)) : new Point(double.NaN, double.NaN)).Where(point => !double.IsNaN(point.X) && !double.IsNaN(point.Y)).Select(point => coordinatesSystemManager.ComputeOutCoordinates(point));
        }
    }

    public enum SeriesType { X, Y }
}
