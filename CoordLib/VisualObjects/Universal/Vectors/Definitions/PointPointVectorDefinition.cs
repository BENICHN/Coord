using System.Windows;

namespace Coord
{
    /// <summary>
    /// Détermine un vecteur du plan grâce à un <see cref="PointVisualObject"/> origine et un <see cref="PointVisualObject"/> extrémité
    /// </summary>
    public class PointPointVectorDefinition : VectorDefinition
    {
        /// <summary>
        /// Origine dans le plan
        /// </summary>
        public PointVisualObject PointA { get => (PointVisualObject)GetValue(PointAProperty); set => SetValue(PointAProperty, value); }
        public static readonly DependencyProperty PointAProperty = CreateProperty<PointVisualObject>(true, true, "PointA", typeof(PointPointVectorDefinition));

        /// <summary>
        /// Extrémité dans le plan
        /// </summary>
        public PointVisualObject PointB { get => (PointVisualObject)GetValue(PointBProperty); set => SetValue(PointBProperty, value); }
        public static readonly DependencyProperty PointBProperty = CreateProperty<PointVisualObject>(true, true, "PointB", typeof(PointPointVectorDefinition));

        protected override void OnChanged()
        {
            if (PointA == null || PointB == null) return;

            var pointA = (Point)PointA;
            var pointB = (Point)PointB;

            InVector = pointB - pointA;
        }
    }
}
