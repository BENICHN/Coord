using BenLib;
using BenLib.WPF;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Coord
{
    public class ELC : VisualObject
    {
        public override string Type => "ELC";

        private Rect[] m_inRectsCache;
        private Point[][] m_inPointsCache;

        private int m_rank;
        public int Rank
        {
            get => m_rank;
            set
            {
                m_rank = value;
                ResetCache();
                NotifyChanged();
            }
        }

        private Brush m_pointsFill;
        public Brush PointsFill
        {
            get => m_pointsFill;
            set
            {
                m_pointsFill = value;
                Register(value);
                NotifyChanged();
            }
        }

        private Pen m_pointsStroke;
        public Pen PointsStroke
        {
            get => m_pointsStroke;
            set
            {
                m_pointsStroke = value;
                Register(value);
                NotifyChanged();
            }
        }

        private double m_pointsRadius;
        public double PointsRadius
        {
            get => m_pointsRadius;
            set
            {
                m_pointsRadius = value;
                NotifyChanged();
            }
        }

        private PointVisualObject m_center;
        public PointVisualObject Center
        {
            get => m_center;
            set
            {
                m_center = value;
                ResetCache();
                Register(value);
                NotifyChanged();
            }
        }

        public void ResetCache()
        {
            m_inRectsCache = new Rect[Rank];
            m_inPointsCache = new Point[Rank][];

            var center = (Point)Center;
            var unitX = 1;
            var unitY = 1;
            var unitX2 = 2 * unitX;
            var unitY2 = 2 * unitY;
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

        public override IReadOnlyCollection<Character> GetCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            var fill = Fill;
            var stroke = Stroke;
            var pointsFill = PointsFill;
            var pointsStroke = PointsStroke;
            double pointsRadius = PointsRadius;

            return new ELCCharacters(
                m_inRectsCache.Select(rect => Character.Rectangle(coordinatesSystemManager.ComputeOutCoordinates(rect), fill, stroke)).ToArray(),
                m_inPointsCache.Select(points => points.Select(point => Character.Ellipse(coordinatesSystemManager.ComputeOutCoordinates(point), pointsRadius, pointsRadius, pointsFill, pointsStroke)).ToArray()).ToArray());
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
        private static readonly SineEase s_squaresEasingFunction = new SineEase { EasingMode = EasingMode.EaseOut };

        public ELCWriteCharacterEffect(IntInterval interval, Progress progress, params SynchronizedProgress[] synchronizedProgresses) : this(interval, progress, false, synchronizedProgresses) { }
        public ELCWriteCharacterEffect(IntInterval interval, Progress progress, bool withTransforms, params SynchronizedProgress[] synchronizedProgresses) : base(interval, progress, withTransforms, synchronizedProgresses) { }

        protected override void ApplyCore(IReadOnlyCollection<Character> characters, ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            if (characters is ELCCharacters elccharacters)
            {
                var easedProgress = EasedProgress;
                var (startProgress, endProgress) = easedProgress.Value.SplitTrimProgress(0.5, 0.3);

                int squaresCount = elccharacters.Squares.Count - 1;
                var squaresProgress = easedProgress.ChangeValue(startProgress);
                elccharacters.Squares.Skip(1).ForEach((i, character) =>
                {
                    double progress = squaresProgress.Get(i, squaresCount).Trim();
                    StrokeCharacterEffect.ApplyOn(character, false, s_squaresEasingFunction.Ease(progress));
                });

                int allPointsCount = elccharacters.AllPoints.Count;
                var pointsProgress = easedProgress.ChangeValue(endProgress);
                elccharacters.AllPoints.ForEach((i, character) =>
                {
                    double progress = pointsProgress.Get(i, allPointsCount);
                    WriteCharacterEffect.ApplyOn(character, false, 3, progress);
                });
            }
        }

        public override CharacterEffect Clone() => new ELCWriteCharacterEffect(Interval, Progress, WithTransforms);
    }

    public class ELCFocusCharacterEffect : CharacterEffect
    {
        private Predicate<int> m_ranksToFocus;
        public Predicate<int> RanksToFocus
        {
            get => m_ranksToFocus;
            set
            {
                m_ranksToFocus = value;
                NotifyChanged();
            }
        }

        private double m_opacity;
        public double Opacity
        {
            get => m_opacity;
            set
            {
                m_opacity = value;
                NotifyChanged();
            }
        }

        public ELCFocusCharacterEffect(IntInterval interval, Predicate<int> ranksToFocus, double opacity, Progress progress, params SynchronizedProgress[] synchronizedProgresses) : this(interval, ranksToFocus, opacity, progress, false, synchronizedProgresses) { }
        public ELCFocusCharacterEffect(IntInterval interval, Predicate<int> ranksToFocus, double opacity, Progress progress, bool withTransforms, params SynchronizedProgress[] synchronizedProgresses) : base(interval, progress, withTransforms, synchronizedProgresses)
        {
            m_ranksToFocus = ranksToFocus;
            Opacity = opacity;
        }

        protected override void ApplyCore(IReadOnlyCollection<Character> characters, ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            if (characters is ELCCharacters elccharacters)
            {
                var easedProgress = EasedProgress;
                var ranksToFocus = RanksToFocus;
                double opacity = Opacity;
                int count = elccharacters.Squares.Count;
                var valid = Enumerable.Range(0, count).Where(i => !ranksToFocus(i)).Select(i => elccharacters.Points[i].Concat(elccharacters.Squares[i])).ToArray();
                int length = valid.Length;
                for (int i = 0; i < length; i++)
                {
                    double progress = easedProgress.Get(i, length);
                    foreach (var character in valid[i]) OpacityCharacterEffect.ApplyOn(character, opacity, opacity, progress);
                }
            }
        }

        public override CharacterEffect Clone() => new ELCFocusCharacterEffect(Interval, RanksToFocus, Opacity, Progress, WithTransforms);
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
        public override string Type => "EA";

        public override IReadOnlyCollection<Character> GetCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            return GetCharactersCore().ToArray();
            IEnumerable<Character> GetCharactersCore()
            {
                yield return Character.Line(coordinatesSystemManager.ComputeOutCoordinates(new Point(0, -1)), coordinatesSystemManager.ComputeOutCoordinates(new Point(0, 1)), Fill, new Pen(FlatBrushes.Alizarin, 4));
                foreach (var character in new ConversionStartingEase().Locations.Select(loc => Character.Line(coordinatesSystemManager.ComputeOutCoordinates(new Point(loc, -1)), coordinatesSystemManager.ComputeOutCoordinates(new Point(loc, 1)), Fill, Stroke))) yield return character;
                yield return Character.Line(coordinatesSystemManager.ComputeOutCoordinates(new Point(1, -1)), coordinatesSystemManager.ComputeOutCoordinates(new Point(1, 1)), Fill, new Pen(FlatBrushes.Alizarin, 4));
            }
        }
    }
}
