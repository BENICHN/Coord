using BenLib.Standard;
using Coord;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using static Coord.VisualObjects;

namespace CoordAnimation
{
    /// <summary>
    /// Logique d'interaction pour VisualObjectSelector.xaml
    /// </summary>
    public partial class VisualObjectSelector : UserControl
    {
        public SelectedVisualObjectsCollection VisualObjects { get => (SelectedVisualObjectsCollection)GetValue(VisualObjectsProperty); set => SetValue(VisualObjectsProperty, value); }
        public static readonly DependencyProperty VisualObjectsProperty = DependencyProperty.Register("VisualObjects", typeof(SelectedVisualObjectsCollection), typeof(VisualObjectSelector));

        public IEnumerable<VisualObject> Results => VisualObjects.Select(vo => vo.Selection >= Interval<int>.PositiveReals ? vo : InCharacters(vo, vo.Selection));
        public VisualObject Result
        {
            get
            {
                var results = Results.ToArray();
                return results.Length == 1 ? results[0] : Group(results);
            }
        }

        public VisualObjectSelector()
        {
            DataContext = this;
            InitializeComponent();
        }
    }
}
