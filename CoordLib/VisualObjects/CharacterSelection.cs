using BenLib.Standard;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Coord
{
    public class CharacterSelection : IReadOnlyDictionary<VisualObject, Interval<int>>
    {
        //public NotifyObjectCollection<CharacterEffect> Effects { get; } = new NotifyObjectCollection<CharacterEffect>();
        protected IDictionary<VisualObject, Interval<int>> Selection { get; }

        public CharacterSelection() : this(new Dictionary<VisualObject, Interval<int>>()) { }
        public CharacterSelection(IDictionary<VisualObject, Interval<int>> selection) => Selection = selection ?? new Dictionary<VisualObject, Interval<int>>(); //Effects.CollectionChanged += Effects_CollectionChanged;

        /*private void Effects_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null) foreach (CharacterEffect effect in e.OldItems) effect.Destroy();
            if (e.NewItems != null) foreach (CharacterEffect effect in e.NewItems) Selection.ForEach(kvp => kvp.Key.Effects.Add(effect, kvp.Value));
        }*/

        public CharacterSelection(params (VisualObject visualObject, Interval<int> selection)[] selection) : this(selection as IEnumerable<(VisualObject, Interval<int>)>) { }
        public CharacterSelection(IEnumerable<(VisualObject visualObject, Interval<int> selection)> selection) : this(selection?.ToDictionary(s => s.visualObject, s => s.selection)) { }

        public CharacterSelection(params Character[] selection) : this(selection as IEnumerable<Character>) { }
        public CharacterSelection(IEnumerable<Character> selection) : this(selection?.GroupBy(c => c.Owner)?.ToDictionary(group => group.Key, group => group.Union(c => Interval<int>.Single(c.Index)))) { }

        public CharacterSelection(IEnumerable<VisualObject> selection) : this(selection?.ToDictionary(v => v, v => v.Selection)) { }
        public CharacterSelection(params VisualObject[] selection) : this(selection as IEnumerable<VisualObject>) { }

        public bool ContainsAny(IEnumerable<Character> characters)
        {
            foreach (var c in characters) { if (Selection.TryGetValue(c.Owner, out var selection) && selection >= c.Index) return true; }
            return false;
        }
        public bool Contains(IEnumerable<Character> characters)
        {
            foreach (var c in characters) { if (!Selection.TryGetValue(c.Owner, out var selection) || !(selection >= c.Index)) return false; }
            return true;
        }

        public CharacterSelection AllAtOnce(bool condition) => condition ? new CharacterSelection(Selection.Select(kvp => (kvp.Key, (Interval<int>)Interval<int>.PositiveReals))) : new CharacterSelection(Selection);

        public static CharacterSelection operator +(CharacterSelection selection1, CharacterSelection selection2) => new CharacterSelection(selection1.Selection.Concat(selection2.Selection).GroupBy(kvp => kvp.Key).Select(group => (group.Key, group.Union(kvp => kvp.Value))));

        public IEnumerable<VisualObject> Keys => Selection.Keys;
        public IEnumerable<Interval<int>> Values => Selection.Values;

        public int Count => Selection.Count;

        public Interval<int> this[VisualObject key] => Selection[key];

        public bool ContainsKey(VisualObject key) => Selection.ContainsKey(key);
        public bool TryGetValue(VisualObject key, out Interval<int> value) => Selection.TryGetValue(key, out value);
        public IEnumerator<KeyValuePair<VisualObject, Interval<int>>> GetEnumerator() => Selection.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Selection.GetEnumerator();
    }

    public class TrackingCharacterSelection : CharacterSelection
    {
        private Character[] m_locked;

        public event EventHandler SelectionChanged;

        public Plane Plane { get; }

        public CharacterSelection Current => new CharacterSelection(Selection.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));

        public TrackingCharacterSelection(Plane plane) : base(plane.AllChildren())
        {
            Plane = plane;
            plane.Grid.SelectionChanged += VisualObjects_SelectionChanged;
            plane.Axes.SelectionChanged += VisualObjects_SelectionChanged;
            plane.AxesNumbers.SelectionChanged += VisualObjects_SelectionChanged;
            plane.VisualObjects.SelectionChanged += VisualObjects_SelectionChanged;
            plane.VisualObjects.CollectionChanged += VisualObjects_CollectionChanged;
        }

        public void Lock() => m_locked = Selection.SelectMany(kvp => kvp.Key.Cache.Characters?.SubCollection(kvp.Value, true) ?? Enumerable.Empty<Character>()).ToArray();
        public void UnLock() => m_locked = null;

        public void Select(IEnumerable<Character> characters, bool allAtOnce)
        {
            Plane.RenderAtSelectionChange = false;
            if (allAtOnce) foreach (var group in characters.GroupBy(c => c.Owner)) group.Key.Selection = Interval<int>.PositiveReals;
            else foreach (var character in characters) character.IsSelected = true;
            Plane.RenderAtSelectionChange = true;
            Plane.RenderChanged();
        }
        public void UnSelect(IEnumerable<Character> characters, bool allAtOnce)
        {
            Plane.RenderAtSelectionChange = false;
            var chars = m_locked.IsNullOrEmpty() ? characters : characters.Except(m_locked);
            if (allAtOnce) foreach (var group in chars.GroupBy(c => c.Owner)) group.Key.Selection = Interval<int>.EmptySet;
            else foreach (var character in chars) character.IsSelected = false;
            Plane.RenderAtSelectionChange = true;
            Plane.RenderChanged();
        }

        public void ClearSelection() { foreach (var visualObject in Selection.Keys.ToArray()) visualObject.ClearSelection(); }

        public void PushEffect(CharacterEffect effect) { foreach (var kvp in Selection) kvp.Key.Effects.Add(effect, kvp.Value); }

        private void VisualObjects_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset) Selection.Clear();
            else
            {
                if (e.OldItems != null)
                    foreach (VisualObject visualObject in e.OldItems)
                    {
                        if (Selection.ContainsKey(visualObject))
                        {
                            Selection.Remove(visualObject);
                            SelectionChanged?.Invoke(visualObject, EventArgs.Empty);
                        }
                    }

                if (e.NewItems != null)
                    foreach (VisualObject visualObject in e.NewItems)
                    {
                        /*if (Selection.ContainsKey(visualObject))
                        {
                            if (visualObject.Selection.IsNullOrEmpty()) Selection.Remove(visualObject);
                            else Selection[visualObject] = visualObject.Selection;
                            SelectionChanged?.Invoke(this, EventArgs.Empty);
                        }
                        else */if (!visualObject.Selection.IsNullOrEmpty())
                        {
                            Selection.Add(visualObject, visualObject.Selection);
                            SelectionChanged?.Invoke(visualObject, EventArgs.Empty);
                        }
                    }
            }
        }

        private void VisualObjects_SelectionChanged(object sender, PropertyChangedExtendedEventArgs<Interval<int>> e)
        {
            if (sender is VisualObject visualObject)
            {
                if (Selection.ContainsKey(visualObject))
                {
                    if (e.NewValue.IsNullOrEmpty()) Selection.Remove(visualObject);
                    else Selection[visualObject] = e.NewValue;
                }
                else Selection.Add(visualObject, e.NewValue);
                SelectionChanged?.Invoke(visualObject, EventArgs.Empty);
            }
        }
    }
}
