namespace Coord
{
    /// <summary>
    /// Détermine les propriétés fondamentales d'un <see cref="PolygonVisualObject"/> à l'aide d'autres propriétés
    /// </summary>
    public abstract class PolygonDefinition : VisualObjectDefinition
    {
        /// <summary>
        /// Points du polygone
        /// </summary>
        public NotifyObjectCollection<PointVisualObject> InPoints { get; protected set; }
    }
}
