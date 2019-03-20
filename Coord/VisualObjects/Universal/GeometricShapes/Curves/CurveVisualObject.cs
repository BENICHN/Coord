using BenLib.WPF;
using System.Collections.Generic;
using System.Windows.Media;

namespace Coord
{
    /// <summary>
    /// Objet visuel représentant une courbe du plan
    /// </summary>
    public class CurveVisualObject : VisualObject
    {
        public override string Type
        {
            get
            {
                var definitionType = Series?.GetType();
                if (definitionType == typeof(PointSeries)) return "PointCurve";
                if (definitionType == typeof(StaticPointSeries)) return "StaticPointCurve";
                if (definitionType == typeof(FunctionSeries)) return "FunctionCurve";
                else return "Curve";
            }
        }

        private Series m_series;
        private bool m_smooth;
        private bool m_closed;
        private double m_smoothValue;

        public CurveVisualObject(Series series, bool closed, bool smooth, double smoothValue = 0.75)
        {
            Series = series;
            Closed = closed;
            Smooth = smooth;
            SmoothValue = smoothValue;
        }

        /// <summary>
        /// Indique si la courbe est lissée
        /// </summary>
        public bool Smooth
        {
            get => m_smooth;
            set
            {
                m_smooth = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Coefficient de lissage de la courbe dans le cas où <see cref="Smooth"/> est <see langword="true"/>
        /// </summary>
        public double SmoothValue
        {
            get => m_smoothValue;
            set
            {
                m_smoothValue = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Points de la courbe
        /// </summary>
        public Series Series
        {
            get => m_series;
            set
            {
                UnRegister(m_series);
                m_series = value;
                Register(m_series);
                NotifyChanged();
            }
        }

        /// <summary>
        /// Indique si la courbe est fermée
        /// </summary>
        public bool Closed
        {
            get => m_closed;
            set
            {
                m_closed = value;
                NotifyChanged();
            }
        }

        public override IReadOnlyCollection<Character> GetCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager) => GetCharacters(coordinatesSystemManager, Fill, Stroke, Series, Closed, Smooth, SmoothValue);
        public static Character[] GetCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager, Brush fill, Pen stroke, Series series, bool closed, bool smooth, double smoothValue = 0.75) => new[] { new Character(GeometryHelper.GetCurve(series.GetOutPoints(coordinatesSystemManager), closed, smooth, smoothValue), fill, stroke) };
    }
}
