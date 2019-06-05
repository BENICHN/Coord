using BenLib.Framework;
using BenLib.Standard;
using BenLib.WPF;
using Coord;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace CoordAnimation
{
    /// <summary>
    /// Logique d'interaction pour PropertiesEditor.xaml
    /// </summary>
    public partial class PropertiesEditor : UserControl
    {
        public ObservableCollection<CoordEditableProperty> Properties { get => (ObservableCollection<CoordEditableProperty>)GetValue(PropertiesProperty); set => SetValue(PropertiesProperty, value); }
        public static readonly DependencyProperty PropertiesProperty = DependencyProperty.Register("Properties", typeof(ObservableCollection<CoordEditableProperty>), typeof(PropertiesEditor));

        public NotifyObject Object { get => (NotifyObject)GetValue(ObjectProperty); set => SetValue(ObjectProperty, value); }
        public static readonly DependencyProperty ObjectProperty = DependencyProperty.Register("Object", typeof(NotifyObject), typeof(PropertiesEditor), new PropertyMetadata((sender, e) =>
        {
            if (sender is PropertiesEditor owner)
            {
                var oldValue = e.OldValue as NotifyObject;
                var newValue = e.NewValue as NotifyObject;

                owner.Properties.Clear();
                if (newValue != null)
                {
                    foreach (var (description, property) in newValue.AllProperties)
                    {
                        var editor = owner.GetEditorFromProperty(newValue, property);
                        if (editor != null) owner.Properties.Add(new CoordEditableProperty(description, property, editor));
                    }
                }

                owner.ObjectChanged?.Invoke(owner, new PropertyChangedExtendedEventArgs<NotifyObject>("Object", oldValue, newValue));
            }
        }));

        public event PropertyChangedExtendedEventHandler<NotifyObject> ObjectChanged;

        public PropertiesEditor()
        {
            InitializeComponent();
            Properties = new ObservableCollection<CoordEditableProperty>();
        }

        public UIElement GetEditorFromProperty(DependencyObject owner, DependencyProperty property)
        {
            var type = property.PropertyType;
            if (type == typeof(string))
            {
                var editor = new SwitchableTextBox();
                editor.SetBinding(SwitchableTextBox.TextProperty, new Binding(property.Name) { Source = owner, Mode = BindingMode.TwoWay });
                return editor;
            }
            else if (type == typeof(int))
            {
                var editor = new IntEditor();
                editor.SetBinding(IntEditor.ValueProperty, new Binding(property.Name) { Source = owner, Mode = BindingMode.TwoWay });
                return editor;
            }
            else if (type == typeof(uint))
            {
                var editor = new UIntEditor();
                editor.SetBinding(UIntEditor.ValueProperty, new Binding(property.Name) { Source = owner, Mode = BindingMode.TwoWay });
                return editor;
            }
            else if (type == typeof(long))
            {
                var editor = new LongEditor();
                editor.SetBinding(LongEditor.ValueProperty, new Binding(property.Name) { Source = owner, Mode = BindingMode.TwoWay });
                return editor;
            }
            else if (type == typeof(ulong))
            {
                var editor = new ULongEditor();
                editor.SetBinding(ULongEditor.ValueProperty, new Binding(property.Name) { Source = owner, Mode = BindingMode.TwoWay });
                return editor;
            }
            else if (type == typeof(double))
            {
                var editor = new DoubleEditor();
                editor.SetBinding(DoubleEditor.ValueProperty, new Binding(property.Name) { Source = owner, Mode = BindingMode.TwoWay });
                return editor;
            }
            else if (type == typeof(decimal))
            {
                var editor = new DecimalEditor();
                editor.SetBinding(DecimalEditor.ValueProperty, new Binding(property.Name) { Source = owner, Mode = BindingMode.TwoWay });
                return editor;
            }
            else if (type == typeof(float))
            {
                var editor = new FloatEditor();
                editor.SetBinding(FloatEditor.ValueProperty, new Binding(property.Name) { Source = owner, Mode = BindingMode.TwoWay });
                return editor;
            }
            else if (type == typeof(short))
            {
                var editor = new ShortEditor();
                editor.SetBinding(ShortEditor.ValueProperty, new Binding(property.Name) { Source = owner, Mode = BindingMode.TwoWay });
                return editor;
            }
            else if (type == typeof(ushort))
            {
                var editor = new UShortEditor();
                editor.SetBinding(UShortEditor.ValueProperty, new Binding(property.Name) { Source = owner, Mode = BindingMode.TwoWay });
                return editor;
            }
            else if (type == typeof(byte))
            {
                var editor = new ByteEditor();
                editor.SetBinding(ByteEditor.ValueProperty, new Binding(property.Name) { Source = owner, Mode = BindingMode.TwoWay });
                return editor;
            }
            else if (type == typeof(sbyte))
            {
                var editor = new SByteEditor();
                editor.SetBinding(SByteEditor.ValueProperty, new Binding(property.Name) { Source = owner, Mode = BindingMode.TwoWay });
                return editor;
            }
            else if (type == typeof(bool))
            {
                var editor = new CheckBox();
                editor.SetBinding(ToggleButton.IsCheckedProperty, new Binding(property.Name) { Source = owner, Mode = BindingMode.TwoWay });
                return editor;
            }
            else if (type == typeof(bool?))
            {
                var editor = new CheckBox { IsThreeState = true };
                editor.SetBinding(ToggleButton.IsCheckedProperty, new Binding(property.Name) { Source = owner, Mode = BindingMode.TwoWay });
                return editor;
            }
            else if (type == typeof(Point))
            {
                var editor = new PointEditor();
                editor.SetBinding(PointEditor.PointProperty, new Binding(property.Name) { Source = owner, Mode = BindingMode.TwoWay });
                return editor;
            }
            else if (type == typeof(Vector))
            {
                var editor = new VectorEditor();
                editor.SetBinding(VectorEditor.VectorProperty, new Binding(property.Name) { Source = owner, Mode = BindingMode.TwoWay });
                return editor;
            }
            else if (type == typeof(Size))
            {
                var editor = new SizeEditor { Width = 150, Height = 100 };
                editor.SetBinding(SizeEditor.SizeProperty, new Binding(property.Name) { Source = owner, Mode = BindingMode.TwoWay });
                return editor;
            }
            else if (type == typeof(Rect))
            {
                var editor = new RectEditor { Width = 150, Height = 100 };
                editor.SetBinding(RectEditor.RectProperty, new Binding(property.Name) { Source = owner, Mode = BindingMode.TwoWay });
                return editor;
            }
            else if (type == typeof(RectPoint))
            {
                var editor = new RectPointEditor { Width = 150, Height = 100 };
                editor.SetBinding(RectPointEditor.RectPointProperty, new Binding(property.Name) { Source = owner, Mode = BindingMode.TwoWay });
                return editor;
            }
            else if (type == typeof(Progress))
            {
                var editor = new DoubleEditor { Value = ((Progress)owner.GetValue(property)).Value, IncrementFactor = 0.001, IsUnsigned = true };
                editor.ValueChanged += (sender, e) => owner.SetValue(property, new Progress(e.NewValue));
                return editor;
            }
            else if (type == typeof(Interval<int>))
            {
                var editor = new IntervalEditor { IntervalType = IntervalType.Int };
                editor.SetBinding(IntervalEditor.IntIntervalProperty, new Binding(property.Name) { Source = owner, Mode = BindingMode.TwoWay });
                return editor;
            }
            else if (type == typeof(VisualObject))
            {
                var editor = new VisualObjectSelector { Selection = App.Scene.Plane.Selection };
                editor.SetBinding(VisualObjectSelector.VisualObjectProperty, new Binding(property.Name) { Source = owner, Mode = BindingMode.TwoWay });
                return editor;
            }
            else if (typeof(VisualObject).IsAssignableFrom(type))
            {
                var editor = new VisualObjectSelector { Selection = App.Scene.Plane.Selection, Filter = vo => type.IsAssignableFrom(vo.GetType()), AllAtOnce = true, AllowMultiple = false };
                editor.SetBinding(VisualObjectSelector.VisualObjectProperty, new Binding(property.Name) { Source = owner, Mode = BindingMode.TwoWay });
                return editor;
            }
            else if (typeof(NotifyObject).IsAssignableFrom(type))
            {
                var editor = new PropertiesEditor();
                editor.SetBinding(ObjectProperty, new Binding(property.Name) { Source = owner, Mode = BindingMode.TwoWay });
                return editor;
            }
            else return null;
        }
    }

    public readonly struct CoordEditableProperty
    {
        public CoordEditableProperty(string description, DependencyProperty property, UIElement editor)
        {
            Description = description;
            Property = property;
            Editor = editor;
        }

        public string Description { get; }
        public DependencyProperty Property { get; }
        public UIElement Editor { get; }
    }
}
