using BenLib.Standard;
using BenLib.Framework;
using Coord;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CoordAnimation
{
    /// <summary>
    /// Logique d'interaction pour RectPointEditor.xaml
    /// </summary>
    public partial class RectPointEditor : UserControl
    {
        public bool IsPointing { get; private set; }

        private RectPoint m_rectPoint;
        public RectPoint RectPoint
        {
            get => m_rectPoint;
            set
            {
                m_rectPoint = value;
                SetPoint();
                SetText();
                RectPointChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler RectPointChanged;

        public double PointsRadius
        {
            get => (double)GetValue(PointsRadiusProperty);
            set => SetValue(PointsRadiusProperty, value);
        }
        public static readonly DependencyProperty PointsRadiusProperty = DependencyProperty.Register("PointsRadius", typeof(double), typeof(RectPointEditor), new PropertyMetadata(7.0, OnPointsRadiusChangd));

        private static void OnPointsRadiusChangd(DependencyObject d, DependencyPropertyChangedEventArgs e) { if (d is RectPointEditor rectPointEditor && e.NewValue is double value) rectPointEditor.border.Margin = new Thickness((value - 1) / 2); }

        public RectPointEditor()
        {
            DataContext = this;
            InitializeComponent();
            Foreground ??= Brushes.Black;
            xprogressSB.CoerceText = IsProgress;
            yprogressSB.CoerceText = IsProgress;

            bool IsProgress(string s)
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

        private void SetText()
        {
            xprogressSB.Text = RectPoint.XProgress.ToString();
            yprogressSB.Text = RectPoint.YProgress.ToString();
        }

        private void SetPoint() => currentPoint.Margin = new Thickness(RectPoint.XProgress * (border.ActualWidth - 1), RectPoint.YProgress * (border.ActualHeight - 1), 0, 0);
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

        private void XprogressSB_Desactivated(object sender, EventArgs<IInputElement> e) => RectPoint = new RectPoint(xprogressSB.Text.ToDouble() ?? 0, RectPoint.YProgress);
        private void YprogressSB_Desactivated(object sender, EventArgs<IInputElement> e) => RectPoint = new RectPoint(RectPoint.XProgress, yprogressSB.Text.ToDouble() ?? 0);
    }
}
