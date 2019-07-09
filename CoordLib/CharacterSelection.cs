using BenLib.Framework;
using BenLib.Standard;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using static BenLib.Standard.Interval<int>;

namespace Coord
{
    public class CharacterSelection : NotifyObject, IReadOnlyDictionary<VisualObject, Interval<int>>
    {
        protected override Freezable CreateInstanceCore() => new CharacterSelection();

        protected IDictionary<VisualObject, Interval<int>> Selection { get; }

        public IEnumerable<VisualObject> Keys => Selection.Keys;
        public IEnumerable<Interval<int>> Values => Selection.Values;

        public int Count => Selection.Count;

        public Interval<int> this[VisualObject key] => Selection[key];

        public bool ContainsKey(VisualObject key) => Selection.ContainsKey(key);
        public bool TryGetValue(VisualObject key, out Interval<int> value) => Selection.TryGetValue(key, out value);
        public IEnumerator<KeyValuePair<VisualObject, Interval<int>>> GetEnumerator() => Selection.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Selection.GetEnumerator();

        public IEnumerable<VisualObject> VisualObjects => Selection.Select(kvp => kvp.Key);

        public CharacterSelection() : this(new Dictionary<VisualObject, Interval<int>>()) { }
        public CharacterSelection(IDictionary<VisualObject, Interval<int>> selection) => Selection = selection ?? new Dictionary<VisualObject, Interval<int>>();

        public CharacterSelection(params (VisualObject visualObject, Interval<int> selection)[] selection) : this(selection as IEnumerable<(VisualObject, Interval<int>)>) { }
        public CharacterSelection(IEnumerable<(VisualObject visualObject, Interval<int> selection)> selection) : this(selection.Where(t => !t.selection.IsEmpty).ToDictionary(s => s.visualObject, s => s.selection)) { }

        public CharacterSelection(params Character[] selection) : this(selection as IEnumerable<Character>) { }
        public CharacterSelection(IEnumerable<Character> selection) : this(selection.GroupBy(c => c.Owner).ToDictionary(group => group.Key, group => group.ToSelection())) { }

        public CharacterSelection(IEnumerable<VisualObject> selection) : this(selection.Where(vo => !vo.Selection.IsEmpty).ToDictionary(v => v, v => v.Selection)) { }
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

        public static CharacterSelection operator +(CharacterSelection left, CharacterSelection right) => new CharacterSelection(left.Selection.Concat(right.Selection).GroupBy(kvp => kvp.Key).Select(group => (group.Key, group.Union(kvp => kvp.Value))));
    }

    public class TrackingCharacterSelection : CharacterSelection
    {
        private bool m_allAtOnce;
        private bool m_allowMultiple;
        private Predicate<VisualObject> m_filter;

        public new event EventHandler<VisualObjectSelectionChangedEventArgs> Changed;
        public event EventHandler<EventArgs<VisualObject>> ObjectPointed;
        private Character[] m_locked;

        public Plane Plane { get; }
        public new ObservableCollection<VisualObject> VisualObjects { get; } = new ObservableCollection<VisualObject>();
        public CharacterSelection Current
        {
            get => new CharacterSelection(Selection.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
            set
            {
                foreach (var vo in Selection.Keys.Except(value.Keys).ToArray()) vo.ClearSelection();
                foreach (var kvp in value) kvp.Key.Selection = kvp.Value;
            }
        }

        public bool IsPointing { get; private set; }

        public bool AllowMultiple { get => (bool)GetValue(AllowMultipleProperty); set => SetValue(AllowMultipleProperty, value); }
        public static readonly DependencyProperty AllowMultipleProperty = CreateProperty<TrackingCharacterSelection, bool>(true, true, true, "AllowMultiple", true, null, (d, v) => d is TrackingCharacterSelection selection ? selection.IsPointing ? selection.AllowMultiple : v : v);

        public bool AllAtOnce { get => (bool)GetValue(AllAtOnceProperty); set => SetValue(AllAtOnceProperty, value); }
        public static readonly DependencyProperty AllAtOnceProperty = CreateProperty<TrackingCharacterSelection, bool>(true, true, true, "AllAtOnce", false, null, (d, v) => d is TrackingCharacterSelection selection ? selection.IsPointing ? selection.AllAtOnce : v : v);

        public Predicate<VisualObject> Filter { get => (Predicate<VisualObject>)GetValue(FilterProperty); set => SetValue(FilterProperty, value); }
        public static readonly DependencyProperty FilterProperty = CreateProperty<TrackingCharacterSelection, Predicate<VisualObject>>(true, true, true, "Filter", null, null, (d, v) => d is TrackingCharacterSelection selection ? selection.IsPointing ? selection.Filter : v : v);

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == AllowMultipleProperty) { if (!(bool)e.NewValue) ClearSelection(); }
            else if (e.Property == AllAtOnceProperty) { if ((bool)e.NewValue) { foreach (var vo in VisualObjects.Where(vo => vo.Selection < PositiveReals).ToArray()) vo.Selection = PositiveReals; } }
            else if (e.Property == FilterProperty)
            {
                var filter = Filter ?? (vo => true);
                foreach (var vo in VisualObjects.Where(vo => !filter(vo)).ToArray()) vo.ClearSelection();
            }
            base.OnPropertyChanged(e);
        }

