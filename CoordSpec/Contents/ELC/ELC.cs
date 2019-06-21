using BenLib.Framework;
using BenLib.Standard;
using BenLib.WPF;
using Coord;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace CoordSpec
{
    public class ELC : VisualObject
    {
        protected override Freezable CreateInstanceCore() => new ELC();

        public override string Type => "ELC";

        private Rect[] m_inRectsCache;
        private Point[][] m_inPointsCache;

        public PointVisualObject Center { get => (PointVisualObject)GetValue(CenterProperty); set => SetValue(CenterProperty, value); }
        public static readonly DependencyProperty CenterProperty = CreateProperty<ELC, PointVisualObject>(true, true, true, "Center");

        public int Rank { get => (int)GetValue(RankProperty); set => SetValue(RankProperty, value); }
        public static readonly DependencyProperty RankProperty = CreateProperty<ELC, int>(true, true, true, "Rank");

        protected override void OnChanged()
        {
            m_inRectsCache = new Rect[Rank];
            m_inPointsCache = new Point[Rank][];

            var center = Center.Definition.InPoint;
            int unitX = 1;
            int unitY = 1;
            int unitX2 = 2 * unitX;
            int unitY2 = 2 * unitY;
            double width = 0;
            double height = 0;

            for (int i = 0; i < Rank; i++)
            {
                double demiWidth = width / 2;
                double demiHeight = height / 2;
                var sq = new Rect(center.X - demiWidth, center.Y - demiHeight, width, height);

                m_inRectsCache[i] = sq;
                m_inPointsCache[i] = FillSquare(sq).ToArray();

                width += unitX2;
                height += unitY2;
            }

            base.OnChanged();

            IEnumerable<Point> FillSquare(Rect rect)
            {
                var point = rect.TopLeft;
                do
                {
                    yield return point;
                    point.X += unitX;
                } while (point.X < rect.Right);

                point = rect.TopRight;
                while (point.Y < rect.Bottom)
                {
                    yield return point;
                    point.Y += unitY;
                }

                point = rect.BottomRight;
                while (point.X > rect.Left)
                {
                    yield return point;
                    point.X -= unitX;
                }

                point = rect.BottomLeft;
                while (point.Y > rect.Top)
                {
                    yield return point;
                    point.Y -= unitY;
                }
            }
        }

        protected override IReadOnlyCollection<Character> GetCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            var fill = Fill;
            var stroke = Stroke;
            var pointsFill = Center.Fill;
            var pointsStroke = Center.Stroke;
            double pointsRadius = Center.Radius;

            return new ELCCharacters(
                m_inRectsCache.Select(rect => Character.Rectangle(coordinatesSystemManager.ComputeOutCoordinates(rect)).Color(fill, stroke)).ToArray(),
                m_inPointsCache.Select(points => points.Select(point => Character.Ellipse(coordinatesSystemManager.ComputeOutCoordinates(point), pointsRadius, pointsRadius).Color(pointsFill, pointsStroke)).ToArray()).ToArray());
        }
    }

    public class ELCCharacters : IReadOnlyCollection<Character>
    {
        public ELCCharacters(IReadOnlyList<Character> squares, IReadOnlyList<Character[]> points)
        {
            Squares = squares;
            Points = points;
            AllPoints = points.SelectMany(c => c).ToArray();
        }

        public IReadOnlyList<Character> Squares { get; }
        public IReadOnlyList<Character[]> Points { get; }
        public IReadOnlyList<Character> AllPoints { get; }

        public int Count => Squares.Count + AllPoints.Count;

        public IEnumerator<Character> GetEnumerator() => new MultiEnumerator<Character>(Squares.GetEnumerator(), AllPoints.GetEnumerator());
        IEnumerator IEnumerable.GetEnumerator() => new MultiEnumerator(Squares.GetEnumerator(), Points.GetEnumerator());
    }

    public class ELCWriteCharacterEffect : CharacterEffect
    {
        protected override Freezable CreateInstanceCore() => new ELCWriteCharacterEffect();

        protected override void ApplyCore(IReadOnlyCollection<Character> characters, Interval<int> interval, in ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            if (characters is ELCCharacters elccharacters)
            {
                var easedProgress = EasedProgress;
                var (startProgress, endProgress) = easedProgress.Value.SplitTrimProgress(0.5, 0.3);

                int squaresCount = elccharacters.Squares.Count - 1;
                var squaresProgress = easedProgress.ChangeValue(startProgress);
                elccharacters.Squares.Skip(1).ForEach((character, i) =>
                {
                    double progress = squaresProgress.Get(i, squaresCount).Trim();
                    StrokeCharacterEffect.ApplyOn(character, false, new SineEase { EasingMode = EasingMode.EaseOut }.Ease(progress));
                });

                int allPointsCount = elccharacters.AllPoints.Count;
                var pointsProgress = easedProgress.ChangeValue(endProgress);
                elccharacters.AllPoints.ForEach((character, i) =>
                {
                    double progress = pointsProgress.Get(i, allPointsCount);
                    WriteCharacterEffect.ApplyOn(character, false, 3, progress);
                });
            }
        }
    }

    public class ELCFocusCharacterEffect : CharacterEffect
    {
        protected override Freezable CreateInstanceCore() => new ELCFocusCharacterEffect();

        public Predicate<int> RanksToFocus { get => (Predicate<int>)GetValue(RanksToFocusProperty); set => SetValue(RanksToFocusProperty, value); }
        public static readonly DependencyProperty RanksToFocusProperty = CreateProperty<ELCFocusCharacterEffect, Predicate<int>>(true, true, true, "RanksToFocus");

        public double Opacity { get => (double)GetValue(OpacityProperty); set => SetValue(OpacityProperty, value); }
        public static readonly DependencyProperty OpacityProperty = CreateProperty<ELCFocusCharacterEffect, double>(true, true, true, "Opacity");

        protected override void ApplyCore(IReadOnlyCollection<Character> characters, Interval<int> interval, in ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            if (characters is ELCCharacters elccharacters)
            {
                var easedProgress = EasedProgress;
                var ranksToFocus = RanksToFocus;
                double opacity = Opacity;
                int count = elccharacters.Squares.Count;
                var valid = Enumerable.Range(0, count).Where(i => !ranksToFocus(i)).Select(i => elccharacters.Points[i].Append(elccharacters.Squares[i])).ToArray();
                int length = valid.Length;
                for (int i = 0; i < length; i++)
                {
                    double progress = easedProgress.Get(i, length);
                    foreach (var character in valid[i]) OpacityCharacterEffect.ApplyOn(character, opacity, opacity, progress);
                }
            }
        }
    }

    public class ConversionStartingEase : IEasingFunction
    {
        private int m_stepCount;

        public int StepCount
        {
            get => m_stepCount;
            set
            {
                m_stepCount = value;
                Locations = GetLocations(value).ToArray();
            }
        }

        public double[] Locations { get; private set; }

        public ConversionStartingEase(int stepCount) => StepCount = stepCount;

        public ConversionStartingEase() => StepCount = 5;

        public double Ease(double normalizedTime) => normalizedTime.GetProgressAfterSplitting(Locations);

        public static IEnumerable<double> GetLocations(int stepCount)
        {
            var outCubic = new PowerEase { EasingMode = EasingMode.EaseOut, Power = 1.2 };
            var inCubic = new PowerEase { EasingMode = EasingMode.EaseIn, Power = 1.2 };

            for (int i = 1; i <= stepCount; i++) yield return outCubic.Ease((double)i / stepCount) / 2;
            for (int i = 1; i <= stepCount; i++) yield return 0.5 + inCubic.Ease((double)i / stepCount) / 2;
        }
    }

    public class EA : VisualObject
    {
        protected override Freezable CreateInstanceCore() => new EA();

        public override string Type => "EA";

        protected override IEnumerable<Character> GetCharactersCore(ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            yield return Character.Line(coordinatesSystemManager.ComputeOutCoordinates(new Point(0, -1)), coordinatesSystemManager.ComputeOutCoordinates(new Point(0, 1))).Color(Fill, new Pen(FlatBrushes.Alizarin, 4));
            foreach (var character in new ConversionStartingEase().Locations.Select(loc => Character.Line(coordinatesSystemManager.ComputeOutCoordinates(new Point(loc, -1)), coordinatesSystemManager.ComputeOutCoordinates(new Point(loc, 1))).Color(Fill, Stroke))) yield return character;
            yield return Character.Line(coordinatesSystemManager.ComputeOutCoordinates(new Point(1, -1)), coordinatesSystemManager.ComputeOutCoordinates(new Point(1, 1))).Color(Fill, new Pen(FlatBrushes.Alizarin, 4));
        }
    }
}
