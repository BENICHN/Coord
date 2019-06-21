using System.Windows;

namespace Coord
{
    /// <summary>
    /// Détermine un vecteur du plan grâce à un <see cref="PointVisualObject"/> origine et un <see cref="PointVisualObject"/> extrémité
    /// </summary>
    public class PointPointVectorDefinition : VectorDefinition
    {
        protected override Freezable CreateInstanceCore() => new PointPointVectorDefinition();

        /// <summary>
        /// Origine dans le plan
        /// </summary>
        public PointVisualObject PointA { get => (PointVisualObject)GetValue(PointAProperty); set => SetValue(PointAProperty, value); }
        public static readonly DependencyProperty PointAProperty = CreateProperty<PointPointVectorDefinition, PointVisualObject>(true, true, true, "PointA");

        /// <summary>
        /// Extrémité dans le plan
        /// </summary>
        public PointVisualObject PointB { get => (PointVisualObject)GetValue(PointBProperty); set => SetValue(PointBProperty, value); }
        public static readonly DependencyProperty PointBProperty = CreateProperty<PointPointVectorDefinition, PointVisualObject>(true, true, true, "PointB");

        protected override void OnChanged()
        {
            if (PointA == null || PointB == null) return;

            var pointA = (Point)PointA;
            var pointB = (Point)PointB;

            InVector = pointB - pointA;
        }
    }
}