        public TrackingCharacterSelection(Plane plane) : base(plane.VisualObjects)
        {
            Plane = plane;
            plane.Grid.PreviewSelectionChanged += VisualObjects_PreviewSelectionChanged;
            plane.Axes.PreviewSelectionChanged += VisualObjects_PreviewSelectionChanged;
            plane.AxesNumbers.PreviewSelectionChanged += VisualObjects_PreviewSelectionChanged;
            plane.VisualObjects.PreviewSelectionChanged += VisualObjects_PreviewSelectionChanged;
            plane.Grid.SelectionChanged += VisualObjects_SelectionChanged;
            plane.Axes.SelectionChanged += VisualObjects_SelectionChanged;
            plane.AxesNumbers.SelectionChanged += VisualObjects_SelectionChanged;
            plane.VisualObjects.SelectionChanged += VisualObjects_SelectionChanged;
            plane.VisualObjects.CollectionChanged += VisualObjects_CollectionChanged;
            Changed += (sender, e) => NotifyChanged();
        }

        public bool EnablePointing(Type type)
        {
            if (!IsPointing && typeof(VisualObject).IsAssignableFrom(type))
            {
                m_allowMultiple = AllowMultiple;
                AllowMultiple = false;
                m_allAtOnce = AllAtOnce;
                AllAtOnce = true;
                m_filter = Filter;
                Filter = vo => type.IsAssignableFrom(vo.GetType());
                IsPointing = true;
                return true;
            }
            else return false;
        }

        public void DisablePointing()
        {
            if (IsPointing)
            {
                IsPointing = false;
                Filter = m_filter;
                AllowMultiple = m_allowMultiple;
                AllAtOnce = m_allAtOnce;
            }
        }

        public void Lock() => m_locked = Selection.SelectMany(kvp => kvp.Key.Cache.Characters?.SubCollection(kvp.Value, true) ?? Enumerable.Empty<Character>()).ToArray();
        public void UnLock() => m_locked = null;

        public void Select(IEnumerable<Character> characters, bool allAtOnce)
        {
            Plane.RenderAtSelectionChange = false;
            if (allAtOnce || AllAtOnce) foreach (var group in characters/*.Where(c => c.IsSelectable)*/.GroupBy(c => c.Owner)) group.Key.Selection = PositiveReals;
            else foreach (var character in characters/*.Where(c => c.IsSelectable)*/) character.IsSelected = true;
            Plane.RenderAtSelectionChange = true;
            Plane.RenderChanged();
        }
        public void UnSelect(IEnumerable<Character> characters, bool allAtOnce, Character[] hitTest)
        {
            Plane.RenderAtSelectionChange = false;
            var chars = m_locked.IsNullOrEmpty() ? characters : characters.Except(m_locked);
            if (allAtOnce || AllAtOnce) foreach (var vo in chars.GroupBy(c => c.Owner).Select(group => group.Key).Where(vo => hitTest.All(c => c.Owner != vo))) vo.ClearSelection();
            else foreach (var character in chars) character.IsSelected = false;
            Plane.RenderAtSelectionChange = true;
            Plane.RenderChanged();
        }

        public void ClearSelection() { while (VisualObjects.Count > 0) VisualObjects[0].ClearSelection(); }

