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
    /// Logique d'interaction pour VectorEditor.xaml
    /// </summary>
    public partial class VectorEditor : UserControl
    {
        public event PropertyChangedExtendedEventHandler<Vector> VectorChanged;

        public bool IsPolar
        {
            get => Polar.Visibility == Visibility.Visible;
            set
            {
                if (value)
                {
                    SetText(GetVectorFromText(), true);
                    (Coordinates.Visibility, Polar.Visibility) = (Visibility.Collapsed, Visibility.Visible);
                }
                else
                {
                    SetText(GetVectorFromText(), false);
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

        public Vector Vector { get => (Vector)GetValue(VectorProperty); set => SetValue(VectorProperty, value); }
        public static readonly DependencyProperty VectorProperty = DependencyProperty.Register("Vector", typeof(Vector), typeof(VectorEditor));

        public VectorEditor() => InitializeComponent();

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == VectorProperty)
            {
                var oldValue = (Vector)e.OldValue;
                var newValue = (Vector)e.NewValue;
                SetText(newValue, IsPolar);
                VectorChanged?.Invoke(this, new PropertyChangedExtendedEventArgs<Vector>("Vector", oldValue, newValue));
            }
            base.OnPropertyChanged(e);
        }

        private Vector GetVectorFromText()
        {
            if (IsPolar)
            {
                double l = L.Value;
                if (l <= 0) return new Vector(double.NaN, double.NaN);
                double a = A.Value;
                if (!IsRadian) a *= PI / 180;
                return new Vector(Round(l * Cos(a), 3), Round(l * Sin(a), 3));
            }
            else return new Vector(X.Value, Y.Value);
        }

        private void SetText(Vector value, bool isPolar)
        {
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
        }

        private void SwitchableTextBox_Desactivated(object sender, EventArgs<IInputElement> e)
        {
            if (sender == e.Param1)
            {
                var vector = Vector;
                SetText(vector, IsPolar);
            }
            else if (e.Param1 != X && e.Param1 != Y && e.Param1 != L && e.Param1 != A) Vector = GetVectorFromText();
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
    }
}
