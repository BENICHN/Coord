using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Coord
{
    public class GridVisualObject : VisualObject
    {
        public override string Type => "Grid";

        private bool m_secondary;
        private bool m_primary;
        private decimal m_horizontalStep;
        private decimal m_verticalStep;
        private int m_secondaryDensity;
        private Pen m_secondaryStroke;

        public GridVisualObject(bool primary, bool secondary, decimal horizontalStep = 0M, decimal verticalStep = 0M, int secondaryDensity = 3)
        {
            Primary = primary;
            Secondary = secondary;
            HorizontalStep = horizontalStep;
            VerticalStep = verticalStep;
            SecondaryDensity = secondaryDensity;
        }

        public bool Primary
        {
            get => m_primary;
            set
            {
                m_primary = value;
                NotifyChanged();
            }
        }
        public bool Secondary
        {
            get => m_secondary;
            set
            {
                m_secondary = value;
                NotifyChanged();
            }
        }

        public decimal HorizontalStep
        {
            get => m_horizontalStep;
            set
            {
                m_horizontalStep = value;
                NotifyChanged();
            }
        }
        public decimal VerticalStep
        {
            get => m_verticalStep;
            set
            {
                m_verticalStep = value;
                NotifyChanged();
            }
        }
        public int SecondaryDensity
        {
            get => m_secondaryDensity;
            set
            {
                m_secondaryDensity = value;
                NotifyChanged();
            }
        }

        public Pen SecondaryStroke
        {
            get => m_secondaryStroke;
            set
            {
                m_secondaryStroke = value;
                NotifyChanged();
            }
        }

        public override IReadOnlyCollection<Character> GetCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
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

                yield return Character.Rectangle(outRange, Fill, null);

                if (Secondary)
                {
                    int secondaryDensity = SecondaryDensity;
                    int horizontalStepProgress = 0;
                    var smallHorizontalStep = horizontalStep / secondaryDensity;

                    for (decimal i = (horizontalStart - horizontalStep) + smallHorizontalStep; i < horizontalEnd; i += smallHorizontalStep)
                    {
                        horizontalStepProgress++;
                        if (horizontalStepProgress == secondaryDensity)
                        {
                            horizontalStepProgress = 0;
                            continue;
                        }

                        double doubleI = (double)i;
                        yield return Character.Line(coordinatesSystemManager.ComputeOutOrthonormalCoordinates(new Point(doubleI, inRange.Bottom)), coordinatesSystemManager.ComputeOutOrthonormalCoordinates(new Point(doubleI, inRange.Top)), null, SecondaryStroke);
                    }

                    int verticalStepProgress = 0;
                    var smallVerticalStep = verticalStep / secondaryDensity;

                    for (decimal i = (verticalStart - verticalStep) + smallVerticalStep; i < verticalEnd; i += smallVerticalStep)
                    {
                        verticalStepProgress++;
                        if (verticalStepProgress == secondaryDensity)
                        {
                            verticalStepProgress = 0;
                            continue;
                        }

                        double doubleI = (double)i;
                        yield return Character.Line(coordinatesSystemManager.ComputeOutOrthonormalCoordinates(new Point(inRange.Left, doubleI)), coordinatesSystemManager.ComputeOutOrthonormalCoordinates(new Point(inRange.Right, doubleI)), null, SecondaryStroke);
                    }
                }

                if (Primary)
                {
                    for (decimal i = horizontalStart; i < horizontalEnd; i += horizontalStep)
                    {
                        double doubleI = (double)i;
                        yield return Character.Line(coordinatesSystemManager.ComputeOutOrthonormalCoordinates(new Point(doubleI, inRange.Bottom)), coordinatesSystemManager.ComputeOutOrthonormalCoordinates(new Point(doubleI, inRange.Top)), null, Stroke);
                    }

                    for (decimal i = verticalStart; i < verticalEnd; i += verticalStep)
                    {
                        double doubleI = (double)i;
                        yield return Character.Line(coordinatesSystemManager.ComputeOutOrthonormalCoordinates(new Point(inRange.Left, doubleI)), coordinatesSystemManager.ComputeOutOrthonormalCoordinates(new Point(inRange.Right, doubleI)), null, Stroke);
                    }
                }
            }
        }
    }
}