        public void PushEffect(CharacterEffect effect) { foreach (var kvp in Selection) kvp.Key.Effects.Add((effect, kvp.Value)); }
        public void PushTransform(Transform transform)
        {
            foreach (var kvp in Selection)
            {
                var transforms = kvp.Key.Transforms;
                /*var lastTransform = transforms.LastOrDefault()?.Object;
                switch (transform)
                {
                    case TranslateTransform translateTransform:
                        if (lastTransform is TranslateTransform lastTranslateTransform)
                        {
                            lastTranslateTransform.Offset += translateTransform.Offset;
                            return;
                        }
                        break;
                    case ScaleTransform scaleTransform:
                        if (lastTransform is ScaleTransform lastScaleTransform && lastScaleTransform.Center == scaleTransform.Center && (lastScaleTransform.RectPoint == scaleTransform.RectPoint || !scaleTransform.Center.IsNaN()))
                        {
                            lastScaleTransform.ScaleX *= scaleTransform.ScaleX;
                            lastScaleTransform.ScaleY *= scaleTransform.ScaleY;
                            return;
                        }
                        break;
                    case RotateTransform rotateTransform:
                        if (lastTransform is RotateTransform lastRotateTransform && lastRotateTransform.Center == rotateTransform.Center && (lastRotateTransform.RectPoint == rotateTransform.RectPoint || !rotateTransform.Center.IsNaN()))
                        {
                            lastRotateTransform.Angle *= rotateTransform.Angle;
                            return;
                        }
                        break;
                    case SkewTransform skewTransform:
                        if (lastTransform is SkewTransform lastSkewTransform)
                        {
                            lastSkewTransform.SkewX += skewTransform.SkewX;
                            lastSkewTransform.SkewY += skewTransform.SkewY;
                            return;
                        }
                        break;
                    case MatrixTransform matrixTransform:
                        if (lastTransform is MatrixTransform lastMatrixTransform)
                        {
                            lastMatrixTransform.Matrix *= matrixTransform.Matrix;
                            return;
                        }
                        break;
                }*/
                transforms.Add((transform, kvp.Value));
            }
        }

        private void VisualObjects_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset) Selection.Clear();
            else
            {
                if (e.OldItems != null)
                {
                    foreach (VisualObject visualObject in e.OldItems)
                    {
                        if (Selection.TryGetValue(visualObject, out var old))
                        {
                            Selection.Remove(visualObject);
                            VisualObjects.Remove(visualObject);
                            Changed?.Invoke(visualObject, new VisualObjectSelectionChangedEventArgs(visualObject, old, null));
                        }
                    }
                }

                if (e.NewItems != null)
                {
                    foreach (VisualObject visualObject in e.NewItems)
                    {
                        if (!visualObject.Selection.IsNullOrEmpty())
                        {
                            if (!AllowMultiple) ClearSelection();
                            Selection.Add(visualObject, visualObject.Selection);
                            VisualObjects.Add(visualObject);
                            Changed?.Invoke(visualObject, new VisualObjectSelectionChangedEventArgs(visualObject, null, visualObject.Selection));
                        }
                    }
                }
            }
        }

        private void VisualObjects_PreviewSelectionChanged(object sender, VisualObjectSelectionChangedEventArgs e) => e.NewValue = !(Filter?.Invoke(e.OriginalSource) ?? true) ? EmptySet : AllAtOnce && !e.NewValue.IsNullOrEmpty() ? PositiveReals : e.NewValue;

        private void VisualObjects_SelectionChanged(object sender, VisualObjectSelectionChangedEventArgs e)
        {
            var visualObject = e.OriginalSource;
            if (Selection.ContainsKey(visualObject))
            {
                if (e.NewValue.IsNullOrEmpty())
                {
                    Selection.Remove(visualObject);
                    VisualObjects.Remove(visualObject);
                }
                else Selection[visualObject] = e.NewValue;
            }
            else if (!e.NewValue.IsNullOrEmpty())
            {
                if (!AllowMultiple) ClearSelection();
                Selection.Add(visualObject, e.NewValue);
                VisualObjects.Add(visualObject);
                if (IsPointing) ObjectPointed?.Invoke(this, EventArgsHelper.Create(e.OriginalSource));
            }
            Changed?.Invoke(this, e);
        }
    }
}
