using BenLib.Standard;
using BenLib.WPF;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using static BenLib.Standard.Interval<int>;

namespace Coord
{
    public abstract class VisualObject : NotifyObject
    {
        public event PropertyChangedExtendedEventHandler<Character[]> CacheChanged;

        public event EventHandler<VisualObjectSelectionChangedEventArgs> SelectionChanged;
        internal event EventHandler<VisualObjectSelectionChangedEventArgs> PreviewSelectionChanged;

        public abstract string Type { get; }
        public bool IsChanged { get; private set; }

        public static Pen SelectionStroke = new Pen(FlatBrushes.Asbestos, 2);

        public bool IsSelectable { get => (bool)GetValue(IsSelectableProperty); set => SetValue(IsSelectableProperty, value); }
        public static readonly DependencyProperty IsSelectableProperty = CreateProperty<VisualObject, bool>(true, true, false, "IsSelectable", true);

        public bool? RenderAtChange { get => (bool?)GetValue(RenderAtChangeProperty); set => SetValue(RenderAtChangeProperty, value); }
        public static readonly DependencyProperty RenderAtChangeProperty = CreateProperty<VisualObject, bool?>(false, false, false, "RenderAtChange", (bool?)null);

        public bool? RenderAtSelectionChange { get => (bool?)GetValue(RenderAtSelectionChangeProperty); set => SetValue(RenderAtSelectionChangeProperty, value); }
        public static readonly DependencyProperty RenderAtSelectionChangeProperty = CreateProperty<VisualObject, bool?>(false, false, false, "RenderAtSelectionChange", (bool?)null);

        public Brush Fill { get => (Brush)GetValue(FillProperty); set => SetValue(FillProperty, value); }
        public static readonly DependencyProperty FillProperty = CreateProperty<VisualObject, Brush>(true, true, true, "Fill");

        public Pen Stroke { get => (Pen)GetValue(StrokeProperty); set => SetValue(StrokeProperty, value); }
        public static readonly DependencyProperty StrokeProperty = CreateProperty<VisualObject, Pen>(true, true, true, "Stroke");

        public VisualObjectCollection Children { get => (VisualObjectCollection)GetValue(ChildrenProperty); set => SetValue(ChildrenProperty, value); }
        public static readonly DependencyProperty ChildrenProperty = CreateProperty<VisualObject, VisualObjectCollection>(true, true, true, "Children");

        public VisualObjectChildrenRenderingMode ChildrenRenderingMode { get => (VisualObjectChildrenRenderingMode)GetValue(ChildrenRenderingModeProperty); set => SetValue(ChildrenRenderingModeProperty, value); }
        public static readonly DependencyProperty ChildrenRenderingModeProperty = CreateProperty<VisualObject, VisualObjectChildrenRenderingMode>(true, true, true, "ChildrenRenderingMode");

        public CharacterEffectDictionary Effects => (CharacterEffectDictionary)GetValue(EffectsProperty);
        public static readonly DependencyProperty EffectsProperty = CreateProperty<VisualObject, CharacterEffectDictionary>(true, false, true, "Effects");

        private (Character[] Characters, ReadOnlyCoordinatesSystemManager CoordinatesSystemManager) m_cache;
        public virtual (Character[] Characters, ReadOnlyCoordinatesSystemManager CoordinatesSystemManager) Cache
        {
            get => m_cache;
            private set
            {
                var old = m_cache.Characters;
                m_cache = value;
                CacheChanged?.Invoke(this, new PropertyChangedExtendedEventArgs<Character[]>("Cache", old, value.Characters));
            }
        }

        public Interval<int> Selection { get => (Interval<int>)GetValue(SelectionProperty); set => SetValue(SelectionProperty, value); }
        public static readonly DependencyProperty SelectionProperty = CreateProperty<VisualObject, Interval<int>>(false, false, false, "Selection", EmptySet, null, (d, v) => ((VisualObject)d).CoerceSelection((Interval<int>)v));

