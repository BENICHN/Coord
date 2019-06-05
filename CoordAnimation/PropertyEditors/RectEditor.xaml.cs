using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using BenLib.Standard;

namespace CoordAnimation
{
    /// <summary>
    /// Logique d'interaction pour RectEditor.xaml
    /// </summary>
    public partial class RectEditor : UserControl, INotifyPropertyChanged
    {
        public Point RectOrigin { get => Rect.TopLeft; set => Rect = new Rect(value, RectSize); }
        public Size RectSize { get => Rect.Size; set => Rect = new Rect(RectOrigin, value); }

        public Rect Rect { get => (Rect)GetValue(RectProperty); set => SetValue(RectProperty, value); }
        public static readonly DependencyProperty RectProperty = DependencyProperty.Register("Rect", typeof(Rect), typeof(RectEditor));

        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangedExtendedEventHandler<Rect> RectChanged;
        protected void NotifyPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public RectEditor() => InitializeComponent();

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == RectProperty)
            {
                NotifyPropertyChanged("RectOrigin");
                NotifyPropertyChanged("RectSize");
                RectChanged?.Invoke(this, new PropertyChangedExtendedEventArgs<Rect>("Rect", (Rect)e.OldValue, (Rect)e.NewValue));
            }
            base.OnPropertyChanged(e);
        }
    }
}
