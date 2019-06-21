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
        public int SelectionUsageCount { get => GetUsageCount(Selection); set => SetUsageCount(Selection, value); }
        public static int GetUsageCount(DependencyObject obj) => (int)obj.GetValue(UsageCountProperty);
        public static void SetUsageCount(DependencyObject obj, int value) => obj.SetValue(UsageCountProperty, value);
        public static readonly DependencyProperty UsageCountProperty = DependencyProperty.RegisterAttached("UsageCount", typeof(int), typeof(VisualObjectSelector));

        public bool IsSelecting { get => (bool)GetValue(IsSelectingProperty); set => SetValue(IsSelectingProperty, value); }
        public static readonly DependencyProperty IsSelectingProperty = DependencyProperty.Register("IsSelecting", typeof(bool), typeof(VisualObjectSelector));

        private INotifyCollectionChanged m_visualObjects;
        public INotifyCollectionChanged VisualObjects
        {
            get => m_visualObjects;
            protected set
            {
                if (m_visualObjects != value)
                {
                    m_visualObjects = value;
                    NotifyPropertyChanged("VisualObjects");
                }
            }
        }

        public TrackingCharacterSelection Selection { get => (TrackingCharacterSelection)GetValue(SelectionProperty); set => SetValue(SelectionProperty, value); }
        public static readonly DependencyProperty SelectionProperty = DependencyProperty.Register("Selection", typeof(TrackingCharacterSelection), typeof(VisualObjectSelector));

        public VisualObject VisualObject { get => (VisualObject)GetValue(VisualObjectProperty); set => SetValue(VisualObjectProperty, value); }
        public static readonly DependencyProperty VisualObjectProperty = DependencyProperty.Register("VisualObject", typeof(VisualObject), typeof(VisualObjectSelector));

        public Type Pointing { get => (Type)GetValue(PointingProperty); set => SetValue(PointingProperty, value); }
        public static readonly DependencyProperty PointingProperty = DependencyProperty.Register("Pointing", typeof(Type), typeof(VisualObjectSelector));

        public bool IsPointing => Pointing != null;
        public bool IsSelectionVisible => Pointing == null;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == SelectionProperty)
            {
                if (e.OldValue is TrackingCharacterSelection oldValue) oldValue.ObjectPointed -= OnObjectPointed;
                if (e.NewValue is TrackingCharacterSelection newValue) newValue.ObjectPointed += OnObjectPointed;
            }
            else if (e.Property == IsSelectingProperty)
            {
                if ((bool)e.NewValue)
                {
                    SelectionUsageCount++;
                    VisualObjects = Selection.VisualObjects;
                    Selection.EnablePointing(Pointing);
                }
                else
                {
                    Selection.DisablePointing();
                    var visualObject = Selection.Count switch
                    {
                        0 => null,
                        1 => VisualObject = Selection.VisualObjects[0].Selection >= Interval<int>.PositiveReals ? Selection.VisualObjects[0] : InCharacters(Selection.VisualObjects[0]),
                        _ => InCharactersGroup(Selection.VisualObjects)
                    };

                    VisualObject = visualObject;
                    if (VisualObjects == Selection.VisualObjects) VisualObjects = GetVisualObjectsFromVisualObject(visualObject);

                    SelectionUsageCount--;
                }
            }
            else if (e.Property == VisualObjectProperty)
            {
                IsSelecting = false;
                if (VisualObjects == null || VisualObjects == Selection.VisualObjects) VisualObjects = GetVisualObjectsFromVisualObject((VisualObject)e.NewValue);
                VisualObjectChanged?.Invoke(this, new PropertyChangedExtendedEventArgs<VisualObject>("VisualObject", e.OldValue as VisualObject, e.NewValue as VisualObject));
            }
            else if (e.Property == PointingProperty && IsSelecting) NotifyPropertyChanged("IsSelectionVisible");
            base.OnPropertyChanged(e);
        }

        private INotifyCollectionChanged GetVisualObjectsFromVisualObject(VisualObject visualObject) => visualObject switch
        {
            InCharactersVisualObject inCharacters => new FreezableCollection<VisualObjectPart>(new[] { new InCharactersVisualObjectPart(inCharacters) }),
            InCharactersVisualObjectGroup inCharactersGroup => new FreezableCollection<VisualObjectPart>(inCharactersGroup.Children.Select((vo, i) => new InCharactersVisualObjectGroupPart(inCharactersGroup, i))),
            _ => new FreezableCollection<VisualObjectPart>(new[] { new VisualObjectVisualObjectPart(visualObject) })
        };

        private void OnObjectPointed(object sender, EventArgs<VisualObject> e) { if (IsSelecting) IsSelecting = false; }

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
