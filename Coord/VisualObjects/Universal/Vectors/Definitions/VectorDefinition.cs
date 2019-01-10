using System.Windows;

namespace Coord
{
    /// <summary>
    /// Détermine les propriétés fondamentales d'un <see cref="VectorVisualObject"/> à l'aide d'autres propriétés
    /// </summary>
    public abstract class VectorDefinition : VisualObjectDefinition
    {
        /// <summary>
        /// Vecteur du plan
        /// </summary>
        public Vector InVector { get; protected set; }
    }
}
