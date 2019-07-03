using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace Coord
{
    public class NotifyObjectCollection<T> : NotifyObject, IList<T>, IList, IReadOnlyList<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        protected override Freezable CreateInstanceCore() => new NotifyObjectCollection<T>();
        protected ObservableCollection<T> Items { get; }

        public NotifyObjectCollection()
        {
            Items = new ObservableCollection<T>();
            CollectionChanged += (sender, e) => NotifyChanged();
        }
        public NotifyObjectCollection(List<T> list)
        {
            Items = new ObservableCollection<T>(list);
            CollectionChanged += (sender, e) => NotifyChanged();
        }
        public NotifyObjectCollection(IEnumerable<T> collection)
        {
            Items = new ObservableCollection<T>(collection);
            CollectionChanged += (sender, e) => NotifyChanged();
        }

        public T this[int index] { get => Items[index]; set => SetItem(index, value); }
        object IList.this[int index] { get => this[index]; set => this[index] = (T)value; }

        public int Count => Items.Count;
        public bool IsReadOnly => false;

        bool IList.IsFixedSize => false;
        object ICollection.SyncRoot => ((ICollection)Items).SyncRoot;
        bool ICollection.IsSynchronized => ((ICollection)Items).IsSynchronized;

        public event NotifyCollectionChangedEventHandler CollectionChanged { add => Items.CollectionChanged += value; remove => Items.CollectionChanged -= value; }
        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged { add => ((INotifyPropertyChanged)Items).PropertyChanged += value; remove => ((INotifyPropertyChanged)Items).PropertyChanged -= value; }

        protected virtual void Register(T item)
        {
            if (item is NotifyObject notifyObject)
            {
                notifyObject.Destroyed += OnItemDestroyed;
                notifyObject.Changed += OnItemChanged;
            }
        }
        protected virtual void UnRegister(T item)
        {
            if (item is NotifyObject notifyObject)
            {
                notifyObject.Destroyed -= OnItemDestroyed;
                notifyObject.Changed -= OnItemChanged;
            }
        }

        private void OnItemDestroyed(object sender, EventArgs e) => Remove((T)sender);
        private void OnItemChanged(object sender, EventArgs e) => NotifyChanged();

        protected virtual void ClearItems()
        {
            foreach (var item in Items) UnRegister(item);
            Items.Clear();
        }
        protected virtual void InsertItem(int index, T item)
        {
            Register(item);
            Items.Insert(index, item);
        }
        protected virtual void RemoveItem(int index)
        {
            UnRegister(Items[index]);
            Items.RemoveAt(index);
        }
        protected virtual void SetItem(int index, T item)
        {
            UnRegister(Items[index]);
            Register(item);
            Items[index] = item;
        }
        protected virtual void MoveItem(int oldIndex, int newIndex) => Items.Move(oldIndex, newIndex);

        public void Add(T item) => InsertItem(Count, item);
        public void Clear() => ClearItems();
        public bool Contains(T item) => Items.Contains(item);
        public void CopyTo(T[] array, int arrayIndex) => Items.CopyTo(array, arrayIndex);
        public int IndexOf(T item) => Items.IndexOf(item);
        public void Insert(int index, T item) => InsertItem(index, item);
        public void Move(int oldIndex, int newIndex) => MoveItem(oldIndex, newIndex);
        public bool Remove(T item)
        {
            int index = IndexOf(item);
            if (index == -1) return false;
            RemoveItem(index);
            return true;
        }
        public void RemoveAt(int index) => RemoveItem(index);

        int IList.Add(object value)
        {
            Add((T)value);
            return Count - 1;
        }
        bool IList.Contains(object value) => value is T item ? Contains(item) : false;
        void ICollection.CopyTo(Array array, int index) => CopyTo((T[])array, index);
        int IList.IndexOf(object value) => value is T item ? IndexOf(item) : -1;
        void IList.Insert(int index, object value) => Insert(index, (T)value);
        void IList.Remove(object value) { if (value is T item) Remove(item); }

        public IEnumerator<T> GetEnumerator() => Items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Items.GetEnumerator();
    }

    public class ObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, INotifyDictionaryChanged<TKey, TValue>
    {
        protected Dictionary<TKey, TValue> Dictionary;
        protected ICollection<KeyValuePair<TKey, TValue>> DictionaryAsCollection => Dictionary;

        public event DictionaryChangedEventHandler<TKey, TValue> DictionaryChanged;

        public ObservableDictionary() => Dictionary = new Dictionary<TKey, TValue>();
        public ObservableDictionary(int capacity) => Dictionary = new Dictionary<TKey, TValue>(capacity);
        public ObservableDictionary(IEqualityComparer<TKey> comparer) => Dictionary = new Dictionary<TKey, TValue>(comparer);
        public ObservableDictionary(IDictionary<TKey, TValue> dictionary) => Dictionary = new Dictionary<TKey, TValue>(dictionary);
        public ObservableDictionary(int capacity, IEqualityComparer<TKey> comparer) => Dictionary = new Dictionary<TKey, TValue>(capacity, comparer);
        public ObservableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) => Dictionary = new Dictionary<TKey, TValue>(dictionary, comparer);

        public TValue this[TKey key]
        {
            get => Dictionary[key];
            set
            {
                var old = new KeyValuePair<TKey, TValue>(key, Dictionary[key]);
                SetItem(key, value);
                OnDictionaryChanged(new DictionaryChangedEventArgs<TKey, TValue>(NotifyDictionaryChangedAction.SetValue, new[] { old }, new[] { new KeyValuePair<TKey, TValue>(key, value) }));
            }
        }
        public ICollection<TKey> Keys => Dictionary.Keys;
        public ICollection<TValue> Values => Dictionary.Values;
        public int Count => Dictionary.Count;
        public bool IsReadOnly => DictionaryAsCollection.IsReadOnly;

        public void Add(TKey key, TValue value)
        {
            AddItem(key, value);
            OnDictionaryChanged(new DictionaryChangedEventArgs<TKey, TValue>(NotifyDictionaryChangedAction.Add, null, new[] { new KeyValuePair<TKey, TValue>(key, value) }));
        }
        public void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> pairs)
        {
            var kvps = pairs.ToArray();
            foreach (var kvp in kvps) AddItem(kvp.Key, kvp.Value);
            OnDictionaryChanged(new DictionaryChangedEventArgs<TKey, TValue>(NotifyDictionaryChangedAction.Add, null, kvps));
        }
        public void Clear()
        {
            ClearItems();
            OnDictionaryChanged(new DictionaryChangedEventArgs<TKey, TValue>(NotifyDictionaryChangedAction.Clear, null, null));
        }
        public bool ContainsKey(TKey key) => Dictionary.ContainsKey(key);
        public bool Remove(TKey key)
        {
            var old = new KeyValuePair<TKey, TValue>(key, Dictionary[key]);
            bool result = RemoveItem(key);
            OnDictionaryChanged(new DictionaryChangedEventArgs<TKey, TValue>(NotifyDictionaryChangedAction.Remove, new[] { old }, null));
            return result;
        }
        public bool RemoveRange(IEnumerable<TKey> keys)
        {
            var kvps = keys.Select(k => new KeyValuePair<TKey, TValue>(k, Dictionary[k])).ToArray();
            bool result = kvps.All(kvp => RemoveItem(kvp.Key));
            OnDictionaryChanged(new DictionaryChangedEventArgs<TKey, TValue>(NotifyDictionaryChangedAction.Remove, kvps, null));
            return result;
        }
        public bool TryGetValue(TKey key, out TValue value) => Dictionary.TryGetValue(key, out value);

        protected virtual void OnDictionaryChanged(DictionaryChangedEventArgs<TKey, TValue> e) => DictionaryChanged?.Invoke(this, e);

        protected virtual void AddItem(TKey key, TValue value) => Dictionary.Add(key, value);
        protected virtual bool RemoveItem(TKey key) => Dictionary.Remove(key);
        protected virtual void ClearItems() => Dictionary.Clear();
        protected virtual void SetItem(TKey key, TValue value) => Dictionary[key] = value;

        public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);
        public bool Contains(KeyValuePair<TKey, TValue> item) => DictionaryAsCollection.Contains(item);
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => DictionaryAsCollection.CopyTo(array, arrayIndex);
        public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => Dictionary.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Dictionary.GetEnumerator();
    }

    public interface INotifyDictionaryChanged<TKey, TValue> { event DictionaryChangedEventHandler<TKey, TValue> DictionaryChanged; }

    public delegate void DictionaryChangedEventHandler<TKey, TValue>(object sender, DictionaryChangedEventArgs<TKey, TValue> e);

    public class DictionaryChangedEventArgs<TKey, TValue> : EventArgs
    {
        public DictionaryChangedEventArgs(NotifyDictionaryChangedAction action, ICollection<KeyValuePair<TKey, TValue>> oldItems, ICollection<KeyValuePair<TKey, TValue>> newItems)
        {
            Action = action;
            OldItems = oldItems;
            NewItems = newItems;
        }

        public NotifyDictionaryChangedAction Action { get; }
        public ICollection<KeyValuePair<TKey, TValue>> OldItems { get; }
        public ICollection<KeyValuePair<TKey, TValue>> NewItems { get; }
    }

    public enum NotifyDictionaryChangedAction { Add, Remove, Clear, SetValue }

    public class VisualObjectCollection : NotifyObjectCollection<VisualObject>
    {
        internal bool IsLocked { get; set; }

        public VisualObjectCollection() : base() { }
        public VisualObjectCollection(IEnumerable<VisualObject> collection) : base(collection) { }
        public VisualObjectCollection(params VisualObject[] collection) : base(collection) { }

        public event EventHandler<VisualObjectSelectionChangedEventArgs> SelectionChanged;
        internal event EventHandler<VisualObjectSelectionChangedEventArgs> PreviewSelectionChanged;

        protected override void Register(VisualObject item)
        {
            if (IsLocked) throw new InvalidOperationException();
            if (item != null)
            {
                base.Register(item);
                item.PreviewSelectionChanged += Item_PreviewSelectionChanged;
                item.SelectionChanged += Item_SelectionChanged;
            }
        }

        protected override void UnRegister(VisualObject item)
        {
            if (IsLocked) throw new InvalidOperationException();
            if (item != null)
            {
                base.UnRegister(item);
                item.PreviewSelectionChanged -= Item_PreviewSelectionChanged;
                item.SelectionChanged -= Item_SelectionChanged;
            }
        }

        protected override void MoveItem(int oldIndex, int newIndex)
        {
            if (IsLocked) throw new InvalidOperationException();
            base.MoveItem(oldIndex, newIndex);
        }

        private void Item_SelectionChanged(object sender, VisualObjectSelectionChangedEventArgs e) => SelectionChanged?.Invoke(this, e);
        private void Item_PreviewSelectionChanged(object sender, VisualObjectSelectionChangedEventArgs e) => PreviewSelectionChanged?.Invoke(this, e);
    }
}
