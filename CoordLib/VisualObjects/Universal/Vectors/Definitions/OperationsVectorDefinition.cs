using System;
using System.Windows;

namespace Coord
{
    /// <summary>
    /// Détermine la transformation d'un <see cref="VectorVisualObject"/> par une <see cref="Func{T, TResult}"/>
    /// </summary>
    public class OperationsVectorDefinition : VectorDefinition
    {
        protected override Freezable CreateInstanceCore() => new OperationsVectorDefinition();

        /// <summary>
        /// Vecteur du plan
        /// </summary>
        public VectorVisualObject Vector { get => (VectorVisualObject)GetValue(VectorProperty); set => SetValue(VectorProperty, value); }
        public static readonly DependencyProperty VectorProperty = CreateProperty<OperationsVectorDefinition, VectorVisualObject>(true, true, true, "Vector");

        /// <summary>
        /// Transformation du vecteur du plan
        /// </summary>
        public Func<Vector, Vector> Operations { get => (Func<Vector, Vector>)GetValue(OperationsProperty); set => SetValue(OperationsProperty, value); }
        public static readonly DependencyProperty OperationsProperty = CreateProperty<OperationsVectorDefinition, Func<Vector, Vector>>(true, true, true, "Operations");

        protected override void OnChanged()
        {
            if (Vector == null || Operations == null) return;
            InVector = Operations(Vector);
        }
    }
}
