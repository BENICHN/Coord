using System.Windows;

namespace Coord
{
    /// <summary>
    /// Détermine les propriétés fondamentales d'un <see cref="PointVisualObject"/> à l'aide d'autres propriétés
    /// </summary>
    public abstract class PointDefinition : NotifyObject
    {
        /// <summary>
        /// Point du plan
        /// </summary>
        public Point InPoint { get; protected set; }
    }
}
