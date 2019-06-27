using BenLib.Framework;
using BenLib.Standard;
using Coord;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace CoordAnimation
{
    /// <summary>
    /// Logique d'interaction pour PropertiesEditor.xaml
    /// </summary>
    public partial class PropertiesEditorBase : UserControl, INotifyPropertyChanged
    {
        #region Champs

        private CancellationTokenSource m_cts = new CancellationTokenSource();
        private bool m_settingObject;
        internal bool? CancelCellChange = false;

        public static event PropertyChangedExtendedEventHandler<FrameworkElement> CurrentEditorChanged;
        public event PropertyChangedExtendedEventHandler<DependencyObject> ObjectChanged;
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #endregion

        #region Propriétés

        #region Static

        private static FrameworkElement s_currentEditor;
        public static FrameworkElement CurrentEditor
        {
            get => s_currentEditor;
            set
            {
                if (value is AnimatablePropertyEditor animatablePropertyEditor) animatablePropertyEditor.DataContext.DataFocus();
                else App.Scene.Timeline.SetKeyFrames<object>(null);

                var old = s_currentEditor;
                if (value != old)
                {
                    s_currentEditor = value;
                    CurrentEditorChanged?.Invoke(null, new PropertyChangedExtendedEventArgs<FrameworkElement>("CurrentEditor", old, value));
                }
            }
        }

        #endregion

        #region Dependency

        public PropertiesEditorElements ElementsVisibility { get => (PropertiesEditorElements)GetValue(ElementsVisibilityProperty); set => SetValue(ElementsVisibilityProperty, value); }
        public static readonly DependencyProperty ElementsVisibilityProperty = DependencyProperty.Register("ElementsVisibility", typeof(PropertiesEditorElements), typeof(PropertiesEditorBase), new PropertyMetadata((PropertiesEditorElements)0b111111));

        public Type Type { get => (Type)GetValue(TypeProperty); set => SetValue(TypeProperty, value); }
        public static readonly DependencyProperty TypeProperty = DependencyProperty.Register("Type", typeof(Type), typeof(PropertiesEditorBase));

        public object Object { get => GetValue(ObjectProperty); set => SetValue(ObjectProperty, value); }
        public static readonly DependencyProperty ObjectProperty = DependencyProperty.Register("Object", typeof(object), typeof(PropertiesEditorBase));

        public bool IsAnimatable { get => (bool)GetValue(IsAnimatableProperty); set => SetValue(IsAnimatableProperty, value); }
        public static readonly DependencyProperty IsAnimatableProperty = DependencyProperty.Register("IsAnimatable", typeof(bool), typeof(PropertiesEditorBase), new PropertyMetadata(true));

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == ElementsVisibilityProperty)
            {
                SetVisibility(Type, Object);
                if (((PropertiesEditorElements)e.OldValue ^ (PropertiesEditorElements)e.NewValue).HasFlag(PropertiesEditorElements.Editors)) NotifyPropertyChanged("IsExpanded");
                //if ((bool)e.NewValue && Object is DependencyObject dependencyObject) await LoadEditors(dependencyObject);
                //else ClearProperties();
            }
            else if (e.Property == TypeProperty)
            {
                var newValue = (Type)e.NewValue;

                typeBlock.Text = ObjectType?.Name;

                typesBox.Items.Clear();
                if (newValue != null && (newValue.IsAbstract || newValue.IsInterface) && App.DependencyObjectTypes.TryGetValue(newValue, out var node)) { foreach (var t in node.DerivedTypes.AllTreeItems().Where(node => !node.Type.IsAbstract).Select(node => node.Type)) typesBox.Items.Add(t); }

                SetVisibility(newValue, Object);
            }
            else if (e.Property == ObjectProperty)
            {
                if (e.OldValue == e.NewValue) return;

                var oldValue = (DependencyObject)e.OldValue;
                var newValue = (DependencyObject)e.NewValue;

                typeBlock.Text = ObjectType?.Name;

                if (IsTypeSelecting)
                {
                    m_settingObject = true;
                    typesBox.SelectedItem = newValue?.GetType();
                    m_settingObject = false;
                }

                ClearProperties();

                SetVisibility(Type, newValue);

                NotifyPropertyChanged("CanFreeze");
                NotifyPropertyChanged("IsFrozen");

                ObjectChanged?.Invoke(this, new PropertyChangedExtendedEventArgs<DependencyObject>("Object", oldValue, newValue));
            }
            base.OnPropertyChanged(e);
        }

        #endregion

        public ObservableCollection<PropertyEditorContainer> Properties { get; } = new ObservableCollection<PropertyEditorContainer>();

        public Type ObjectType => Type ?? Object?.GetType();
        public bool IsUndefined => Type == null && Object == null;
        public bool IsTypeSelecting => Type != null && (Type.IsAbstract || Type.IsInterface);

        public bool CanFreeze => Object is Freezable freezable && !(freezable is NotifyObject) && freezable.CanFreeze;
        public bool IsFrozen
        {
            get => Object is Freezable freezable && freezable.IsFrozen;
            set
            {
                if (Object is Freezable freezable)
                {
                    if (value && freezable.CanFreeze) Object = freezable.GetCurrentValueAsFrozen();
                    else if (!value && freezable.IsFrozen) Object = freezable.CloneCurrentValue();
                }
            }
        }

        public bool IsExpanded
        {
            get => ElementsVisibility.HasFlag(PropertiesEditorElements.Editors); set
            {
                if (value) ElementsVisibility |= PropertiesEditorElements.Editors;
                else ElementsVisibility &= ~PropertiesEditorElements.Editors;
            }
        }

        #endregion

        public PropertiesEditorBase()
        {
            InitializeComponent();

            Editors.ItemsSource = Properties;
            ScrollViewer.SetCanContentScroll(Editors, false);
            Editors.SelectedCellsChanged += Editors_SelectedCellsChanged;
            Editors.PreviewKeyDown += Editors_PreviewKeyDown;
            CurrentEditorChanged += OnCurrentEditorChanged;

            var editor = new FrameworkElementFactory(typeof(ContentPresenter));
            editor.SetBinding(ContentPresenter.ContentProperty, new Binding("Editor") { Mode = BindingMode.OneTime });
            Editors.Columns.Add(new DataGridTemplateColumn { Width = new DataGridLength(1, DataGridLengthUnitType.Star), CellTemplate = new DataTemplate { VisualTree = editor } });

            SetVisibility(null, null);
        }

        #region Méthodes

        public bool ContainsEditor(FrameworkElement editor) => Properties.Any(p => p.Editor == editor || p.Editor is PropertiesEditorBase propertiesEditorBase && propertiesEditorBase.ContainsEditor(editor));

        protected virtual void SetVisibility(Type type, object dependencyObject)
        {
            var elementsVisibility = ElementsVisibility;
            bool istypeSelecting = type != null && (type.IsAbstract || type.IsInterface);
            bool typeBlockV = elementsVisibility.HasFlag(PropertiesEditorElements.TypeBlock) && dependencyObject != null && !istypeSelecting;
            bool typesBoxV = elementsVisibility.HasFlag(PropertiesEditorElements.TypesBox) && istypeSelecting;
            bool nullV = elementsVisibility.HasFlag(PropertiesEditorElements.Null) && dependencyObject != null;
            bool lockV = elementsVisibility.HasFlag(PropertiesEditorElements.Lock) && dependencyObject != null;
            bool editorsV = elementsVisibility.HasFlag(PropertiesEditorElements.Editors) && dependencyObject != null;
            bool instanceButtonV = elementsVisibility.HasFlag(PropertiesEditorElements.New) && dependencyObject == null && type != null && !istypeSelecting;

            typeBlock.Visibility = typeBlockV ? Visibility.Visible : Visibility.Collapsed;
            typesBox.Visibility = typesBoxV ? Visibility.Visible : Visibility.Collapsed;
            lockButton.Visibility = lockV ? Visibility.Visible : Visibility.Collapsed;
            nullButton.Visibility = nullV ? Visibility.Visible : Visibility.Collapsed;
            Editors.Visibility = editorsV ? Visibility.Visible : Visibility.Collapsed;
            instanceButton.Visibility = instanceButtonV ? Visibility.Visible : Visibility.Collapsed;

            Grid.SetColumnSpan(typesBox, nullV ? 1 : lockV ? 2 : 3);
            firstRow.Height = new GridLength(typesBoxV || typeBlockV ? 25 : 0);
        }

        protected virtual void CreateInstance(Type type) => Object = (DependencyObject)Activator.CreateInstance(type);
        protected virtual void Destroy(object obj)
        {
            (obj as NotifyObject)?.Destroy();
            Object = null;
        }

        protected Task AddEditor(PropertyEditorContainer container) => InsertEditor(Properties.Count, container);
        protected Task InsertEditor(int index, PropertyEditorContainer container)
        {
            CancellationTokenRegistration reg = default;
            var tcs = new TaskCompletionSource<object>();
            if (container.Editor != null) Properties.Insert(index, container);

            if (container.Editor == null || container.Editor.IsLoaded) tcs.TrySetResult(null);
            else
            {
                container.Editor.Loaded += Result_Loaded;
                reg = m_cts.Token.Register(() =>
                {
                    End();
                    tcs.TrySetCanceled(m_cts.Token);
                });
            }

            return tcs.Task;

            void Result_Loaded(object sender, RoutedEventArgs e)
            {
                End();
                tcs.TrySetResult(null);
            }

            void End()
            {
                container.Editor.Loaded -= Result_Loaded;
                reg.Dispose();
            }
        }

        protected void RemovePropertyAt(int index)
        {
            var properties = Properties;
            if (properties[index].Editor == CurrentEditor || properties[index].Editor is PropertiesEditorBase propertiesEditorBase && propertiesEditorBase.ContainsEditor(CurrentEditor)) CurrentEditor = default;

            var p = properties[index];
            p.Editor.ClearAllBindings();
            if (p.Editor is IDisposable disposable) disposable.Dispose();
            p.Editor = null;
            properties.RemoveAt(index);
        }

        protected void ClearProperties()
        {
            m_cts.Cancel();
            m_cts = new CancellationTokenSource();
            if (ContainsEditor(CurrentEditor)) CurrentEditor = default;
            var properties = Properties;
            int count;
            while ((count = properties.Count - 1) > -1)
            {
                var p = properties[count];
                var e = p.Editor;
                p.Editor = null;
                e.ClearAllBindings();
                if (e is IDisposable disposable) disposable.Dispose();
                properties.RemoveAt(count);
            }
        }

        public void Dispose()
        {
            CurrentEditorChanged -= OnCurrentEditorChanged;
            ClearProperties();
        }

        #endregion

        #region Events

        private void Editors_PreviewKeyDown(object sender, KeyEventArgs e) { if (e.Key == Key.Enter) CancelCellChange = true; }
        private void Editors_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            var editors = (DataGrid)sender;
            if (CancelCellChange != false)
            {
                if (CancelCellChange == true)
                {
                    CancelCellChange = null;
                    editors.SelectedItem = e.RemovedCells.Count > 0 ? e.RemovedCells[0].Item : null;
                }
                else CancelCellChange = false;
            }
            else
            {
                var editor = ((PropertyEditorContainer)editors.SelectedItem)?.Editor;
                if (IsAnimatable && editor != null && !(editor is PropertiesEditorBase propertiesEditorBase && propertiesEditorBase.ContainsEditor(CurrentEditor))) CurrentEditor = ((PropertyEditorContainer)editors.SelectedItem).Editor;
            }
        }

        private void Types_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!m_settingObject && typesBox.SelectedItem is Type type)
            {
                try { Object = (DependencyObject)Activator.CreateInstance(type); }
                catch { Object = null; }
            }
        }

        private void OnCurrentEditorChanged(object sender, PropertyChangedExtendedEventArgs<FrameworkElement> e) { if (!ContainsEditor(e.NewValue)) Editors.SelectedItem = null; }

        private void InstanceButton_Click(object sender, RoutedEventArgs e) => CreateInstance(Type);
        private void NullButton_Click(object sender, RoutedEventArgs e) => Destroy(Object);

        #endregion
    }

    [Flags]
    public enum PropertiesEditorElements
    {
        None = 0b0,
        Editors = 0b1,
        TypeBlock = 0b10,
        TypesBox = 0b100,
        Lock = 0b1000,
        Null = 0b10000,
        New = 0b100000,
    }

    /*public class VariantConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
    }*/

    public abstract class PropertyEditorContainer : DependencyObject
    {
        public FrameworkElement Editor { get => (FrameworkElement)GetValue(EditorProperty); set => SetValue(EditorProperty, value); }
        public static readonly DependencyProperty EditorProperty = DependencyProperty.Register("Editor", typeof(FrameworkElement), typeof(PropertyEditorContainer));
    }
}
