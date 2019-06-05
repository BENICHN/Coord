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

        public double HorizontalStep { get => (double)GetValue(HorizontalStepProperty); set => SetValue(HorizontalStepProperty, value); }
        public static readonly DependencyProperty HorizontalStepProperty = CreateProperty<double>(true, true, "HorizontalStep", typeof(GridVisualObject));

        public double VerticalStep { get => (double)GetValue(VerticalStepProperty); set => SetValue(VerticalStepProperty, value); }
        public static readonly DependencyProperty VerticalStepProperty = CreateProperty<double>(true, true, "VerticalStep", typeof(GridVisualObject));

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
}
