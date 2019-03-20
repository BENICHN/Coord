using BenLib;
using BenLib.WPF;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Coord
{
    /// <summary>
    /// Objet visuel représentant une droite du plan
    /// </summary>
    public class LineVisualObject : LineVisualObjectBase
    {
        public override string Type
        {
            get
            {
                var definitionType = Definition?.GetType();
                if (definitionType == typeof(EquationLineDefinition)) return "EquationLine";
                if (definitionType == typeof(ParallelLineDefinition)) return "ParallelLine";
                if (definitionType == typeof(PerpendicularBisectorLineDefinition)) return "PerpendicularBisectorLine";
                if (definitionType == typeof(PerpendicularLineDefinition)) return "PerpendicularLine";
                if (definitionType == typeof(PointPointLineDefinition)) return "PointPointLine";
                if (definitionType == typeof(PointVectorLineDefinition)) return "PointVectorLine";
                else return "Line";
            }
        }

        private LineDefinition m_definition;

        public LineVisualObject(LineDefinition definition) => Definition = definition;

        /// <summary>
        /// Détermine les propriétés fondamentales de ce <see cref="LineVisualObject"/>
        /// </summary>
        public LineDefinition Definition
        {
            get => m_definition;
            set
            {
                UnRegister(m_definition);
                m_definition = value;
                Register(m_definition);
                NotifyChanged();
            }
        }

        /// <summary>
        /// Équation de droite qui décrit ce <see cref="LineVisualObjectBase"/>
        /// </summary>
        public override LinearEquation Equation => Definition.Equation;

        /// <summary>
        /// Calcule les deux extrémités du segment de l'écran qui représente la droite du plan
        /// </summary>
        /// <param name="coordinatesSystemManager">Système de coordonnées du <see cref="Plane"/></param>
        /// <returns>Extrémités du segment de l'écran qui représente la droite du plan</returns>
        public static (Point Point1, Point Point2) GetEndpoints(LinearEquation equation, ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            var inRange = coordinatesSystemManager.InputRange;
            if (equation.B == 0.0)
            {
                double x = equation.X(0.0);
                return (new Point(x, inRange.Bottom), new Point(x, inRange.Top));
            }
            else
            {
                var endpoints = GeometryHelper.LineRectIntersection(new Point(inRange.Left, equation.Y(inRange.Left)), new Point(inRange.Right, equation.Y(inRange.Right)), new Rect(inRange.TopLeft, inRange.BottomRight)).ToArray();
                if (endpoints.Length == 2) return (endpoints[0], endpoints[1]);
                else return (new Point(double.NaN, double.NaN), new Point(double.NaN, double.NaN));
            }
        }

        public override string ToString() => Definition.Equation.ToString();

        public override IReadOnlyCollection<Character> GetCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            var (point1, point2) = GetEndpoints(Equation, coordinatesSystemManager);
            return new[] { Character.Line(coordinatesSystemManager.ComputeOutCoordinates(point1), coordinatesSystemManager.ComputeOutCoordinates(point2), Fill, Stroke) };
        }
    }
}
