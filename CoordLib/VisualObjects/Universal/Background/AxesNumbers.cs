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

        public AxesDirection AlwaysVisible { get => (AxesDirection)GetValue(AlwaysVisibleProperty); set => SetValue(AlwaysVisibleProperty, value); }
        public static readonly DependencyProperty AlwaysVisibleProperty = CreateProperty<AxesNumbers, AxesDirection>(true, true, true, "AlwaysVisible");

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
            var alwaysVisible = AlwaysVisible;

            var outRange = coordinatesSystemManager.OutputRange;

            var center = coordinatesSystemManager.OrthonormalOrigin;
            double demiThickness = fontSize + NumbersOffset;

            if (direction.HasFlag(AxesDirection.Horizontal) && (alwaysVisible.HasFlag(AxesDirection.Horizontal) || outRange.HeightContainsRange(center.Y - demiThickness, center.Y + demiThickness, false)))
            {
                double y = coordinatesSystemManager.ComputeOutOrthonormalYCoordinate(0) + NumbersOffset;
                foreach (double i in coordinatesSystemManager.GetHorizontalSteps(horizontalStep > 0 ? horizontalStep : coordinatesSystemManager.GetHorizontalStep()))
                {
                    if (i == 0.0 && direction.HasFlag(AxesDirection.Vertical) || onlyIntegers && Math.Truncate(i) != i) continue;

                    var formattedText = new FormattedText(FormatAxisNumber(i), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, fontSize, fill, 1) { TextAlignment = TextAlignment.Center };

                    var point = new Point(coordinatesSystemManager.ComputeOutOrthonormalXCoordinate(i), y);
                    if (i == 0.0)
                    {
                        formattedText.TextAlignment = TextAlignment.Left;
                        point.X += NumbersOffset;
                    }

                    var geo = formattedText.BuildGeometry(point);
                    var result = geo.ToCharacter(fill, stroke);
                    if (alwaysVisible.HasFlag(AxesDirection.Horizontal))
                    {
                        var bounds = geo.Bounds;
                        result.Transform.Translate(0, (outRange.Top - bounds.Top + NumbersOffset).Trim(min:0) + (outRange.Bottom - bounds.Bottom - NumbersOffset).Trim(max:0));
                    }
                    yield return result;
                }
            }

            if ((direction.HasFlag(AxesDirection.Vertical)) && (alwaysVisible.HasFlag(AxesDirection.Vertical) || outRange.WidthContainsRange(center.X - demiThickness, center.X + demiThickness + MaxAxisTextWidth * fontSize, false)))
            {
                double x = coordinatesSystemManager.ComputeOutOrthonormalXCoordinate(0) + NumbersOffset;
                foreach (double i in coordinatesSystemManager.GetVerticalSteps(verticalStep > 0 ? verticalStep : coordinatesSystemManager.GetVerticalStep()))
                {
                    if (onlyIntegers && Math.Truncate(i) != i) continue;

                    var formattedText = new FormattedText(FormatAxisNumber(i), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, fontSize, fill, 1.0) { TextAlignment = TextAlignment.Left };

                    var point = new Point(x, coordinatesSystemManager.ComputeOutOrthonormalYCoordinate(i));
                    if (i == 0.0) point.Y += NumbersOffset;

                    var geo = formattedText.BuildGeometry(point);
                    var result = geo.ToCharacter(fill, stroke);
                    if (alwaysVisible.HasFlag(AxesDirection.Vertical))
                    {
                        var bounds = geo.Bounds;
                        result.Transform.Translate((outRange.Left - bounds.Left + NumbersOffset).Trim(min: 0) + (outRange.Right - bounds.Right - NumbersOffset).Trim(max: 0) , 0);
                    }
                    yield return result;
                }
            }
        }
    }
}
