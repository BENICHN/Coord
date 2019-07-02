using BenLib.Framework;
using BenLib.Standard;
using Coord;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
                    if (newValue == false) owner.OwnerElement.SelectedChildrenCount--;
                    else if (oldValue == false) owner.OwnerElement.SelectedChildrenCount++;
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
            true => Imaging.BrushFromHex(0xFF007ACC),
            null => Imaging.BrushFromHex(0x5F007ACC),
            false => Brushes.Transparent
        };

        protected Element(VisualObjectElement ownerElement) => OwnerElement = ownerElement;
        public static implicit operator Element(VisualObject visualObject) => (VisualObjectElement)visualObject;

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

        private int m_selectedChildrenCount;
        public int SelectedChildrenCount
        {
            get => m_selectedChildrenCount;
            set
            {
                m_selectedChildrenCount = value;
                if (value == 0) SetIsSelected(false, false);
            }
        }

        private bool m_refreshAtChange;
        public bool RefreshAtChange
        {
            get => m_refreshAtChange;
            set
            {
                if (IsIndependent)
                {
                    m_refreshAtChange = value;
                    if (Children != null) foreach (var element in Children.Nodes.OfType<VisualObjectElement>()) element.RefreshAtChange = value;
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
            var visualObject = VisualObject;
            var mode = visualObject.ChildrenRenderingMode;
            cache ??= visualObject.Cache.Characters?.GroupBy(c => c.Creator)?.ToDictionary(group => group.Key, group => group.ToArray());
            if (mode != VisualObjectChildrenRenderingMode.Discard && visualObject.Children != null) foreach (var vo in visualObject.Children) yield return new VisualObjectElement(this, vo, mode == VisualObjectChildrenRenderingMode.Independent, cache);
            if (cache != null)
            {
                if (cache.TryGetValue(visualObject, out var characters))
                {
                    foreach (var c in characters) yield return new CharacterElement(this, c);
                    cache.Remove(visualObject);
                }
                if (IsIndependent) foreach (var kvp in cache) yield return new VisualObjectElement(this, kvp.Key, false, new Dictionary<VisualObject, Character[]> { { kvp.Key, kvp.Value } });
            }
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
                    var ce = (CharacterElement)children.Nodes[i]; //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
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

                if (!Children.IsNullOrEmpty() && SelectedChildrenCount == 0) SetValue(IsSelectedProperty, false);
            }
        }
        public override void SetIsEnabled(bool value)
        {
            if (Children != null) foreach (var element in Children.Nodes) element.SetIsEnabled(value);
            base.SetIsEnabled(value);
        }

        public static implicit operator VisualObjectElement(VisualObject visualObject) => new VisualObjectElement(null, visualObject, true, null);
        private VisualObjectElement(VisualObjectElement ownerElement, VisualObject visualObject, bool isIndependent, IDictionary<VisualObject, Character[]> cache) : base(ownerElement)
        {
            IsIndependent = isIndependent;
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

        private void VisualObject_SelectionChanged(object sender, VisualObjectSelectionChangedEventArgs e) { if (e.OriginalSource == VisualObject && e.NewValue >= PositiveReals) IsSelected = true; }
        private void VisualObject_CacheChanged(object sender, PropertyChangedExtendedEventArgs<Character[]> e) { if (e.OldValue != e.NewValue) { if (e.NewValue != null && RefreshAtChange) Refresh(); else IsEnabled = false; } }

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
}
