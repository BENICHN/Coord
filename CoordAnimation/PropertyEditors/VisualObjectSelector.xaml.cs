using BenLib.Standard;
using Coord;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using static Coord.VisualObjects;

namespace CoordAnimation
{
    /// <summary>
    /// Logique d'interaction pour VisualObjectSelector.xaml
    /// </summary>
    public partial class VisualObjectSelector : UserControl, INotifyPropertyChanged
    {
        private bool m_allAtOnce;
        private bool m_allowMultiple;
        private Predicate<VisualObject> m_filter;

        public bool IsSelecting { get => (bool)GetValue(IsSelectingProperty); set => SetValue(IsSelectingProperty, value); }
        public static readonly DependencyProperty IsSelectingProperty = DependencyProperty.Register("IsSelecting", typeof(bool), typeof(VisualObjectSelector));

        public INotifyCollectionChanged VisualObjects { get => (INotifyCollectionChanged)GetValue(VisualObjectsProperty); protected set => SetValue(VisualObjectsProperty, value); }
        protected static readonly DependencyProperty VisualObjectsProperty = DependencyProperty.Register("VisualObjects", typeof(INotifyCollectionChanged), typeof(VisualObjectSelector));

        public TrackingCharacterSelection Selection { get => (TrackingCharacterSelection)GetValue(SelectionProperty); set => SetValue(SelectionProperty, value); }
        public static readonly DependencyProperty SelectionProperty = DependencyProperty.Register("Selection", typeof(TrackingCharacterSelection), typeof(VisualObjectSelector));

        public VisualObject VisualObject { get => (VisualObject)GetValue(VisualObjectProperty); set => SetValue(VisualObjectProperty, value); }
        public static readonly DependencyProperty VisualObjectProperty = DependencyProperty.Register("VisualObject", typeof(VisualObject), typeof(VisualObjectSelector));

        public bool AllowMultiple { get => (bool)GetValue(AllowMultipleProperty); set => SetValue(AllowMultipleProperty, value); }
        public static readonly DependencyProperty AllowMultipleProperty = DependencyProperty.Register("AllowMultiple", typeof(bool), typeof(VisualObjectSelector), new PropertyMetadata(true));

        public bool AllAtOnce { get => (bool)GetValue(AllAtOnceProperty); set => SetValue(AllAtOnceProperty, value); }
        public static readonly DependencyProperty AllAtOnceProperty = DependencyProperty.Register("AllAtOnce", typeof(bool), typeof(VisualObjectSelector), new PropertyMetadata(false));

        public bool IsSelectionVisible => !AllAtOnce;

