﻿using BenLib.Standard;
using BenLib.WPF;
using Coord;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using static BenLib.Standard.Interval<int>;

namespace CoordAnimation
{
    public class ElementTree : ObservableTree<Element>
    {
        private bool m_refreshAtChange;
        public bool RefreshAtChange
        {
            get => m_refreshAtChange;
            set
            {
                m_refreshAtChange = value;
                foreach (var element in Nodes.OfType<VisualObjectElement>()) element.RefreshAtChange = value;
            }
        }

        public ElementTree() { }
        public ElementTree(ObservableCollection<Element> nodes) : base(nodes) { }
        public ElementTree(List<Element> items) : base(items) { }
        public ElementTree(IEnumerable<Element> items) : base(items) { }
        public ElementTree(params Element[] items) : base(items) { }

        public void ClearSelection() { foreach (var element in Nodes) element.IsSelected = false; }

        public void Register() { foreach (var element in Nodes) Register(element); }
        public void Refresh() { foreach (var element in Nodes.OfType<VisualObjectElement>()) if (!element.IsEnabled) element.Refresh(); }

        private void Register(Element item)
        {
            item.Attach();
            if (item is VisualObjectElement visualObjectElement) visualObjectElement.RefreshAtChange = RefreshAtChange;
        }
        private void UnRegister(Element item) => item.Detach();

        protected override void ClearItems()
        {
            foreach (var item in Items) UnRegister(item);
            base.ClearItems();
        }
        protected override void InsertItem(int index, Element item)
        {
            Register(item);
            base.InsertItem(index, item);
        }
        protected override void RemoveItem(int index)
        {
            UnRegister(Items[index]);
            base.RemoveItem(index);
        }
        protected override void SetItem(int index, Element item)
        {
            UnRegister(Items[index]);
            Register(item);
            base.SetItem(index, item);
        }
    }

