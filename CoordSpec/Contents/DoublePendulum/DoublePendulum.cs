using BenLib.Standard;
using Coord;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using static Coord.VisualObjects;
using static System.Math;

namespace CoordSpec
{
    public class DoublePendulum : VisualObjectGroupBase
    {
        public override string Type => "DoublePendulum";

        public PointVisualObject Point1 { get; }
        public PointVisualObject Point2 { get; }
        public SegmentVisualObject Segment1 { get; }
        public SegmentVisualObject Segment2 { get; }

        public double Angle1 { get => (double)GetValue(Angle1Property); set => SetValue(Angle1Property, value); }
        public static readonly DependencyProperty Angle1Property = CreateProperty<double>(false, false, "Angle1", typeof(DoublePendulum));

        public double Angle2 { get => (double)GetValue(Angle2Property); set => SetValue(Angle2Property, value); }
        public static readonly DependencyProperty Angle2Property = CreateProperty<double>(false, false, "Angle2", typeof(DoublePendulum));

        public double Length1 { get => (double)GetValue(Length1Property); set => SetValue(Length1Property, value); }
        public static readonly DependencyProperty Length1Property = CreateProperty<double>(false, false, "Length1", typeof(DoublePendulum));

        public double Length2 { get => (double)GetValue(Length2Property); set => SetValue(Length2Property, value); }
        public static readonly DependencyProperty Length2Property = CreateProperty<double>(false, false, "Length2", typeof(DoublePendulum));

        public double Mass1 { get => (double)GetValue(Mass1Property); set => SetValue(Mass1Property, value); }
        public static readonly DependencyProperty Mass1Property = CreateProperty<double>(false, false, "Mass1", typeof(DoublePendulum));

        public double Mass2 { get => (double)GetValue(Mass2Property); set => SetValue(Mass2Property, value); }
        public static readonly DependencyProperty Mass2Property = CreateProperty<double>(false, false, "Mass2", typeof(DoublePendulum));

        public (double theta1, double z1, double theta2, double z2) State { get => ((double theta1, double z1, double theta2, double z2))GetValue(StateProperty); set => SetValue(StateProperty, value); }
        public static readonly DependencyProperty StateProperty = CreateProperty<(double theta1, double z1, double theta2, double z2)>(false, false, "State", typeof(DoublePendulum));

        private const double g = 9.81;

        public DoublePendulum()
        {
            Point1 = Point(0, 0).Style(Fill);
            Point2 = Point(0, 0).Style(Fill);
            Segment1 = Segment(Point(0, 0), Point1);
            Segment2 = Segment(Point1, Point2);
            Children = new VisualObjectCollection(Segment1, Segment2, Point1, Point2);
        }

        public void Update(double dt)
        {
            var (length1, length2, mass1, mass2) = (Length1, Length2, Mass1, Mass2);
            var (theta1, z1, theta2, z2) = State;
            double c = Cos(theta1 - theta2);
            double s = Sin(theta1 - theta2);


            State = (
                theta1 + dt * z1,
                0.9999 * (z1 + dt * ((mass2 * g * Sin(theta2) * c - mass2 * s * (length1 * z1.Pow(2) * c + length2 * z2.Pow(2)) - (mass1 + mass2) * g * Sin(theta1)) / length1 / (mass1 + mass2 * s.Pow(2)))),
                theta2 + dt * z2,
                0.9999 * (z2 + dt * (((mass1 + mass2) * (length1 * z1.Pow(2) * s - g * Sin(theta2) + g * Sin(theta1) * c) + mass2 * length2 * z2.Pow(2) * s * c) / length2 / (mass1 + mass2 * s.Pow(2)))));
            NotifyChanged();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Fill":
                    Point1.Fill = Point2.Fill = Fill;
                    break;
                case "Stroke":
                    Segment1.Stroke = Segment2.Stroke = Stroke;
                    break;
                case "State":
                    var (length1, length2) = (Length1, Length2);
                    var (theta1, _, theta2, _) = State;
                    var point = new Point(length1 * Sin(theta1), -length1 * Cos(theta1));
                    var vector = new Vector(length2 * Sin(theta2), -length2 * Cos(theta2));
                    Point1?.SetInPoint(point);
                    Point2?.SetInPoint(point + vector);
                    break;
                case "Angle1":
                case "Angle2":
                    State = (Angle1 + Num.Random(1E-5), 0 + Num.Random(1E-5), Angle2 + Num.Random(1E-5), 0 + Num.Random(1E-5));
                    break;
            }
            base.OnPropertyChanged(e);
        }
    }
}
