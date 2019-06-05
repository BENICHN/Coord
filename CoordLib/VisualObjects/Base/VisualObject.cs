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
        public event PropertyChangedExtendedEventHandler<(Character[] Characters, ReadOnlyCoordinatesSystemManager ReadOnlyCoordinatesSystemManager)> CacheChanged;
        public event PropertyChangedExtendedEventHandler<Interval<int>> SelectionChanged;

        internal Func<VisualObject, Interval<int>, Interval<int>> CoerceSelection { get; set; }

        public abstract string Type { get; }
        public bool IsChanged { get; private set; }

        public static PlanePen SelectionStroke = new PlanePen(FlatBrushes.Asbestos, 2);

        public bool RenderAtChange { get => (bool)GetValue(RenderAtChangeProperty); set => SetValue(RenderAtChangeProperty, value); }
        public static readonly DependencyProperty RenderAtChangeProperty = CreateProperty(false, false, "RenderAtChange", typeof(VisualObject), true);

        public bool RenderAtSelectionChange { get => (bool)GetValue(RenderAtSelectionChangeProperty); set => SetValue(RenderAtSelectionChangeProperty, value); }
        public static readonly DependencyProperty RenderAtSelectionChangeProperty = CreateProperty(false, false, "RenderAtSelectionChange", typeof(VisualObject), true);

        public Brush Fill { get => (Brush)GetValue(FillProperty); set => SetValue(FillProperty, value); }
        public static readonly DependencyProperty FillProperty = CreateProperty<Brush>(true, true, "Fill", typeof(VisualObject));

        public PlanePen Stroke { get => (PlanePen)GetValue(StrokeProperty); set => SetValue(StrokeProperty, value); }
        public static readonly DependencyProperty StrokeProperty = CreateProperty<PlanePen>(true, true, "Stroke", typeof(VisualObject));

        public CharacterEffectDictionary Effects => (CharacterEffectDictionary)GetValue(EffectsProperty);
        public static readonly DependencyProperty EffectsProperty = CreateProperty<CharacterEffectDictionary>(true, false, "Effects", typeof(VisualObject));

        private (Character[] Characters, ReadOnlyCoordinatesSystemManager ReadOnlyCoordinatesSystemManager) m_cache;
        public virtual (Character[] Characters, ReadOnlyCoordinatesSystemManager ReadOnlyCoordinatesSystemManager) Cache
        {
            get => m_cache;
            private set
            {
                var old = m_cache;
                m_cache = value;
                CacheChanged?.Invoke(this, new PropertyChangedExtendedEventArgs<(Character[] Characters, ReadOnlyCoordinatesSystemManager ReadOnlyCoordinatesSystemManager)>("Cache", old, value));
            }
        }

        public Interval<int> Selection { get => (Interval<int>)GetValue(SelectionProperty); set => SetValue(SelectionProperty, value); }
        public static readonly DependencyProperty SelectionProperty = CreateProperty<Interval<int>>(false, false, "Selection", typeof(VisualObject), EmptySet, (sender, e) =>
        {
            if (sender is VisualObject owner)
            {
                var oldValue = e.OldValue as Interval<int>;
                var newValue = e.NewValue as Interval<int>;
                var diff = oldValue ^ newValue;
                if (!diff.IsEmpty)
                {
                    owner.NotifySelectionChanged(oldValue, newValue);
                    if (!owner.Cache.Characters.IsNullOrEmpty()) { foreach (var character in owner.Cache.Characters.SubCollection(diff, true)) character.NotifyIsSelectedChanged(); }
                }
            }
        }, (d, v) => d is VisualObject visualObject && v is Interval<int> selection ? ((visualObject.CoerceSelection == null ? selection : visualObject.CoerceSelection(visualObject, selection)) & (visualObject.Cache.Characters?.Where(c => c.IsSelectable)?.ToSelection() ?? Reals)) : EmptySet);

        public VisualObject() => SetValue(EffectsProperty, new CharacterEffectDictionary());

        protected override void OnChanged()
        {
            IsChanged = true;
            Cache = default;
        }

        protected void NotifySelectionChanged(Interval<int> oldValue, Interval<int> newValue, object sender = null)
        {
            IsChanged = true;
            SelectionChanged?.Invoke(sender ?? this, new PropertyChangedExtendedEventArgs<Interval<int>>("Selection", oldValue, newValue));
        }

        public virtual void ClearSelection() => Selection = EmptySet;

        public virtual IEnumerable<Character> HitTestCache(Point point) => Cache.Characters?.HitTest(point) ?? Enumerable.Empty<Character>();
        public virtual IEnumerable<Character> HitTestCache(Rect rect) => Cache.Characters?.HitTest(rect) ?? Enumerable.Empty<Character>();

        public void Render(DrawingContext drawingContext, ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            RenderCore(drawingContext, coordinatesSystemManager);
            IsChanged = false;
        }
        protected virtual void RenderCore(DrawingContext drawingContext, ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            foreach (var character in GetTransformedCharacters(coordinatesSystemManager)) drawingContext.DrawCharacter(character);
            Debug.WriteLine($"{Type} rendered");
        }

        protected abstract IReadOnlyCollection<Character> GetCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager);
        public IReadOnlyCollection<Character> GetTransformedCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            if (Cache.Characters == null || Cache.ReadOnlyCoordinatesSystemManager != coordinatesSystemManager)
            {
                var characters = GetCharacters(coordinatesSystemManager);
                if (!Effects.IsNullOrEmpty()) foreach (var kvp in Effects) kvp.Key.Apply(characters, kvp.Value, coordinatesSystemManager);
                foreach (var character in characters)
                {
                    character.ApplyTransforms();
                    character.Stroke = character.Stroke?.GetOutPen(coordinatesSystemManager);
                }
                Cache = (characters.AttachCharacters(this).ToArray(), coordinatesSystemManager);
            }
            return Cache.Characters.CloneCharacters().ToArray();
        }

        public virtual void Move(Vector inOffset) { }

        public override string ToString() => Type;
    }
}
