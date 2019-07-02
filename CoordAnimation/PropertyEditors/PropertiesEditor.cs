using BenLib.WPF;
using Coord;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace CoordAnimation
{
    public class PropertiesEditor : PropertiesEditorBase
    {
        private bool m_createInstance;

        protected override async void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == ObjectProperty)
            {
                if (e.OldValue == e.NewValue) return;
                if (e.NewValue is DependencyObject dependencyObject)
                {
                    if (m_createInstance) IsExpanded = true;
                    await LoadEditors(dependencyObject);
                }
                m_createInstance = false;
            }
        }

        public PropertiesEditor() => Editors.Columns.Insert(0, new DataGridTextColumn { Binding = new Binding("Description") { Mode = BindingMode.OneTime }, IsReadOnly = true });

        protected override void CreateInstance(Type type)
        {
            m_createInstance = true;
            base.CreateInstance(type);
            m_createInstance = false;
        }

        private async Task LoadEditors(DependencyObject dependencyObject)
        {
            string s = TypeEditionHelper.FromType(typeof(VisualObject)).Serialize(App.Scene.Plane.VisualObjects[9]);
            object o = TypeEditionHelper.FromType(typeof(VisualObject)).Deserialize(s);

            bool isAnimatable = IsAnimatable;
            var properties = dependencyObject is NotifyObject notifyObject ? notifyObject.AllDisplayableProperties : dependencyObject.GetType().GetAllDependencyProperties().Select(dp =>
            {
                var metadata = dp.GetMetadata(dependencyObject);
                return (dp, metadata as NotifyObjectPropertyMetadata ?? new NotifyObjectPropertyMetadata { Description = dp.Name });
            });

            try { foreach (var (property, metadata) in properties) await AddEditor(new EditableProperty(metadata.Description, TypeEditionHelper.GetEditorFromProperty(dependencyObject, property, metadata.Data, isAnimatable))); }
            catch (OperationCanceledException) { }
        }
    }

    public class EditableProperty : PropertyEditorContainer
    {
        public EditableProperty(string description, FrameworkElement editor)
        {
            Description = description;
            Editor = editor;
        }

        public string Description { get => (string)GetValue(DescriptionProperty); set => SetValue(DescriptionProperty, value); }
        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register("Description", typeof(string), typeof(EditableProperty));

        public override string ToString() => Description;
    }
}
