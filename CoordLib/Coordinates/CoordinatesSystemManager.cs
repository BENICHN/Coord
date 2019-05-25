using System;
using System.Collections.Generic;
using System.Windows;

namespace Coord
{
    /// <summary>
    /// Met en relation une zone d'un plan muni d'un repère orthonormé standard avec une zone de l'écran
    /// </summary>
    public class CoordinatesSystemManager : NotifyObject
    {
        /// <summary>
        /// Zone du plan
        /// </summary>
        public MathRect InputRange { get => (MathRect)GetValue(InputRangeProperty); set => SetValue(InputRangeProperty, value); }
        public static readonly DependencyProperty InputRangeProperty = CreateProperty<MathRect>(true, true, "InputRange", typeof(CoordinatesSystemManager), (sender, e) => { if (sender is CoordinatesSystemManager owner) owner.UpdateRatio(); });

        /// <summary>
        /// Zone de l'écran
        /// </summary>
        public Rect OutputRange { get => (Rect)GetValue(OutputRangeProperty); set => SetValue(OutputRangeProperty, value); }
        public static readonly DependencyProperty OutputRangeProperty = CreateProperty<Rect>(true, true, "OutputRange", typeof(CoordinatesSystemManager), (sender, e) => { if (sender is CoordinatesSystemManager owner) owner.UpdateRatio(); });

        /// <summary>
        /// Rapport (largeur de la zone de l'écran) / (largeur de la zone du plan)
        /// </summary>
        public double WidthRatio { get; private set; }

        /// <summary>
        /// Rapport (hauteur de la zone de l'écran) / (hauteur de la zone du plan)
        /// </summary>
        public double HeightRatio { get; private set; }

        /// <summary>
        /// Système de coordonnées du plan
        /// </summary>
        public CoordinatesSystem CoordinatesSystem { get => (CoordinatesSystem)GetValue(CoordinatesSystemProperty); set => SetValue(CoordinatesSystemProperty, value); }
        public static readonly DependencyProperty CoordinatesSystemProperty = CreateProperty<CoordinatesSystem>(true, true, "CoordinatesSystem", typeof(CoordinatesSystemManager));

        /// <summary>
        /// Calcule <see cref="WidthRatio"/> et <see cref="HeightRatio"/>
        /// </summary>
        private void UpdateRatio()
        {
            WidthRatio = OutputRange.Width / InputRange.Width;
            HeightRatio = OutputRange.Height / InputRange.Height;
        }

        public ReadOnlyCoordinatesSystemManager AsReadOnly() => new ReadOnlyCoordinatesSystemManager(InputRange, OutputRange, CoordinatesSystem);
    }

