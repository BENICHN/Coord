using BenLib.Framework;
using BenLib.Standard;
using BenLib.WPF;
using Coord;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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
                var editor = new SwitchableTextBox { Text = (string)owner.GetValue(property) };
                editor.Desactivated += (sender, e) => owner.SetValue(property, ((SwitchableTextBox)sender).Text);
                return editor;
            }
            else if (type == typeof(int))
            {
                var editor = new IntEditor { Value = (int)owner.GetValue(property) };
                editor.ValueChanged += (sender, e) => owner.SetValue(property, e.NewValue);
                return editor;
            }
            else if (type == typeof(uint))
            {
                var editor = new UIntEditor { Value = (uint)owner.GetValue(property) };
                editor.ValueChanged += (sender, e) => owner.SetValue(property, e.NewValue);
                return editor;
            }
            else if (type == typeof(long))
            {
                var editor = new LongEditor { Value = (long)owner.GetValue(property) };
                editor.ValueChanged += (sender, e) => owner.SetValue(property, e.NewValue);
                return editor;
            }
            else if (type == typeof(ulong))
            {
                var editor = new ULongEditor { Value = (ulong)owner.GetValue(property) };
                editor.ValueChanged += (sender, e) => owner.SetValue(property, e.NewValue);
                return editor;
            }
            else if (type == typeof(double))
            {
                var editor = new DoubleEditor { Value = (double)owner.GetValue(property) };
                editor.ValueChanged += (sender, e) => owner.SetValue(property, e.NewValue);
                return editor;
            }
            else if (type == typeof(decimal))
            {
                var editor = new DecimalEditor { Value = (decimal)owner.GetValue(property) };
                editor.ValueChanged += (sender, e) => owner.SetValue(property, e.NewValue);
                return editor;
            }
            else if (type == typeof(float))
            {
                var editor = new FloatEditor { Value = (float)owner.GetValue(property) };
                editor.ValueChanged += (sender, e) => owner.SetValue(property, e.NewValue);
                return editor;
            }
            else if (type == typeof(short))
            {
                var editor = new ShortEditor { Value = (short)owner.GetValue(property) };
                editor.ValueChanged += (sender, e) => owner.SetValue(property, e.NewValue);
                return editor;
            }
            else if (type == typeof(ushort))
            {
                var editor = new UShortEditor { Value = (ushort)owner.GetValue(property) };
                editor.ValueChanged += (sender, e) => owner.SetValue(property, e.NewValue);
                return editor;
            }
            else if (type == typeof(byte))
            {
                var editor = new ByteEditor { Value = (byte)owner.GetValue(property) };
                editor.ValueChanged += (sender, e) => owner.SetValue(property, e.NewValue);
                return editor;
            }
            else if (type == typeof(sbyte))
            {
                var editor = new SByteEditor { Value = (sbyte)owner.GetValue(property) };
                editor.ValueChanged += (sender, e) => owner.SetValue(property, e.NewValue);
                return editor;
            }
            else if (type == typeof(bool))
            {
                var editor = new CheckBox { IsChecked = (bool)owner.GetValue(property) };
                editor.Checked += (sender, e) => owner.SetValue(property, true);
                editor.Unchecked += (sender, e) => owner.SetValue(property, false);
                return editor;
            }
            else if (type == typeof(bool?))
            {
                var editor = new CheckBox { IsThreeState = true, IsChecked = (bool)owner.GetValue(property) };
                editor.Checked += (sender, e) => owner.SetValue(property, ((CheckBox)sender).IsChecked);
                editor.Unchecked += (sender, e) => owner.SetValue(property, ((CheckBox)sender).IsChecked);
                return editor;
            }
            else if (type == typeof(Point))
            {
                var editor = new PointEditor { Point = (Point)owner.GetValue(property) };
                editor.PointChanged += (sender, e) => owner.SetValue(property, e.NewValue);
                return editor;
            }
            else if (type == typeof(Vector))
            {
                var editor = new VectorEditor { Vector = (Vector)owner.GetValue(property) };
                editor.VectorChanged += (sender, e) => owner.SetValue(property, e.NewValue);
                return editor;
            }
            else if (type == typeof(Size))
            {
                var editor = new SizeEditor { Size = (Size)owner.GetValue(property), Width = 150, Height = 100 };
                editor.SizeChanged += (sender, e) => owner.SetValue(property, e.NewValue);
                return editor;
            }
            else if (type == typeof(Rect))
            {
                var editor = new RectEditor { Rect = (Rect)owner.GetValue(property), Width = 150, Height = 100 };
                editor.RectChanged += (sender, e) => owner.SetValue(property, e.NewValue);
                return editor;
            }
            else if (type == typeof(RectPoint))
            {
                var editor = new RectPointEditor { RectPoint = (RectPoint)owner.GetValue(property), Width = 150, Height = 100 };
                editor.RectPointChanged += (sender, e) => owner.SetValue(property, e.NewValue);
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
                var editor = new IntervalEditor(IntervalType.Int, owner.GetValue(property));
                editor.IntervalChanged += (sender, e) => owner.SetValue(property, ((IntervalEditor)sender).IntInterval);
                return editor;
            }
            else if (type == typeof(VisualObject))
            {
                var editor = new VisualObjectSelector { Selection = App.Scene.Plane.Selection, VisualObject = (VisualObject)owner.GetValue(property) };
                editor.VisualObjectChanged += (sender, e) => owner.SetValue(property, ((VisualObjectSelector)sender).VisualObject);
                return editor;
            }
            else if (typeof(VisualObject).IsAssignableFrom(type))
            {
                var editor = new VisualObjectSelector { Selection = App.Scene.Plane.Selection, VisualObject = (VisualObject)owner.GetValue(property), Filter = vo => type.IsAssignableFrom(vo.GetType()), AllAtOnce = true, AllowMultiple = false };
                editor.VisualObjectChanged += (sender, e) => owner.SetValue(property, ((VisualObjectSelector)sender).VisualObject);
                return editor;
            }
            else if (typeof(NotifyObject).IsAssignableFrom(type))
            {
                var editor = new PropertiesEditor { Object = (NotifyObject)owner.GetValue(property) };
                editor.ObjectChanged += (sender, e) => owner.SetValue(property, ((PropertiesEditor)sender).Object);
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
