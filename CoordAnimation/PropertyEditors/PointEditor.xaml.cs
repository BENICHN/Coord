using BenLib.Framework;
using BenLib.Standard;
using BenLib.WPF;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static System.Math;

namespace CoordAnimation
{
    /// <summary>
    /// Logique d'interaction pour PointEditor.xaml
    /// </summary>
    public partial class PointEditor : UserControl
    {
        private bool m_settingText;

        public event PropertyChangedExtendedEventHandler<Point> PointChanged;

        public bool IsPolar
        {
            get => Polar.Visibility == Visibility.Visible;
            set
            {
                if (value)
                {
                    SetText(GetPointFromText(), true);
                    (Coordinates.Visibility, Polar.Visibility) = (Visibility.Collapsed, Visibility.Visible);
                }
                else
                {
                    SetText(GetPointFromText(), false);
                    (Coordinates.Visibility, Polar.Visibility) = (Visibility.Visible, Visibility.Collapsed);
                }
            }
        }

        private async Task SetIsPolar(bool value)
        {
            if (value != IsPolar)
            {
                if (value && (!X.IsActivated || X.Desactivate(L)) && (!Y.IsActivated || Y.Desactivate(L)))
                {
                    IsPolar = true;
                    await Activate(L, X);
                }
                else if (!value && (!L.IsActivated || L.Desactivate(X)) && (!A.IsActivated || A.Desactivate(X)))
                {
                    IsPolar = false;
                    await Activate(X, L);
                }
            }
        }

        public bool IsRadian { get => AngleUnit.Text == "rad"; set => AngleUnit.Text = value ? "rad" : "°"; }

        public bool Prefix { get => (bool)GetValue(PrefixProperty); set => SetValue(PrefixProperty, value); }
        public static readonly DependencyProperty PrefixProperty = DependencyProperty.Register("Prefix", typeof(bool), typeof(PointEditor), new PropertyMetadata(true));

        public Point Point { get => (Point)GetValue(PointProperty); set => SetValue(PointProperty, value); }
        public static readonly DependencyProperty PointProperty = DependencyProperty.Register("Point", typeof(Point), typeof(PointEditor));

        public PointEditor() => InitializeComponent();

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == PointProperty)
            {
                var oldValue = (Point)e.OldValue;
                var newValue = (Point)e.NewValue;
                SetText(newValue, IsPolar);
                PointChanged?.Invoke(this, new PropertyChangedExtendedEventArgs<Point>("Point", oldValue, newValue));
            }
            base.OnPropertyChanged(e);
        }

        private Point GetPointFromText()
        {
            if (IsPolar)
            {
                double l = L.Value;
                if (l <= 0) return new Point(double.NaN, double.NaN);
                double a = A.Value;
                if (!IsRadian) a *= PI / 180;
                return new Point(Round(l * Cos(a), 3), Round(l * Sin(a), 3));
            }
            else return new Point(X.Value, Y.Value);
        }

        private void SetText(Point value, bool isPolar)
        {
            m_settingText = true;
            var (x, y) = (value.X, value.Y);
            if (isPolar)
            {
                double l = Sqrt(x * x + y * y);
                double a = Atan(y / x);
                L.Value = Round(l, 3);
                A.Value = IsRadian ? a : Round(a * 180 / PI, 3);
            }
            else
            {
                X.Value = x;
                Y.Value = y;
            }
            m_settingText = false;
        }

        private void SwitchableTextBox_Desactivated(object sender, EventArgs<IInputElement> e)
        {
            if (sender == e.Param1)
            {
                var point = Point;
                SetText(point, IsPolar);
            }
            else if (e.Param1 != X && e.Param1 != Y && e.Param1 != L && e.Param1 != A) Point = GetPointFromText();
        }

        protected async Task<bool> Activate(SwitchableTextBox newValue, SwitchableTextBox oldValue)
        {
            if (oldValue.Desactivate(newValue)) { while (!newValue.Activate()) await Task.Delay(1); }
            return newValue.IsActivated;
        }

        protected override async void OnPreviewKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.C:
                    e.Handled = true;
                    await SetIsPolar(false);
                    break;
                case Key.D:
                    e.Handled = true;
                    IsRadian = false;
                    await SetIsPolar(true);
                    break;
                case Key.R:
                    e.Handled = true;
                    IsRadian = true;
                    await SetIsPolar(true);
                    break;
                case Key.Tab:
                    e.Handled = true;
                    if (Input.IsShiftPressed())
                    {
                        if (A.IsActivated) await Activate(L, A);
                        if (Y.IsActivated) await Activate(X, Y);
                    }
                    else
                    {
                        if (L.IsActivated) await Activate(A, L);
                        if (X.IsActivated) await Activate(Y, X);
                    }
                    break;
                case Key.Escape:
                    if (X.IsActivated) X.Desactivate(X);
                    if (Y.IsActivated) Y.Desactivate(Y);
                    if (L.IsActivated) L.Desactivate(L);
                    if (A.IsActivated) A.Desactivate(A);
                    break;
            }
        }

        private void DoubleEditor_ValueChanged(object sender, PropertyChangedExtendedEventArgs<double> e) { if (!m_settingText && !(sender as DoubleEditor).IsActivated) Point = GetPointFromText(); }
    }
}
