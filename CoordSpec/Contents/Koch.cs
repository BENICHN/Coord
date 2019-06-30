using Coord;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using BenLib.Framework;
using BenLib.WPF;
using static System.Math;
using static BenLib.Framework.NumFramework;

namespace CoordSpec
{
    public class Koch : VisualObject, IProgressive
    {
        public override string Type => "Koch";
        protected override Freezable CreateInstanceCore() => new Koch();

        public PointVisualObject Start { get => (PointVisualObject)GetValue(StartProperty); set => SetValue(StartProperty, value); }
        public static readonly DependencyProperty StartProperty = CreateProperty<Koch, PointVisualObject>(true, true, true, "Start");

        public PointVisualObject End { get => (PointVisualObject)GetValue(EndProperty); set => SetValue(EndProperty, value); }
        public static readonly DependencyProperty EndProperty = CreateProperty<Koch, PointVisualObject>(true, true, true, "End");

        public int Step { get => (int)GetValue(StepProperty); set => SetValue(StepProperty, value); }
        public static readonly DependencyProperty StepProperty = CreateProperty<Koch, int>(true, true, true, "Step");

        public Progress Progress { get => (Progress)GetValue(ProgressProperty); set => SetValue(ProgressProperty, value); }
        public static readonly DependencyProperty ProgressProperty = CreateProperty<Koch, Progress>(true, true, true, "Progress", 1);

        private (Point[] points, int step) m_cache;

        protected override IEnumerable<Character> GetCharactersCore(ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            if (m_cache.points is Point[] points && points.Length > 0)
            {
                var stroke = Stroke;
                double penThickness2 = stroke?.Thickness ?? 1;
                penThickness2 = Max(1, penThickness2 * penThickness2 / 4);
                var result = new StreamGeometry();
                using (var context = result.Open())
                {
                    var ctx = new OptimizedStreamGeometryContext(context, penThickness2);
                    ctx.BeginFigure(coordinatesSystemManager.ComputeOutCoordinates(points[0]), true, false);
                    ctx.PolyLineTo(points.Skip(1).Select(p => coordinatesSystemManager.ComputeOutCoordinates(p)), true, true);
                }
                yield return GeometryHelper.ClipGeometry(PathGeometry.CreateFromGeometry(result), coordinatesSystemManager.OutputRange).ToCharacter(stroke);
                //yield return result.ToCharacter(stroke);
            }
        }

        protected override void OnChanged()
        {
            if (Start is PointVisualObject s && End is PointVisualObject e)
            {
                var start = s.Definition.InPoint;
                var end = e.Definition.InPoint;
                int step = Min(10, Step);
                m_cache = (GetPoints(start, end, step, Progress.Value).Append(end).ToArray(), step);
                static IEnumerable<Point> GetPoints(Point start, Point end, int step, double progress)
                {
                    var diff = end - start;
                    var diff3 = diff / 3;
                    double l = diff.Length;
                    var p1 = start + diff3;
                    var p3 = p1 + diff3;
                    var p2 = p1 + diff3.Rotate(PI / 3);
                    if (progress < 1) p2 = Interpolate(start + diff / 2, p2, progress);

                    if (step == 0)
                    {
                        yield return start;
                        yield return p1;
                        yield return p2;
                        yield return p3;
                    }
                    else
                    {
                        foreach (var point in GetPoints(start, p1, step - 1, progress)) yield return point;
                        foreach (var point in GetPoints(p1, p2, step - 1, progress)) yield return point;
                        foreach (var point in GetPoints(p2, p3, step - 1, progress)) yield return point;
                        foreach (var point in GetPoints(p3, end, step - 1, progress)) yield return point;
                    }
                }
            }
            base.OnChanged();
        }
    }
}
