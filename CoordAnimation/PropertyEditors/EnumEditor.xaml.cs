using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace CoordAnimation
{
    /// <summary>
    /// Logique d'interaction pour EnumEditor.xaml
    /// </summary>
    public partial class EnumEditor : UserControl
    {
        public Enum Enumeration { get => (Enum)GetValue(EnumerationProperty); set => SetValue(EnumerationProperty, value); }
        public static readonly DependencyProperty EnumerationProperty = DependencyProperty.Register("Enumeration", typeof(Enum), typeof(EnumEditor));

        public EnumEditor() => InitializeComponent();

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == EnumerationProperty)
            {
                if (e.NewValue is Enum enumeration)
                {
                    var newType = enumeration.GetType();
                    if (e.OldValue?.GetType() != newType)
                    {
                        cb.ItemsSource = new ObservableCollection<string>(Enum.GetNames(newType));
                        cb.SelectedItem = e.NewValue.ToString();
                    }
                }
                else cb.ItemsSource = null;
            }
            base.OnPropertyChanged(e);
        }

        private void Cb_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (cb.SelectedItem is string s) Enumeration = (Enum)Enum.Parse(Enumeration.GetType(), s); }
    }
}
