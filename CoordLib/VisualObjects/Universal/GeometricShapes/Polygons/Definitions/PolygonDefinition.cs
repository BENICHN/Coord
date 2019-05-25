using System.Windows;

namespace Coord
{
    /// <summary>
    /// Détermine les propriétés fondamentales d'un <see cref="PolygonVisualObject"/> à l'aide d'autres propriétés
    /// </summary>
    public abstract class PolygonDefinition : NotifyObject
    {
        /// <summary>
        /// Points du polygone
        /// </summary>
        public Point[] InPoints { get; protected set; }
    }
}
