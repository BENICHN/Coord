using BenLib.Framework;
using BenLib.Standard;
using Coord;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CoordAnimation
{
    /// <summary>
    /// Logique d'interaction pour RectPointEditor.xaml
    /// </summary>
    public partial class RectPointEditor : UserControl, INotifyPropertyChanged
    {
        public bool IsPointing { get; private set; }

        public Thickness PointMargin => new Thickness(RectPoint.XProgress * (border?.ActualWidth ?? 0 - 1), RectPoint.YProgress * (border?.ActualHeight ?? 0 - 1), 0, 0);
        public double RectPointXProgress { get => RectPoint.XProgress; set => RectPoint = new RectPoint(value, RectPoint.YProgress); }
        public double RectPointYProgress { get => RectPoint.YProgress; set => RectPoint = new RectPoint(RectPoint.XProgress, value); }

        public RectPoint RectPoint { get => (RectPoint)GetValue(RectPointProperty); set => SetValue(RectPointProperty, value); }
        public static readonly DependencyProperty RectPointProperty = DependencyProperty.Register("RectPoint", typeof(RectPoint), typeof(RectPointEditor));

        public event PropertyChangedExtendedEventHandler<RectPoint> RectPointChanged;
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public double PointsRadius
        {
            get => (double)GetValue(PointsRadiusProperty);
            set => SetValue(PointsRadiusProperty, value);
        }
        public static readonly DependencyProperty PointsRadiusProperty = DependencyProperty.Register("PointsRadius", typeof(double), typeof(RectPointEditor), new PropertyMetadata(7.0));

        public RectPointEditor()
        {
            InitializeComponent();
            Loaded += (sender, e) => NotifyPropertyChanged("PointMargin");
            xprogressSB.TextValidation = IsProgress;
            yprogressSB.TextValidation = IsProgress;

            static bool IsProgress(string s)
            {
                if (s.IsDouble(out double d).ShowException())
                {
                    if (d < 0 || d > 1)
                    {
                        ThreadingFramework.ShowException(new ArgumentOutOfRangeException("progress", "La valeur doit être comprise entre 0 et 1"));
                        return false;
                    }
                    return true;
                }
                else return false;
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == RectPointProperty)
            {
                NotifyPropertyChanged("PointMargin");
                NotifyPropertyChanged("RectPointXProgress");
                NotifyPropertyChanged("RectPointYProgress");
                RectPointChanged?.Invoke(this, new PropertyChangedExtendedEventArgs<RectPoint>("RectPoint", (RectPoint)e.OldValue, (RectPoint)e.NewValue));
            }
            else if (e.Property == PointsRadiusProperty) border.Margin = new Thickness(((double)e.NewValue - 1) / 2);
            base.OnPropertyChanged(e);
        }

        private RectPoint GetPoint(Point position)
        {
            double w = border.ActualWidth - 1;
            double h = border.ActualHeight - 1;
            double tw = Input.IsShiftPressed() ? 0.1 * w : 0;
            double th = Input.IsShiftPressed() ? 0.1 * h : 0;
            return new RectPoint(Math.Round((position.X.Magnet(0, tw).Magnet(w / 2, tw).Magnet(w, tw) / w).Trim(), 3), Math.Round((position.Y.Magnet(0, th).Magnet(h / 2, th).Magnet(h, th) / h).Trim(), 3));
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsPointing && e.OnlyPressed(MouseButton.Left) && border.CaptureMouse())
            {
                IsPointing = true;
                RectPoint = GetPoint(e.GetPosition(border));
            }
        }
        private void Border_MouseMove(object sender, MouseEventArgs e) { if (IsPointing) RectPoint = GetPoint(e.GetPosition(border)); }
        private void Border_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (IsPointing && e.LeftButton == MouseButtonState.Released)
            {
                border.ReleaseMouseCapture();
                IsPointing = false;
                RectPoint = GetPoint(e.GetPosition(border));
            }
        }
    }
}
