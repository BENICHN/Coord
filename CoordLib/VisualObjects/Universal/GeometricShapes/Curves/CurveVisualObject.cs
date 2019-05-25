using BenLib.WPF;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Coord
{
    /// <summary>
    /// Objet visuel représentant une courbe du plan
    /// </summary>
    public class CurveVisualObject : VisualObject
    {
        public override string Type => Series switch
        {
            PointSeries _ => "PointCurve",
            StaticPointSeries _ => "StaticPointCurve",
            FunctionSeries _ => "FunctionCurve",
            _ => "Curve"
        };

        /// <summary>
        /// Indique si la courbe est lissée
        /// </summary>
        public bool Smooth { get => (bool)GetValue(SmoothProperty); set => SetValue(SmoothProperty, value); }
        public static readonly DependencyProperty SmoothProperty = CreateProperty<bool>(true, true, "Smooth", typeof(CurveVisualObject));

        /// <summary>
        /// Coefficient de lissage de la courbe dans le cas où <see cref="Smooth"/> est <see langword="true"/>
        /// </summary>
        public double SmoothValue { get => (double)GetValue(SmoothValueProperty); set => SetValue(SmoothValueProperty, value); }
        public static readonly DependencyProperty SmoothValueProperty = CreateProperty(true, true, "SmoothValue", typeof(CurveVisualObject), 0.75);

        /// <summary>
        /// Points de la courbe
        /// </summary>
        public Series Series { get => (Series)GetValue(SeriesProperty); set => SetValue(SeriesProperty, value); }
        public static readonly DependencyProperty SeriesProperty = CreateProperty<Series>(true, true, "Series", typeof(CurveVisualObject));

        /// <summary>
        /// Indique si la courbe est fermée
        /// </summary>
        public bool Closed { get => (bool)GetValue(ClosedProperty); set => SetValue(ClosedProperty, value); }
        public static readonly DependencyProperty ClosedProperty = CreateProperty<bool>(true, true, "Closed", typeof(CurveVisualObject));

        protected override IReadOnlyCollection<Character> GetCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager) => GetCharacters(coordinatesSystemManager, Fill, Stroke, Series, Closed, Smooth, SmoothValue).ToArray();
        public static IEnumerable<Character> GetCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager, Brush fill, PlanePen stroke, Series series, bool closed, bool smooth, double smoothValue = 0.75) => GeometryHelper.GetCurve(series.GetOutPoints(coordinatesSystemManager), closed, smooth, smoothValue).ToCharacters(fill, stroke);
    }
}
