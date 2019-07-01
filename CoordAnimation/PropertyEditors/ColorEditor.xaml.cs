using BenLib.Framework;
using BenLib.Standard;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static BenLib.Framework.Imaging;

namespace CoordAnimation
{
    /// <summary>
    /// Logique d'interaction pour ColorEditor.xaml
    /// </summary>
    public partial class ColorEditor : UserControl, INotifyPropertyChanged
    {
        private bool m_setting;
        private double m_currentHue;
        private uint m_currentColorHex;

        public bool IsPointingHue { get; private set; }
        public bool IsPointingSB { get; private set; }

        public ImageSource HueImage
        {
            get
            {
                double width = hueselector.ActualWidth;
                double height = hueselector.ActualHeight;
                if (width * height == 0) return null;
                double fact = 360 / height;
                int h = (int)height;
                return LineByline((int)width, h, i => FromHSB(fact * (h - i), 1, 1));
            }
        }
        public ImageSource SBImage
        {
            get
            {
                double width = sbselector.ActualWidth;
                double height = sbselector.ActualHeight;
                if (width * height == 0) return null;
                double hue = m_currentHue;
                return PixelByPixel((int)width, (int)height, (row, column) => FromHSB(hue, column / width, 1 - row / height));
            }
        }

        public double Hue
        {
            get => Color.HSB().Hue;
            set
            {
                m_setting = true;
                var (_, s, b) = Color.HSB();
                OnColorChanged(value, s, b);
                Color = ColorFromHSB(m_currentHue, s, b);
                m_setting = false;
            }
        }

        private void SetCurrentHue(double h) { if (h >= 0) m_currentHue = h % 360; }

        private void OnColorChanged(double h, double s, double b)
        {
            SetCurrentHue(h);
            UpdateHueCursor(h);
            UpdateSBCursor(s, b);
            NotifyPropertyChanged("SBImage");
        }

        private void UpdateSBCursor(double s, double b) => sbcursor.Margin = new Thickness(s * sbselector.ActualWidth - 4, (1 - b) * sbselector.ActualHeight - 4, 0, 0);
        private void UpdateHueCursor(double h) => huecursor.Margin = new Thickness(0, (360 - (h >= 0 ? h : m_currentHue)).Trim(0, 358) * hueselector.ActualHeight / 360, 0, 0);

        public (double Saturation, double Brightness) SB
        {
            get
            {
                var (_, s, b) = Color.HSB();
                return (s, b);
            }
            set
            {
                m_setting = true;
                var (s, b) = value;
                Color = ColorFromHSB(m_currentHue, s, b);
                UpdateSBCursor(s, b);
                m_setting = false;
            }
        }

        public string ColorHex
        {
            get => Color.ToHex();
            set => Color = ColorFromHex(m_currentColorHex);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public Color Color { get => (Color)GetValue(ColorProperty); set => SetValue(ColorProperty, value); }
        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register("Color", typeof(Color), typeof(ColorEditor));

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == ColorProperty)
            {
                if (!m_setting && IsLoaded)
                {
                    var (h, s, b) = ((Color)e.NewValue).HSB();
                    OnColorChanged(h, s, b);
                }
                NotifyPropertyChanged("ColorHex");
            }

            base.OnPropertyChanged(e);
        }

        public ColorEditor()
        {
            InitializeComponent();
            colorBox.Regex = new Regex(@"^#?(\d|a|b|c|d|e|f){0,6}$", RegexOptions.IgnoreCase);
            colorBox.TextValidation = s =>
            {
                try { m_currentColorHex = uint.Parse(s, NumberStyles.HexNumber); return true; }
                catch (FormatException) { return false; }
            };
        }

        private void Hue_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OnlyPressed(MouseButton.Left) && hueselector.CaptureMouse())
            {
                IsPointingHue = true;
                SetHue(e.GetPosition(hueselector));
            }
        }
        private void Hue_MouseMove(object sender, MouseEventArgs e) { if (IsPointingHue) SetHue(e.GetPosition(hueselector)); }
        private void Hue_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
            {
                hueselector.ReleaseMouseCapture();
                IsPointingHue = false;
            }
        }
        private void SetHue(Point position) => Hue = (360 - position.Y * (360 / hueselector.ActualHeight)).Trim(0, 360);

        private void SB_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OnlyPressed(MouseButton.Left) && sbselector.CaptureMouse())
            {
                IsPointingSB = true;
                SetSB(e.GetPosition(sbselector));
            }
        }
        private void SB_MouseMove(object sender, MouseEventArgs e) { if (IsPointingSB) SetSB(e.GetPosition(sbselector)); }
        private void SB_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
            {
                sbselector.ReleaseMouseCapture();
                IsPointingSB = false;
            }
        }
        private void SetSB(Point position) => SB = ((position.X / sbselector.ActualWidth).Trim(0, 1), 1 - (position.Y / sbselector.ActualHeight).Trim(0, 1));

        private void LoadImages(object sender, EventArgs e)
        {
            NotifyPropertyChanged("HueImage");
            var (h, s, b) = Color.HSB();
            OnColorChanged(h, s, b);
        }
    }
}
