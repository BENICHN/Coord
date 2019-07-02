using BenLib.Standard;
using Coord;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace CoordAnimation
{
    /// <summary>
    /// Logique d'interaction pour ProgressEditor.xaml
    /// </summary>
    public partial class ProgressEditor : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public double Value
        {
            get => Progress.Value;
            set
            {
                var progress = Progress;
                Progress = new Progress(value, progress.Mode, progress.LagFactor);
            }
        }

        public ProgressMode Mode
        {
            get => Progress.Mode;
            set
            {
                var progress = Progress;
                Progress = new Progress(progress.Value, value, progress.LagFactor);
            }
        }

        public double LagFactor
        {
            get => Progress.LagFactor;
            set
            {
                var progress = Progress;
                Progress = new Progress(progress.Value, progress.Mode, value);
            }
        }

        public bool IsLagVisible => Mode == ProgressMode.LaggedStart;

        public event PropertyChangedExtendedEventHandler<Progress> ProgressChanged;

        public Progress Progress { get => (Progress)GetValue(ProgressProperty); set => SetValue(ProgressProperty, value); }
        public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register("Progress", typeof(Progress), typeof(ProgressEditor));

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == ProgressProperty)
            {
                NotifyPropertyChanged("Value");
                NotifyPropertyChanged("Mode");
                NotifyPropertyChanged("IsLagVisible");
                NotifyPropertyChanged("LagFactor");
                ProgressChanged?.Invoke(this, new PropertyChangedExtendedEventArgs<Progress>("Progress", (Progress)e.OldValue, (Progress)e.NewValue));
            }
            base.OnPropertyChanged(e);
        }

        public ProgressEditor() => InitializeComponent();
    }
}
