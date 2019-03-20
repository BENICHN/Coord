using BenLib;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using static System.Math;

namespace Coord
{
    public class DoublePendulum : VisualObject
    {
        public override string Type => "DoublePendulum";

        public (double angle1, double angle2, double length1, double length2, double mass1, double mass2) Configuration { get; }
        private (double theta1, double z1, double theta2, double z2) m_state;
        private const double g = 9.81;

        public DoublePendulum(double angle1, double angle2, double length1, double length2, double mass1, double mass2)
        {
            Configuration = (angle1, angle2, length1, length2, mass1, mass2);
            m_state = (angle1 + Num.Random(1E-5), 0 + Num.Random(1E-5), angle2 + Num.Random(1E-5), 0 + Num.Random(1E-5));
        }

        public override IReadOnlyCollection<Character> GetCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            return GetCharactersCore().ToArray();
            IEnumerable<Character> GetCharactersCore()
            {
                var (angle1, angle2, length1, length2, mass1, mass2) = Configuration;
                var (theta1, z1, theta2, z2) = m_state;
                var point = coordinatesSystemManager.ComputeOutCoordinates(new Point(length1 * Sin(theta1), -length1 * Cos(theta1)));
                var vector = coordinatesSystemManager.ComputeOutCoordinates(new Vector(length2 * Sin(theta2), -length2 * Cos(theta2)));

                yield return Character.Line(coordinatesSystemManager.Origin, point, Fill, Stroke);
                yield return Character.Line(point, point + vector, Fill, Stroke);
                yield return Character.Ellipse(point, 5, 5, Fill, null);
                yield return Character.Ellipse(point + vector, 5, 5, Fill, null);
            }
        }

        public void Update(double dt)
        {
            var (angle1, angle2, length1, length2, mass1, mass2) = Configuration;
            var (theta1, z1, theta2, z2) = m_state;
            double c = Cos(theta1 - theta2);
            double s = Sin(theta1 - theta2);
            m_state = (
                theta1 + dt * z1,
                0.9999 * (z1 + dt * ((mass2 * g * Sin(theta2) * c - mass2 * s * (length1 * z1.Pow(2) * c + length2 * z2.Pow(2)) - (mass1 + mass2) * g * Sin(theta1)) / length1 / (mass1 + mass2 * s.Pow(2)))),
                theta2 + dt * z2,
                0.9999 * (z2 + dt * (((mass1 + mass2) * (length1 * z1.Pow(2) * s - g * Sin(theta2) + g * Sin(theta1) * c) + mass2 * length2 * z2.Pow(2) * s * c) / length2 / (mass1 + mass2 * s.Pow(2)))));
            NotifyChanged();
        }
    }
}
