using BenLib;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Coord
{
    public class AxesNumbers : VisualObject
    {
        public AxesDirection Direction
        {
            get => m_direction;
            set
            {
                m_direction = value;
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

        public const double NumbersOffset = 5.0;
        public const double MaxAxisTextWidth = 2.5;
        private decimal m_horizontalStep;
        private decimal m_verticalStep;
        private AxesDirection m_direction;
        private Typeface m_typeface = new Typeface("Cambria Math");
        private double m_fontSize = 30.0;

        public AxesNumbers(AxesDirection direction, decimal horizontalStep = 0M, decimal verticalStep = 0M)
        {
            Direction = direction;
            HorizontalStep = horizontalStep;
            VerticalStep = verticalStep;
        }
        public AxesNumbers(AxesDirection direction, Typeface typeface, double fontSize, decimal horizontalStep = 0M, decimal verticalStep = 0M) : this(direction, horizontalStep, verticalStep)
        {
            Typeface = typeface;
            FontSize = fontSize;
        }

        public Typeface Typeface
        {
            get => m_typeface;
            set
            {
                m_typeface = value;
                NotifyChanged();
            }
        }
        public double FontSize
        {
            get => m_fontSize;
            set
            {
                m_fontSize = value;
                NotifyChanged();
            }
        }
        private static string FormatAxisNumber(double number) => number.ToString("G4").Replace(new[] { "+0", "+" }, string.Empty);

        public override IReadOnlyCollection<Character> GetCharacters(CoordinatesSystemManager coordinatesSystemManager)
        {
            return GetCharactersCore().ToArray();
            IEnumerable<Character> GetCharactersCore()
            {
                var direction = Direction;
                var outRange = coordinatesSystemManager.OutputRange;
                var inRange = coordinatesSystemManager.InputRange;

                var center = coordinatesSystemManager.OrthonormalOrigin;
                var demiThickness = FontSize + NumbersOffset;

                if ((direction == AxesDirection.Horizontal || direction == AxesDirection.Both) && outRange.HeightContainsRange(center.Y - demiThickness, center.Y + demiThickness, false))
                {
                    decimal horizontalStep = HorizontalStep > 0M ? HorizontalStep : coordinatesSystemManager.GetHorizontalStep();

                    decimal start = coordinatesSystemManager.GetHorizontalStart(horizontalStep);
                    decimal end = coordinatesSystemManager.GetHorizontalEnd(horizontalStep);

                    for (decimal i = start; i <= end; i += horizontalStep)
                    {
                        double doubleI = (double)i;
                        if (doubleI == 0.0 && direction != AxesDirection.Horizontal) continue;

                        var formattedText = new FormattedText(FormatAxisNumber(doubleI), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface, FontSize, Fill, 1.0) { TextAlignment = TextAlignment.Center };

                        var point = coordinatesSystemManager.ComputeOutOrthonormalCoordinates(new Point(doubleI, 0.0));
                        if (doubleI == 0.0)
                        {
                            formattedText.TextAlignment = TextAlignment.Left;
                            point.X += NumbersOffset;
                        }
                        point.Y += NumbersOffset;

                        yield return new Character(formattedText.BuildGeometry(point), Fill, Stroke);
                    }
                }

                if ((direction == AxesDirection.Vertical || direction == AxesDirection.Both) && outRange.WidthContainsRange(center.X - demiThickness, center.X + demiThickness + MaxAxisTextWidth * FontSize, false))
                {
                    decimal verticalStep = VerticalStep > 0M ? VerticalStep : coordinatesSystemManager.GetVerticalStep();

                    decimal start = coordinatesSystemManager.GetVerticalStart(verticalStep);
                    decimal end = coordinatesSystemManager.GetVerticalEnd(verticalStep);

                    for (decimal i = start; i <= end; i += verticalStep)
                    {
                        double doubleI = (double)i;

                        var formattedText = new FormattedText(FormatAxisNumber(doubleI), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface, FontSize, Fill, 1.0) { TextAlignment = TextAlignment.Left };

                        var point = coordinatesSystemManager.ComputeOutOrthonormalCoordinates(new Point(0.0, doubleI));
                        point.X += NumbersOffset;
                        if (doubleI == 0.0) point.Y += NumbersOffset;

                        yield return new Character(formattedText.BuildGeometry(point), Fill, Stroke);
                    }
                }
            }
        }
    }
}
