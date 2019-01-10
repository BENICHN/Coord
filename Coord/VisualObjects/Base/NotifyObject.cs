using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace Coord
{
    /// <summary>
    /// Objet fournissant des notifications quand il change ou quand il est détruit
    /// </summary>
    public class NotifyObject
    {
        /// <summary>
        /// Déclenche <see cref="Destroyed"/> à la destruction de l'objet
        /// </summary>
        ~NotifyObject() => Destroyed?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Se produit à la destruction de l'objet
        /// </summary>
        public event EventHandler Destroyed;

        /// <summary>
        /// Se produit à un changement de l'objet
        /// </summary>
        public event EventHandler Changed;

        /// <summary>
        /// Déclenche <see cref="Destroyed"/> pour indiquer que l'objet doit être détruit
        /// </summary>
        public void Destroy()
        {
            Destroyed?.Invoke(this, EventArgs.Empty);
            OnDestroyed();
        }

        /// <summary>
        /// Déclenche <see cref="Changed"/> pour indiquer que l'objet a changé
        /// </summary>
        public void NotifyChanged()
        {
            OnChanged();
            Changed?.Invoke(this, EventArgs.Empty);
        }

        private void NotifyChangedHandler(object sender, EventArgs e) => NotifyChanged();
        private void DestroyHandler(object sender, EventArgs e) => Destroy();

        protected void Register(NotifyObject notifyObject)
        {
            if (notifyObject != null)
            {
                notifyObject.Changed += NotifyChangedHandler;
                notifyObject.Destroyed += DestroyHandler;
            }
        }
        protected void Register<T>(NotifyObjectCollection<T> collection) where T : NotifyObject { if (collection != null) collection.Changed += NotifyChangedHandler; }
        protected void Register<T>(ObservableCollection<T> collection) { if (collection != null) collection.CollectionChanged += NotifyChangedHandler; }
        protected void Register(Freezable freezable) { if (freezable != null && !freezable.IsFrozen) freezable.Changed += NotifyChangedHandler; }

        protected void UnRegister(NotifyObject notifyObject)
        {
            if (notifyObject != null)
            {
                notifyObject.Changed -= NotifyChangedHandler;
                notifyObject.Destroyed -= DestroyHandler;
            }
        }
        protected void UnRegister<T>(NotifyObjectCollection<T> collection) where T : NotifyObject { if (collection != null) collection.Changed -= NotifyChangedHandler; }
        protected void UnRegister<T>(ObservableCollection<T> collection) { if (collection != null) collection.CollectionChanged -= NotifyChangedHandler; }
        protected void UnRegister(Freezable freezable) { if (freezable != null && !freezable.IsFrozen) freezable.Changed -= NotifyChangedHandler; }

        /// <summary>
        /// Se produit à la destruction de l'objet avant le déclenchement de <see cref="Destroyed"/>
        /// </summary>
        protected virtual void OnDestroyed() { }

        /// <summary>
        /// Se produit à un changement de l'objet avant le déclenchement de <see cref="Changed"/>
        /// </summary>
        protected virtual void OnChanged() { }
    }

    public interface IProgressive
    {
        Progress Progress { get; set; }
    }
}
