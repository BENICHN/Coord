using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Coord
{
    public class GridVisualObject : VisualObject
    {
        public override string Type => "Grid";

        public bool Primary { get => (bool)GetValue(PrimaryProperty); set => SetValue(PrimaryProperty, value); }
        public static readonly DependencyProperty PrimaryProperty = CreateProperty<bool>(true, true, "Primary", typeof(GridVisualObject));

        public bool Secondary { get => (bool)GetValue(SecondaryProperty); set => SetValue(SecondaryProperty, value); }
        public static readonly DependencyProperty SecondaryProperty = CreateProperty<bool>(true, true, "Secondary", typeof(GridVisualObject));

        public decimal HorizontalStep { get => (decimal)GetValue(HorizontalStepProperty); set => SetValue(HorizontalStepProperty, value); }
        public static readonly DependencyProperty HorizontalStepProperty = CreateProperty<decimal>(true, true, "HorizontalStep", typeof(GridVisualObject));

        public decimal VerticalStep { get => (decimal)GetValue(VerticalStepProperty); set => SetValue(VerticalStepProperty, value); }
        public static readonly DependencyProperty VerticalStepProperty = CreateProperty<decimal>(true, true, "VerticalStep", typeof(GridVisualObject));

        public int SecondaryDensity { get => (int)GetValue(SecondaryDensityProperty); set => SetValue(SecondaryDensityProperty, value); }
        public static readonly DependencyProperty SecondaryDensityProperty = CreateProperty(true, true, "SecondaryDensity", typeof(GridVisualObject), 3);

        public PlanePen SecondaryStroke { get => (PlanePen)GetValue(SecondaryStrokeProperty); set => SetValue(SecondaryStrokeProperty, value); }
        public static readonly DependencyProperty SecondaryStrokeProperty = CreateProperty<PlanePen>(true, true, "SecondaryStroke", typeof(GridVisualObject));

        public override IEnumerable<Character> HitTestCache(Point point) => Cache.Characters?.Skip(1).HitTest(point) ?? Enumerable.Empty<Character>();
        public override IEnumerable<Character> HitTestCache(Rect rect) => Cache.Characters?.Skip(1).HitTest(rect) ?? Enumerable.Empty<Character>();

        protected override IReadOnlyCollection<Character> GetCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            return GetCharactersCore().ToArray();
            IEnumerable<Character> GetCharactersCore()
            {
                var outRange = coordinatesSystemManager.OutputRange;
                var inRange = coordinatesSystemManager.InputRange;

                decimal horizontalStep = HorizontalStep > 0M ? HorizontalStep : coordinatesSystemManager.GetHorizontalStep();
                decimal verticalStep = VerticalStep > 0M ? VerticalStep : coordinatesSystemManager.GetVerticalStep();

                decimal horizontalStart = coordinatesSystemManager.GetHorizontalStart(horizontalStep);
                decimal verticalStart = coordinatesSystemManager.GetVerticalStart(verticalStep);

                decimal horizontalEnd = coordinatesSystemManager.GetHorizontalEnd(horizontalStep);
                decimal verticalEnd = coordinatesSystemManager.GetVerticalEnd(verticalStep);

                yield return Character.Rectangle(outRange).Color(Fill);

                if (Secondary)
                {
                    int secondaryDensity = SecondaryDensity;
                    int horizontalStepProgress = 0;
                    decimal smallHorizontalStep = horizontalStep / secondaryDensity;

                    for (decimal i = horizontalStart - horizontalStep + smallHorizontalStep; i < horizontalEnd; i += smallHorizontalStep)
                    {
                        horizontalStepProgress++;
                        if (horizontalStepProgress == secondaryDensity)
                        {
                            horizontalStepProgress = 0;
                            continue;
                        }

                        double doubleI = (double)i;
                        yield return Character.Line(coordinatesSystemManager.ComputeOutOrthonormalCoordinates(new Point(doubleI, inRange.Bottom)), coordinatesSystemManager.ComputeOutOrthonormalCoordinates(new Point(doubleI, inRange.Top))).Color(SecondaryStroke);
                    }

                    int verticalStepProgress = 0;
                    decimal smallVerticalStep = verticalStep / secondaryDensity;

                    for (decimal i = verticalStart - verticalStep + smallVerticalStep; i < verticalEnd; i += smallVerticalStep)
                    {
                        verticalStepProgress++;
                        if (verticalStepProgress == secondaryDensity)
                        {
                            verticalStepProgress = 0;
                            continue;
                        }

                        double doubleI = (double)i;
                        yield return Character.Line(coordinatesSystemManager.ComputeOutOrthonormalCoordinates(new Point(inRange.Left, doubleI)), coordinatesSystemManager.ComputeOutOrthonormalCoordinates(new Point(inRange.Right, doubleI))).Color(SecondaryStroke);
                    }
                }

                if (Primary)
                {
                    for (decimal i = horizontalStart; i < horizontalEnd; i += horizontalStep)
                    {
                        double doubleI = (double)i;
                        yield return Character.Line(coordinatesSystemManager.ComputeOutOrthonormalCoordinates(new Point(doubleI, inRange.Bottom)), coordinatesSystemManager.ComputeOutOrthonormalCoordinates(new Point(doubleI, inRange.Top))).Color(Stroke);
                    }

                    for (decimal i = verticalStart; i < verticalEnd; i += verticalStep)
                    {
                        double doubleI = (double)i;
                        yield return Character.Line(coordinatesSystemManager.ComputeOutOrthonormalCoordinates(new Point(inRange.Left, doubleI)), coordinatesSystemManager.ComputeOutOrthonormalCoordinates(new Point(inRange.Right, doubleI))).Color(Stroke);
                    }
                }
            }
        }
    }
}