    public abstract class Element : DependencyObject, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public bool HasOwner => OwnerElement != null;
        public VisualObjectElement OwnerElement { get; set; }

        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected", typeof(bool?), typeof(Element), new PropertyMetadata((bool?)false, (sender, e) =>
        {
            if (sender is Element owner)
            {
                bool? oldValue = (bool?)e.OldValue;
                bool? newValue = (bool?)e.NewValue;

                if (owner.HasOwner)
                {
                    if (newValue == false) owner.OwnerElement.SC--;
                    else if (oldValue == false) owner.OwnerElement.SC++;
                }
                owner.NotifyPropertyChanged("Background");
            }
        }));
        public bool? IsSelected { get => (bool?)GetValue(IsSelectedProperty); set => SetIsSelected(value, null); }

        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.Register("IsEnabled", typeof(bool), typeof(Element), new PropertyMetadata(true, (sender, e) => (sender as Element).NotifyPropertyChanged("Opacity")));
        public bool IsEnabled { get => (bool)GetValue(IsEnabledProperty); set => SetIsEnabled(value); }

        public bool IsExpanded { get => (bool)GetValue(IsExpandedProperty); set => SetValue(IsExpandedProperty, value); }
        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register("IsExpanded", typeof(bool), typeof(Element));

        public bool IsAttached { get; protected set; }

        public double Opacity => IsEnabled ? 1 : 0.313;
        public Brush Background => IsSelected switch
        {
            true => Misc.BrushFromHex(0xFF007ACC),
            null => Misc.BrushFromHex(0x5F007ACC),
            false => Brushes.Transparent
        };

        protected Element(VisualObjectElement ownerElement) => OwnerElement = ownerElement;

        public virtual void SetIsEnabled(bool value) => SetValue(IsEnabledProperty, value);
        public abstract void SetIsSelected(bool? value, bool? fromOwner);
        protected void NotifyPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public abstract void Attach();
        public abstract void Detach();
    }

    public class VisualObjectElement : Element, ITreeNode<Element>
    {
        public static string GetName(DependencyObject obj) => (string)obj.GetValue(NameProperty);
        public static void SetName(DependencyObject obj, string value) => obj.SetValue(NameProperty, value);
        public static readonly DependencyProperty NameProperty = DependencyProperty.RegisterAttached("Name", typeof(string), typeof(Element));

        private static readonly Dictionary<string, int> ElementNames = new Dictionary<string, int>();
        public static string GetElementName(string type)
        {
            if (ElementNames.TryGetValue(type, out int count)) ElementNames[type] = ++count;
            else ElementNames.Add(type, 0);
            return $"{type}{count}";
        }

        public bool IsIndependent { get; set; }

        public int SC
        {
            get => m_sC;
            set
            {
                m_sC = value;
                if (value == 0) SetIsSelected(false, false);
            }
        }

        private bool m_refreshAtChange;
        private int m_sC;

        public bool RefreshAtChange
        {
            get => m_refreshAtChange;
            set
            {
                if (IsIndependent)
                {
                    m_refreshAtChange = value;
                    if (!Children.IsNullOrEmpty()) foreach (var element in Children.Nodes.OfType<VisualObjectElement>()) element.RefreshAtChange = value;
                }
            }
        }

        public VisualObjectElement IndependentElement => IsIndependent ? this : OwnerElement.IndependentElement;

        public VisualObject VisualObject { get => (VisualObject)GetValue(VisualObjectProperty); set => SetValue(VisualObjectProperty, value); }
        public static readonly DependencyProperty VisualObjectProperty = DependencyProperty.Register("VisualObject", typeof(VisualObject), typeof(VisualObjectElement));

        public ElementTree Children { get => (ElementTree)GetValue(ChildrenProperty); set => SetValue(ChildrenProperty, value); }
        public static readonly DependencyProperty ChildrenProperty = DependencyProperty.Register("Children", typeof(ElementTree), typeof(VisualObjectElement));

        ITree<Element> ITreeNode<Element>.Children => Children;
        protected IEnumerable<Element> GetChildren(IDictionary<VisualObject, Character[]> cache = null)
        {
            cache ??= VisualObject.Cache.Characters?.GroupBy(c => c.Creator)?.ToDictionary(group => group.Key, group => group.ToArray());
            if (cache != null)
            {
                if (VisualObject is VisualObjectGroupBase groupBase) foreach (var vo in groupBase.Children) yield return new VisualObjectElement(this, vo, cache);
                if (cache.TryGetValue(VisualObject, out var characters))
                {
                    foreach (var c in characters) yield return new CharacterElement(this, c);
                    cache.Remove(VisualObject);
                }
                if (IsIndependent) foreach (var kvp in cache) yield return new VisualObjectElement(this, kvp.Key, new Dictionary<VisualObject, Character[]> { { kvp.Key, kvp.Value } });
            }
            else if (VisualObject is VisualObjectGroupBase groupBase) foreach (var vo in groupBase.Children) yield return new VisualObjectElement(this, vo);
        }

        public void ReplaceChildren(ICollection<Element> elements)
        {
            var children = Children;
            int i = 0;

            foreach (var visualObjectElement in elements.OfType<VisualObjectElement>())
            {
                visualObjectElement.OwnerElement = this;
                int index = children.Nodes.IndexOf(e => e is VisualObjectElement voe && voe.VisualObject == visualObjectElement.VisualObject);
                if (index == -1) children.Insert(i, visualObjectElement);
                else
                {
                    children.Nodes.Move(index, i);
                    var voe = children.Nodes[i] as VisualObjectElement;
                    voe.ReplaceChildren(visualObjectElement.Children.Nodes);
                    visualObjectElement.Detach();
                }
                i++;
            }

            for (int j = i; j < children.Nodes.Count; j++)
            {
                if (children.Nodes[j] is VisualObjectElement) children.RemoveAt(j);
                else break;
            }

            foreach (var characterElement in elements.OfType<CharacterElement>())
            {
                if (children.Nodes.Count > i)
                {
                    var ce = (CharacterElement)children.Nodes[i];
                    ce.Character = characterElement.Character;
                }
                else
                {
                    characterElement.OwnerElement = this;
                    children.Add(characterElement);
                }
                i++;
            }

            while (i < children.Nodes.Count) children.RemoveAt(i);
        }

        public override void SetIsSelected(bool? value, bool? fromOwner)
        {
            if (IsSelected != value)
            {
                if (fromOwner == null)
                {
                    IndependentElement.VisualObject.RenderAtSelectionChange = false;
                    if (IsIndependent)
                    {
                        if (value == true && VisualObject.Selection < PositiveReals) VisualObject.Selection = PositiveReals;
                        if (value == false && !VisualObject.Selection.IsEmpty) VisualObject.Selection = EmptySet;
                        if (IsSelected == value) return;
                    }
                }

                SetValue(IsSelectedProperty, value);

                if (fromOwner != true && HasOwner && OwnerElement.IsSelected.HasValue && (!value.HasValue || OwnerElement.IsSelected.Value ^ value.Value)) OwnerElement.SetIsSelected(null, false);
                if (fromOwner != false && value.HasValue) { foreach (var e in Children.Nodes) e.SetIsSelected(value, true); }

                if (fromOwner == null) IndependentElement.VisualObject.RenderAtSelectionChange = true;
            }
        }
        public override void SetIsEnabled(bool value)
        {
            if (!Children.IsNullOrEmpty()) foreach (var element in Children.Nodes) element.SetIsEnabled(value);
            base.SetIsEnabled(value);
        }

        public VisualObjectElement(VisualObjectElement ownerElement, VisualObject visualObject, IDictionary<VisualObject, Character[]> cache = null) : base(ownerElement)
        {
            IsIndependent = cache == null;
            VisualObject = visualObject;
            if (GetName(visualObject) == null) SetName(visualObject, GetElementName(visualObject.Type));
            Children = new ElementTree(GetChildren(cache));
        }

        public void Refresh()
        {
            if (IsIndependent)
            {
                ReplaceChildren(GetChildren().ToArray());
                Children.Refresh();
                IsEnabled = true;
            }
        }
        public override void Attach()
        {
            if (IsIndependent && !IsAttached)
            {
                VisualObject.CacheChanged += VisualObject_CacheChanged;
                VisualObject.SelectionChanged += VisualObject_SelectionChanged;
                if (VisualObject.Selection >= PositiveReals) IsSelected = true;
                IsAttached = true;
            }
            Children.Register();
        }
        public override void Detach()
        {
            if (IsIndependent && IsAttached)
            {
                VisualObject.CacheChanged -= VisualObject_CacheChanged;
                VisualObject.SelectionChanged -= VisualObject_SelectionChanged;
                Children.Clear();
                IsAttached = false;
            }
        }

        private void VisualObject_SelectionChanged(object sender, PropertyChangedExtendedEventArgs<Interval<int>> e) { if (sender == VisualObject && e.NewValue >= PositiveReals) IsSelected = true; }
        private void VisualObject_CacheChanged(object sender, PropertyChangedExtendedEventArgs<(Character[] Characters, ReadOnlyCoordinatesSystemManager ReadOnlyCoordinatesSystemManager)> e) { if (e.OldValue.Characters != e.NewValue.Characters) { if (e.NewValue.Characters != null && RefreshAtChange) Refresh(); else IsEnabled = false; } }

        public override string ToString() => GetName(VisualObject);
    }

    public class CharacterElement : Element
    {
        public Character Character { get => (Character)GetValue(CharacterProperty); set => SetValue(CharacterProperty, value); }
        public static readonly DependencyProperty CharacterProperty = DependencyProperty.Register("Character", typeof(Character), typeof(CharacterElement), new PropertyMetadata((sender, e) =>
        {
            if (sender is CharacterElement owner && owner.IsAttached)
            {
                var oldValue = e.OldValue as Character;
                var newValue = e.NewValue as Character;

                oldValue.IsSelectedChanged -= owner.Character_IsSelectedChanged;
                if (!newValue.IsSelected) owner.SetValue(IsSelectedProperty, false);
                owner.IsAttached = false;

                owner.Attach();
            }
        }));

        public CharacterElement(VisualObjectElement ownerElement, Character character) : base(ownerElement) => Character = character;

        private void Character_IsSelectedChanged(object sender, PropertyChangedExtendedEventArgs<bool> e) => IsSelected = e.NewValue;

        public override void SetIsSelected(bool? value, bool? fromOwner)
        {
            if (IsSelected != value)
            {
                bool v = value.Value;
                if (Character.IsSelected != v) Character.IsSelected = v;
                else
                {
                    SetValue(IsSelectedProperty, value);
                    if (fromOwner != true && OwnerElement.IsSelected.HasValue && OwnerElement.IsSelected.Value ^ v) OwnerElement.SetIsSelected(null, false);
                }
            }
        }

        public override void Attach()
        {
            if (!IsAttached)
            {
                Character.IsSelectedChanged += Character_IsSelectedChanged;
                IsSelected = OwnerElement.IsSelected == true || Character.IsSelected;
                IsAttached = true;
            }
        }
        public override void Detach()
        {
            if (IsAttached)
            {
                Character.IsSelectedChanged -= Character_IsSelectedChanged;
                SetValue(IsSelectedProperty, false);
                IsAttached = false;
            }
        }
    }

    public class SelectedVisualObjectsCollection : FreezableCollection<VisualObject>
    {
        public VisualObjectCollection VisualObjects { get => (VisualObjectCollection)GetValue(VisualObjectsProperty); set => SetValue(VisualObjectsProperty, value); }
        public static readonly DependencyProperty VisualObjectsProperty = DependencyProperty.Register("VisualObjects", typeof(VisualObjectCollection), typeof(SelectedVisualObjectsCollection), new PropertyMetadata((sender, e) =>
        {
            if (sender is SelectedVisualObjectsCollection owner)
            {
                if (e.OldValue is VisualObjectCollection oldValue)
                {
                    owner.Clear();
                    oldValue.CollectionChanged -= owner.VisualObjects_CollectionChanged;
                    oldValue.SelectionChanged -= owner.VisualObjects_SelectionChanged;
                }
                if (e.NewValue is VisualObjectCollection newValue)
                {
                    newValue.CollectionChanged += owner.VisualObjects_CollectionChanged;
                    newValue.SelectionChanged += owner.VisualObjects_SelectionChanged;
                    owner.AddRange(newValue.Where(vo => vo.Selection.IsNullOrEmpty()));
                }
            }
        }));

        public SelectedVisualObjectsCollection() { }
        public SelectedVisualObjectsCollection(VisualObjectCollection visualObjects) : base(visualObjects.Where(vo => vo.Selection.IsNullOrEmpty())) => VisualObjects = visualObjects;

        private void VisualObjects_SelectionChanged(object sender, PropertyChangedExtendedEventArgs<Interval<int>> e)
        {
            if (sender is VisualObject vo)
            {
                bool empty = e.NewValue.IsNullOrEmpty();
                int index = IndexOf(vo);
                if (empty && index != -1) RemoveAt(index);
                else if (!empty && index == -1) Add(vo);
            }
        }

        private void VisualObjects_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset) Clear();
            else
            {
                if (e.OldItems != null) foreach (var vo in e.OldItems.OfType<VisualObject>()) { /*int index = IndexOf(voe) if (Contains(voe)) */Remove(vo); }
                if (e.NewItems != null) foreach (var vo in e.NewItems.OfType<VisualObject>()) { if (!vo.Selection.IsNullOrEmpty() && !Contains(vo)) Add(vo); }
            }
        }
    }
}
