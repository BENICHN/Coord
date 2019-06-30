using System.Windows;
using System.Windows.Media;

namespace Coord
{
    public static class PlanePen
    {
        public static bool GetIn(Pen obj) => (bool)obj.GetValue(InProperty);
        public static void SetIn(Pen obj, bool value) => obj.SetValue(InProperty, value);
        public static readonly DependencyProperty InProperty = DependencyProperty.RegisterAttached("In", typeof(bool), typeof(PlanePen), new PropertyMetadata(false));

        public static Pen AsIn(this Pen pen)
        {
            SetIn(pen, true);
            return pen;
        }

        public static Pen GetOutPen(this Pen pen, ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            if (pen?.CloneCurrentValue() is Pen p)
            {
                if (GetIn(p)) p.Thickness *= coordinatesSystemManager.WidthRatio;
                return p;
            }
            else return null;
        }
    }
}
