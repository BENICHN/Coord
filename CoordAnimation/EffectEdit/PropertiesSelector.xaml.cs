using Coord;
using System.Windows;
using System.Windows.Controls;

namespace CoordAnimation
{
    /// <summary>
    /// Logique d'interaction pour PropertiesSelector.xaml
    /// </summary>
    public partial class PropertiesSelector : UserControl
    {
        public ICoordEditable Object { get => (ICoordEditable)GetValue(ObjectProperty); set => SetValue(ObjectProperty, value); }
        public static readonly DependencyProperty ObjectProperty = DependencyProperty.Register("Object", typeof(ICoordEditable), typeof(PropertiesSelector));

        public PropertiesSelector() => InitializeComponent();

        private void Types_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (Object is ICoordSelectable selectable && Types.SelectedItem is string s && selectable.Type != s) Object = selectable.GetObject(s); }
    }
}
