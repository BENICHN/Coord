using BenLib.Standard;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

        public Interval<int> IntInterval { get => (Interval<int>)GetValue(IntIntervalProperty); set => SetValue(IntIntervalProperty, value); }
        public static readonly DependencyProperty IntIntervalProperty = DependencyProperty.Register("IntInterval", typeof(Interval<int>), typeof(IntervalEditor), new PropertyMetadata(Interval<int>.EmptySet, (sender, e) => (sender as IntervalEditor).IntervalChanged?.Invoke(sender, EventArgs.Empty)));

        public Interval<double> DoubleInterval { get => (Interval<double>)GetValue(DoubleIntervalProperty); set => SetValue(DoubleIntervalProperty, value); }
        public static readonly DependencyProperty DoubleIntervalProperty = DependencyProperty.Register("DoubleInterval", typeof(Interval<double>), typeof(IntervalEditor), new PropertyMetadata(Interval<double>.EmptySet, (sender, e) => (sender as IntervalEditor).IntervalChanged?.Invoke(sender, EventArgs.Empty)));

        public Interval<decimal> DecimalInterval { get => (Interval<decimal>)GetValue(DecimalIntervalProperty); set => SetValue(DecimalIntervalProperty, value); }
        public static readonly DependencyProperty DecimalIntervalProperty = DependencyProperty.Register("DecimalInterval", typeof(Interval<decimal>), typeof(IntervalEditor), new PropertyMetadata(Interval<decimal>.EmptySet, (sender, e) => (sender as IntervalEditor).IntervalChanged?.Invoke(sender, EventArgs.Empty)));

        public object Interval
        {
            get => IntervalType switch
            {
                IntervalType.Int => IntInterval,
                IntervalType.Double => DoubleInterval,
                IntervalType.Decimal => DecimalInterval,
                _ => (object)null
            };
            set
            {
                switch (IntervalType)
                {
                    case IntervalType.Int:
                        IntInterval = (Interval<int>)value ?? Interval<int>.EmptySet;
                        break;
                    case IntervalType.Double:
                        DoubleInterval = (Interval<double>)value ?? Interval<double>.EmptySet;
                        break;
                    case IntervalType.Decimal:
                        DecimalInterval = (Interval<decimal>)value ?? Interval<decimal>.EmptySet;
                        break;
                }
            }
        }

        public IntervalType IntervalType { get => (IntervalType)GetValue(IntervalTypeProperty); set => SetValue(IntervalTypeProperty, value); }
        public static readonly DependencyProperty IntervalTypeProperty = DependencyProperty.Register("IntervalType", typeof(IntervalType), typeof(IntervalEditor), new PropertyMetadata(IntervalType.Int));

        private readonly ObservableCollection<RangeEditorConteiner> m_rangeEditors;

        #endregion

        #region Constructeurs

        public IntervalEditor() : this(IntervalType.Int, null) { }
        public IntervalEditor(IntervalType intervalType, object interval)
        {
            InitializeComponent();

            m_rangeEditors = new ObservableCollection<RangeEditorConteiner>();
            m_rangeEditors.CollectionChanged += (sender, e) =>
            {
                if (m_rangeEditors.Count > 0)
                {
                    for (int i = 0; i < m_rangeEditors.Count - 1; i++) m_rangeEditors[i].UnionVisibility = Visibility.Visible;
                    m_rangeEditors.Last().UnionVisibility = Visibility.Collapsed;
                }
            };
            its.ItemsSource = m_rangeEditors;

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

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == IntervalTypeProperty)
            {
                var newValue = (IntervalType)e.NewValue;
                foreach (var rangeEditor in m_rangeEditors) rangeEditor.RangeEditor.RangeType = newValue;
            }
            base.OnPropertyChanged(e);
        }

        #endregion

        #region Méthodes

        private void SetInterval()
        {
            foreach (var range in m_rangeEditors) range.RangeEditor.SetRange();
            switch (IntervalType)
            {
                case IntervalType.Int:
                    IntInterval = m_rangeEditors.Union(rE => rE.RangeEditor.IntRange);
                    break;
                case IntervalType.Double:
                    DoubleInterval = m_rangeEditors.Union(rE => rE.RangeEditor.DoubleRange);
                    break;
                case IntervalType.Decimal:
                    DecimalInterval = m_rangeEditors.Union(rE => rE.RangeEditor.DecimalRange);
                    break;
            }
        }

        public void SetUI()
        {
            object[] rs = IntervalType switch
            {
                IntervalType.Int => IntInterval?.Ranges.ToArray() ?? Array.Empty<object>(),
                IntervalType.Double => DoubleInterval?.Ranges.ToArray() ?? Array.Empty<object>(),
                IntervalType.Decimal => DecimalInterval?.Ranges.ToArray() ?? Array.Empty<object>(),
                _ => Array.Empty<object>()
            };

            while (rs.Length > m_rangeEditors.Count) InsertNewRange(m_rangeEditors.Count, true);
            while (rs.Length < m_rangeEditors.Count) m_rangeEditors.RemoveAt(m_rangeEditors.Count - 1);

            for (int i = 0; i < rs.Length; i++) m_rangeEditors[i].RangeEditor.Range = rs[i];
        }

        private RangeEditor InsertNewRange(int index, bool showOver, object range = null)
        {
            var rangeEditor = new RangeEditor(IntervalType, range, showOver);

            rangeEditor.RightTabOverflow += RangeEditor_RightTabOverflow;
            rangeEditor.LeftTabOverflow += RangeEditor_LeftTabOverflow;
            rangeEditor.RemoveRequested += RangeEditor_RemoveRequested;
            rangeEditor.TextBoxDesactivated += RangeEditor_TextBoxDesactivated;

            m_rangeEditors.Insert(index, new RangeEditorConteiner(rangeEditor));

            return rangeEditor;
        }

        private Task<RangeEditor> InsertNewRangeAsync(int index, bool showOver, object range = null)
        {
            var tcs = new TaskCompletionSource<RangeEditor>();

            var rangeEditor = new RangeEditor(IntervalType, range, showOver);

            rangeEditor.RightTabOverflow += RangeEditor_RightTabOverflow;
            rangeEditor.LeftTabOverflow += RangeEditor_LeftTabOverflow;
            rangeEditor.RemoveRequested += RangeEditor_RemoveRequested;
            rangeEditor.TextBoxDesactivated += RangeEditor_TextBoxDesactivated;

            m_rangeEditors.Insert(index, new RangeEditorConteiner(rangeEditor));

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
                int index = m_rangeEditors.IndexOf(rC => rC.RangeEditor == rangeEditor);
                if (index == 0)
                {
                    var range = await InsertNewRangeAsync(0, false);
                    await range.ActivateRight();
                }
                else if (m_rangeEditors[index - 1] is RangeEditorConteiner rE) await rE.RangeEditor.ActivateRight();
            }
        }
        private async void RangeEditor_RightTabOverflow(object sender, EventArgs e)
        {
            if (sender is RangeEditor rangeEditor && rangeEditor.DesactivateRight(rangeEditor))
            {
                int index = m_rangeEditors.IndexOf(rC => rC.RangeEditor == rangeEditor);
                if (index == m_rangeEditors.Count - 1)
                {
                    var range = await InsertNewRangeAsync(index, false);
                    await range.ActivateLeft();
                }
                else if (m_rangeEditors[index + 1] is RangeEditorConteiner rE) await rE.RangeEditor.ActivateLeft();
            }
        }

        private void RangeEditor_RemoveRequested(object sender, EventArgs e)
        {
            if (m_rangeEditors.Count == 1) m_rangeEditors[0].RangeEditor.Range = null;
            else if (sender is RangeEditor rangeEditor)
            {
                /*int index = */m_rangeEditors.RemoveAt(m_rangeEditors.IndexOf(rC => rC.RangeEditor == rangeEditor));
                /*if (index < ranges.Children.Count) await ((RangeEditor)ranges.Children[index]).ActivateLeft();
                else await ((RangeEditor)ranges.Children[index - 2]).ActivateRight();*/
            }
        }

        private void RangeEditor_TextBoxDesactivated(object sender, EventArgs e) => SetInterval();

        #endregion
    }

    public class RangeEditorConteiner : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public RangeEditorConteiner(RangeEditor rangeEditor) => RangeEditor = rangeEditor;
        public RangeEditor RangeEditor { get; }

        private Visibility m_unionVisibility = Visibility.Visible;
        public Visibility UnionVisibility
        {
            get => m_unionVisibility;
            set
            {
                m_unionVisibility = value;
                NotifyPropertyChanged("UnionVisibility");
            }
        }
    }

    public enum IntervalType { Int, Double, Decimal }
}
