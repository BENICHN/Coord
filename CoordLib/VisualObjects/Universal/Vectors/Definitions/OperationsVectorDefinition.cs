using System;
using System.Windows;

namespace Coord
{
    /// <summary>
    /// Détermine la transformation d'un <see cref="VectorVisualObject"/> par une <see cref="Func{T, TResult}"/>
    /// </summary>
    public class OperationsVectorDefinition : VectorDefinition
    {
        /// <summary>
        /// Vecteur du plan
        /// </summary>
        public VectorVisualObject Vector { get => (VectorVisualObject)GetValue(VectorProperty); set => SetValue(VectorProperty, value); }
        public static readonly DependencyProperty VectorProperty = CreateProperty<VectorVisualObject>(true, true, "Vector", typeof(OperationsVectorDefinition));

        /// <summary>
        /// Transformation du vecteur du plan
        /// </summary>
        public Func<Vector, Vector> Operations { get => (Func<Vector, Vector>)GetValue(OperationsProperty); set => SetValue(OperationsProperty, value); }
        public static readonly DependencyProperty OperationsProperty = CreateProperty<Func<Vector, Vector>>(true, true, "Operations", typeof(OperationsVectorDefinition));

        protected override void OnChanged()
        {
            if (Vector == null || Operations == null) return;
            InVector = Operations(Vector);
        }
    }
}
