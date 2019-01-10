using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Coord
{
    /// <summary>
    /// Représente une collection de <see cref="NotifyObject"/> qui fournit des notifications quand des éléments sont ajoutés, supprimés, changés ou que l'intégralité de la liste est actualisée
    /// </summary>
    /// <typeparam name="T">Type des éléments de la collection</typeparam>
    public class NotifyObjectCollection<T> : ObservableCollection<T> where T : NotifyObject
    {
        /// <summary>
        /// Se produit quand l'événement <see cref="NotifyObject.Changed"/> d'un élément ou l'événement <see cref="ObservableCollection{T}.CollectionChanged"/> est déclenché
        /// </summary>
        public event EventHandler Changed;

        public NotifyObjectCollection() : base() { }
        public NotifyObjectCollection(IEnumerable<T> collection) { if (collection != null) foreach (var visualObject in collection) Add(visualObject); }

        /// <summary>
        /// Observe les événements <see cref="NotifyObject.Changed"/> et <see cref="NotifyObject.Destroyed"/> d'un élément. En cas de changement de l'élément, l'événement <see cref="NotifyObjectCollection{T}.Changed"/> est déclenché. En cas de destruction de l'élément, celui-ci est supprimé de la collection
        /// </summary>
        /// <param name="item">Élément à observer</param>
        private void Register(T item)
        {
            item.Destroyed += Item_Destroyed;
            item.Changed += Item_Changed;
        }

        /// <summary>
        /// Arrête d'observer les événements <see cref="NotifyObject.Changed"/> et <see cref="NotifyObject.Destroyed"/> d'un élément
        /// </summary>
        /// <param name="item">Élément à ne plus observer</param>
        private void UnRegister(T item)
        {
            item.Destroyed -= Item_Destroyed;
            item.Changed -= Item_Changed;
        }

        /// <summary>
        /// Se produit à la destruction d'un élément observé
        /// </summary>
        /// <param name="sender">Élément observé détruit</param>
        /// <param name="e">Données d'événement</param>
        private void Item_Destroyed(object sender, EventArgs e) => Remove((T)sender);

        /// <summary>
        /// Se produit à un changement d'un élément observé
        /// </summary>
        /// <param name="sender">Élément observé changé</param>
        /// <param name="e">Données d'événement</param>
        private void Item_Changed(object sender, EventArgs e) => NotifyChanged();

        /// <summary>
        /// Observe les éléments ajoutés et arrête d'observer les éléments supprimés puis appelle <see cref="NotifyChanged"/> et déclenche événement <see cref="ObservableCollection{T}.CollectionChanged"/> avec les arguments fournis
        /// </summary>
        /// <param name="e">Arguments de l'événement déclenché</param>
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            var oldItem = e.OldItems == null ? null : e.OldItems[0] as T;
            var newItem = e.NewItems == null ? null : e.NewItems[0] as T;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Register(newItem);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    UnRegister(oldItem);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    UnRegister(oldItem);
                    Register(newItem);
                    break;
            }

            NotifyChanged();
            base.OnCollectionChanged(e);
        }

        /// <summary>
        /// Déclenche <see cref="Changed"/> pour indiquer que la collection ou les éléments de la collection ont changé
        /// </summary>
        protected void NotifyChanged()
        {
            OnChanged();
            Changed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Se produit à un changement de la collection ou des éléments de la collection avant le déclenchement de <see cref="Changed"/>
        /// </summary>
        protected virtual void OnChanged() { }
    }
}
