using BenLib.Framework;
using BenLib.Standard;
using BenLib.WPF;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CoordAnimation
{
    /// <summary>
    /// Logique d'interaction pour RangeEditor.xaml
    /// </summary>
    public partial class RangeEditor : UserControl
    {
        #region Champs et propriétés

        public bool EnableUICorrection
        {
            get => (bool)GetValue(EnableUICorrectionProperty);
            set => SetValue(EnableUICorrectionProperty, value);
        }
        public static readonly DependencyProperty EnableUICorrectionProperty = DependencyProperty.Register("EnableUICorrection", typeof(bool), typeof(RangeEditor), new PropertyMetadata(true));

        public bool OverDisplayed => overSB.Visibility == Visibility.Visible;

        public event EventHandler RangeChanged;
        public event EventHandler LeftTabOverflow;
        public event EventHandler RightTabOverflow;
        public event EventHandler RemoveRequested;
        public event EventHandler TextBoxDesactivated;

        public Range<int> IntRange { get => (Range<int>)GetValue(IntRangeProperty); set => SetValue(IntRangeProperty, value ?? Interval<int>.EmptySet); }
        public static readonly DependencyProperty IntRangeProperty = DependencyProperty.Register("IntRange", typeof(Range<int>), typeof(RangeEditor), new PropertyMetadata(Range<int>.EmptySet));

        public Range<double> DoubleRange { get => (Range<double>)GetValue(DoubleRangeProperty); set => SetValue(DoubleRangeProperty, value ?? Interval<double>.EmptySet); }
        public static readonly DependencyProperty DoubleRangeProperty = DependencyProperty.Register("DoubleRange", typeof(Range<double>), typeof(RangeEditor), new PropertyMetadata(Interval<double>.EmptySet));

        public Range<decimal> DecimalRange { get => (Range<decimal>)GetValue(DecimalRangeProperty); set => SetValue(DecimalRangeProperty, value ?? Interval<decimal>.EmptySet); }
        public static readonly DependencyProperty DecimalRangeProperty = DependencyProperty.Register("DecimalRange", typeof(Range<decimal>), typeof(RangeEditor), new PropertyMetadata(Interval<decimal>.EmptySet));

        public IntervalType RangeType { get => (IntervalType)GetValue(RangeTypeProperty); set => SetValue(RangeTypeProperty, value); }
        public static readonly DependencyProperty RangeTypeProperty = DependencyProperty.Register("RangeType", typeof(IntervalType), typeof(RangeEditor), new PropertyMetadata(IntervalType.Int));

        public object Range
        {
            get => RangeType switch
            {
                IntervalType.Int => IntRange,
                IntervalType.Double => DoubleRange,
                IntervalType.Decimal => DecimalRange,
                _ => (object)null
            };
            set
            {
                switch (RangeType)
                {
                    case IntervalType.Int:
                        IntRange = (Range<int>)value ?? Range<int>.EmptySet;
                        break;
                    case IntervalType.Double:
                        DoubleRange = (Range<double>)value ?? Range<double>.EmptySet;
                        break;
                    case IntervalType.Decimal:
                        DecimalRange = (Range<decimal>)value ?? Range<decimal>.EmptySet;
                        break;
                }
            }
        }

        #endregion

        #region Constructeurs

        public RangeEditor() : this(IntervalType.Int, null, true) { }
        public RangeEditor(IntervalType rangeType, object range, bool showOver)
        {
            InitializeComponent();

            startSB.AllowedStrings = new[] { "-∞", "NaN" };
            endSB.AllowedStrings = new[] { "+∞", "NaN" };

            RangeType = rangeType;
            switch (rangeType)
            {
                case IntervalType.Int:
                    if (range is Range<int> intRange) IntRange = intRange;
                    break;
                case IntervalType.Double:
                    if (range is Range<double> doubleRange) DoubleRange = doubleRange;
                    break;
                case IntervalType.Decimal:
                    if (range is Range<decimal> decimalRange) DecimalRange = decimalRange;
                    break;
            }

            RangeChanged += (sender, e) => SetUI();
            SetBrackets();
            SetText();
            if (showOver) ShowOver();
        }

        #endregion

        #region Méthodes

        public void SetUI()
        {
            SetBrackets();
            SetText();
            ShowOver();
        }
        public void SetBrackets()
        {
            switch (RangeType)
            {
                case IntervalType.Int:
                    startBracket.Text = IntRange.LeftBracket.ToString();
                    endBracket.Text = IntRange.RightBracket.ToString();
                    break;
                case IntervalType.Double:
                    startBracket.Text = DoubleRange.LeftBracket.ToString();
                    endBracket.Text = DoubleRange.RightBracket.ToString();
                    break;
                case IntervalType.Decimal:
                    startBracket.Text = DecimalRange.LeftBracket.ToString();
                    endBracket.Text = DecimalRange.RightBracket.ToString();
                    break;
            }
        }
        public void SetText()
        {
            switch (RangeType)
            {
                case IntervalType.Int:
                    startSB.Text = IntRange.Start.ToString(false);
                    endSB.Text = IntRange.End.ToString(false);
                    break;
                case IntervalType.Double:
                    startSB.Text = DoubleRange.Start.ToString(false);
                    endSB.Text = DoubleRange.End.ToString(false);
                    break;
                case IntervalType.Decimal:
                    startSB.Text = DecimalRange.Start.ToString(false);
                    endSB.Text = DecimalRange.End.ToString(false);
                    break;
            }
        }
        public void SetRange()
        {
            switch (RangeType)
            {
                case IntervalType.Int:
                    if (startSB.Text == "NaN" || endSB.Text == "NaN") IntRange = Interval<int>.EmptySet;
                    int? si = null, ei = null;
                    if ((startSB.Text == "-∞" || startSB.Text.IsInt(out si)) && (endSB.Text == "+∞" || endSB.Text.IsInt(out ei))) IntRange = (startBracket.Text, endBracket.Text) switch
                    {
                        ("⟦", "⟧") => Range<int>.CC(si, ei),
                        ("⟦", "⟦") => Range<int>.CO(si, ei),
                        ("⟧", "⟧") => Range<int>.OC(si, ei),
                        ("⟧", "⟦") => Range<int>.OO(si, ei),
                        _ => (si, ei)
                    };
                    break;
                case IntervalType.Double:
                    if (startSB.Text == "NaN" || endSB.Text == "NaN") DoubleRange = Interval<double>.EmptySet;
                    double? sd = null, ed = null;
                    if ((startSB.Text == "-∞" || startSB.Text.IsDouble(out sd)) && (endSB.Text == "+∞" || endSB.Text.IsDouble(out ed))) DoubleRange = (startBracket.Text, endBracket.Text) switch
                    {
                        ("[", "]") => Interval<double>.CC(sd, ed),
                        ("[", "[") => Interval<double>.CO(sd, ed),
                        ("]", "]") => Interval<double>.OC(sd, ed),
                        ("]", "[") => Interval<double>.OO(sd, ed),
                        _ => (sd, ed)
                    };
                    break;
                case IntervalType.Decimal:
                    if (startSB.Text == "NaN" || endSB.Text == "NaN") DecimalRange = Interval<decimal>.EmptySet;
                    decimal? sm = null, em = null;
                    if ((startSB.Text == "-∞" || startSB.Text.IsDecimal(out sm)) && (endSB.Text == "+∞" || endSB.Text.IsDecimal(out em))) DecimalRange = (startBracket.Text, endBracket.Text) switch
                    {
                        ("[", "]") => Interval<decimal>.CC(sm, em),
                        ("[", "[") => Interval<decimal>.CO(sm, em),
                        ("]", "]") => Interval<decimal>.OC(sm, em),
                        ("]", "[") => Interval<decimal>.OO(sm, em),
                        _ => (sm, em)
                    };
                    break;
            }
        }

        private void SetNegativeInfinity()
        {
            startSB.Text = "-∞";
            SetStartOpen();
        }
        private void SetPositiveInfinity()
        {
            endSB.Text = "+∞";
            SetEndOpen();
        }
        private void SetStartOpen() => startBracket.Text = RangeType == IntervalType.Int ? "⟧" : "]";
        private void SetStartClosed() => startBracket.Text = RangeType == IntervalType.Int ? "⟦" : "[";
        private void SetEndOpen() => endBracket.Text = RangeType == IntervalType.Int ? "⟦" : "[";
        private void SetEndClosed() => endBracket.Text = RangeType == IntervalType.Int ? "⟧" : "]";

        public bool DesactivateLeft(IInputElement newFocus) => startSB.Desactivate(newFocus);
        public bool DesactivateRight(IInputElement newFocus) => endSB.Desactivate(newFocus);

        public async Task<bool> ActivateLeft()
        {
            HideOver();
            if (DesactivateRight(this)) { while (!startSB.Activate()) await Task.Delay(1); }
            return startSB.IsActivated;
        }
        public async Task<bool> ActivateRight()
        {
            HideOver();
            if (DesactivateLeft(this)) { while (!endSB.Activate()) await Task.Delay(1); }
            return endSB.IsActivated;
        }

        public void ShowOver()
        {
            string text = RangeType switch
            {
                IntervalType.Int => IntRange.ToString(),
                IntervalType.Double => DoubleRange.ToString(),
                IntervalType.Decimal => DecimalRange.ToString(),
                _ => "∅"
            };
            if (text.ContainsAny('[', ']', '⟦', '⟧')) HideOver();
            else
            {
                overSB.Text = text;
                ends.Visibility = Visibility.Collapsed;
                overSB.Visibility = Visibility.Visible;
            }
        }
        public void HideOver()
        {
            overSB.Visibility = Visibility.Collapsed;
            ends.Visibility = Visibility.Visible;
        }

        #endregion

        #region Events

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == IntRangeProperty || e.Property == DoubleRangeProperty || e.Property == DecimalRangeProperty) RangeChanged?.Invoke(this, EventArgs.Empty);
            else if (e.Property == RangeTypeProperty)
            {
                var oldValue = (IntervalType)e.OldValue;
                var newValue = (IntervalType)e.NewValue;

                switch (newValue)
                {
                    case IntervalType.Int:
                        {
                            startSB.ContentType = ContentType.Integer;
                            startSB.TextValidation = s => s.IsInt().ShowException();
                            endSB.ContentType = ContentType.Integer;
                            endSB.TextValidation = s => s.IsInt().ShowException();
                            switch (oldValue)
                            {
                                case IntervalType.Double:
                                    IntRange = DoubleRange.Convert(d => d.TrimToInt());
                                    break;
                                case IntervalType.Decimal:
                                    IntRange = DecimalRange.Convert(m => m.TrimToInt());
                                    break;
                            }
                        }
                        break;
                    case IntervalType.Double:
                        {
                            startSB.ContentType = ContentType.Decimal;
                            startSB.TextValidation = s => s.IsDouble().ShowException();
                            endSB.ContentType = ContentType.Decimal;
                            endSB.TextValidation = s => s.IsDouble().ShowException();
                            switch (oldValue)
                            {
                                case IntervalType.Int:
                                    DoubleRange = IntRange.Convert<double>();
                                    break;
                                case IntervalType.Decimal:
                                    DoubleRange = DecimalRange.Convert<double>();
                                    break;
                            }
                        }
                        break;
                    case IntervalType.Decimal:
                        {
                            startSB.ContentType = ContentType.Decimal;
                            startSB.TextValidation = s => s.IsDecimal().ShowException();
                            endSB.ContentType = ContentType.Decimal;
                            endSB.TextValidation = s => s.IsDecimal().ShowException();
                            switch (oldValue)
                            {
                                case IntervalType.Int:
                                    DecimalRange = IntRange.Convert<decimal>();
                                    break;
                                case IntervalType.Double:
                                    DecimalRange = DoubleRange.Convert(d => d.TrimToDecimal());
                                    break;
                            }
                        }
                        break;
                }
            }
            base.OnPropertyChanged(e);
        }

        private void StartSB_Desactivated(object sender, EventArgs<IInputElement> e)
        {
            if (EnableUICorrection && !(e.Param1 is RangeEditor || e.Param1 is DependencyObject dependencyObject && dependencyObject.FindParent<RangeEditor>() != null))
            {
                SetRange();
                TextBoxDesactivated?.Invoke(this, EventArgs.Empty);
            }
        }
        private void EndSB_Desactivated(object sender, EventArgs<IInputElement> e)
        {
            if (EnableUICorrection && !(e.Param1 is RangeEditor || e.Param1 is DependencyObject dependencyObject && dependencyObject.FindParent<RangeEditor>() != null))
            {
                SetRange();
                TextBoxDesactivated?.Invoke(this, EventArgs.Empty);
            }
        }

        private async void StartSB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.O:
                    e.Handled = true;
                    SetStartOpen();
                    break;
                case Key.C:
                    e.Handled = true;
                    SetStartClosed();
                    break;
                case Key.R:
                    e.Handled = true;
                    SetNegativeInfinity();
                    SetPositiveInfinity();
                    break;
                case Key.Tab:
                    e.Handled = true;
                    if (!Input.IsShiftPressed()) await ActivateRight();
                    else LeftTabOverflow?.Invoke(this, EventArgs.Empty);
                    break;
                case Key.Oem102:
                    e.Handled = true;
                    if (Input.IsShiftPressed()) SetPositiveInfinity();
                    else SetNegativeInfinity();
                    break;
                case Key.Delete:
                    if (Input.IsControlPressed())
                    {
                        e.Handled = true;
                        RemoveRequested?.Invoke(this, EventArgs.Empty);
                    }
                    break;
            }
        }
        private async void EndSB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.O:
                    e.Handled = true;
                    SetEndOpen();
                    break;
                case Key.C:
                    e.Handled = true;
                    SetEndClosed();
                    break;
                case Key.R:
                    e.Handled = true;
                    SetNegativeInfinity();
                    SetPositiveInfinity();
                    break;
                case Key.Tab:
                    e.Handled = true;
                    if (Input.IsShiftPressed()) await ActivateLeft();
                    else RightTabOverflow?.Invoke(this, EventArgs.Empty);
                    break;
                case Key.Oem102:
                    e.Handled = true;
                    if (Input.IsShiftPressed()) SetPositiveInfinity();
                    else SetNegativeInfinity();
                    break;
                case Key.Delete:
                    if (Input.IsControlPressed())
                    {
                        e.Handled = true;
                        RemoveRequested?.Invoke(this, EventArgs.Empty);
                    }
                    break;
            }
        }

        private async void OverSB_Activated(object sender, EventArgs e) => await ActivateLeft();

        #endregion
    }
}