        public Predicate<VisualObject> Filter { get => (Predicate<VisualObject>)GetValue(FilterProperty); set => SetValue(FilterProperty, value); }
        public static readonly DependencyProperty FilterProperty = DependencyProperty.Register("Filter", typeof(Predicate<VisualObject>), typeof(VisualObjectSelector));

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == SelectionProperty)
            {
                if (e.OldValue is TrackingCharacterSelection oldValue) oldValue.Changed -= OnSelectionChanged;
                if (e.NewValue is TrackingCharacterSelection newValue) newValue.Changed += OnSelectionChanged;
            }
            else if (e.Property == IsSelectingProperty)
            {
                if ((bool)e.NewValue)
                {
                    Selection.UsageCount++;
                    VisualObjects = Selection.VisualObjects;
                    m_allowMultiple = Selection.AllowMultiple;
                    Selection.AllowMultiple = AllowMultiple;
                    m_allAtOnce = Selection.AllAtOnce;
                    Selection.AllAtOnce = AllAtOnce;
                    m_filter = Selection.Filter;
                    Selection.Filter = Filter;
                }
                else
                {
                    Selection.UsageCount--;
                    Selection.Filter = m_filter;
                    Selection.AllowMultiple = m_allowMultiple;
                    Selection.AllAtOnce = m_allAtOnce;

                    m_filter = null;

                    VisualObject = Selection.Count switch
                    {
                        0 => null,
                        1 => VisualObject = Selection.VisualObjects[0].Selection >= Interval<int>.PositiveReals ? Selection.VisualObjects[0] : InCharacters(Selection.VisualObjects[0]),
                        _ => InCharactersGroup(Selection.VisualObjects)
                    };
                }
            }
            else if (e.Property == VisualObjectProperty)
            {
                IsSelecting = false;
                VisualObjects = e.NewValue switch
                {
                    InCharactersVisualObject inCharacters => new FreezableCollection<VisualObjectPart>(new[] { new InCharactersVisualObjectPart(inCharacters) }),
                    InCharactersVisualObjectGroup inCharactersGroup => new FreezableCollection<VisualObjectPart>(inCharactersGroup.Children.Select((vo, i) => new InCharactersVisualObjectGroupPart(inCharactersGroup, i))),
                    VisualObject visualObject => new FreezableCollection<VisualObjectPart>(new[] { new VisualObjectVisualObjectPart(visualObject) }),
                    _ => null
                };
                VisualObjectChanged?.Invoke(this, new PropertyChangedExtendedEventArgs<VisualObject>("VisualObject", e.OldValue as VisualObject, e.NewValue as VisualObject));
            }
            else if (e.Property == AllowMultipleProperty && IsSelecting) Selection.AllowMultiple = (bool)e.NewValue;
            else if (e.Property == AllAtOnceProperty && IsSelecting)
            {
                Selection.AllAtOnce = (bool)e.NewValue;
                NotifyPropertyChanged("IsSelectionVisible");
            }
            else if (e.Property == FilterProperty && IsSelecting) Selection.Filter = (Predicate<VisualObject>)e.NewValue;
            base.OnPropertyChanged(e);
        }

        private void OnSelectionChanged(object sender, EventArgs e) { if (IsSelecting && !AllowMultiple && AllAtOnce && Selection.Count > 0) IsSelecting = false; }

        public event PropertyChangedExtendedEventHandler<VisualObject> VisualObjectChanged;

        public VisualObjectSelector() => InitializeComponent();
    }

    public class InCharactersVisualObjectGroupPart : VisualObjectPart
    {
        public InCharactersVisualObjectGroupPart(InCharactersVisualObjectGroup owner, int index)
        {
            Owner = owner;
            Index = index;
        }

        public InCharactersVisualObjectGroup Owner { get => (InCharactersVisualObjectGroup)GetValue(OwnerProperty); set => SetValue(OwnerProperty, value); }
        public static readonly DependencyProperty OwnerProperty = DependencyProperty.Register("Owner", typeof(InCharactersVisualObjectGroup), typeof(InCharactersVisualObjectGroupPart));

        public int Index { get => (int)GetValue(IndexProperty); set => SetValue(IndexProperty, value); }
        public static readonly DependencyProperty IndexProperty = DependencyProperty.Register("Index", typeof(int), typeof(InCharactersVisualObjectGroupPart));

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == OwnerProperty || e.Property == IndexProperty)
            {
                VisualObject = Owner.Children[Index];
                Interval = Owner.Intervals[Index];
            }
            else if (e.Property == IntervalProperty) Owner.Intervals[Index] = e.NewValue as Interval<int>;
            base.OnPropertyChanged(e);
        }
    }

    public class InCharactersVisualObjectPart : VisualObjectPart
    {
        public InCharactersVisualObject InCharacters { get => (InCharactersVisualObject)GetValue(InCharactersProperty); set => SetValue(InCharactersProperty, value); }
        public static readonly DependencyProperty InCharactersProperty = DependencyProperty.Register("InCharacters", typeof(InCharactersVisualObject), typeof(InCharactersVisualObjectPart));

        public InCharactersVisualObjectPart(InCharactersVisualObject inCharacters) => InCharacters = inCharacters;

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == InCharactersProperty)
            {
                VisualObject = InCharacters.VisualObject;
                Interval = InCharacters.Interval;
            }
            else if (e.Property == IntervalProperty) InCharacters.Interval = e.NewValue as Interval<int>;
            base.OnPropertyChanged(e);
        }
    }

    public class VisualObjectVisualObjectPart : VisualObjectPart
    {
        public VisualObjectVisualObjectPart(VisualObject visualObject) => VisualObject = visualObject;

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == VisualObjectProperty) Interval = (e.NewValue as VisualObject)?.Selection;
            else if (e.Property == IntervalProperty) VisualObject.Selection = e.NewValue as Interval<int>;
            base.OnPropertyChanged(e);
        }
    }

    public class VisualObjectPart : DependencyObject
    {
        public VisualObject VisualObject { get => (VisualObject)GetValue(VisualObjectProperty); set => SetValue(VisualObjectProperty, value); }
        public static readonly DependencyProperty VisualObjectProperty = DependencyProperty.Register("VisualObject", typeof(VisualObject), typeof(VisualObjectPart));

        public Interval<int> Interval { get => (Interval<int>)GetValue(IntervalProperty); set => SetValue(IntervalProperty, value); }
        public static readonly DependencyProperty IntervalProperty = DependencyProperty.Register("Interval", typeof(Interval<int>), typeof(VisualObjectPart));
    }
}