    public readonly struct ReadOnlyCoordinatesSystemManager : IEquatable<ReadOnlyCoordinatesSystemManager>
    {
        /// <summary>
        /// Largeur et hauteur à l'écran maximale d'une celllule de la grille du plan
        /// </summary>
        public const double MaxCellSize = 200.0;

        /// <summary>
        /// Largeur et hauteur à l'écran maximale d'une celllule de la grille du plan
        /// </summary>
        public const double MinCellSize = 100.0;

        /// <summary>
        /// Point de l'écran correspondant à l'origine du plan transformée par le système de coordonnées
        /// </summary>
        public Point Origin { get; }

        /// <summary>
        /// Point de l'écran correspondant à l'origine du plan non transformée par le système de coordonnées
        /// </summary>
        public Point OrthonormalOrigin { get; }

        /// <summary>
        /// Zone du plan
        /// </summary>
        public MathRect InputRange { get; }

        /// <summary>
        /// Zone de l'écran
        /// </summary>
        public Rect OutputRange { get; }

        /// <summary>
        /// Rapport (largeur de la zone de l'écran) / (largeur de la zone du plan)
        /// </summary>
        public double WidthRatio { get; }

        /// <summary>
        /// Rapport (hauteur de la zone de l'écran) / (hauteur de la zone du plan)
        /// </summary>
        public double HeightRatio { get; }

        /// <summary>
        /// Système de coordonnées du plan
        /// </summary>
        public CoordinatesSystem CoordinatesSystem { get; }

        /// <summary>
        /// Calcule les coordonnées à l'écran d'un point du plan
        /// </summary>
        /// <param name="point">Point du plan</param>
        /// <returns>Point de l'écran correspondant au point du plan</returns>
        public Point ComputeOutOrthonormalCoordinates(Point point) => new Point(ComputeOutOrthonormalXCoordinate(point.X), ComputeOutOrthonormalYCoordinate(point.Y));

        /// <summary>
        /// Calcule les coordonnées à l'écran d'un vecteur du plan
        /// </summary>
        /// <param name="vector">Vecteur du plan</param>
        /// <returns>Vecteur de l'écran correspondant au vecteur du plan</returns>
        public Vector ComputeOutOrthonormalCoordinates(Vector vector) => ComputeOutOrthonormalCoordinates((Point)vector) - OrthonormalOrigin;

        public Rect ComputeOutOrthonormalCoordinates(Rect rect) => new Rect(ComputeOutOrthonormalCoordinates(rect.TopLeft), ComputeOutOrthonormalCoordinates(rect.BottomRight));

        public Rect ComputeOutOrthonormalCoordinates(MathRect rect) => new Rect(ComputeOutOrthonormalCoordinates(rect.TopLeft), ComputeOutOrthonormalCoordinates(rect.BottomRight));

        /// <summary>
        /// Calcule l'abscisse à l'écran d'une abscisse dans le plan
        /// </summary>
        /// <param name="xCoordinate">Abscisse dans le plan</param>
        /// <returns>Abscisse à l'écran correspondant à l'abscisse dans le plan</returns>
        public double ComputeOutOrthonormalXCoordinate(double xCoordinate) => OutputRange.X + (xCoordinate - InputRange.X) * WidthRatio;

        /// <summary>
        /// Calcule l'ordonnée à l'écran d'une ordonnée dans le plan
        /// </summary>
        /// <param name="yCoordinate">Ordonnée dans le plan</param>
        /// <returns>Ordonnée à l'écran correspondant à l'ordonnée dans le plan</returns>
        public double ComputeOutOrthonormalYCoordinate(double yCoordinate) => OutputRange.Y + OutputRange.Height - (yCoordinate - InputRange.Y) * HeightRatio;

        /// <summary>
        /// Retrouve le point du plan correspondant à un point de l'écran non transformé par le système de coordonnées
        /// </summary>
        /// <param name="point">Point de l'écran</param>
        /// <returns>Point du plan correspondant au point de l'écran</returns>
        public Point ComputeInOrthonormalCoordinates(Point point) => new Point(ComputeInOrthonormalXCoordinate(point.X), ComputeInOrthonormalYCoordinate(point.Y));

        /// <summary>
        /// Retrouve le vecteur du plan correspondant à un vecteur de l'écran non transformé par le système de coordonnées
        /// </summary>
        /// <param name="vector">Vecteur de l'écran</param>
        /// <returns>Vecteur du plan correspondant au vecteur de l'écran</returns>
        public Vector ComputeInOrthonormalCoordinates(Vector vector) => (Vector)ComputeInOrthonormalCoordinates(OrthonormalOrigin + vector);

        public Rect ComputeInOrthonormalCoordinates(Rect rect) => new Rect(ComputeInOrthonormalCoordinates(rect.TopLeft), ComputeInOrthonormalCoordinates(rect.BottomRight));

        public Rect ComputeInOrthonormalCoordinates(MathRect rect) => new Rect(ComputeInOrthonormalCoordinates(rect.TopLeft), ComputeInOrthonormalCoordinates(rect.BottomRight));

        /// <summary>
        /// Retrouve l'abscisse dans le plan correspondant à une abscisse à l'écran
        /// </summary>
        /// <param name="xCoordinate">Abscisse à l'écran</param>
        /// <returns>Abscisse dans le plan correspondant à l'abscisse à l'écran</returns>
        public double ComputeInOrthonormalXCoordinate(double xCoordinate) => xCoordinate / WidthRatio + InputRange.X - OutputRange.X;

        /// <summary>
        /// Retrouve l'ordonnée dans le plan correspondant à une ordonnée à l'écran
        /// </summary>
        /// <param name="yCoordinate">Ordonnée à l'écran</param>
        /// <returns>Ordonnée dans le plan correspondant à l'ordonnée à l'écran</returns>
        public double ComputeInOrthonormalYCoordinate(double yCoordinate) => -(yCoordinate - OutputRange.Height) / HeightRatio + InputRange.Y - OutputRange.Y;

        /// <summary>
        /// Transforme un point du plan par le système de coordonnées puis calcule ses coordonnées à l'écran
        /// </summary>
        /// <param name="point">Point du plan</param>
        /// <returns>Point de l'écran correspondant au point du plan</returns>
        public Point ComputeOutCoordinates(Point point) => ComputeOutOrthonormalCoordinates(CoordinatesSystem?.ComputeOutCoordinates(point) ?? point);

        /// <summary>
        /// Transforme un vecteur du plan par le système de coordonnées puis calcule ses coordonnées à l'écran
        /// </summary>
        /// <param name="vector">Vecteur du plan</param>
        /// <returns>Vecteur de l'écran correspondant au vecteur du plan</returns>
        public Vector ComputeOutCoordinates(Vector vector) => ComputeOutOrthonormalCoordinates(CoordinatesSystem?.ComputeOutCoordinates(vector) ?? vector);

        public Rect ComputeOutCoordinates(Rect rect) => new Rect(ComputeOutCoordinates(rect.TopLeft), ComputeOutCoordinates(rect.BottomRight));

        public Rect ComputeOutCoordinates(MathRect rect) => new Rect(ComputeOutCoordinates(rect.TopLeft), ComputeOutCoordinates(rect.BottomRight));

        /// <summary>
        /// Retrouve le point du plan correspondant à un point de l'écran transformé par le système de coordonnées
        /// </summary>
        /// <param name="point">Point de l'écran</param>
        /// <returns>Point du plan correspondant au point de l'écran</returns>
        public Point ComputeInCoordinates(Point point) => CoordinatesSystem?.ComputeInCoordinates(ComputeInOrthonormalCoordinates(point)) ?? ComputeInOrthonormalCoordinates(point);

        /// <summary>
        /// Retrouve le vecteur du plan correspondant à un vecteur de l'écran transformé par le système de coordonnées
        /// </summary>
        /// <param name="vector">Vecteur de l'écran</param>
        /// <returns>Vecteur du plan correspondant au vecteur de l'écran</returns>
        public Vector ComputeInCoordinates(Vector vector) => CoordinatesSystem?.ComputeInCoordinates(ComputeInOrthonormalCoordinates(vector)) ?? ComputeInOrthonormalCoordinates(vector);

        public Rect ComputeInCoordinates(Rect rect) => new Rect(ComputeInCoordinates(rect.TopLeft), ComputeInCoordinates(rect.BottomRight));

        public Rect ComputeInCoordinates(MathRect rect) => new Rect(ComputeInCoordinates(rect.TopLeft), ComputeInCoordinates(rect.BottomRight));

        public ReadOnlyCoordinatesSystemManager(MathRect inputRange, Rect outputRange, CoordinatesSystem coordinatesSystem) : this()
        {
            InputRange = inputRange;
            OutputRange = outputRange;
            CoordinatesSystem = coordinatesSystem;
            WidthRatio = outputRange.Width / inputRange.Width;
            HeightRatio = outputRange.Height / inputRange.Height;
            Origin = ComputeOutCoordinates(new Point(0.0, 0.0));
            OrthonormalOrigin = ComputeOutOrthonormalCoordinates(new Point(0.0, 0.0));
        }

        /// <summary>
        /// Calcule la largeur unitaire de l'axe horizontal dans le plan
        /// </summary>
        /// <returns>Longueur unitaire de l'axe horizontal dans le plan</returns>
        public decimal GetHorizontalStep()
        {
            bool horizontalStepEnd = false;
            double horizontalStep = 1.0;
            double horizontalStepFactor = 2.0;

            double outhorizontalStep = horizontalStep * WidthRatio;

            if (outhorizontalStep < MinCellSize)
            {
                while ((outhorizontalStep = horizontalStep * WidthRatio) < MinCellSize)
                {
                    horizontalStep *= horizontalStepFactor;

                    switch (horizontalStepFactor)
                    {
                        case 2.0:
                            if (horizontalStepEnd)
                            {
                                horizontalStepFactor = 2.0;
                                horizontalStepEnd = false;
                            }
                            else horizontalStepFactor = 2.5;
                            break;
                        case 2.5:
                            horizontalStepFactor = 2.0;
                            horizontalStepEnd = true;
                            break;
                    }
                }
            }
            else if (outhorizontalStep > MaxCellSize)
            {
                while ((outhorizontalStep = horizontalStep * WidthRatio) > MaxCellSize)
                {
                    horizontalStep /= horizontalStepFactor;

                    switch (horizontalStepFactor)
                    {
                        case 2.0:
                            if (horizontalStepEnd)
                            {
                                horizontalStepFactor = 2.0;
                                horizontalStepEnd = false;
                            }
                            else horizontalStepFactor = 2.5;
                            break;
                        case 2.5:
                            horizontalStepFactor = 2.0;
                            horizontalStepEnd = true;
                            break;
                    }
                }
            }

            return (decimal)horizontalStep;
        }

        /// <summary>
        /// Calcule la longueur unitaire de l'axe vertical dans le plan
        /// </summary>
        /// <returns>Longueur unitaire de l'axe vertical dans le plan</returns>
        public decimal GetVerticalStep()
        {
            bool verticalStepEnd = false;
            double verticalStep = 1.0;
            double verticalStepFactor = 2.0;

            double outverticalStep = verticalStep * WidthRatio;

            if (outverticalStep < MinCellSize)
            {
                while ((outverticalStep = verticalStep * WidthRatio) < MinCellSize)
                {
                    verticalStep *= verticalStepFactor;

                    switch (verticalStepFactor)
                    {
                        case 2.0:
                            if (verticalStepEnd)
                            {
                                verticalStepFactor = 2.0;
                                verticalStepEnd = false;
                            }
                            else verticalStepFactor = 2.5;
                            break;
                        case 2.5:
                            verticalStepFactor = 2.0;
                            verticalStepEnd = true;
                            break;
                    }
                }
            }
            else if (outverticalStep > MaxCellSize)
            {
                while ((outverticalStep = verticalStep * WidthRatio) > MaxCellSize)
                {
                    verticalStep /= verticalStepFactor;

                    switch (verticalStepFactor)
                    {
                        case 2.0:
                            if (verticalStepEnd)
                            {
                                verticalStepFactor = 2.0;
                                verticalStepEnd = false;
                            }
                            else verticalStepFactor = 2.5;
                            break;
                        case 2.5:
                            verticalStepFactor = 2.0;
                            verticalStepEnd = true;
                            break;
                    }
                }
            }

            return (decimal)verticalStep;
        }

        /// <summary>
        /// Calcule le plus grand multiple de la largeur unitaire inférieur à l'abscisse tout à gauche de la zone du plan
        /// </summary>
        /// <param name="step">Longueur unitaire de l'axe horizontal dans le plan</param>
        /// <returns>Plus grand multiple de la largeur unitaire inférieur à l'abscisse tout à gauche de la zone du plan</returns>
        public decimal GetHorizontalStart(decimal step)
        {
            decimal inLeft = (decimal)InputRange.Left;
            decimal start = inLeft - inLeft % step;
            if (inLeft < 0M) start -= step;
            return start;
        }

        /// <summary>
        /// Calcule le plus petit multiple de la largeur unitaire supérieur à l'abscisse tout à droite de la zone du plan
        /// </summary>
        /// <param name="step">Longueur unitaire de l'axe horizontal dans le plan</param>
        /// <returns>Plus petit multiple de la largeur unitaire supérieur à l'abscisse tout à droite de la zone du plan</returns>
        public decimal GetHorizontalEnd(decimal step)
        {
            decimal inRight = (decimal)InputRange.Right;
            decimal end = inRight + step - inRight % step;
            return end;
        }

        /// <summary>
        /// Calcule le plus grand multiple de la hauteur unitaire inférieur à l'ordonnée tout en bas de la zone du plan
        /// </summary>
        /// <param name="step">Longueur unitaire de l'axe vertical dans le plan</param>
        /// <returns>Plus grand multiple de la hauteur unitaire inférieur à l'ordonnée tout en bas de la zone du plan</returns>
        public decimal GetVerticalStart(decimal step)
        {
            decimal inBottom = (decimal)InputRange.Bottom;
            decimal start = inBottom - inBottom % step;
            if (inBottom < 0M) start -= step;
            return start;
        }

        /// <summary>
        /// Calcule le plus petit multiple de la hauteur unitaire supérieur à l'ordonnée tout en haut de la zone du plan
        /// </summary>
        /// <param name="step">Longueur unitaire de l'axe vertical dans le plan</param>
        /// <returns>Plus petit multiple de la hauteur unitaire supérieur à l'ordonnée tout en haut de la zone du plan</returns>
        public decimal GetVerticalEnd(decimal step)
        {
            decimal inTop = (decimal)InputRange.Top;
            decimal end = inTop + step - inTop % step;
            return end;
        }

        public IEnumerable<decimal> GetHorizontalSteps(decimal step)
        {
            decimal start = GetHorizontalStart(step);
            decimal end = GetHorizontalEnd(step);
            for (decimal i = start; i <= end; i += step) yield return i;
        }

        public IEnumerable<decimal> GetVerticalSteps(decimal step)
        {
            decimal start = GetVerticalStart(step);
            decimal end = GetVerticalEnd(step);
            for (decimal i = start; i <= end; i += step) yield return i;
        }

        public Point MagnetOut(Point outPoint, double tolerance = 4)
        {
            double x = double.NaN;
            double y = double.NaN;

            foreach (double i in GetHorizontalSteps(GetHorizontalStep()))
            {
                double step = ComputeOutOrthonormalXCoordinate(i);
                if (Math.Abs(outPoint.X - step) <= tolerance)
                {
                    x = step;
                    break;
                }
            }

            foreach (double i in GetVerticalSteps(GetVerticalStep()))
            {
                double step = ComputeOutOrthonormalYCoordinate(i);
                if (Math.Abs(outPoint.Y - step) <= tolerance)
                {
                    y = step;
                    break;
                }
            }

            return new Point(double.IsNaN(x) ? outPoint.X : x, double.IsNaN(y) ? outPoint.Y : y);
        }

        public Point MagnetIn(Point outPoint, double stepProportionTolerance = 0.05)
        {
            decimal stepX = GetHorizontalStep();
            decimal stepY = GetVerticalStep();
            double toleranceX = stepProportionTolerance * (double)stepX;
            double toleranceY = stepProportionTolerance * (double)stepY;
            double x = double.NaN;
            double y = double.NaN;

            foreach (double i in GetHorizontalSteps(stepX))
            {
                if (Math.Abs(outPoint.X - i) <= toleranceX)
                {
                    x = i;
                    break;
                }
            }

            foreach (double i in GetVerticalSteps(stepY))
            {
                if (Math.Abs(outPoint.Y - i) <= toleranceY)
                {
                    y = i;
                    break;
                }
            }

            return new Point(double.IsNaN(x) ? outPoint.X : x, double.IsNaN(y) ? outPoint.Y : y);
        }

        public override bool Equals(object obj) => obj is ReadOnlyCoordinatesSystemManager manager && Equals(manager);

        public bool Equals(ReadOnlyCoordinatesSystemManager other) => EqualityComparer<MathRect>.Default.Equals(InputRange, other.InputRange) && EqualityComparer<Rect>.Default.Equals(OutputRange, other.OutputRange) && EqualityComparer<CoordinatesSystem>.Default.Equals(CoordinatesSystem, other.CoordinatesSystem);

        public override int GetHashCode()
        {
            int hashCode = -7085462;
            hashCode = hashCode * -1521134295 + EqualityComparer<MathRect>.Default.GetHashCode(InputRange);
            hashCode = hashCode * -1521134295 + EqualityComparer<Rect>.Default.GetHashCode(OutputRange);
            hashCode = hashCode * -1521134295 + EqualityComparer<CoordinatesSystem>.Default.GetHashCode(CoordinatesSystem);
            return hashCode;
        }

        public static bool operator ==(ReadOnlyCoordinatesSystemManager left, ReadOnlyCoordinatesSystemManager right) => left.Equals(right);
        public static bool operator !=(ReadOnlyCoordinatesSystemManager left, ReadOnlyCoordinatesSystemManager right) => !(left == right);
    }
}
