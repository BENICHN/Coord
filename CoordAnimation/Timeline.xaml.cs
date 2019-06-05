using Coord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CoordAnimation
{
    /// <summary>
    /// Logique d'interaction pour Timeline.xaml
    /// </summary>
    public partial class Timeline : UserControl
    {
        public Timeline() => InitializeComponent();

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            plane.Grid.Primary = plane.Grid.Secondary = false;
            // /*plane.Axes.Direction = */plane.AxesNumbers.Direction = AxesDirection.None;
            double h = 5;//300 * plane.CoordinatesSystemManager.OutputRange.Height / plane.CoordinatesSystemManager.OutputRange.Width;
            plane.InputRange = new MathRect(0, -h, 300, h);
            plane.VisualObjects.Add(new TimelineBackground());
        }
    }
}
