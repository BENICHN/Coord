using BenLib.WPF;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Coord
{
    /// <summary>
    /// Objet visuel représentant une courbe du plan
    /// </summary>
    public class CurveVisualObject : VisualObject
    {
        protected override Freezable CreateInstanceCore() => new CurveVisualObject();

        public override string Type => $"{Series.GetType().Name.Replace("Series", string.Empty)}Curve";

        /// <summary>
        /// Indique si la courbe est lissée
        /// </summary>
        public bool Smooth { get => (bool)GetValue(SmoothProperty); set => SetValue(SmoothProperty, value); }
        public static readonly DependencyProperty SmoothProperty = CreateProperty<CurveVisualObject, bool>(true, true, true, "Smooth");

        /// <summary>
        /// Coefficient de lissage de la courbe dans le cas où <see cref="Smooth"/> est <see langword="true"/>
        /// </summary>
        public double SmoothValue { get => (double)GetValue(SmoothValueProperty); set => SetValue(SmoothValueProperty, value); }
        public static readonly DependencyProperty SmoothValueProperty = CreateProperty<CurveVisualObject, double>(true, true, true, "SmoothValue", 0.75);

        /// <summary>
        /// Points de la courbe
        /// </summary>
        public Series Series { get => (Series)GetValue(SeriesProperty); set => SetValue(SeriesProperty, value); }
        public static readonly DependencyProperty SeriesProperty = CreateProperty<CurveVisualObject, Series>(true, true, true, "Series");

        /// <summary>
        /// Indique si la courbe est fermée
        /// </summary>
        public bool Closed { get => (bool)GetValue(ClosedProperty); set => SetValue(ClosedProperty, value); }
        public static readonly DependencyProperty ClosedProperty = CreateProperty<CurveVisualObject, bool>(true, true, true, "Closed");

        protected override IReadOnlyCollection<Character> GetCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager) => GeometryHelper.GetCurve(Series.GetOutPoints(coordinatesSystemManager).ToArray(), Closed, Smooth, SmoothValue).ToCharacters(Fill, Stroke).ToArray();
    }
}