        private Interval<int> CoerceSelection(Interval<int> selection) => IsSelectable ? selection /*/ (Cache.Characters?.Where(c => !c.IsSelectable)?.ToSelection() ?? EmptySet)*/ : EmptySet;

        public VisualObject() => SetValue(EffectsProperty, new CharacterEffectDictionary());

        protected override void OnDestroyed()
        {
            ClearCache();
            ClearSelection();
        }

        protected override void OnChanged()
        {
            IsChanged = true;
            ClearCache();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == SelectionProperty) NotifySelectionChanged(new VisualObjectSelectionChangedEventArgs(this, (Interval<int>)e.OldValue, (Interval<int>)e.NewValue));
            if (e.Property == ChildrenProperty)
            {
                if (e.OldValue is VisualObjectCollection oldValue)
                {
                    oldValue.PreviewSelectionChanged -= OnChildrenPreviewSelectionChanged;
                    oldValue.SelectionChanged -= OnChildrenSelectionChanged;
                }
                if (e.NewValue is VisualObjectCollection newValue)
                {
                    newValue.PreviewSelectionChanged += OnChildrenPreviewSelectionChanged;
                    newValue.SelectionChanged += OnChildrenSelectionChanged;
                }
            }
            base.OnPropertyChanged(e);
        }

        private void OnChildrenPreviewSelectionChanged(object sender, VisualObjectSelectionChangedEventArgs e) => PreviewSelectionChanged?.Invoke(this, e);
        private void OnChildrenSelectionChanged(object sender, VisualObjectSelectionChangedEventArgs e) => NotifySelectionChanged(e);

        private void NotifySelectionChanged(VisualObjectSelectionChangedEventArgs e)
        {
            var diff = e.OldValue ^ e.NewValue;
            if (!diff.IsEmpty)
            {
                var temp = e.NewValue;
                PreviewSelectionChanged?.Invoke(this, e);
                var curr = CoerceSelection(e.NewValue);
                if (curr == temp)
                {
                    IsChanged = true;
                    SelectionChanged?.Invoke(this, e);
                    if (e.OriginalSource == this && !Cache.Characters.IsNullOrEmpty()) { foreach (var character in Cache.Characters.SubCollection(diff, true)) character.NotifyIsSelectedChanged(); }
                }
                else Selection = curr;
            }
        }

        public void ClearCache()
        {
            if (Children is VisualObjectCollection children) foreach (var visualObject in children) visualObject.ClearCache();
            Cache = default;
        }

        public void ClearSelection()
        {
            if (Children is VisualObjectCollection children) foreach (var visualObject in children) visualObject.ClearSelection();
            Selection = EmptySet;
        }

        public virtual IEnumerable<Character> HitTestCache(Point point)
        {
            var characters = Cache.Characters?.HitTest(point) ?? Enumerable.Empty<Character>();
            return (ChildrenRenderingMode == VisualObjectChildrenRenderingMode.Independent && Children is VisualObjectCollection children) ? children.SelectMany(visualObject => visualObject.HitTestCache(point)).Concat(characters) : characters;
        }

        public virtual IEnumerable<Character> HitTestCache(Rect rect)
        {
            var characters = Cache.Characters?.HitTest(rect) ?? Enumerable.Empty<Character>();
            return (ChildrenRenderingMode == VisualObjectChildrenRenderingMode.Independent && Children is VisualObjectCollection children) ? children.SelectMany(visualObject => visualObject.HitTestCache(rect)).Concat(characters) : characters;
        }

        public void Render(DrawingContext drawingContext, ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            if (ChildrenRenderingMode == VisualObjectChildrenRenderingMode.Independent && Children is VisualObjectCollection children) { foreach (var visualObject in children) visualObject.Render(drawingContext, coordinatesSystemManager); }
            RenderCore(drawingContext, coordinatesSystemManager);
            IsChanged = false;
        }
        protected virtual void RenderCore(DrawingContext drawingContext, ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            foreach (var character in GetTransformedCharacters(coordinatesSystemManager)) drawingContext.DrawCharacter(character);
            Debug.WriteLine($"{Type} rendered");
        }

