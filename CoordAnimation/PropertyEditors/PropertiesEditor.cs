using BenLib.Framework;
using BenLib.Standard;
using BenLib.WPF;
using Coord;
using Newtonsoft.Json;
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
    public class EditableTypeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => TypeEditionHelper.FromType(objectType) != null;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) => throw new NotImplementedException();

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is DependencyObject dependencyObject)
            {
                writer.WriteStartObject();
                foreach (var property in dependencyObject is NotifyObject notifyObject ? notifyObject.AllProperties : dependencyObject.GetType().GetAllDependencyProperties())
                {
                    if (TypeEditionHelper.FromType(property.PropertyType) is TypeEditionHelper helper)
                    {
                        writer.WritePropertyName(property.Name);
                        writer.WriteValue(helper.Serialize(dependencyObject.GetValue(property)));
                    }
                }
            }
        }
    }

    public abstract class TypeEditionHelper
    {
        public abstract FrameworkElement GetEditor(Dictionary<string, object> data);
        public abstract void SetBinding(FrameworkElement editor, BindingBase binding);

        public abstract object Deserialize(string data);
        public abstract string Serialize(object obj);

        public static TypeEditionHelper FromType(Type type) =>
            type == typeof(float) ? new FloatEditionHelper() :
            type == typeof(double) ? new DoubleEditionHelper() :
            type == typeof(decimal) ? new DecimalEditionHelper() :
            type == typeof(long) ? new LongEditionHelper() :
            type == typeof(ulong) ? new ULongEditionHelper() :
            type == typeof(int) ? new IntEditionHelper() :
            type == typeof(uint) ? new UIntEditionHelper() :
            type == typeof(short) ? new ShortEditionHelper() :
            type == typeof(ushort) ? new UShortEditionHelper() :
            type == typeof(sbyte) ? new SByteEditionHelper() :
            type == typeof(byte) ? new ByteEditionHelper() :
            type == typeof(Point) ? new PointEditionHelper() :
            type == typeof(Vector) ? new VectorEditionHelper() :
            type == typeof(Rect) ? new RectEditionHelper() :
            type == typeof(Size) ? new SizeEditionHelper() :
            type == typeof(Color) ? new ColorEditionHelper() :
            type == typeof(RectPoint) ? new RectPointEditionHelper() :
            type == typeof(bool) ? new BooleanEditionHelper() :
            type == typeof(bool?) ? new NullableBooleanEditionHelper() :
            typeof(Enum).IsAssignableFrom(type) ? new EnumEditionHelper(type) :
            typeof(DependencyObject).IsAssignableFrom(type) ? new DependencyObjectEditionHelper(type) :
            (TypeEditionHelper)null;
    }

    public class FloatEditionHelper : TypeEditionHelper
    {
        public override object Deserialize(string data) => float.Parse(data);
        public override FrameworkElement GetEditor(Dictionary<string, object> data)
        {
            var editor = new FloatEditor();
            if (data != null)
            {
                if (data.TryGetValue("min", out object min) && min is float minValue) editor.MinValue = minValue;
                if (data.TryGetValue("max", out object max) && max is float maxValue) editor.MaxValue = maxValue;
            }
            return editor;
        }

        public override string Serialize(object obj) => obj.ToString();
        public override void SetBinding(FrameworkElement editor, BindingBase binding) => editor.SetBinding(FloatEditor.ValueProperty, binding);
    }

    public class DoubleEditionHelper : TypeEditionHelper
    {
        public override object Deserialize(string data) => double.Parse(data);
        public override FrameworkElement GetEditor(Dictionary<string, object> data)
        {
            var editor = new DoubleEditor();
            if (data != null)
            {
                if (data.TryGetValue("min", out object min) && min is double minValue) editor.MinValue = minValue;
                if (data.TryGetValue("max", out object max) && max is double maxValue) editor.MaxValue = maxValue;
            }
            return editor;
        }

        public override string Serialize(object obj) => obj.ToString();
        public override void SetBinding(FrameworkElement editor, BindingBase binding) => editor.SetBinding(DoubleEditor.ValueProperty, binding);
    }

    public class DecimalEditionHelper : TypeEditionHelper
    {
        public override object Deserialize(string data) => decimal.Parse(data);
        public override FrameworkElement GetEditor(Dictionary<string, object> data)
        {
            var editor = new DecimalEditor();
            if (data != null)
            {
                if (data.TryGetValue("min", out object min) && min is decimal minValue) editor.MinValue = minValue;
                if (data.TryGetValue("max", out object max) && max is decimal maxValue) editor.MaxValue = maxValue;
            }
            return editor;
        }

        public override string Serialize(object obj) => obj.ToString();
        public override void SetBinding(FrameworkElement editor, BindingBase binding) => editor.SetBinding(DecimalEditor.ValueProperty, binding);
    }

    public class LongEditionHelper : TypeEditionHelper
    {
        public override object Deserialize(string data) => long.Parse(data);
        public override FrameworkElement GetEditor(Dictionary<string, object> data)
        {
            var editor = new LongEditor();
            if (data != null)
            {
                if (data.TryGetValue("min", out object min) && min is long minValue) editor.MinValue = minValue;
                if (data.TryGetValue("max", out object max) && max is long maxValue) editor.MaxValue = maxValue;
            }
            return editor;
        }

        public override string Serialize(object obj) => obj.ToString();
        public override void SetBinding(FrameworkElement editor, BindingBase binding) => editor.SetBinding(LongEditor.ValueProperty, binding);
    }

    public class ULongEditionHelper : TypeEditionHelper
    {
        public override object Deserialize(string data) => ulong.Parse(data);
        public override FrameworkElement GetEditor(Dictionary<string, object> data)
        {
            var editor = new ULongEditor();
            if (data != null)
            {
                if (data.TryGetValue("min", out object min) && min is ulong minValue) editor.MinValue = minValue;
                if (data.TryGetValue("max", out object max) && max is ulong maxValue) editor.MaxValue = maxValue;
            }
            return editor;
        }

        public override string Serialize(object obj) => obj.ToString();
        public override void SetBinding(FrameworkElement editor, BindingBase binding) => editor.SetBinding(ULongEditor.ValueProperty, binding);
    }

    public class IntEditionHelper : TypeEditionHelper
    {
        public override object Deserialize(string data) => int.Parse(data);
        public override FrameworkElement GetEditor(Dictionary<string, object> data)
        {
            var editor = new IntEditor();
            if (data != null)
            {
                if (data.TryGetValue("min", out object min) && min is int minValue) editor.MinValue = minValue;
                if (data.TryGetValue("max", out object max) && max is int maxValue) editor.MaxValue = maxValue;
            }
            return editor;
        }

        public override string Serialize(object obj) => obj.ToString();
        public override void SetBinding(FrameworkElement editor, BindingBase binding) => editor.SetBinding(IntEditor.ValueProperty, binding);
    }

    public class UIntEditionHelper : TypeEditionHelper
    {
        public override object Deserialize(string data) => uint.Parse(data);
        public override FrameworkElement GetEditor(Dictionary<string, object> data)
        {
            var editor = new UIntEditor();
            if (data != null)
            {
                if (data.TryGetValue("min", out object min) && min is uint minValue) editor.MinValue = minValue;
                if (data.TryGetValue("max", out object max) && max is uint maxValue) editor.MaxValue = maxValue;
            }
            return editor;
        }

        public override string Serialize(object obj) => obj.ToString();
        public override void SetBinding(FrameworkElement editor, BindingBase binding) => editor.SetBinding(UIntEditor.ValueProperty, binding);
    }

    public class ShortEditionHelper : TypeEditionHelper
    {
        public override object Deserialize(string data) => short.Parse(data);
        public override FrameworkElement GetEditor(Dictionary<string, object> data)
        {
            var editor = new ShortEditor();
            if (data != null)
            {
                if (data.TryGetValue("min", out object min) && min is short minValue) editor.MinValue = minValue;
                if (data.TryGetValue("max", out object max) && max is short maxValue) editor.MaxValue = maxValue;
            }
            return editor;
        }

        public override string Serialize(object obj) => obj.ToString();
        public override void SetBinding(FrameworkElement editor, BindingBase binding) => editor.SetBinding(ShortEditor.ValueProperty, binding);
    }

    public class UShortEditionHelper : TypeEditionHelper
    {
        public override object Deserialize(string data) => ushort.Parse(data);
        public override FrameworkElement GetEditor(Dictionary<string, object> data)
        {
            var editor = new UShortEditor();
            if (data != null)
            {
                if (data.TryGetValue("min", out object min) && min is ushort minValue) editor.MinValue = minValue;
                if (data.TryGetValue("max", out object max) && max is ushort maxValue) editor.MaxValue = maxValue;
            }
            return editor;
        }

        public override string Serialize(object obj) => obj.ToString();
        public override void SetBinding(FrameworkElement editor, BindingBase binding) => editor.SetBinding(UShortEditor.ValueProperty, binding);
    }

    public class SByteEditionHelper : TypeEditionHelper
    {
        public override object Deserialize(string data) => sbyte.Parse(data);
        public override FrameworkElement GetEditor(Dictionary<string, object> data)
        {
            var editor = new SByteEditor();
            if (data != null)
            {
                if (data.TryGetValue("min", out object min) && min is sbyte minValue) editor.MinValue = minValue;
                if (data.TryGetValue("max", out object max) && max is sbyte maxValue) editor.MaxValue = maxValue;
            }
            return editor;
        }

        public override string Serialize(object obj) => obj.ToString();
        public override void SetBinding(FrameworkElement editor, BindingBase binding) => editor.SetBinding(SByteEditor.ValueProperty, binding);
    }

    public class ByteEditionHelper : TypeEditionHelper
    {
        public override object Deserialize(string data) => byte.Parse(data);
        public override FrameworkElement GetEditor(Dictionary<string, object> data)
        {
            var editor = new ByteEditor();
            if (data != null)
            {
                if (data.TryGetValue("min", out object min) && min is byte minValue) editor.MinValue = minValue;
                if (data.TryGetValue("max", out object max) && max is byte maxValue) editor.MaxValue = maxValue;
            }
            return editor;
        }

        public override string Serialize(object obj) => obj.ToString();
        public override void SetBinding(FrameworkElement editor, BindingBase binding) => editor.SetBinding(ByteEditor.ValueProperty, binding);
    }

    public class PointEditionHelper : TypeEditionHelper
    {
        public override object Deserialize(string data) => Point.Parse(data);
        public override FrameworkElement GetEditor(Dictionary<string, object> data) => new PointEditor();

        public override string Serialize(object obj) => obj.ToString();
        public override void SetBinding(FrameworkElement editor, BindingBase binding) => editor.SetBinding(PointEditor.PointProperty, binding);
    }

    public class VectorEditionHelper : TypeEditionHelper
    {
        public override object Deserialize(string data) => Vector.Parse(data);
        public override FrameworkElement GetEditor(Dictionary<string, object> data) => new VectorEditor();

        public override string Serialize(object obj) => obj.ToString();
        public override void SetBinding(FrameworkElement editor, BindingBase binding) => editor.SetBinding(VectorEditor.VectorProperty, binding);
    }

    public class RectEditionHelper : TypeEditionHelper
    {
        public override object Deserialize(string data) => Rect.Parse(data);
        public override FrameworkElement GetEditor(Dictionary<string, object> data) => new RectEditor { Height = 120 };

        public override string Serialize(object obj) => obj.ToString();
        public override void SetBinding(FrameworkElement editor, BindingBase binding) => editor.SetBinding(RectEditor.RectProperty, binding);
    }

    public class SizeEditionHelper : TypeEditionHelper
    {
        public override object Deserialize(string data) => Size.Parse(data);
        public override FrameworkElement GetEditor(Dictionary<string, object> data) => new SizeEditor { Height = 120 };

        public override string Serialize(object obj) => obj.ToString();
        public override void SetBinding(FrameworkElement editor, BindingBase binding) => editor.SetBinding(SizeEditor.SizeProperty, binding);
    }

    public class ColorEditionHelper : TypeEditionHelper
    {
        public override object Deserialize(string data) => Imaging.ColorFromHex(uint.Parse(data));
        public override FrameworkElement GetEditor(Dictionary<string, object> data) => new ColorEditor { Height = 180 };

        public override string Serialize(object obj) => ((Color)obj).ToHex();
        public override void SetBinding(FrameworkElement editor, BindingBase binding) => editor.SetBinding(ColorEditor.ColorProperty, binding);
    }

    public class RectPointEditionHelper : TypeEditionHelper
    {
        public override object Deserialize(string data) => RectPoint.Parse(data);
        public override FrameworkElement GetEditor(Dictionary<string, object> data) => new RectPointEditor { Height = 120 };

        public override string Serialize(object obj) => obj.ToString();
        public override void SetBinding(FrameworkElement editor, BindingBase binding) => editor.SetBinding(RectPointEditor.RectPointProperty, binding);
    }

    public class BooleanEditionHelper : TypeEditionHelper
    {
        public override object Deserialize(string data) => bool.Parse(data);
        public override FrameworkElement GetEditor(Dictionary<string, object> data) => new CheckBox();

        public override string Serialize(object obj) => obj.ToString();
        public override void SetBinding(FrameworkElement editor, BindingBase binding) => editor.SetBinding(ToggleButton.IsCheckedProperty, binding);
    }

    public class NullableBooleanEditionHelper : TypeEditionHelper
    {
        public override object Deserialize(string data) => bool.Parse(data);
        public override FrameworkElement GetEditor(Dictionary<string, object> data) => new CheckBox { IsThreeState = true };

        public override string Serialize(object obj) => obj?.ToString();
        public override void SetBinding(FrameworkElement editor, BindingBase binding) => editor.SetBinding(ToggleButton.IsCheckedProperty, binding);
    }

    public class EnumEditionHelper : TypeEditionHelper
    {
        public EnumEditionHelper(Type enumType) => EnumType = enumType;

        public Type EnumType { get; }

        public override object Deserialize(string data) => Enum.Parse(EnumType, data);
        public override FrameworkElement GetEditor(Dictionary<string, object> data) => new EnumEditor();

        public override string Serialize(object obj) => obj.ToString();
        public override void SetBinding(FrameworkElement editor, BindingBase binding) => editor.SetBinding(EnumEditor.EnumerationProperty, binding);
    }

    public class DependencyObjectEditionHelper : TypeEditionHelper
    {
        public DependencyObjectEditionHelper(Type type) => Type = type;

        public Type Type { get; }

        public override object Deserialize(string data) => JsonConvert.DeserializeObject(data, Type, new EditableTypeConverter());
        public override FrameworkElement GetEditor(Dictionary<string, object> data) => new PropertiesEditor();

        public override string Serialize(object obj) => JsonConvert.SerializeObject(obj, new EditableTypeConverter());
        public override void SetBinding(FrameworkElement editor, BindingBase binding) => editor.SetBinding(PropertiesEditorBase.ObjectProperty, binding);
    }

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

        public static FrameworkElement GetEditorFromProperty(DependencyObject owner, DependencyProperty property, Dictionary<string, object> data, bool isAnimatable)
        {
            string s = TypeEditionHelper.FromType(typeof(VisualObject)).Serialize(App.Scene.Plane.VisualObjects[9]);
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
                if (data != null)
                {
                    if (data.TryGetValue("min", out object min) && min is int minValue) editor.MinValue = minValue;
                    if (data.TryGetValue("max", out object max) && max is int maxValue) editor.MaxValue = maxValue;
                }
                editor.SetBinding(IntEditor.ValueProperty, binding);
                return isAnimatable ? CreateAnimatablePropertyEditor<int>(editor, owner, property) : editor;
            }
            else if (type == typeof(uint))
            {
                var editor = new UIntEditor();
                if (data != null)
                {
                    if (data.TryGetValue("min", out object min) && min is uint minValue) editor.MinValue = minValue;
                    if (data.TryGetValue("max", out object max) && max is uint maxValue) editor.MaxValue = maxValue;
                }
                editor.SetBinding(UIntEditor.ValueProperty, binding);
                return isAnimatable ? CreateAnimatablePropertyEditor<uint>(editor, owner, property) : editor;
            }
            else if (type == typeof(long))
            {
                var editor = new LongEditor();
                if (data != null)
                {
                    if (data.TryGetValue("min", out object min) && min is long minValue) editor.MinValue = minValue;
                    if (data.TryGetValue("max", out object max) && max is long maxValue) editor.MaxValue = maxValue;
                }
                editor.SetBinding(LongEditor.ValueProperty, binding);
                return isAnimatable ? CreateAnimatablePropertyEditor<long>(editor, owner, property) : editor;
            }
            else if (type == typeof(ulong))
            {
                var editor = new ULongEditor();
                if (data != null)
                {
                    if (data.TryGetValue("min", out object min) && min is ulong minValue) editor.MinValue = minValue;
                    if (data.TryGetValue("max", out object max) && max is ulong maxValue) editor.MaxValue = maxValue;
                }
                editor.SetBinding(ULongEditor.ValueProperty, binding);
                return isAnimatable ? CreateAnimatablePropertyEditor<ulong>(editor, owner, property) : editor;
            }
            else if (type == typeof(double))
            {
                var editor = new DoubleEditor();
                if (data != null)
                {
                    if (data.TryGetValue("min", out object min) && min is double minValue) editor.MinValue = minValue;
                    if (data.TryGetValue("max", out object max) && max is double maxValue) editor.MaxValue = maxValue;
                }
                editor.SetBinding(DoubleEditor.ValueProperty, binding);
                return isAnimatable ? CreateAnimatablePropertyEditor<double>(editor, owner, property) : editor;
            }
            else if (type == typeof(decimal))
            {
                var editor = new DecimalEditor();
                if (data != null)
                {
                    if (data.TryGetValue("min", out object min) && min is decimal minValue) editor.MinValue = minValue;
                    if (data.TryGetValue("max", out object max) && max is decimal maxValue) editor.MaxValue = maxValue;
                }
                editor.SetBinding(DecimalEditor.ValueProperty, binding);
                return isAnimatable ? CreateAnimatablePropertyEditor<decimal>(editor, owner, property) : editor;
            }
            else if (type == typeof(float))
            {
                var editor = new FloatEditor();
                if (data != null)
                {
                    if (data.TryGetValue("min", out object min) && min is float minValue) editor.MinValue = minValue;
                    if (data.TryGetValue("max", out object max) && max is float maxValue) editor.MaxValue = maxValue;
                }
                editor.SetBinding(FloatEditor.ValueProperty, binding);
                return isAnimatable ? CreateAnimatablePropertyEditor<float>(editor, owner, property) : editor;
            }
            else if (type == typeof(short))
            {
                var editor = new ShortEditor();
                if (data != null)
                {
                    if (data.TryGetValue("min", out object min) && min is short minValue) editor.MinValue = minValue;
                    if (data.TryGetValue("max", out object max) && max is short maxValue) editor.MaxValue = maxValue;
                }
                editor.SetBinding(ShortEditor.ValueProperty, binding);
                return isAnimatable ? CreateAnimatablePropertyEditor<short>(editor, owner, property) : editor;
            }
            else if (type == typeof(ushort))
            {
                var editor = new UShortEditor();
                if (data != null)
                {
                    if (data.TryGetValue("min", out object min) && min is ushort minValue) editor.MinValue = minValue;
                    if (data.TryGetValue("max", out object max) && max is ushort maxValue) editor.MaxValue = maxValue;
                }
                editor.SetBinding(UShortEditor.ValueProperty, binding);
                return isAnimatable ? CreateAnimatablePropertyEditor<ushort>(editor, owner, property) : editor;
            }
            else if (type == typeof(byte))
            {
                var editor = new ByteEditor();
                if (data != null)
                {
                    if (data.TryGetValue("min", out object min) && min is byte minValue) editor.MinValue = minValue;
                    if (data.TryGetValue("max", out object max) && max is byte maxValue) editor.MaxValue = maxValue;
                }
                editor.SetBinding(ByteEditor.ValueProperty, binding);
                return isAnimatable ? CreateAnimatablePropertyEditor<byte>(editor, owner, property) : editor;
            }
            else if (type == typeof(sbyte))
            {
                var editor = new SByteEditor();
                if (data != null)
                {
                    if (data.TryGetValue("min", out object min) && min is sbyte minValue) editor.MinValue = minValue;
                    if (data.TryGetValue("max", out object max) && max is sbyte maxValue) editor.MaxValue = maxValue;
                }
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
                var editor = new DoubleEditor { Value = ((Progress)owner.GetValue(property)).Value, IncrementFactor = 0.001, MinValue = 0 };
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
                var editor = new ListEditor { Type = type, Margin = new Thickness(0, 0, -3, 0) };
                var genericIList = type.GetInterfaces().FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IList<>));
                if (genericIList != null) editor.ItemType = genericIList.GenericTypeArguments[0];
                editor.SetBinding(ObjectProperty, binding);
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
                var editor = new PropertiesEditor { IsAnimatable = isAnimatable, Type = type, Margin = new Thickness(0, 0, -3, 0) };
                editor.SetBinding(ObjectProperty, binding);
                return editor;
            }
            else return owner.GetValue(property) is DependencyObject dependencyObject ? new PropertiesEditor { IsAnimatable = isAnimatable, Object = dependencyObject, Margin = new Thickness(0, 0, -3, 0) } : null;
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
            var properties = dependencyObject is NotifyObject notifyObject ? notifyObject.AllDisplayableProperties : dependencyObject.GetType().GetAllDependencyProperties().Select(dp =>
            {
                var metadata = dp.GetMetadata(dependencyObject);
                return (dp, metadata as NotifyObjectPropertyMetadata ?? new NotifyObjectPropertyMetadata { Description = dp.Name });
            });

            try { foreach (var (property, metadata) in properties) await AddEditor(new EditableProperty(metadata.Description, GetEditorFromProperty(dependencyObject, property, metadata.Data, isAnimatable))); }
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
