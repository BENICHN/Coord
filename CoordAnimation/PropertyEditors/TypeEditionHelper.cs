using BenLib.Framework;
using BenLib.Standard;
using BenLib.WPF;
using Coord;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
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

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) => reader.TokenType switch
        {
            JsonToken.StartObject => ReadObject(reader),
            JsonToken.StartArray => (object)ReadList(reader),
            _ => throw new FormatException()
        };

        private DependencyObject ReadObject(JsonReader reader)
        {
            Type type = null;
            DependencyObject result = null;
            string pn = null;
            DependencyProperty dp = null;
            try
            {
                while (reader.Read())
                {
                    if (pn == "[Type]")
                    {
                        type = Type.GetType((string)reader.Value);
                        result = (DependencyObject)Activator.CreateInstance(type);
                    }
                    else
                    {
                        if (reader.TokenType == JsonToken.PropertyName)
                        {
                            pn = (string)reader.Value;
                            if (pn != "[Type]") dp = DependencyPropertyDescriptor.FromName(pn, type, type).DependencyProperty;
                            continue;
                        }
                        else if (reader.TokenType == JsonToken.Null) result.SetValue(dp, null);
                        else if (reader.TokenType == JsonToken.StartObject) result.SetValue(dp, ReadObject(reader));
                        else if (reader.TokenType == JsonToken.StartArray) result.SetValue(dp, ReadList(reader));
                        else if (reader.TokenType == JsonToken.EndObject) break;
                        else if (TypeEditionHelper.FromType(dp.PropertyType) is ITypeEditionHelper helper) result.SetValue(dp, helper.Deserialize((string)reader.Value));
                    }
                    pn = null;
                    dp = null;
                }
                return result;
            }
            catch { return null; }
        }

        private IList ReadList(JsonReader reader)
        {
            Type itemType = null;
            IList result = null;
            bool isType = true;
            try
            {
                while (reader.Read())
                {
                    if (isType)
                    {
                        var type = Type.GetType((string)reader.Value);
                        result = (IList)Activator.CreateInstance(type);
                        var genericIList = type.GetInterfaces().FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IList<>));
                        if (genericIList != null) itemType = genericIList.GenericTypeArguments[0];
                        isType = false;
                    }
                    else
                    {
                        if (reader.TokenType == JsonToken.Null) result.Add(null);
                        else if (reader.TokenType == JsonToken.StartObject) result.Add(ReadObject(reader));
                        else if (reader.TokenType == JsonToken.StartArray) result.Add(ReadList(reader));
                        else if (reader.TokenType == JsonToken.EndArray) break;
                        else if (TypeEditionHelper.FromType(itemType) is ITypeEditionHelper helper) result.Add(helper.Deserialize((string)reader.Value));
                    }
                }
                return result;
            }
            catch { return null; }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is IList list)
            {
                writer.WriteStartArray();
                writer.WriteValue(list.GetType().AssemblyQualifiedName);
                foreach (object item in list) { if (TypeEditionHelper.FromType(item.GetType()) is ITypeEditionHelper helper) writer.WriteRawValue(helper.Serialize(item)); }
                writer.WriteEndArray();
            }
            else if (value is DependencyObject dependencyObject)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("[Type]");
                writer.WriteValue(dependencyObject.GetType().AssemblyQualifiedName);
                foreach (var property in dependencyObject is NotifyObject notifyObject ? notifyObject.AllProperties : dependencyObject.GetType().GetAllDependencyProperties())
                {
                    if (TypeEditionHelper.FromType(property.PropertyType) is ITypeEditionHelper helper)
                    {
                        writer.WritePropertyName(property.Name);
                        string s = helper.Serialize(dependencyObject.GetValue(property));
                        if (helper.IsRaw) writer.WriteRawValue(s);
                        else writer.WriteValue(s);
                    }
                }
                writer.WriteEndObject();
            }
        }
    }

    public static class TypeEditionHelper
    {
        public static FrameworkElement GetEditorFromProperty(DependencyObject owner, DependencyProperty property, Dictionary<string, object> data, bool isAnimatable)
        {
            var helper = FromType(property.PropertyType);
            return
                helper == null ? (FrameworkElement)(owner.GetValue(property) is DependencyObject dependencyObject ? new PropertiesEditor { IsAnimatable = isAnimatable, Object = dependencyObject, Margin = new Thickness(0, 0, -3, 0) } : null) :
                helper.GetEditorFromProperty(owner, property, data, isAnimatable);
        }

        public static ITypeEditionHelper FromType(Type type) =>
            type == typeof(string) ? new StringEditionHelper() :
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
            typeof(Interval<int>).IsAssignableFrom(type) ? new IntIntervalEditionHelper() :
            typeof(Interval<double>).IsAssignableFrom(type) ? new DoubleIntervalEditionHelper() :
            typeof(Interval<decimal>).IsAssignableFrom(type) ? new DecimalIntervalEditionHelper() :
            type == typeof(Point) ? new PointEditionHelper() :
            type == typeof(Vector) ? new VectorEditionHelper() :
            type == typeof(Rect) ? new RectEditionHelper() :
            type == typeof(Size) ? new SizeEditionHelper() :
            type == typeof(Color) ? new ColorEditionHelper() :
            type == typeof(RectPoint) ? new RectPointEditionHelper() :
            type == typeof(Progress) ? new ProgressEditionHelper() :
            type == typeof(bool) ? new BooleanEditionHelper() :
            type == typeof(bool?) ? new NullableBooleanEditionHelper() :
            typeof(Enum).IsAssignableFrom(type) ? new EnumEditionHelper(type) :
            typeof(IList).IsAssignableFrom(type) ? new ListEditionHelper(type) :
            typeof(VisualObject).IsAssignableFrom(type) ? new VisualObjectEditionHelper(type) :
            typeof(DependencyObject).IsAssignableFrom(type) || type.IsInterface && App.DependencyObjectTypes.Contains(type) ? new DependencyObjectEditionHelper(type) :
            (ITypeEditionHelper)null;
    }

    public interface ITypeEditionHelper
    {
        bool IsRaw { get; }
        object Deserialize(string data);
        string Serialize(object obj);

        bool IsAnimatable { get; }
        FrameworkElement GetEditorFromProperty(DependencyObject owner, DependencyProperty property, Dictionary<string, object> data, bool isAnimatable);
        FrameworkElement GetEditor(Dictionary<string, object> data);
        DependencyProperty BindingProperty { get; }
    }

    public abstract class TypeEditionHelper<T> : ITypeEditionHelper
    {
        public virtual bool IsAnimatable => true;
        public virtual bool IsRaw => false;

        public FrameworkElement GetEditorFromProperty(DependencyObject owner, DependencyProperty property, Dictionary<string, object> data, bool isAnimatable)
        {
            var editor = GetEditor(data);
            editor.SetBinding(BindingProperty, new Binding(property.Name) { Source = owner, Mode = (((owner as Freezable)?.IsFrozen ?? false) || property.ReadOnly) ? BindingMode.OneTime : BindingMode.TwoWay });
            return isAnimatable && IsAnimatable ? CreateAnimatablePropertyEditor(editor, owner, property) : editor;
        }

        private static FrameworkElement CreateAnimatablePropertyEditor(FrameworkElement editor, DependencyObject dependencyObject, DependencyProperty dependencyProperty)
        {
            var result = AnimatablePropertyEditor.Create<T>(editor, dependencyObject, dependencyProperty);
            result.DataContext.RequestDataFocus += (sender, e) => App.Scene.Timeline.SetKeyFrames((AbsoluteKeyFrameCollection<T>)e.Param1);
            return result;
        }

        public abstract FrameworkElement GetEditor(Dictionary<string, object> data);
        public abstract DependencyProperty BindingProperty { get; }

        public abstract T Deserialize(string data);
        public abstract string Serialize(T obj);

        object ITypeEditionHelper.Deserialize(string data) => Deserialize(data);
        string ITypeEditionHelper.Serialize(object obj) => Serialize((T)obj);
    }

    public class StringEditionHelper : TypeEditionHelper<string>
    {
        public override string Deserialize(string data) => data;
        public override FrameworkElement GetEditor(Dictionary<string, object> data) => new SwitchableTextBox();

        public override string Serialize(string obj) => obj;
        public override DependencyProperty BindingProperty => SwitchableTextBox.TextProperty;
    }

    public class FloatEditionHelper : TypeEditionHelper<float>
    {
        public override float Deserialize(string data) => float.Parse(data);
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

        public override string Serialize(float obj) => obj.ToString(CultureInfo.InvariantCulture);
        public override DependencyProperty BindingProperty => FloatEditor.ValueProperty;
    }

    public class DoubleEditionHelper : TypeEditionHelper<double>
    {
        public override double Deserialize(string data) => double.Parse(data);
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

        public override string Serialize(double obj) => obj.ToString(CultureInfo.InvariantCulture);
        public override DependencyProperty BindingProperty => DoubleEditor.ValueProperty;
    }

    public class DecimalEditionHelper : TypeEditionHelper<decimal>
    {
        public override decimal Deserialize(string data) => decimal.Parse(data);
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

        public override string Serialize(decimal obj) => obj.ToString(CultureInfo.InvariantCulture);
        public override DependencyProperty BindingProperty => DecimalEditor.ValueProperty;
    }

    public class LongEditionHelper : TypeEditionHelper<long>
    {
        public override long Deserialize(string data) => long.Parse(data);
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

        public override string Serialize(long obj) => obj.ToString(CultureInfo.InvariantCulture);
        public override DependencyProperty BindingProperty => LongEditor.ValueProperty;
    }

    public class ULongEditionHelper : TypeEditionHelper<ulong>
    {
        public override ulong Deserialize(string data) => ulong.Parse(data);
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

        public override string Serialize(ulong obj) => obj.ToString(CultureInfo.InvariantCulture);
        public override DependencyProperty BindingProperty => ULongEditor.ValueProperty;
    }

    public class IntEditionHelper : TypeEditionHelper<int>
    {
        public override int Deserialize(string data) => int.Parse(data);
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

        public override string Serialize(int obj) => obj.ToString(CultureInfo.InvariantCulture);
        public override DependencyProperty BindingProperty => IntEditor.ValueProperty;
    }

    public class UIntEditionHelper : TypeEditionHelper<uint>
    {
        public override uint Deserialize(string data) => uint.Parse(data);
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

        public override string Serialize(uint obj) => obj.ToString(CultureInfo.InvariantCulture);
        public override DependencyProperty BindingProperty => UIntEditor.ValueProperty;
    }

    public class ShortEditionHelper : TypeEditionHelper<short>
    {
        public override short Deserialize(string data) => short.Parse(data);
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

        public override string Serialize(short obj) => obj.ToString(CultureInfo.InvariantCulture);
        public override DependencyProperty BindingProperty => ShortEditor.ValueProperty;
    }

    public class UShortEditionHelper : TypeEditionHelper<ushort>
    {
        public override ushort Deserialize(string data) => ushort.Parse(data);
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

        public override string Serialize(ushort obj) => obj.ToString(CultureInfo.InvariantCulture);
        public override DependencyProperty BindingProperty => UShortEditor.ValueProperty;
    }

    public class SByteEditionHelper : TypeEditionHelper<sbyte>
    {
        public override sbyte Deserialize(string data) => sbyte.Parse(data);
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

        public override string Serialize(sbyte obj) => obj.ToString(CultureInfo.InvariantCulture);
        public override DependencyProperty BindingProperty => SByteEditor.ValueProperty;
    }

    public class ByteEditionHelper : TypeEditionHelper<byte>
    {
        public override byte Deserialize(string data) => byte.Parse(data);
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

        public override string Serialize(byte obj) => obj.ToString(CultureInfo.InvariantCulture);
        public override DependencyProperty BindingProperty => ByteEditor.ValueProperty;
    }

    public class PointEditionHelper : TypeEditionHelper<Point>
    {
        public override Point Deserialize(string data) => Point.Parse(data);
        public override FrameworkElement GetEditor(Dictionary<string, object> data) => new PointEditor();

        public override string Serialize(Point obj) => obj.ToString(CultureInfo.InvariantCulture);
        public override DependencyProperty BindingProperty => PointEditor.PointProperty;
    }

    public class VectorEditionHelper : TypeEditionHelper<Vector>
    {
        public override Vector Deserialize(string data) => Vector.Parse(data);
        public override FrameworkElement GetEditor(Dictionary<string, object> data) => new VectorEditor();

        public override string Serialize(Vector obj) => obj.ToString(CultureInfo.InvariantCulture);
        public override DependencyProperty BindingProperty => VectorEditor.VectorProperty;
    }

    public class RectEditionHelper : TypeEditionHelper<Rect>
    {
        public override Rect Deserialize(string data) => Rect.Parse(data);
        public override FrameworkElement GetEditor(Dictionary<string, object> data) => new RectEditor { Height = 120 };

        public override string Serialize(Rect obj) => obj.ToString(CultureInfo.InvariantCulture);
        public override DependencyProperty BindingProperty => RectEditor.RectProperty;
    }

    public class SizeEditionHelper : TypeEditionHelper<Size>
    {
        public override Size Deserialize(string data) => Size.Parse(data);
        public override FrameworkElement GetEditor(Dictionary<string, object> data) => new SizeEditor { Height = 120 };

        public override string Serialize(Size obj) => obj.ToString(CultureInfo.InvariantCulture);
        public override DependencyProperty BindingProperty => SizeEditor.SizeProperty;
    }

    public class ColorEditionHelper : TypeEditionHelper<Color>
    {
        public override Color Deserialize(string data) => Imaging.ColorFromHex(uint.Parse(data, NumberStyles.HexNumber));
        public override FrameworkElement GetEditor(Dictionary<string, object> data) => new ColorEditor { Height = 180 };

        public override string Serialize(Color obj) => obj.ToHex();
        public override DependencyProperty BindingProperty => ColorEditor.ColorProperty;
    }

    public class RectPointEditionHelper : TypeEditionHelper<RectPoint>
    {
        public override RectPoint Deserialize(string data) => RectPoint.Parse(data);
        public override FrameworkElement GetEditor(Dictionary<string, object> data) => new RectPointEditor { Height = 120 };

        public override string Serialize(RectPoint obj) => obj.ToString();
        public override DependencyProperty BindingProperty => RectPointEditor.RectPointProperty;
    }

    public class ProgressEditionHelper : TypeEditionHelper<Progress>
    {
        public override Progress Deserialize(string data) => Progress.Parse(data);        

        public override FrameworkElement GetEditor(Dictionary<string, object> data) => new ProgressEditor();

        public override string Serialize(Progress obj) => obj.ToString();
        public override DependencyProperty BindingProperty => ProgressEditor.ProgressProperty;
    }

    public class BooleanEditionHelper : TypeEditionHelper<bool>
    {
        public override bool Deserialize(string data) => bool.Parse(data);
        public override FrameworkElement GetEditor(Dictionary<string, object> data) => new CheckBox();

        public override string Serialize(bool obj) => obj.ToString();
        public override DependencyProperty BindingProperty => ToggleButton.IsCheckedProperty;
    }

    public class NullableBooleanEditionHelper : TypeEditionHelper<bool?>
    {
        public override bool? Deserialize(string data) => data == null ? (bool?)null : bool.Parse(data);
        public override FrameworkElement GetEditor(Dictionary<string, object> data) => new CheckBox { IsThreeState = true };

        public override string Serialize(bool? obj) => obj?.ToString();
        public override DependencyProperty BindingProperty => ToggleButton.IsCheckedProperty;
    }

    public class IntIntervalEditionHelper : TypeEditionHelper<Interval<int>>
    {
        public override Interval<int> Deserialize(string data) => Interval<int>.Parse(data, s => int.Parse(s));
        public override FrameworkElement GetEditor(Dictionary<string, object> data) => new IntervalEditor { IntervalType = IntervalType.Int };

        public override string Serialize(Interval<int> obj) => obj.ToString();
        public override DependencyProperty BindingProperty => IntervalEditor.IntIntervalProperty;
    }

    public class DoubleIntervalEditionHelper : TypeEditionHelper<Interval<double>>
    {
        public override Interval<double> Deserialize(string data) => Interval<double>.Parse(data, s => double.Parse(s));
        public override FrameworkElement GetEditor(Dictionary<string, object> data) => new IntervalEditor { IntervalType = IntervalType.Double };

        public override string Serialize(Interval<double> obj) => obj.ToString();
        public override DependencyProperty BindingProperty => IntervalEditor.DoubleIntervalProperty;
    }

    public class DecimalIntervalEditionHelper : TypeEditionHelper<Interval<decimal>>
    {
        public override Interval<decimal> Deserialize(string data) => Interval<decimal>.Parse(data, s => decimal.Parse(s));
        public override FrameworkElement GetEditor(Dictionary<string, object> data) => new IntervalEditor { IntervalType = IntervalType.Decimal };

        public override string Serialize(Interval<decimal> obj) => obj.ToString();
        public override DependencyProperty BindingProperty => IntervalEditor.DecimalIntervalProperty;
    }

    public class EnumEditionHelper : TypeEditionHelper<Enum>
    {
        public EnumEditionHelper(Type enumType) => EnumType = enumType;

        public Type EnumType { get; }

        public override Enum Deserialize(string data) => (Enum)Enum.Parse(EnumType, data);
        public override FrameworkElement GetEditor(Dictionary<string, object> data) => new EnumEditor();

        public override string Serialize(Enum obj) => obj.ToString();
        public override DependencyProperty BindingProperty => EnumEditor.EnumerationProperty;
    }

    public class ListEditionHelper : TypeEditionHelper<IList>
    {
        public override bool IsAnimatable => false;
        public override bool IsRaw => true;
        public ListEditionHelper(Type type) => Type = type;

        public Type Type { get; }

        public override IList Deserialize(string data) => (IList)JsonConvert.DeserializeObject(data, Type, new EditableTypeConverter());
        public override FrameworkElement GetEditor(Dictionary<string, object> data)
        {
            var type = Type;
            var editor = new ListEditor { Type = type, Margin = new Thickness(0, 0, -3, 0) };
            var genericIList = type.GetInterfaces().FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IList<>));
            if (genericIList != null) editor.ItemType = genericIList.GenericTypeArguments[0];
            return editor;
        }

        public override string Serialize(IList obj) => JsonConvert.SerializeObject(obj, Formatting.Indented, new EditableTypeConverter());
        public override DependencyProperty BindingProperty => PropertiesEditorBase.ObjectProperty;
    }

    public class DependencyObjectEditionHelper : TypeEditionHelper<DependencyObject>
    {
        public override bool IsAnimatable => false;
        public override bool IsRaw => true;
        public DependencyObjectEditionHelper(Type type) => Type = type;

        public Type Type { get; }

        public override DependencyObject Deserialize(string data) => (DependencyObject)JsonConvert.DeserializeObject(data, Type, new EditableTypeConverter());
        public override FrameworkElement GetEditor(Dictionary<string, object> data) => new PropertiesEditor { Type = Type, Margin = new Thickness(0, 0, -3, 0) };

        public override string Serialize(DependencyObject obj) => JsonConvert.SerializeObject(obj, Formatting.Indented, new EditableTypeConverter());
        public override DependencyProperty BindingProperty => PropertiesEditorBase.ObjectProperty;
    }

    public class VisualObjectEditionHelper : DependencyObjectEditionHelper
    {
        public bool IsPointing => Type != typeof(VisualObject);

        public VisualObjectEditionHelper() : base(typeof(VisualObject)) { }
        public VisualObjectEditionHelper(Type type) : base(type) { }

        public override FrameworkElement GetEditor(Dictionary<string, object> data) => new VisualObjectSelector { Selection = App.Scene.Plane.Selection, Pointing = IsPointing ? Type : null };
        public override DependencyProperty BindingProperty => VisualObjectSelector.VisualObjectProperty;
    }
}
