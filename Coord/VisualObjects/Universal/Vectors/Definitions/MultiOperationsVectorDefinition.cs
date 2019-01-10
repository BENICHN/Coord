using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Coord
{
    /// <summary>
    /// Détermine le vecteur résultant de la transformation de plusieurs <see cref="VectorVisualObject"/> par une <see cref="Func{T, TResult}"/>
    /// </summary>
    public class MultiOperationsVectorDefinition : VectorDefinition
    {
        private NotifyObjectCollection<VectorVisualObject> m_vectors;
        private Func<Vector[], Vector> m_operations;

        public MultiOperationsVectorDefinition(NotifyObjectCollection<VectorVisualObject> vectors, Func<Vector[], Vector> operations)
        {
            Vectors = vectors;
            Operations = operations;
        }
        public MultiOperationsVectorDefinition(IEnumerable<VectorVisualObject> vectors, Func<Vector[], Vector> operations) : this(new NotifyObjectCollection<VectorVisualObject>(vectors), operations) { }

        /// <summary>
        /// Vecteurs du plan
        /// </summary>
        public NotifyObjectCollection<VectorVisualObject> Vectors
        {
            get => m_vectors;
            set
            {
                m_vectors = value;
                RegisterCompute(m_vectors);
                ComputeAndNotifyChanged();
            }
        }

        /// <summary>
        /// Transformation des vecteurs du plan
        /// </summary>
        public Func<Vector[], Vector> Operations
        {
            get => m_operations;
            set
            {
                m_operations = value;
                ComputeAndNotifyChanged();
            }
        }

        /// <summary>
        /// Calcule et met en cache les propriétés fondamentales du vecteur du plan
        /// </summary>
        protected override void Compute()
        {
            if (Vectors == null || Operations == null) return;
            InVector = Operations(Vectors.Select(v => (Vector)v).ToArray());
        }
    }
}
