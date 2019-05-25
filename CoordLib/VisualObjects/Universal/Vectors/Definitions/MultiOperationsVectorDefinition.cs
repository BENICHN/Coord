using System;
using System.Linq;
using System.Windows;

namespace Coord
{
    /// <summary>
    /// Détermine le vecteur résultant de la transformation de plusieurs <see cref="VectorVisualObject"/> par une <see cref="Func{T, TResult}"/>
    /// </summary>
    public class MultiOperationsVectorDefinition : VectorDefinition
    {
        /// <summary>
        /// Vecteurs du plan
        /// </summary>
        public NotifyObjectCollection<VectorVisualObject> Vectors { get => (NotifyObjectCollection<VectorVisualObject>)GetValue(VectorsProperty); set => SetValue(VectorsProperty, value); }
        public static readonly DependencyProperty VectorsProperty = CreateProperty<NotifyObjectCollection<VectorVisualObject>>(true, true, "Vectors", typeof(MultiOperationsVectorDefinition));

        /// <summary>
        /// Transformation des vecteurs du plan
        /// </summary>
        public Func<Vector[], Vector> Operations { get => (Func<Vector[], Vector>)GetValue(OperationsProperty); set => SetValue(OperationsProperty, value); }
        public static readonly DependencyProperty OperationsProperty = CreateProperty<Func<Vector[], Vector>>(true, true, "Operations", typeof(MultiOperationsVectorDefinition));

        protected override void OnChanged()
        {
            if (Vectors == null || Operations == null) return;
            InVector = Operations(Vectors.Select(v => (Vector)v).ToArray());
        }
    }
}
