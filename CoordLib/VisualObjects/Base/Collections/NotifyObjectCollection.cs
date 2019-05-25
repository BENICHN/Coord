using BenLib.Standard;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Coord
{
    public interface INotifyItemChanged { event EventHandler ItemChanged; }

    public class NotifyObjectCollection<T> : ObservableRangeCollection<T>, INotifyItemChanged where T : NotifyObject
    {
        public event EventHandler ItemChanged;

        public NotifyObjectCollection() { }
        public NotifyObjectCollection(IEnumerable<T> collection) : base(collection) { foreach (var item in collection) Register(item); }
        public NotifyObjectCollection(List<T> list) : base(list) { foreach (var item in list) Register(item); }

        protected virtual void Register(T item)
        {
            if (item != null)
            {
                item.Destroyed += OnItemDestroyed;
                item.Changed += OnItemChanged;
            }
        }
        protected virtual void UnRegister(T item)
        {
            if (item != null)
            {
                item.Destroyed -= OnItemDestroyed;
                item.Changed -= OnItemChanged;
            }
        }

        private void OnItemDestroyed(object sender, EventArgs e) => Remove((T)sender);
        private void OnItemChanged(object sender, EventArgs e) => ItemChanged?.Invoke(sender, e);

        protected override void ClearItems()
        {
            foreach (var item in Items) UnRegister(item);
            base.ClearItems();
        }
        protected override void InsertItem(int index, T item)
        {
            Register(item);
            base.InsertItem(index, item);
        }
        protected override void RemoveItem(int index)
        {
            UnRegister(Items[index]);
            base.RemoveItem(index);
        }
        protected override void SetItem(int index, T item)
        {
            UnRegister(Items[index]);
            Register(item);
            base.SetItem(index, item);
        }
    }

    public class CharacterEffectDictionary : ObservableDictionary<CharacterEffect, Interval<int>>, INotifyItemChanged
    {
        public event EventHandler ItemChanged;

        public CharacterEffectDictionary() { }
        public CharacterEffectDictionary(int capacity) : base(capacity) { }
        public CharacterEffectDictionary(IEqualityComparer<CharacterEffect> comparer) : base(comparer) { }
        public CharacterEffectDictionary(IDictionary<CharacterEffect, Interval<int>> dictionary) : base(dictionary) { }
        public CharacterEffectDictionary(int capacity, IEqualityComparer<CharacterEffect> comparer) : base(capacity, comparer) { }
        public CharacterEffectDictionary(IDictionary<CharacterEffect, Interval<int>> dictionary, IEqualityComparer<CharacterEffect> comparer) : base(dictionary, comparer) { }

        protected virtual void Register(CharacterEffect item)
        {
            item.Destroyed += OnItemDestroyed;
            item.Changed += OnItemChanged;
        }
        protected virtual void UnRegister(CharacterEffect item)
        {
            item.Destroyed -= OnItemDestroyed;
            item.Changed -= OnItemChanged;
        }

        private void OnItemDestroyed(object sender, EventArgs e) => Remove((CharacterEffect)sender);
        private void OnItemChanged(object sender, EventArgs e) => ItemChanged?.Invoke(sender, e);

        protected override void ClearItems()
        {
            foreach (var item in Dictionary.Keys) UnRegister(item);
            base.ClearItems();
        }

        protected override void AddItem(CharacterEffect key, Interval<int> value)
        {
            Register(key);
            base.AddItem(key, value);
        }

        protected override bool RemoveItem(CharacterEffect key)
        {
            UnRegister(key);
            return base.RemoveItem(key);
        }
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
        public VisualObjectCollection() : base() { }
        public VisualObjectCollection(IEnumerable<VisualObject> collection) : base(collection) { }
        public VisualObjectCollection(params VisualObject[] collection) : base(collection) { }

        public event PropertyChangedExtendedEventHandler<Interval<int>> SelectionChanged;

        protected override void Register(VisualObject item)
        {
            if (item != null)
            {
                base.Register(item);
                item.SelectionChanged += Item_SelectionChanged;
            }
        }
        protected override void UnRegister(VisualObject item)
        {
            if (item != null)
            {
                base.UnRegister(item);
                item.SelectionChanged -= Item_SelectionChanged;
            }
        }

        private void Item_SelectionChanged(object sender, PropertyChangedExtendedEventArgs<Interval<int>> e) => SelectionChanged?.Invoke(sender, e);
    }
}
