using System.Windows;
using System.Windows.Controls;

namespace CoordAnimation
{
    /// <summary>
    /// Logique d'interaction pour VisualObjectControl.xaml
    /// </summary>
    public partial class VisualObjectControl : UserControl
    {
        public string ObjectName
        {
            get => (string)GetValue(ObjectNameProperty);
            set => SetValue(ObjectNameProperty, value);
        }
        public static readonly DependencyProperty ObjectNameProperty = DependencyProperty.Register("ObjectName", typeof(string), typeof(VisualObjectControl));

        public string ObjectType
        {
            get => (string)GetValue(ObjectTypeProperty);
            set => SetValue(ObjectTypeProperty, value);
        }
        public static readonly DependencyProperty ObjectTypeProperty = DependencyProperty.Register("ObjectType", typeof(string), typeof(VisualObjectControl));

        public VisualObjectControl()
        {
            InitializeComponent();
            dk.DataContext = this;
        }
    }

    public class VisualObjectIcon : ContentControl
    {
        public string Type
        {
            get => (string)GetValue(TypeProperty);
            set => SetValue(TypeProperty, value);
        }
        public static readonly DependencyProperty TypeProperty = DependencyProperty.Register("Type", typeof(string), typeof(VisualObjectIcon), new PropertyMetadata(string.Empty, OnTypeChanged));

        private static void OnTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) { if (d is ContentControl contentControl) contentControl.SetResourceReference(ContentProperty, e.NewValue == null || contentControl.TryFindResource(e.NewValue) == null ? "VisualObject" : e.NewValue); }
    }
}
