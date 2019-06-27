using BenLib.Framework;
using BenLib.Standard;
using BenLib.WPF;
using Coord;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

namespace CoordAnimation
{
    public class PropertiesEditor : PropertiesEditorBase
    {
        protected override async void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == ObjectProperty)
            {
                if (e.OldValue == e.NewValue) return;
                if (e.NewValue is DependencyObject dependencyObject) await LoadEditors(dependencyObject);
            }
        }

        public PropertiesEditor() => Editors.Columns.Insert(0, new DataGridTextColumn { Binding = new Binding("Description") { Mode = BindingMode.OneTime }, IsReadOnly = true });

        public static FrameworkElement GetEditorFromProperty(DependencyObject owner, DependencyProperty property, NotifyObjectPropertyMetadata metadata, bool isAnimatable)
        {
            var type = property.PropertyType;
            var binding = new Binding(property.Name) { Source = owner, Mode = (((owner as Freezable)?.IsFrozen ?? false) || property.ReadOnly) ? BindingMode.OneTime : BindingMode.TwoWay };
            if (type == typeof(string))
            {
                var editor = new SwitchableTextBox();
                editor.SetBinding(SwitchableTextBox.TextProperty, binding);
                return isAnimatable ? CreateAnimatablePropertyEditor<string>(editor, owner, property) : editor;
            }
            else if (type == typeof(int))
            {
                var editor = new IntEditor();
                editor.SetBinding(IntEditor.ValueProperty, binding);
                return isAnimatable ? CreateAnimatablePropertyEditor<int>(editor, owner, property) : editor;
            }
            else if (type == typeof(uint))
            {
                var editor = new UIntEditor();
                editor.SetBinding(UIntEditor.ValueProperty, binding);
                return isAnimatable ? CreateAnimatablePropertyEditor<uint>(editor, owner, property) : editor;
            }
            else if (type == typeof(long))
            {
                var editor = new LongEditor();
                editor.SetBinding(LongEditor.ValueProperty, binding);
                return isAnimatable ? CreateAnimatablePropertyEditor<long>(editor, owner, property) : editor;
            }
            else if (type == typeof(ulong))
            {
                var editor = new ULongEditor();
                editor.SetBinding(ULongEditor.ValueProperty, binding);
                return isAnimatable ? CreateAnimatablePropertyEditor<ulong>(editor, owner, property) : editor;
            }
            else if (type == typeof(double))
            {
                var editor = new DoubleEditor();
                editor.SetBinding(DoubleEditor.ValueProperty, binding);
                return isAnimatable ? CreateAnimatablePropertyEditor<double>(editor, owner, property) : editor;
            }
            else if (type == typeof(decimal))
            {
                var editor = new DecimalEditor();
                editor.SetBinding(DecimalEditor.ValueProperty, binding);
                return isAnimatable ? CreateAnimatablePropertyEditor<decimal>(editor, owner, property) : editor;
            }
            else if (type == typeof(float))
            {
                var editor = new FloatEditor();
                editor.SetBinding(FloatEditor.ValueProperty, binding);
                return isAnimatable ? CreateAnimatablePropertyEditor<float>(editor, owner, property) : editor;
            }
            else if (type == typeof(short))
            {
                var editor = new ShortEditor();
                editor.SetBinding(ShortEditor.ValueProperty, binding);
                return isAnimatable ? CreateAnimatablePropertyEditor<short>(editor, owner, property) : editor;
            }
            else if (type == typeof(ushort))
            {
                var editor = new UShortEditor();
                editor.SetBinding(UShortEditor.ValueProperty, binding);
                return isAnimatable ? CreateAnimatablePropertyEditor<ushort>(editor, owner, property) : editor;
            }
            else if (type == typeof(byte))
            {
                var editor = new ByteEditor();
                editor.SetBinding(ByteEditor.ValueProperty, binding);
                return isAnimatable ? CreateAnimatablePropertyEditor<byte>(editor, owner, property) : editor;
            }
            else if (type == typeof(sbyte))
            {
                var editor = new SByteEditor();
                editor.SetBinding(SByteEditor.ValueProperty, binding);
                return isAnimatable ? CreateAnimatablePropertyEditor<sbyte>(editor, owner, property) : editor;
            }
            else if (type == typeof(bool))
            {
                var editor = new CheckBox();
                editor.SetBinding(ToggleButton.IsCheckedProperty, binding);
                return isAnimatable ? CreateAnimatablePropertyEditor<bool>(editor, owner, property) : editor;
            }
            else if (type == typeof(bool?))
            {
                var editor = new CheckBox { IsThreeState = true };
                editor.SetBinding(ToggleButton.IsCheckedProperty, binding);
                return isAnimatable ? CreateAnimatablePropertyEditor<bool?>(editor, owner, property) : editor;
            }
            else if (type == typeof(Point))
            {
                var editor = new PointEditor();
                editor.SetBinding(PointEditor.PointProperty, binding);
                return isAnimatable ? CreateAnimatablePropertyEditor<Point>(editor, owner, property) : editor;
            }
            else if (type == typeof(Vector))
            {
                var editor = new VectorEditor();
                editor.SetBinding(VectorEditor.VectorProperty, binding);
                return isAnimatable ? CreateAnimatablePropertyEditor<Vector>(editor, owner, property) : editor;
            }
            else if (type == typeof(Size))
            {
                var editor = new SizeEditor { Height = 120 };
                editor.SetBinding(SizeEditor.SizeProperty, binding);
                return isAnimatable ? CreateAnimatablePropertyEditor<Size>(editor, owner, property) : editor;
            }
            else if (type == typeof(Rect))
            {
                var editor = new RectEditor { Height = 120 };
                editor.SetBinding(RectEditor.RectProperty, binding);
                return isAnimatable ? CreateAnimatablePropertyEditor<Rect>(editor, owner, property) : editor;
            }
            else if (type == typeof(RectPoint))
            {
                var editor = new RectPointEditor { Height = 120 };
                editor.SetBinding(RectPointEditor.RectPointProperty, binding);
                return isAnimatable ? CreateAnimatablePropertyEditor<RectPoint>(editor, owner, property) : editor;
            }
            else if (type == typeof(Progress))
            {
                var editor = new DoubleEditor { Value = ((Progress)owner.GetValue(property)).Value, IncrementFactor = 0.001, IsUnsigned = true };
                editor.ValueChanged += (sender, e) => owner.SetValue(property, new Progress(e.NewValue));
                return isAnimatable ? CreateAnimatablePropertyEditor<Progress>(editor, owner, property) : editor;
            }
            else if (type == typeof(Color))
            {
                var editor = new ColorEditor { Height = 180 };
                editor.SetBinding(ColorEditor.ColorProperty, binding);
                return isAnimatable ? CreateAnimatablePropertyEditor<Color>(editor, owner, property) : editor;
            }
            else if (typeof(Enum).IsAssignableFrom(type))
            {
                var editor = new EnumEditor();
                editor.SetBinding(EnumEditor.EnumerationProperty, binding);
                return isAnimatable ? CreateAnimatablePropertyEditor<Enum>(editor, owner, property) : editor;
            }
            else if (type == typeof(Interval<int>))
            {
                var editor = new IntervalEditor { IntervalType = IntervalType.Int };
                editor.SetBinding(IntervalEditor.IntIntervalProperty, binding);
                return isAnimatable ? CreateAnimatablePropertyEditor<Interval<int>>(editor, owner, property) : editor;
            }
            else if (type == typeof(Interval<double>))
            {
                var editor = new IntervalEditor { IntervalType = IntervalType.Double };
                editor.SetBinding(IntervalEditor.DoubleIntervalProperty, binding);
                return isAnimatable ? CreateAnimatablePropertyEditor<Interval<double>>(editor, owner, property) : editor;
            }
            else if (type == typeof(Interval<decimal>))
            {
                var editor = new IntervalEditor { IntervalType = IntervalType.Decimal };
                editor.SetBinding(IntervalEditor.DecimalIntervalProperty, binding);
                return isAnimatable ? CreateAnimatablePropertyEditor<Interval<decimal>>(editor, owner, property) : editor;
            }
            else if (type == typeof(VisualObject))
            {
                var editor = new VisualObjectSelector { Selection = App.Scene.Plane.Selection };
                editor.SetBinding(VisualObjectSelector.VisualObjectProperty, binding);
                return isAnimatable ? CreateAnimatablePropertyEditor<VisualObject>(editor, owner, property) : editor;
            }
            else if (typeof(IList).IsAssignableFrom(type))
            {
                var editor = new ListEditor { CollectionType = type };
                var genericIList = type.GetInterfaces().FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IList<>));
                if (genericIList != null) editor.ItemType = genericIList.GenericTypeArguments[0];
                editor.SetBinding(ListEditor.ListProperty, binding);
                return editor;
            }
            else if (typeof(VisualObject).IsAssignableFrom(type))
            {
                var editor = new VisualObjectSelector { Selection = App.Scene.Plane.Selection, Pointing = type };
                editor.SetBinding(VisualObjectSelector.VisualObjectProperty, binding);
                return isAnimatable ? CreateAnimatablePropertyEditor<VisualObject>(editor, owner, property) : editor;
            }
            else if (typeof(DependencyObject).IsAssignableFrom(type) || type.IsInterface && App.DependencyObjectTypes.Contains(type))
            {
                var editor = new PropertiesEditor { IsAnimatable = isAnimatable, Type = type };
                editor.SetBinding(ObjectProperty, binding);
                return editor;
            }
            else return owner.GetValue(property) is DependencyObject dependencyObject ? new PropertiesEditor { IsAnimatable = isAnimatable, Object = dependencyObject } : null;
        }
        private static FrameworkElement CreateAnimatablePropertyEditor<T>(FrameworkElement editor, DependencyObject dependencyObject, DependencyProperty dependencyProperty)
        {
            var result = AnimatablePropertyEditor.Create<T>(editor, dependencyObject, dependencyProperty);
            result.DataContext.RequestDataFocus += (sender, e) => App.Scene.Timeline.SetKeyFrames((AbsoluteKeyFrameCollection<T>)e.Param1);
            return result;
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
    }

    public class EditableProperty : PropertyEditorContainer
    {
        public EditableProperty(string description, DependencyProperty property, FrameworkElement editor)
        {
            Description = description;
            Property = property;
            Editor = editor;
        }

        public string Description { get => (string)GetValue(DescriptionProperty); set => SetValue(DescriptionProperty, value); }
        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register("Description", typeof(string), typeof(EditableProperty));

        public DependencyProperty Property { get; }

        public override string ToString() => Description;
    }
}
