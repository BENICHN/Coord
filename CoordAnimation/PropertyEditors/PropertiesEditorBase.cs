using BenLib.Framework;
using BenLib.Standard;
using BenLib.WPF;
using Coord;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace CoordAnimation
{
    public class PropertiesEditorBase : UserControl, IDisposable
    {
        public ObservableCollection<DataGridColumn> Columns => Editors.Columns;

        public bool IsAnimatable { get => (bool)GetValue(IsAnimatableProperty); set => SetValue(IsAnimatableProperty, value); }
        public static readonly DependencyProperty IsAnimatableProperty = DependencyProperty.Register("IsAnimatable", typeof(bool), typeof(PropertiesEditorBase), new PropertyMetadata(true));

        public ObservableCollection<IPropertyEditorContainer> Properties { get; } = new ObservableCollection<IPropertyEditorContainer>();
        public DataGrid Editors { get; private set; } = new DataGrid { SelectionMode = DataGridSelectionMode.Single, AutoGenerateColumns = false, CanUserSortColumns = false, CanUserReorderColumns = false, HeadersVisibility = DataGridHeadersVisibility.None, GridLinesVisibility = DataGridGridLinesVisibility.All };

        private static FrameworkElement s_currentEditor;
        internal bool? CancelCellChange = false;

        public PropertiesEditorBase()
        {
            Editors.ItemsSource = Properties;
            ScrollViewer.SetCanContentScroll(Editors, false);
            Editors.SelectedCellsChanged += Editors_SelectedCellsChanged;
            Editors.PreviewKeyDown += Editors_PreviewKeyDown;
            CurrentEditorChanged += OnCurrentEditorChanged;
        }

        private void OnCurrentEditorChanged(object sender, PropertyChangedExtendedEventArgs<FrameworkElement> e) { if (!ContainsEditor(e.NewValue)) Editors.SelectedItem = null; }
        public bool ContainsEditor(FrameworkElement editor) => Properties.Any(p => p.Editor == editor || p.Editor is PropertiesEditorBase propertiesEditorBase && propertiesEditorBase.ContainsEditor(editor));

        protected Task AddEditor(IPropertyEditorContainer container) => InsertEditor(Properties.Count, container);
        protected Task InsertEditor(int index, IPropertyEditorContainer container)
        {
            var tcs = new TaskCompletionSource<object>();
            if (container.Editor != null) Properties.Insert(index, container);

            if (container.Editor == null || container.Editor.IsLoaded) tcs.TrySetResult(null);
            else container.Editor.Loaded += Result_Loaded;

            return tcs.Task;

            void Result_Loaded(object sender, RoutedEventArgs e)
            {
                container.Editor.Loaded -= Result_Loaded;
                tcs.TrySetResult(null);
            }
        }

        protected void RemovePropertyAt(int index)
        {
            var properties = Properties;
            if (properties[index] == CurrentEditor || properties[index] is PropertiesEditorBase propertiesEditorBase && propertiesEditorBase.ContainsEditor(CurrentEditor)) CurrentEditor = default;

            var p = properties[index];
            p.Editor.ClearAllBindings();
            if (p.Editor is IDisposable disposable) disposable.Dispose();
            properties.RemoveAt(index);
        }

        public static FrameworkElement GetEditorFromProperty(DependencyObject owner, DependencyProperty property, NotifyObjectPropertyMetadata metadata, bool isAnimatable)
        {
            var type = property.PropertyType;
            var binding = new Binding(property.Name) { Source = owner, Mode = (((owner as Freezable)?.IsFrozen ?? false) || property.ReadOnly) ? BindingMode.OneTime : BindingMode.TwoWay, Converter = new VariantConverter() };
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
            else if (typeof(DependencyObject).IsAssignableFrom(type))
            {
                var editor = new PropertiesEditor { IsAnimatable = isAnimatable, Type = type };
                editor.SetBinding(PropertiesEditor.ObjectProperty, binding);
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

        protected void ClearProperties()
        {
            if (ContainsEditor(CurrentEditor)) CurrentEditor = default;
            var properties = Properties;
            int count;
            while ((count = properties.Count - 1) > -1)
            {
                var p = properties[count];
                p.Editor.ClearAllBindings();
                if (p.Editor is IDisposable disposable) disposable.Dispose();
                properties.RemoveAt(count);
            }
        }

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
                var editor = ((IPropertyEditorContainer)editors.SelectedItem)?.Editor;
                if (IsAnimatable && editor != null && !(editor is PropertiesEditorBase propertiesEditorBase && propertiesEditorBase.ContainsEditor(CurrentEditor))) CurrentEditor = ((IPropertyEditorContainer)editors.SelectedItem).Editor;
            }
        }

        public void Dispose()
        {
            CurrentEditorChanged -= OnCurrentEditorChanged;
            ClearProperties();
        }

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

        public static event PropertyChangedExtendedEventHandler<FrameworkElement> CurrentEditorChanged;
    }

    public class VariantConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
    }

    public interface IPropertyEditorContainer { FrameworkElement Editor { get; } }
}
