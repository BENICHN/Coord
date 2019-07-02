using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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

        public VisualObjectControl() => InitializeComponent();
    }

    public class VisualObjectIcon : Image
    {
        public string Type { get => (string)GetValue(TypeProperty); set => SetValue(TypeProperty, value); }
        public static readonly DependencyProperty TypeProperty = DependencyProperty.Register("Type", typeof(string), typeof(VisualObjectIcon));

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == TypeProperty)
            {
                string value = $"DI_{e.NewValue}";
                SetResourceReference(SourceProperty, TryFindResource(value) is ImageSource ? value : "DI_VisualObject");
            }

            base.OnPropertyChanged(e);
        }
    }
}
