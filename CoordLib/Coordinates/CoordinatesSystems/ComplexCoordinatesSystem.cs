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
        protected override Freezable CreateInstanceCore() => new ComplexCoordinatesSystem();

        /// <summary>
        /// Opérations à effectuer
        /// </summary>
        public Func<Complex, Complex> Operations { get => (Func<Complex, Complex>)GetValue(OperationsProperty); set => SetValue(OperationsProperty, value); }
        public static readonly DependencyProperty OperationsProperty = CreateProperty<ComplexCoordinatesSystem, Func<Complex, Complex>>(true, true, true, "Operations");

        /// <summary>
        /// Opéraitons permettant de retrouver l'antécédent d'un nombre complexe par la fonction <see cref="Operations"/>
        /// </summary>
        public Func<Complex, Complex> InvertOperations { get => (Func<Complex, Complex>)GetValue(InvertOperationsProperty); set => SetValue(InvertOperationsProperty, value); }
        public static readonly DependencyProperty InvertOperationsProperty = CreateProperty<ComplexCoordinatesSystem, Func<Complex, Complex>>(true, true, true, "InvertOperations");

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
