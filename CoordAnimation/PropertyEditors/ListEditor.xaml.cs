using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CoordAnimation
{
    /// <summary>
    /// Logique d'interaction pour ListEditor.xaml
    /// </summary>
    public partial class ListEditor : PropertiesEditorBase
    {
        public Type CollectionType { get => (Type)GetValue(CollectionTypeProperty); set => SetValue(CollectionTypeProperty, value); }
        public static readonly DependencyProperty CollectionTypeProperty = DependencyProperty.Register("CollectionType", typeof(Type), typeof(ListEditor));

        public Type ItemType { get => (Type)GetValue(ItemTypeProperty); set => SetValue(ItemTypeProperty, value); }
        public static readonly DependencyProperty ItemTypeProperty = DependencyProperty.Register("ItemType", typeof(Type), typeof(ListEditor));

        public IList List { get => (IList)GetValue(ListProperty); set => SetValue(ListProperty, value); }
        public static readonly DependencyProperty ListProperty = DependencyProperty.Register("List", typeof(IList), typeof(ListEditor));

        protected override async void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == CollectionTypeProperty) SetVisibility((Type)e.NewValue, List);
            if (e.Property == ListProperty)
            {
                SetVisibility(CollectionType, (IList)e.NewValue);
                ClearProperties();
                if (e.OldValue is INotifyCollectionChanged oldNotifyCollectionChanged) oldNotifyCollectionChanged.CollectionChanged -= OnCollectionChanged;
                if (e.NewValue is INotifyCollectionChanged newNotifyCollectionChanged) newNotifyCollectionChanged.CollectionChanged += OnCollectionChanged;
                try { if (e.NewValue is IList newList) for (int i = 0; i < newList.Count; i++) await AddEditor(new ListElement(newList, i)); }
                catch (OperationCanceledException) { }
            }
            base.OnPropertyChanged(e);
        }

        private void SetVisibility(Type collectionType, IList list)
        {
            bool editorsV = list != null;
            bool instanceButtonV = list == null && collectionType != null;

            Editors.Visibility = editorsV ? Visibility.Visible : Visibility.Collapsed;
            instanceButton.Visibility = instanceButtonV ? Visibility.Visible : Visibility.Collapsed;
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Move:
                    Properties.Move(e.OldStartingIndex, e.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    ClearProperties();
                    break;
                default:
                    if (e.OldItems != null) { for (int i = e.OldStartingIndex; i < e.OldStartingIndex + e.OldItems.Count; i++) RemovePropertyAt(i); }
                    if (e.NewItems != null) { for (int i = e.NewStartingIndex; i < e.NewStartingIndex + e.NewItems.Count; i++) InsertEditor(i, new ListElement((IList)sender, i)); }
                    break;
            }
            for (int i = 0; i < Properties.Count; i++) ((ListElement)Properties[i]).Index = i;
        }

        public ListEditor()
        {
            Editors.SelectedCellsChanged += Editors_SelectedCellsChanged;
            InitializeComponent();
        }

        private void InstanceButton_Click(object sender, RoutedEventArgs e) => List = (IList)Activator.CreateInstance(CollectionType);

        private void Editors_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (CancelCellChange == false)
            {
                if (e.RemovedCells.FirstOrDefault().Item is ListElement oldListElement && oldListElement.Editor is PropertiesEditor oldPropertiesEditor) oldPropertiesEditor.AreEditorsVisible = false;
                if (e.AddedCells.FirstOrDefault().Item is ListElement newListElement && newListElement.Editor is PropertiesEditor newPropertiesEditor) newPropertiesEditor.AreEditorsVisible = true;
            }
        }
    }

    public class ListElement : DependencyObject, IPropertyEditorContainer
    {
        public IList List { get; }

        private int m_index;
        public int Index
        {
            get => m_index;
            set
            {
                m_index = value;
                NotifyPropertyChanged("Index");
            }
        }

        private FrameworkElement m_editor;
        public FrameworkElement Editor
        {
            get => m_editor;
            set
            {
                m_editor = value;
                NotifyPropertyChanged("Editor");
            }
        }

        public object Value { get => GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(object), typeof(ListElement));

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public ListElement(IList list, int index)
        {
            Value = list[index];
            List = list;
            Index = index;
            Editor = PropertiesEditorBase.GetEditorFromProperty(this, ValueProperty, null, true);
            if (Editor is PropertiesEditor propertiesEditor) propertiesEditor.AreEditorsVisible = false;
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == ValueProperty && List is IList list) list[Index] = e.NewValue;
            base.OnPropertyChanged(e);
        }
    }
}
