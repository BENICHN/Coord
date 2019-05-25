using BenLib.Framework;
using System.Windows;

namespace Coord
{
    /// <summary>
    /// Détermine l'intersection de deux droites du plan
    /// </summary>
    public class LineIntersectionPointDefinition : PointDefinition
    {
        /// <summary>
        /// Première droite
        /// </summary>
        public LineVisualObject LineA { get => (LineVisualObject)GetValue(LineAProperty); set => SetValue(LineAProperty, value); }
        public static readonly DependencyProperty LineAProperty = CreateProperty<LineVisualObject>(true, true, "LineA", typeof(LineIntersectionPointDefinition));

        /// <summary>
        /// Seconde droite
        /// </summary>
        public LineVisualObject LineB { get => (LineVisualObject)GetValue(LineBProperty); set => SetValue(LineBProperty, value); }
        public static readonly DependencyProperty LineBProperty = CreateProperty<LineVisualObject>(true, true, "LineB", typeof(LineIntersectionPointDefinition));

        protected override void OnChanged()
        {
            if (LineA == null || LineB == null) return;
            InPoint = LinearEquation.Intersection(LineA.Equation, LineB.Equation);
        }
    }
}
