using BenLib.Standard;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CoordAnimation
{
    /// <summary>
    /// Logique d'interaction pour IntervalEditor.xaml
    /// </summary>
    public partial class IntervalEditor : UserControl
    {
        #region Champs et propriétés

        public bool EnableUICorrection
        {
            get => (bool)GetValue(EnableUICorrectionProperty);
            set => SetValue(EnableUICorrectionProperty, value);
        }
        public static readonly DependencyProperty EnableUICorrectionProperty = DependencyProperty.Register("EnableUICorrection", typeof(bool), typeof(IntervalEditor), new PropertyMetadata(true));

        public event EventHandler IntervalChanged;

        public Interval<int> IntInterval
        {
            get => (Interval<int>)GetValue(IntIntervalProperty);
            set
            {
                SetValue(IntIntervalProperty, value);
                SetUI();
            }
        }
        public static readonly DependencyProperty IntIntervalProperty = DependencyProperty.Register("IntInterval", typeof(Interval<int>), typeof(IntervalEditor), new PropertyMetadata(Interval<int>.EmptySet, (sender, e) => (sender as IntervalEditor).IntervalChanged?.Invoke(sender, EventArgs.Empty)));

        public Interval<double> DoubleInterval
        {
            get => (Interval<double>)GetValue(DoubleIntervalProperty);
            set
            {
                SetValue(DoubleIntervalProperty, value);
                SetUI();
            }
        }
        public static readonly DependencyProperty DoubleIntervalProperty = DependencyProperty.Register("DoubleInterval", typeof(Interval<double>), typeof(IntervalEditor), new PropertyMetadata(Interval<double>.EmptySet, (sender, e) => (sender as IntervalEditor).IntervalChanged?.Invoke(sender, EventArgs.Empty)));

        public Interval<decimal> DecimalInterval
        {
            get => (Interval<decimal>)GetValue(DecimalIntervalProperty);
            set
            {
                SetValue(DecimalIntervalProperty, value);
                SetUI();
            }
        }
        public static readonly DependencyProperty DecimalIntervalProperty = DependencyProperty.Register("DecimalInterval", typeof(Interval<decimal>), typeof(IntervalEditor), new PropertyMetadata(Interval<decimal>.EmptySet, (sender, e) => (sender as IntervalEditor).IntervalChanged?.Invoke(sender, EventArgs.Empty)));

        private IntervalType m_rangeType;
        public IntervalType IntervalType
        {
            get => m_rangeType;
            set
            {
                m_rangeType = value;
                foreach (var rangeEditor in ranges.Children.OfType<RangeEditor>()) rangeEditor.RangeType = value;
            }
        }

        #endregion

        #region Constructeurs

        public IntervalEditor() : this(IntervalType.Int, null) { }
        public IntervalEditor(IntervalType intervalType, object interval)
        {
            InitializeComponent();

            IntervalType = intervalType;
            switch (intervalType)
            {
                case IntervalType.Int:
                    if (interval is Interval<int> intInterval) SetValue(IntIntervalProperty, intInterval);
                    break;
                case IntervalType.Double:
                    if (interval is Interval<double> doubleIinterval) SetValue(DoubleIntervalProperty, interval);
                    break;
                case IntervalType.Decimal:
                    if (interval is Interval<decimal> decimalInterval) SetValue(DecimalIntervalProperty, decimalInterval);
                    break;
            }

            IntervalChanged += (sender, e) => SetUI();
            SetUI();
        }

        #endregion

        #region Méthodes

        private void SetInterval()
        {
            foreach (var range in ranges.Children.OfType<RangeEditor>()) range.SetInterval();
            switch (IntervalType)
            {
                case IntervalType.Int:
                    IntInterval = ranges.Children.OfType<RangeEditor>().Union(rE => rE.IntRange);
                    break;
                case IntervalType.Double:
                    DoubleInterval = ranges.Children.OfType<RangeEditor>().Union(rE => rE.DoubleRange);
                    break;
                case IntervalType.Decimal:
                    DecimalInterval = ranges.Children.OfType<RangeEditor>().Union(rE => rE.DecimalRange);
                    break;
            }
        }

        public void SetUI()
        {
            ranges.Children.Clear();
            switch (IntervalType)
            {
                case IntervalType.Int:
                    if (!IntInterval.IsNullOrEmpty()) foreach (var intRange in IntInterval.Ranges) InsertNewRange(ranges.Children.Count, false, true, intRange);
                    break;
                case IntervalType.Double:
                    if (!DoubleInterval.IsNullOrEmpty()) foreach (var doubleRange in DoubleInterval.Ranges) InsertNewRange(ranges.Children.Count, false, true, doubleRange);
                    break;
                case IntervalType.Decimal:
                    if (!DecimalInterval.IsNullOrEmpty()) foreach (var decimalRange in DecimalInterval.Ranges) InsertNewRange(ranges.Children.Count, false, true, decimalRange);
                    break;
            }

            //if (ranges.Children.Count == 0) InsertNewRange(0, true, true);
            //ranges.Children.RemoveAt(ranges.Children.Count - 1);
        }

        private Task<RangeEditor> InsertNewRange(int index, bool left, bool showOver, object range = null)
        {
            var tcs = new TaskCompletionSource<RangeEditor>();

            var textBlock = new TextBlock { Text = " ∪ ", VerticalAlignment = VerticalAlignment.Center };
            var rangeEditor = new RangeEditor(IntervalType, range, showOver);

            rangeEditor.RightTabOverflow += RangeEditor_RightTabOverflow;
            rangeEditor.LeftTabOverflow += RangeEditor_LeftTabOverflow;
            rangeEditor.RemoveRequested += RangeEditor_RemoveRequested;
            rangeEditor.TextBoxDesactivated += RangeEditor_TextBoxDesactivated;

            if (ranges.Children.Count == 0) ranges.Children.Add(rangeEditor);
            else
            {
                if (left)
                {
                    ranges.Children.Insert(index, textBlock);
                    ranges.Children.Insert(index, rangeEditor);
                }
                else
                {
                    ranges.Children.Insert(index, rangeEditor);
                    ranges.Children.Insert(index, textBlock);
                }
            }

            if (rangeEditor.IsLoaded) tcs.TrySetResult(rangeEditor);
            else rangeEditor.Loaded += (sender, e) => tcs.TrySetResult(rangeEditor);

            return tcs.Task;
        }

        #endregion

        #region Events

        private async void RangeEditor_LeftTabOverflow(object sender, EventArgs e)
        {
            if (sender is RangeEditor rangeEditor && rangeEditor.DesactivateLeft(rangeEditor))
            {
                int index = ranges.Children.IndexOf(rangeEditor);
                if (index == 0)
                {
                    var range = await InsertNewRange(0, true, false);
                    await range.ActivateRight();
                }
                else if (ranges.Children[index - 2] is RangeEditor rE) await rE.ActivateRight();
            }
        }
        private async void RangeEditor_RightTabOverflow(object sender, EventArgs e)
        {
            if (sender is RangeEditor rangeEditor && rangeEditor.DesactivateRight(rangeEditor))
            {
                int index = ranges.Children.IndexOf(rangeEditor) + 1;
                if (index == ranges.Children.Count)
                {
                    var range = await InsertNewRange(index, false, false);
                    await range.ActivateLeft();
                }
                else if (ranges.Children[index + 1] is RangeEditor rE) await rE.ActivateLeft();
            }
        }

        private async void RangeEditor_RemoveRequested(object sender, EventArgs e)
        {
            if (ranges.Children.Count == 1)
            {
                ranges.Children.Clear();
                await InsertNewRange(0, false, true);
            }
            else if (sender is RangeEditor rangeEditor)
            {
                int index = ranges.Children.IndexOf(rangeEditor);
                if (index == 0) ranges.Children.RemoveRange(0, 2);
                else ranges.Children.RemoveRange(index - 1, 2);
                if (index < ranges.Children.Count) await ((RangeEditor)ranges.Children[index]).ActivateLeft();
                else await ((RangeEditor)ranges.Children[index - 2]).ActivateRight();
            }
        }

        private void RangeEditor_TextBoxDesactivated(object sender, EventArgs e) => SetInterval();

        #endregion
    }

    public enum IntervalType { Int, Double, Decimal }
}
