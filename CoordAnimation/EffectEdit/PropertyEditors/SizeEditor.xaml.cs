using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using BenLib.Standard;

namespace CoordAnimation
{
    /// <summary>
    /// Logique d'interaction pour SizeEditor.xaml
    /// </summary>
    public partial class SizeEditor : UserControl, INotifyPropertyChanged
    {
        public double SizeWidth { get => Size.Width; set => Size = new Size(value, Size.Height); }
        public double SizeHeight { get => Size.Height; set => Size = new Size(Size.Width, value); }

        public Size Size { get => (Size)GetValue(SizeProperty); set => SetValue(SizeProperty, value); }
        public static readonly DependencyProperty SizeProperty = DependencyProperty.Register("Size", typeof(Size), typeof(SizeEditor));

        public event PropertyChangedEventHandler PropertyChanged;
        public new event PropertyChangedExtendedEventHandler<Size> SizeChanged;
        protected void NotifyPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public SizeEditor() => InitializeComponent();

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == SizeProperty)
            {
                NotifyPropertyChanged("SizeWidth");
                NotifyPropertyChanged("SizeHeight");
                SizeChanged?.Invoke(this, new PropertyChangedExtendedEventArgs<Size>("Size", (Size)e.OldValue, (Size)e.NewValue));
            }
            base.OnPropertyChanged(e);
        }
    }
}
