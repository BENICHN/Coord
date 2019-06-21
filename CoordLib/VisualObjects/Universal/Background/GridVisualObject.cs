using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Coord
{
    public class GridVisualObject : VisualObject
    {
        protected override Freezable CreateInstanceCore() => new GridVisualObject();

        public override string Type => "Grid";

        public bool Primary { get => (bool)GetValue(PrimaryProperty); set => SetValue(PrimaryProperty, value); }
        public static readonly DependencyProperty PrimaryProperty = CreateProperty<GridVisualObject, bool>(true, true, true, "Primary");

        public bool Secondary { get => (bool)GetValue(SecondaryProperty); set => SetValue(SecondaryProperty, value); }
        public static readonly DependencyProperty SecondaryProperty = CreateProperty<GridVisualObject, bool>(true, true, true, "Secondary");

        public double HorizontalStep { get => (double)GetValue(HorizontalStepProperty); set => SetValue(HorizontalStepProperty, value); }
        public static readonly DependencyProperty HorizontalStepProperty = CreateProperty<GridVisualObject, double>(true, true, true, "HorizontalStep");

        public double VerticalStep { get => (double)GetValue(VerticalStepProperty); set => SetValue(VerticalStepProperty, value); }
        public static readonly DependencyProperty VerticalStepProperty = CreateProperty<GridVisualObject, double>(true, true, true, "VerticalStep");

        public int SecondaryDensity { get => (int)GetValue(SecondaryDensityProperty); set => SetValue(SecondaryDensityProperty, value); }
        public static readonly DependencyProperty SecondaryDensityProperty = CreateProperty<GridVisualObject, int>(true, true, true, "SecondaryDensity", 3);

        public Pen SecondaryStroke { get => (Pen)GetValue(SecondaryStrokeProperty); set => SetValue(SecondaryStrokeProperty, value); }
        public static readonly DependencyProperty SecondaryStrokeProperty = CreateProperty<GridVisualObject, Pen>(true, true, true, "SecondaryStroke");

        public override IEnumerable<Character> HitTestCache(Point point) => Cache.Characters?.Skip(1).HitTest(point) ?? Enumerable.Empty<Character>();
        public override IEnumerable<Character> HitTestCache(Rect rect) => Cache.Characters?.Skip(1).HitTest(rect) ?? Enumerable.Empty<Character>();

        protected override IEnumerable<Character> GetCharactersCore(ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            var outRange = coordinatesSystemManager.OutputRange;
            var inRange = coordinatesSystemManager.InputRange;

            double horizontalStep = HorizontalStep > 0 ? HorizontalStep : coordinatesSystemManager.GetHorizontalStep();
            double verticalStep = VerticalStep > 0 ? VerticalStep : coordinatesSystemManager.GetVerticalStep();

            double horizontalStart = coordinatesSystemManager.GetHorizontalStart(horizontalStep);
            double verticalStart = coordinatesSystemManager.GetVerticalStart(verticalStep);

            double horizontalEnd = coordinatesSystemManager.GetHorizontalEnd(horizontalStep);
            double verticalEnd = coordinatesSystemManager.GetVerticalEnd(verticalStep);

            yield return Character.Rectangle(outRange).Color(Fill);

            if (Secondary)
            {
                int secondaryDensity = SecondaryDensity;
                int horizontalStepProgress = 0;
                double smallHorizontalStep = horizontalStep / secondaryDensity;

                for (double i = horizontalStart - horizontalStep + smallHorizontalStep; i < horizontalEnd; i += smallHorizontalStep)
                {
                    horizontalStepProgress++;
                    if (horizontalStepProgress == secondaryDensity)
                    {
                        horizontalStepProgress = 0;
                        continue;
                    }

                    yield return Character.Line(coordinatesSystemManager.ComputeOutOrthonormalCoordinates(new Point(i, inRange.Bottom)), coordinatesSystemManager.ComputeOutOrthonormalCoordinates(new Point(i, inRange.Top))).Color(SecondaryStroke);
                }

                int verticalStepProgress = 0;
                double smallVerticalStep = verticalStep / secondaryDensity;

                for (double i = verticalStart - verticalStep + smallVerticalStep; i < verticalEnd; i += smallVerticalStep)
                {
                    verticalStepProgress++;
                    if (verticalStepProgress == secondaryDensity)
                    {
                        verticalStepProgress = 0;
                        continue;
                    }

                    yield return Character.Line(coordinatesSystemManager.ComputeOutOrthonormalCoordinates(new Point(inRange.Left, i)), coordinatesSystemManager.ComputeOutOrthonormalCoordinates(new Point(inRange.Right, i))).Color(SecondaryStroke);
                }
            }

            if (Primary)
            {
                for (double i = horizontalStart; i < horizontalEnd; i += horizontalStep) yield return Character.Line(coordinatesSystemManager.ComputeOutOrthonormalCoordinates(new Point(i, inRange.Bottom)), coordinatesSystemManager.ComputeOutOrthonormalCoordinates(new Point(i, inRange.Top))).Color(Stroke);
                for (double i = verticalStart; i < verticalEnd; i += verticalStep) yield return Character.Line(coordinatesSystemManager.ComputeOutOrthonormalCoordinates(new Point(inRange.Left, i)), coordinatesSystemManager.ComputeOutOrthonormalCoordinates(new Point(inRange.Right, i))).Color(Stroke);
            }
        }
    }
}
