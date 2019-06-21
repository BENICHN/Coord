using System.Windows;

namespace Coord
{
    /// <summary>
    /// Détermine la translation d'un <see cref="PointVisualObject"/> par un <see cref="VectorVisualObject"/>
    /// </summary>
    public class TranslationPointDefinition : PointDefinition
    {
        protected override Freezable CreateInstanceCore() => new TranslationPointDefinition();

        /// <summary>
        /// Point du plan à translater
        /// </summary>
        public PointVisualObject Point { get => (PointVisualObject)GetValue(PointProperty); set => SetValue(PointProperty, value); }
        public static readonly DependencyProperty PointProperty = CreateProperty<TranslationPointDefinition, PointVisualObject>(true, true, true, "Point");

        /// <summary>
        /// Vecteur du plan associé à la translation
        /// </summary>
        public VectorVisualObject Vector { get => (VectorVisualObject)GetValue(VectorProperty); set => SetValue(VectorProperty, value); }
        public static readonly DependencyProperty VectorProperty = CreateProperty<TranslationPointDefinition, VectorVisualObject>(true, true, true, "Vector");

        protected override void OnChanged()
        {
            if (Point == null || Vector == null) return;
            InPoint = Point.Definition.InPoint + Vector;
        }
    }
}
