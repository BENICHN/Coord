using BenLib.Standard;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Coord
{
    public class AxesNumbers : VisualObject
    {
        public override string Type => "AxesNumbers";

        public AxesDirection Direction { get => (AxesDirection)GetValue(DirectionProperty); set => SetValue(DirectionProperty, value); }
        public static readonly DependencyProperty DirectionProperty = CreateProperty<AxesDirection>(true, true, "Direction", typeof(AxesNumbers));

        public double HorizontalStep { get => (double)GetValue(HorizontalStepProperty); set => SetValue(HorizontalStepProperty, value); }
        public static readonly DependencyProperty HorizontalStepProperty = CreateProperty<double>(true, true, "HorizontalStep", typeof(AxesNumbers));

        public double VerticalStep { get => (double)GetValue(VerticalStepProperty); set => SetValue(VerticalStepProperty, value); }
        public static readonly DependencyProperty VerticalStepProperty = CreateProperty<double>(true, true, "VerticalStep", typeof(AxesNumbers));

        public Typeface Typeface { get => (Typeface)GetValue(TypefaceProperty); set => SetValue(TypefaceProperty, value); }
        public static readonly DependencyProperty TypefaceProperty = CreateProperty(true, true, "Typeface", typeof(AxesNumbers), new Typeface("Cambria Math"));

        public double FontSize { get => (double)GetValue(FontSizeProperty); set => SetValue(FontSizeProperty, value); }
        public static readonly DependencyProperty FontSizeProperty = CreateProperty(true, true, "FontSize", typeof(AxesNumbers), 30.0);

        public const double NumbersOffset = 5.0;
        public const double MaxAxisTextWidth = 2.5;

        private static string FormatAxisNumber(double number) => number.ToString(Math.Abs(number) >= 10000 ? "G4" : string.Empty).Replace(new[] { "+0", "+" }, string.Empty);

        protected override IReadOnlyCollection<Character> GetCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            return GetCharactersCore().ToArray();
            IEnumerable<Character> GetCharactersCore()
            {
                var direction = Direction;
                var outRange = coordinatesSystemManager.OutputRange;

                var center = coordinatesSystemManager.OrthonormalOrigin;
                double demiThickness = FontSize + NumbersOffset;

                if ((direction == AxesDirection.Horizontal || direction == AxesDirection.Both) && outRange.HeightContainsRange(center.Y - demiThickness, center.Y + demiThickness, false))
                {
                    foreach (double i in coordinatesSystemManager.GetHorizontalSteps(HorizontalStep > 0 ? HorizontalStep : coordinatesSystemManager.GetHorizontalStep()))
                    {
                        if (i == 0.0 && direction != AxesDirection.Horizontal) continue;

                        var formattedText = new FormattedText(FormatAxisNumber(i), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface, FontSize, Fill, 1.0) { TextAlignment = TextAlignment.Center };

                        var point = coordinatesSystemManager.ComputeOutOrthonormalCoordinates(new Point(i, 0.0));
                        if (i == 0.0)
                        {
                            formattedText.TextAlignment = TextAlignment.Left;
                            point.X += NumbersOffset;
                        }
                        point.Y += NumbersOffset;

                        yield return formattedText.BuildGeometry(point).ToCharacter(Fill, Stroke);
                    }
                }

                if ((direction == AxesDirection.Vertical || direction == AxesDirection.Both) && outRange.WidthContainsRange(center.X - demiThickness, center.X + demiThickness + MaxAxisTextWidth * FontSize, false))
                {
                    foreach (double i in coordinatesSystemManager.GetVerticalSteps(VerticalStep > 0 ? VerticalStep : coordinatesSystemManager.GetVerticalStep()))
                    {
                        var formattedText = new FormattedText(FormatAxisNumber(i), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface, FontSize, Fill, 1.0) { TextAlignment = TextAlignment.Left };

                        var point = coordinatesSystemManager.ComputeOutOrthonormalCoordinates(new Point(0.0, i));
                        point.X += NumbersOffset;
                        if (i == 0.0) point.Y += NumbersOffset;

                        yield return formattedText.BuildGeometry(point).ToCharacter(Fill, Stroke);
                    }
                }
            }
        }
    }
}
