using System.Windows;

namespace Coord
{
    /// <summary>
    /// Détermine les propriétés fondamentales d'un <see cref="VisualObject"/> à l'aide d'autres propriétés
    /// </summary>
    public abstract class VisualObjectDefinition : NotifyObject
    {
        /// <summary>
        /// Calcule et met en cache les propriétés fondamentales
        /// </summary>
        protected abstract void Compute();

        /// <summary>
        /// Appelle <see cref="Compute"/> puis <see cref="NotifyObject.NotifyChanged"/>
        /// </summary>
        public void ComputeAndNotifyChanged()
        {
            Compute();
            NotifyChanged();
        }

        /// <summary>
        /// Méthode simillaire à <see cref="NotifyObject.Register(NotifyObject)"/> mais qui appelle <see cref="ComputeAndNotifyChanged"/> au lieu de <see cref="NotifyObject.NotifyChanged"/>
        /// </summary>
        /// <param name="notifyObject">Objet à observer</param>
        protected void RegisterCompute(NotifyObject notifyObject)
        {
            notifyObject.Changed += (sender, e) => ComputeAndNotifyChanged();
            notifyObject.Destroyed += (sender, e) => Destroy();
        }

        /// <summary>
        /// Méthode simillaire à <see cref="NotifyObject.Register(Freezable)"/> mais qui appelle <see cref="ComputeAndNotifyChanged"/> au lieu de <see cref="NotifyObject.NotifyChanged"/>
        /// </summary>
        /// <param name="freezable">Objet à observer</param>
        protected void RegisterCompute(Freezable freezable)
        {
            if (freezable != null && !freezable.IsFrozen) freezable.Changed += (sender, e) => ComputeAndNotifyChanged();
        }

        /// <summary>
        /// Méthode simillaire à <see cref="NotifyObject.Register(NotifyObjectCollection{NotifyObject})"/> mais qui appelle <see cref="ComputeAndNotifyChanged"/> au lieu de <see cref="NotifyObject.NotifyChanged"/>
        /// </summary>
        /// <param name="collection"></param>
        protected void RegisterCompute<T>(NotifyObjectCollection<T> collection) where T : NotifyObject => collection.Changed += (sender, e) => ComputeAndNotifyChanged();
    }
}
