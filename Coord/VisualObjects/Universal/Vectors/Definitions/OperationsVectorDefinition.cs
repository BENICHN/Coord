using System;
using System.Windows;

namespace Coord
{
    /// <summary>
    /// Détermine la transformation d'un <see cref="VectorVisualObject"/> par une <see cref="Func{T, TResult}"/>
    /// </summary>
    public class OperationsVectorDefinition : VectorDefinition
    {
        private VectorVisualObject m_vector;
        private Func<Vector, Vector> m_operations;

        public OperationsVectorDefinition(VectorVisualObject vector, Func<Vector, Vector> operations)
        {
            Vector = vector;
            Operations = operations;
        }

        /// <summary>
        /// Vecteur du plan
        /// </summary>
        public VectorVisualObject Vector
        {
            get => m_vector;
            set
            {
                m_vector = value;
                RegisterCompute(m_vector);
                ComputeAndNotifyChanged();
            }
        }

        /// <summary>
        /// Transformation du vecteur du plan
        /// </summary>
        public Func<Vector, Vector> Operations
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
            if (Vector == null || Operations == null) return;
            InVector = Operations(Vector);
        }
    }
}
