using BenLib.Standard;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace Coord
{
    public class AxesNumbers : VisualObject
    {
        protected override Freezable CreateInstanceCore() => new AxesNumbers();

        public override string Type => "AxesNumbers";

        public AxesDirection Direction { get => (AxesDirection)GetValue(DirectionProperty); set => SetValue(DirectionProperty, value); }
        public static readonly DependencyProperty DirectionProperty = CreateProperty<AxesNumbers, AxesDirection>(true, true, true, "Direction");

        public double HorizontalStep { get => (double)GetValue(HorizontalStepProperty); set => SetValue(HorizontalStepProperty, value); }
        public static readonly DependencyProperty HorizontalStepProperty = CreateProperty<AxesNumbers, double>(true, true, true, "HorizontalStep");

        public double VerticalStep { get => (double)GetValue(VerticalStepProperty); set => SetValue(VerticalStepProperty, value); }
        public static readonly DependencyProperty VerticalStepProperty = CreateProperty<AxesNumbers, double>(true, true, true, "VerticalStep");

        public Typeface Typeface { get => (Typeface)GetValue(TypefaceProperty); set => SetValue(TypefaceProperty, value); }
        public static readonly DependencyProperty TypefaceProperty = CreateProperty<AxesNumbers, Typeface>(true, true, true, "Typeface", new Typeface("Cambria Math"));

        public double FontSize { get => (double)GetValue(FontSizeProperty); set => SetValue(FontSizeProperty, value); }
        public static readonly DependencyProperty FontSizeProperty = CreateProperty<AxesNumbers, double>(true, true, true, "FontSize", 30);

        public bool OnlyIntegers { get => (bool)GetValue(OnlyIntegersProperty); set => SetValue(OnlyIntegersProperty, value); }
        public static readonly DependencyProperty OnlyIntegersProperty = CreateProperty<AxesNumbers, bool>(true, true, true, "OnlyIntegers");

        public const double NumbersOffset = 5.0;
        public const double MaxAxisTextWidth = 2.5;

        public static string FormatAxisNumber(double number) => number.ToString(Math.Abs(number) >= 10000 ? "G4" : string.Empty).Replace(new[] { "+0", "+" }, string.Empty);

        protected override IEnumerable<Character> GetCharactersCore(ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            double horizontalStep = HorizontalStep;
            double verticalStep = VerticalStep;
            var fill = Fill;
            var stroke = Stroke;
            double fontSize = FontSize;
            var typeface = Typeface;
            bool onlyIntegers = OnlyIntegers;
            var direction = Direction;

            var outRange = coordinatesSystemManager.OutputRange;

            var center = coordinatesSystemManager.OrthonormalOrigin;
            double demiThickness = fontSize + NumbersOffset;

            if ((direction == AxesDirection.Horizontal || direction == AxesDirection.Both) && outRange.HeightContainsRange(center.Y - demiThickness, center.Y + demiThickness, false))
            {
                foreach (double i in coordinatesSystemManager.GetHorizontalSteps(horizontalStep > 0 ? horizontalStep : coordinatesSystemManager.GetHorizontalStep()))
                {
                    if (i == 0.0 && direction != AxesDirection.Horizontal || onlyIntegers && Math.Truncate(i) != i) continue;

                    var formattedText = new FormattedText(FormatAxisNumber(i), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, fontSize, fill, 1) { TextAlignment = TextAlignment.Center };

                    var point = coordinatesSystemManager.ComputeOutOrthonormalCoordinates(new Point(i, 0.0));
                    if (i == 0.0)
                    {
                        formattedText.TextAlignment = TextAlignment.Left;
                        point.X += NumbersOffset;
                    }
                    point.Y += NumbersOffset;

                    yield return formattedText.BuildGeometry(point).ToCharacter(fill, stroke);
                }
            }

            if ((direction == AxesDirection.Vertical || direction == AxesDirection.Both) && outRange.WidthContainsRange(center.X - demiThickness, center.X + demiThickness + MaxAxisTextWidth * fontSize, false))
            {
                foreach (double i in coordinatesSystemManager.GetVerticalSteps(verticalStep > 0 ? verticalStep : coordinatesSystemManager.GetVerticalStep()))
                {
                    if (onlyIntegers && Math.Truncate(i) != i) continue;

                    var formattedText = new FormattedText(FormatAxisNumber(i), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, fontSize, fill, 1.0) { TextAlignment = TextAlignment.Left };

                    var point = coordinatesSystemManager.ComputeOutOrthonormalCoordinates(new Point(0.0, i));
                    point.X += NumbersOffset;
                    if (i == 0.0) point.Y += NumbersOffset;

                    yield return formattedText.BuildGeometry(point).ToCharacter(fill, stroke);
                }
            }
        }
    }
}