        protected IEnumerable<Character> GetChildrenTransformedCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager) => Children.SelectMany(visualObject => visualObject.GetTransformedCharacters(coordinatesSystemManager));

        protected virtual IReadOnlyCollection<Character> GetCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager) => GetCharactersCore(coordinatesSystemManager)?.ToArray();
        protected virtual IEnumerable<Character> GetCharactersCore(ReadOnlyCoordinatesSystemManager coordinatesSystemManager) => null;

        public IReadOnlyCollection<Character> GetTransformedCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            if (Cache.Characters == null || Cache.CoordinatesSystemManager != coordinatesSystemManager)
            {
                IReadOnlyCollection<Character> characters;
                try
                {
                    characters = GetCharacters(coordinatesSystemManager);
                    if (ChildrenRenderingMode == VisualObjectChildrenRenderingMode.Embedded && Children is VisualObjectCollection children)
                    {
                        var childrenCharacters = children.SelectMany(visualObject => visualObject.GetTransformedCharacters(coordinatesSystemManager));
                        characters = characters == null ? childrenCharacters.ToArray() : childrenCharacters.Concat(characters).ToArray();
                    }
                }
                catch { characters = null; }

                if (characters == null)
                {
                    Cache = (null, coordinatesSystemManager);
                    return Array.Empty<Character>();
                }
                else
                {
                    if (!Effects.IsNullOrEmpty()) foreach (var kvp in Effects) kvp.Key.Apply(characters, kvp.Value, coordinatesSystemManager);
                    foreach (var character in characters)
                    {
                        character.ApplyTransforms();
                        character.Stroke = character.Stroke?.GetOutPen(coordinatesSystemManager);
                    }
                    Cache = (characters.AttachCharacters(this).ToArray(), coordinatesSystemManager);
                }
            }
            return Cache.Characters.CloneCharacters().ToArray();
        }

        protected internal virtual void Move(Point inPosition, Vector totalInOffset, Vector inOffset, Character clickHitTest) { }
        protected internal virtual void OnMouseDown(Point inPosition, Character hitTest) { }
        protected internal virtual void OnMouseUp(Point inPosition, Character hitTest) { }
        protected internal virtual void OnMouseEnter(Point inPosition, Character hitTest) { }
        protected internal virtual void OnMouseLeave(Point inPosition, Character hitTest) { }

        public override string ToString() => Type;
    }

    public enum VisualObjectChildrenRenderingMode { Discard, Independent, Embedded }

    public class VisualObjectSelectionChangedEventArgs : EventArgs
    {
        public VisualObjectSelectionChangedEventArgs(VisualObject originalSource, Interval<int> oldValue, Interval<int> newValue)
        {
            OriginalSource = originalSource;
            OldValue = oldValue;
            NewValue = newValue;
        }

        public VisualObject OriginalSource { get; }
        public Interval<int> OldValue { get; }
        public Interval<int> NewValue { get; internal set; }
    }

    public abstract class VisualObjectRendererBase : VisualObject { static VisualObjectRendererBase() => OverrideDefaultValue<VisualObjectRendererBase, VisualObjectChildrenRenderingMode>(ChildrenRenderingModeProperty, VisualObjectChildrenRenderingMode.Independent); }
    public abstract class VisualObjectGroupBase : VisualObject { static VisualObjectGroupBase() => OverrideDefaultValue<VisualObjectGroupBase, VisualObjectChildrenRenderingMode>(ChildrenRenderingModeProperty, VisualObjectChildrenRenderingMode.Embedded); }

    public sealed class VisualObjectContainer : VisualObject
    {
        public override string Type => "VisualObjectContainer";
        protected override Freezable CreateInstanceCore() => new VisualObjectContainer();
    }
}
