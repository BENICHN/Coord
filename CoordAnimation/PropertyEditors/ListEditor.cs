using BenLib.Standard;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace CoordAnimation
{
    public class ListEditor : PropertiesEditorBase
    {
        public Type ItemType { get => (Type)GetValue(ItemTypeProperty); set => SetValue(ItemTypeProperty, value); }
        public static readonly DependencyProperty ItemTypeProperty = DependencyProperty.Register("ItemType", typeof(Type), typeof(ListEditor));

        protected override async void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == ObjectProperty)
            {
                if (e.OldValue == e.NewValue) return;
                if (e.NewValue is IList newList)
                {
                    if (e.OldValue is INotifyCollectionChanged oldNotifyCollectionChanged) oldNotifyCollectionChanged.CollectionChanged -= OnCollectionChanged;
                    if (newList is INotifyCollectionChanged newNotifyCollectionChanged) newNotifyCollectionChanged.CollectionChanged += OnCollectionChanged;
                    try { for (int i = 0; i < newList.Count; i++) await AddEditor(new ListElement(newList, i)); }
                    catch (OperationCanceledException) { }
                }
            }
        }

        private async void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
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
                    if (e.NewItems != null)
                    {
                        for (int i = e.NewStartingIndex; i < e.NewStartingIndex + e.NewItems.Count; i++)
                        {
                            try { await InsertEditor(i, new ListElement((IList)sender, i)); }
                            catch (OperationCanceledException) { }
                        }
                    }
                    break;
            }
            if (Properties.Count == 0) SetVisibility(Type, Object);
            else for (int i = 0; i < Properties.Count; i++) ((ListElement)Properties[i]).Index = i;
        }

        protected override async void OnKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Insert:
                    int index = (Editors.SelectedItem is ListElement selected ? selected.Index : -1) + 1;
                    await CreateItem(index);
                    break;
            }
        }

        protected override void SetVisibility(Type type, object obj)
        {
            base.SetVisibility(type, obj);
            if (obj is IList list && list.Count == 0 && IsExpanded) instanceButton.Visibility = Visibility.Visible;
        }

        protected override async void CreateInstance(Type type)
        {
            if (Object is IList list)
            {
                await CreateItem(0);
                SetVisibility(Type, list);
            }
            else base.CreateInstance(type);
        }

        public async Task CreateItem(int index)
        {
            if (Object is IList list && ItemType is Type itemType)
            {
                object newItem = null;
                if (itemType.IsAbstract || itemType.IsInterface)
                {
                    if (App.DependencyObjectTypes.TryGetValue(itemType, out var node))
                    {
                        try { newItem = Activator.CreateInstance(await SelectType(node.DerivedTypes.AllTreeItems().Where(node => !node.Type.IsAbstract).Select(node => node.Type))); }
                        catch { newItem = null; }
                    }
                }
                else
                {
                    try { newItem = Activator.CreateInstance(ItemType); }
                    catch { newItem = null; }
                }
                if (newItem != null) list.Insert(index, newItem);
                if (!(list is INotifyCollectionChanged))
                {
                    try { await InsertEditor(index, new ListElement(list, index)); }
                    catch (OperationCanceledException) { }
                    int i = 1;
                    foreach (var el in Properties.OfType<ListElement>().Where(el => el.Index > index))
                    {
                        el.Index = index + i;
                        i++;
                    }
                }
            }
        }

        private Task<Type> SelectType(IEnumerable<Type> types)
        {
            var tcs = new TaskCompletionSource<Type>();

            var contextMenu = new ContextMenu { ItemsSource = new ObservableCollection<Type>(types) };
            ContextMenu = contextMenu;
            contextMenu.IsOpen = true;

            contextMenu.AddHandler(MenuItem.ClickEvent, new RoutedEventHandler((object sender, RoutedEventArgs e) =>
            {
                tcs.TrySetResult((Type)((MenuItem)e.OriginalSource).Header);
                ContextMenu = null;
            }));
            contextMenu.Closed += (sender, e) =>
            {
                tcs.TrySetCanceled();
                ContextMenu = null;
            };

            return tcs.Task;
        }

        public ListEditor()
        {
            var tb = new FrameworkElementFactory(typeof(TextBlock));
            tb.SetBinding(TextBlock.TextProperty, new Binding("Index"));
            tb.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Center);
            tb.SetValue(VerticalAlignmentProperty, VerticalAlignment.Center);
            Editors.Columns.Insert(0, new DataGridTemplateColumn { CellTemplate = new DataTemplate { VisualTree = tb } });
        }
    }

    public class ListElement : PropertyEditorContainer
    {
        public IList List { get; }

        public int Index { get => (int)GetValue(IndexProperty); set => SetValue(IndexProperty, value); }
        public static readonly DependencyProperty IndexProperty = DependencyProperty.Register("Index", typeof(int), typeof(ListElement));

        public object Value { get => GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(object), typeof(ListElement));

        public ListElement(IList list, int index)
        {
            Value = list[index];
            List = list;
            Index = index;
            Editor = PropertiesEditor.GetEditorFromProperty(this, ValueProperty, null, true);
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != e.NewValue && e.Property == ValueProperty && List is IList list) list[Index] = e.NewValue;
            base.OnPropertyChanged(e);
        }
    }
}
