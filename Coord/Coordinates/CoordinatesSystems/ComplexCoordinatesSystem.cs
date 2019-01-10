using System;
using System.Numerics;
using System.Windows;

namespace Coord
{
    /// <summary>
    /// Système de coordonnées effectuant des opérations sur les points comme s'il s'agissait de nombres complexes
    /// </summary>
    public class ComplexCoordinatesSystem : CoordinatesSystem
    {
        public ComplexCoordinatesSystem(Func<Complex, Complex> operations, Func<Complex, Complex> invertOperations)
        {
            Operations = operations;
            InvertOperations = invertOperations;
        }

        /// <summary>
        /// Opérations à effectuer
        /// </summary>
        public Func<Complex, Complex> Operations { get; set; }

        /// <summary>
        /// Opéraitons permettant de retrouver l'antécédent d'un nombre complexe par la fonction <see cref="Operations"/>
        /// </summary>
        public Func<Complex, Complex> InvertOperations { get; set; }

        protected override Point ComputeInCoordinatesCore(Point point)
        {
            var complex = new Complex(point.X, point.Y);
            var newComplex = InvertOperations(complex);

            return new Point(newComplex.Real, newComplex.Imaginary);
        }

        protected override Point ComputeOutCoordinatesCore(Point point)
        {
            var complex = new Complex(point.X, point.Y);
            var newComplex = Operations(complex);

            return new Point(newComplex.Real, newComplex.Imaginary);
        }

        protected override Vector ComputeInCoordinatesCore(Vector vector) => (Vector)ComputeInCoordinatesCore((Point)vector);
        protected override Vector ComputeOutCoordinatesCore(Vector vector) => (Vector)ComputeOutCoordinatesCore((Point)vector);
    }
}
