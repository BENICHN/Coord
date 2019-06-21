using BenLib.Standard;
using BenLib.WPF;
using Coord;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CoordAnimation
{
    /// <summary>
    /// Logique d'interaction pour PropertiesEditor.xaml
    /// </summary>
    public partial class PropertiesEditor : PropertiesEditorBase, INotifyPropertyChanged
    {
        public bool AreEditorsVisible { get => (bool)GetValue(AreEditorsVisibleProperty); set => SetValue(AreEditorsVisibleProperty, value); }
        public static readonly DependencyProperty AreEditorsVisibleProperty = DependencyProperty.Register("AreEditorsVisible", typeof(bool), typeof(PropertiesEditor), new PropertyMetadata(true));

        private bool m_settingObject;

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

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public Type ObjectType => Type ?? Object?.GetType();

        public event PropertyChangedExtendedEventHandler<DependencyObject> ObjectChanged;

        public Type Type { get => (Type)GetValue(TypeProperty); set => SetValue(TypeProperty, value); }
        public static readonly DependencyProperty TypeProperty = DependencyProperty.Register("Type", typeof(Type), typeof(PropertiesEditor));

        public DependencyObject Object { get => (DependencyObject)GetValue(ObjectProperty); set => SetValue(ObjectProperty, value); }
        public static readonly DependencyProperty ObjectProperty = DependencyProperty.Register("Object", typeof(DependencyObject), typeof(PropertiesEditor));

        protected override async void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == AreEditorsVisibleProperty)
            {
                SetVisibility(Type, Object);
                if ((bool)e.NewValue && Object is DependencyObject dependencyObject) await LoadEditors(dependencyObject);
                else ClearProperties();
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

                SetVisibility(Type, newValue);

                ClearProperties();

                if (newValue != null && AreEditorsVisible) await LoadEditors(newValue);

                NotifyPropertyChanged("CanFreeze");
                NotifyPropertyChanged("IsFrozen");

                ObjectChanged?.Invoke(this, new PropertyChangedExtendedEventArgs<DependencyObject>("Object", oldValue, newValue));
            }
            base.OnPropertyChanged(e);
        }

        private async Task LoadEditors(DependencyObject dependencyObject)
        {
            bool isAnimatable = IsAnimatable;
            var properties = dependencyObject is NotifyObject notifyObject ? notifyObject.AllDisplayableProperties : dependencyObject.GetType().GetAllDependencyProperties().Select(fi =>
            {
                var dp = (DependencyProperty)fi.GetValue(null);
                var metadata = dp.GetMetadata(dependencyObject);
                return (dp, metadata as NotifyObjectPropertyMetadata ?? new NotifyObjectPropertyMetadata { Description = dp.Name });
            });

            try { foreach (var (property, metadata) in properties) await AddEditor(new EditableProperty(metadata.Description, property, GetEditorFromProperty(dependencyObject, property, metadata, isAnimatable))); }
            catch (OperationCanceledException) { }
        }

        private void Types_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!m_settingObject && typesBox.SelectedItem is Type type)
            {
                try { Object = (DependencyObject)Activator.CreateInstance(type); }
                catch { Object = null; }
            }
        }

        public PropertiesEditor()
        {
            InitializeComponent();
            SetVisibility(null, null);
        }

        private void SetVisibility(Type type, DependencyObject dependencyObject)
        {
            bool istypeSelecting = type != null && (type.IsAbstract || type.IsInterface);
            bool typeBlockV = dependencyObject != null && !istypeSelecting;
            bool typesBoxV = istypeSelecting;
            bool buttonsV = dependencyObject != null;
            bool editorsV = dependencyObject != null && AreEditorsVisible;
            bool instanceButtonV = dependencyObject == null && type != null && !istypeSelecting;

            typeBlock.Visibility = typeBlockV ? Visibility.Visible : Visibility.Collapsed;
            typesBox.Visibility = typesBoxV ? Visibility.Visible : Visibility.Collapsed;
            lockButton.Visibility = nullButton.Visibility = buttonsV ? Visibility.Visible : Visibility.Collapsed;
            Editors.Visibility = editorsV ? Visibility.Visible : Visibility.Collapsed;
            instanceButton.Visibility = instanceButtonV ? Visibility.Visible : Visibility.Collapsed;

            Grid.SetColumnSpan(typesBox, buttonsV ? 1 : 3);
            firstRow.Height = new GridLength(typesBoxV || typeBlockV ? 25 : 0);
        }

        private void InstanceButton_Click(object sender, RoutedEventArgs e) => Object = (DependencyObject)Activator.CreateInstance(Type);

        private void NullButton_Click(object sender, RoutedEventArgs e)
        {
            (Object as NotifyObject)?.Destroy();
            Object = null;
        }

        private void NullButton_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) { if (CurrentEditor != this && this.FindParent<PropertiesEditorBase>() is PropertiesEditorBase propertiesEditorBase) propertiesEditorBase.CancelCellChange = true; }
    }

    public class EditableProperty : IPropertyEditorContainer
    {
        public EditableProperty(string description, DependencyProperty property, FrameworkElement editor)
        {
            Description = description;
            Property = property;
            Editor = editor;
        }

        public string Description { get; }
        public DependencyProperty Property { get; }

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

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public override string ToString() => Description;
    }
}
