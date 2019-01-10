using System.Windows;

namespace Coord
{
    /// <summary>
    /// Détermine un vecteur du plan grâce à un <see cref="Vector"/>
    /// </summary>
    public class VectorVectorDefinition : VectorDefinition
    {
        /// <summary>
        /// Vecteur du plan
        /// </summary>
        public new Vector InVector
        {
            get => base.InVector;
            set
            {
                base.InVector = value;
                NotifyChanged();
            }
        }

        public VectorVectorDefinition(Vector inVector) => InVector = inVector;

        /// <summary>
        /// Calcule et met en cache les propriétés fondamentales du vecteur du plan
        /// </summary>
        protected override void Compute() { }
    }
}
