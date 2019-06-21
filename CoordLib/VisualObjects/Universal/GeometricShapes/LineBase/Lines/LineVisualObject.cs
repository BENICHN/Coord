using BenLib.Framework;
using BenLib.WPF;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Coord
{
    /// <summary>
    /// Objet visuel représentant une droite du plan
    /// </summary>
    public class LineVisualObject : LineVisualObjectBase
    {
        protected override Freezable CreateInstanceCore() => new LineVisualObject();

        public override string Type => Definition switch
        {
            EquationLineDefinition _ => "EquationLine",
            ParallelLineDefinition _ => "ParallelLine",
            PerpendicularBisectorLineDefinition _ => "PerpendicularBisectorLine",
            PerpendicularLineDefinition _ => "PerpendicularLine",
            PointPointLineDefinition _ => "PointPointLine",
            PointVectorLineDefinition _ => "PointVectorLine",
            _ => "Line"
        };

        /// <summary>
        /// Détermine les propriétés fondamentales de ce <see cref="LineVisualObject"/>
        /// </summary>
        public LineDefinition Definition { get => (LineDefinition)GetValue(DefinitionProperty); set => SetValue(DefinitionProperty, value); }
        public static readonly DependencyProperty DefinitionProperty = CreateProperty<LineVisualObject, LineDefinition>(true, true, true, "Definition");

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
                return endpoints.Length == 2 ? (endpoints[0], endpoints[1]) : (new Point(double.NaN, double.NaN), new Point(double.NaN, double.NaN));
            }
        }

        public override string ToString() => Definition.Equation.ToString();

        protected override IReadOnlyCollection<Character> GetCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            var (point1, point2) = GetEndpoints(Equation, coordinatesSystemManager);
            return new[] { Character.Line(coordinatesSystemManager.ComputeOutCoordinates(point1), coordinatesSystemManager.ComputeOutCoordinates(point2)).Color(Fill, Stroke) };
        }
    }
}
