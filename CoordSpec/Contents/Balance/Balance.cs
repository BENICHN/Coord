using BenLib.Standard;
using BenLib.WPF;
using Coord;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static Coord.MainWindow;
using static System.Math;

namespace CoordSpec
{
    public class Balance : PhysicalObject
    {
        public override string Type => "Balance";

        private Polygon m_body;

        private double m_radius;
        public double Radius
        {
            get => m_radius;
            set
            {
                m_radius = value;
                NotifyChanged("Radius");
            }
        }

        private double m_length;
        public double Length
        {
            get => m_length;
            set
            {
                m_length = value;
                var length = new Vector(Length, 0);
                m_body = new Polygon(Location - length, Location + length);
                NotifyChanged("Length");
            }
        }

        private (double Mass, double X)[] m_balls;
        public (double Mass, double X)[] Balls
        {
            get => m_balls;
            set
            {
                m_balls = value;
                NotifyChanged("Balls");
            }
        }

        protected override IReadOnlyCollection<Character> GetCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            var location = coordinatesSystemManager.ComputeOutCoordinates(Location);
            var length = new Vector(Length * coordinatesSystemManager.WidthRatio, 0);
            var body = m_body.ComputeOutCoordinates(coordinatesSystemManager);
            var dims = coordinatesSystemManager.ComputeOutCoordinates(m_body.VectorBetween(0, 1).ReLength(1));
            double radX = Radius * coordinatesSystemManager.WidthRatio;
            double radY = Radius * coordinatesSystemManager.HeightRatio;
            var radXv = new Vector(radX, 0);
            var radYv = new Vector(0, radY);

            return new[] 
            {
                Character.Arc(location - radXv, location + radXv, new Size(radX, radY), 0, false, SweepDirection.Counterclockwise).Color(Fill, Stroke),
                Character.Line(location, location + radYv).Color(Fill, Stroke)
            }.RotateAt(-Angle, location, 1)
            .Concat(Balls.Select(ball => 
            {
                double rad = ball.Mass * coordinatesSystemManager.WidthRatio;
                return Character.Ellipse(location + dims * ball.X - new Vector(0, rad), rad, rad).Color(Stroke.Brush);
            }))
            .Concat(Character.Line(body[0], body[1]).Color(Fill, Stroke)).ToArray();
        }

        public override void Update()
        {
            if (m_body[0].Y <= 0 || m_body[1].Y <= 0) Torque = -FPS * AngularSpeed;
            else
            {
                foreach (var (mass, x) in Balls)
                {
                    var force = new Vector(0, 9.81 * mass);
                    var dimensions = m_body.VectorBetween(1, 0).ReLength(x);
                    ApplyForce(force, dimensions);
                }
            }

            Force = new Vector(Radius * Torque, 0);

            base.Update();

            m_body.Translate(Speed / FPS);
            m_body.RotateAt(AngularSpeed / FPS, Location);

            NotifyChanged("Update");
        }
    }
}
