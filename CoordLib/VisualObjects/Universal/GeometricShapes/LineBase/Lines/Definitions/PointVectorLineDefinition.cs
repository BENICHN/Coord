using BenLib.Framework;
using System.Windows;

namespace Coord
{
    /// <summary>
    /// Détermine l'équation d'une droite passant par un <see cref="PointVisualObject"/> et dirigée par un <see cref="VectorVisualObject"/>
    /// </summary>
    public class PointVectorLineDefinition : LineDefinition
    {
        protected override Freezable CreateInstanceCore() => new PointVectorLineDefinition();

        /// <summary>
        /// Point de la droite
        /// </summary>
        public PointVisualObject Point { get => (PointVisualObject)GetValue(PointProperty); set => SetValue(PointProperty, value); }
        public static readonly DependencyProperty PointProperty = CreateProperty<PointVectorLineDefinition, PointVisualObject>(true, true, true, "Point");

        /// <summary>
        /// Vecteur directeur de la droite
        /// </summary>
        public VectorVisualObject Vector { get => (VectorVisualObject)GetValue(VectorProperty); set => SetValue(VectorProperty, value); }
        public static readonly DependencyProperty VectorProperty = CreateProperty<PointVectorLineDefinition, VectorVisualObject>(true, true, true, "Vector");

        protected override void OnChanged()
        {
            if (Point == null || Vector == null) return;

            var point = Point.Definition.InPoint;
            var vector = Vector.Definition.InVector;

            if (vector.X == 0) Equation = new LinearEquation(point.X);

            double a = vector.Y / vector.X;
            double b = point.Y - a * point.X;

            Equation = new LinearEquation(a, b);
        }
    }
}
