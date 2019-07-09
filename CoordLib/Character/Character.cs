using BenLib.Framework;
using BenLib.Standard;
using BenLib.WPF;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Coord
{
    public class Character : ICloneable<Character>
    {
        public Character(Geometry geometry)
        {
            Geometry = geometry;
            Transform = geometry.Transform?.Value ?? Matrix.Identity;
            if (geometry.IsFrozen) IsTransformed = true;
            else geometry.Transform = null;
        }
        public Character(Geometry geometry, Brush fill, Pen stroke) : this(geometry)
        {
            Fill = fill;
            Stroke = stroke;
        }
        private Character(VisualObject owner, VisualObject creator, int index, Geometry geometry, Brush fill, Pen stroke, Brush selectionFill, Pen selectionStroke, Matrix transform, bool isTransformed, bool isHitTestVisible, object data)
        {
            Owner = owner;
            Creator = creator;
            Index = index;
            Geometry = geometry;
            Fill = fill;
            Stroke = stroke;
            SelectionFill = selectionFill;
            SelectionStroke = selectionStroke;
            Transform = transform;
            IsTransformed = isTransformed;
            IsHitTestVisible = isHitTestVisible;
            Data = data;
        }

        public Character Clone() => new Character(Owner, Creator, Index, Geometry?.CloneCurrentValue(), Fill?.CloneCurrentValue(), Stroke?.CloneCurrentValue(), SelectionFill?.CloneCurrentValue(), SelectionStroke?.CloneCurrentValue(), Transform, IsTransformed, IsHitTestVisible, Data);
        public Character Clone(VisualObject owner, int index) => new Character(owner, Creator ?? Owner, index, Geometry?.CloneCurrentValue(), Fill?.CloneCurrentValue(), Stroke?.CloneCurrentValue(), SelectionFill?.CloneCurrentValue(), SelectionStroke?.CloneCurrentValue(), Transform, IsTransformed, IsHitTestVisible, Data);
        public Character Attach(VisualObject owner, int index)
        {
            Owner = owner;
            Creator ??= owner;
            Index = index;
            return this;
        }
        public Character WithData(object data)
        {
            Data = data;
            return this;
        }
        public Character WithData(Func<object, object> operation)
        {
            Data = operation(Data);
            return this;
        }
        public Character WithData<T>(Func<T, object> operation)
        {
            Data = operation((T)Data);
            return this;
        }
        public Character Color(Brush fill, Pen stroke)
        {
            Fill = fill;
            Stroke = stroke;
            return this;
        }
        public Character Color(Brush fill)
        {
            Fill = fill;
            return this;
        }
        public Character Color(Pen stroke)
        {
            Stroke = stroke;
            return this;
        }
        public Character ColorSelection(Brush fill, Pen stroke)
        {
            SelectionFill = fill;
            SelectionStroke = stroke;
            return this;
        }
        public Character ColorSelection(Brush fill)
        {
            SelectionFill = fill;
            return this;
        }
        public Character ColorSelection(Pen stroke)
        {
            SelectionStroke = stroke;
            return this;
        }
        public Character HideSelection() => ColorSelection(null, null);
        public Character HideHitTest()
        {
            IsHitTestVisible = false;
            return this;
        }

        public Brush Fill { get; set; }
        public Pen Stroke { get; set; }

        public Brush SelectionFill { get; set; } = null;
        public Pen SelectionStroke { get; set; } = VisualObject.SelectionStroke;

        public Matrix Transform;

        public bool IsTransformed { get; private set; }
        public Geometry Geometry { get; }

        public object Data { get; set; }

        public VisualObject Owner { get; private set; }
        public VisualObject Creator { get; private set; }
        public int Index { get; private set; }

        private bool m_isHitTestVisible = true;
        public bool IsHitTestVisible { get => m_isHitTestVisible && (Owner?.IsHitTestVisible ?? true); set => m_isHitTestVisible = value; }

        public bool IsSelected
        {
            get => Owner?.Selection >= Index;
            set { if (Owner != null) Owner.Selection = value ? Owner.Selection | Interval<int>.Single(Index) : Owner.Selection / Interval<int>.Single(Index); }
        }

        public event PropertyChangedExtendedEventHandler<bool> IsSelectedChanged;
        internal void NotifyIsSelectedChanged() { bool newValue = IsSelected; IsSelectedChanged?.Invoke(this, new PropertyChangedExtendedEventArgs<bool>("IsSelected", !newValue, newValue)); }

        public void ApplyTransforms()
        {
            if (!Geometry.IsFrozen)
            {
                Geometry.Transform = new System.Windows.Media.MatrixTransform(Transform);
                IsTransformed = true;
            }
        }

        public void ReleaseTransforms()
        {
            if (!Geometry.IsFrozen)
            {
                Geometry.Transform = null;
                IsTransformed = false;
            }
        }

        public override string ToString() => ToString(false);
        public string ToString(bool detail) => "{" + Transform + "} => " + (detail ? Geometry.ToString() : Geometry.GetType().Name);

        public static IEnumerable<Character> FromGeometry(Geometry geometry, Brush fill = null, Pen stroke = null)
        {
            if (geometry is GeometryGroup geometryGroup) { foreach (var c in geometryGroup.Children.SelectMany(g => FromGeometry(g, fill, stroke))) yield return c; }
            else yield return new Character(geometry, fill, stroke);
        }
        public static IEnumerable<Character> FromGeometry(IEnumerable<Geometry> geometries, Brush fill = null, Pen stroke = null) => geometries.SelectMany(g => FromGeometry(g, fill, stroke));

        public static Character Text(Point anchorPoint, string text, Typeface typeface, double fontSize, TextAlignment textAlignment = TextAlignment.Left) => new Character(new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, fontSize, Brushes.Transparent, 1) { TextAlignment = textAlignment }.BuildGeometry(anchorPoint));
        public static Character Line(Point startPoint, Point endPoint) => new Character(new LineGeometry(startPoint, endPoint));
        public static Character Ellipse(Point center, double radiusX, double radiusY) => new Character(new EllipseGeometry(center, radiusX, radiusY));
        public static Character Ellipse(Rect rect) => new Character(new EllipseGeometry(rect));
        public static Character Rectangle(Rect rect) => new Character(new RectangleGeometry(rect));
        public static Character Rectangle(Rect rect, double radiusX, double radiusY) => new Character(new RectangleGeometry(rect, radiusX, radiusY));
        public static Character Arc(Point startPoint, Point endPoint, Size size, double rotationAngle, bool isLargeArc, SweepDirection sweepDirection)
        {
            var result = new StreamGeometry();
            using (var context = result.Open())
            {
                context.BeginFigure(startPoint, true, false);
                context.ArcTo(endPoint, size, rotationAngle, isLargeArc, sweepDirection, true, true);
            }
            return new Character(result);
        }

        public static Character Path(IEnumerable<PathFigure> figures, FillRule fillRule, System.Windows.Media.Transform transform) => new Character(new PathGeometry(figures, fillRule, transform));
        public static Character Path(params PathFigure[] figures) => new Character(new PathGeometry(figures));
    }

    public static partial class Extensions
    {
        public static IEnumerable<Character> HitTest(this IEnumerable<Character> characters, Rect rect)
        {
            var rectangle = new RectangleGeometry(rect);
            foreach (var character in characters.Where(c => c.IsHitTestVisible))
            {
                bool fill = character.Fill != null && character.Fill.Opacity > 0;
                bool stroke = character.Stroke != null && character.Stroke.Thickness > 0 && character.Stroke.Brush != null && character.Stroke.Brush.Opacity > 0;
                bool transformed = character.IsTransformed;
                if (!transformed) character.ApplyTransforms();
                IntersectionDetail intersectionDetail;
                if ((fill, stroke) switch
                {
                    (false, false) => false,
                    (false, true) => (intersectionDetail = character.Geometry.StrokeContainsWithDetail(character.Stroke, rectangle)) == IntersectionDetail.FullyInside || intersectionDetail == IntersectionDetail.Intersects,
                    _ => (intersectionDetail = character.Geometry.FillContainsWithDetail(rectangle)) == IntersectionDetail.FullyInside || intersectionDetail == IntersectionDetail.Intersects,
                }) yield return character;
                if (!transformed) character.ReleaseTransforms();
            }
        }

        public static IEnumerable<Character> HitTest(this IEnumerable<Character> characters, Point point)
        {
            foreach (var character in characters.Where(c => c.IsHitTestVisible))
            {
                bool fill = character.Fill != null && character.Fill.Opacity > 0;
                bool stroke = character.Stroke != null && character.Stroke.Thickness > 0 && character.Stroke.Brush != null && character.Stroke.Brush.Opacity > 0;
                bool transformed = character.IsTransformed;
                if (!transformed) character.ApplyTransforms();
                if (fill && character.Geometry.FillContains(point) || stroke && character.Geometry.StrokeContains(character.Stroke, point)) yield return character;
                if (!transformed) character.ReleaseTransforms();
            }
        }

        public static Interval<int> ToSelection(this IEnumerable<Character> characters) => characters.Union(c => Interval<int>.Single(c.Index));

        public static IEnumerable<Character> CloneCharacters(this IEnumerable<Character> characters) => characters.Select(c => c.Clone());
        public static IEnumerable<Character> CloneCharacters(this IEnumerable<Character> characters, VisualObject owner) => characters.Select((c, i) => c.Clone(owner, i));
        public static IEnumerable<Character> AttachCharacters(this IEnumerable<Character> characters, VisualObject owner) => characters.Select((c, i) => c.Attach(owner, i));

        public static void Transform(this IEnumerable<Character> characters, Matrix matrix, bool replace) { foreach (var character in characters) character.Transform = replace ? matrix : character.Transform * matrix; }

        public static IEnumerable<Character> Translate(this IEnumerable<Character> characters, Vector offset, Progress progress, int count = -1)
        {
            int length = progress.Mode == ProgressMode.OneAtATime ? 0 : count < 0 ? characters.Count() : count;
            int i = 0;
            foreach (var character in characters)
            {
                double p = progress.Get(i, length);
                var vector = offset * p;
                character.Transform.Translate(vector.X, vector.Y);
                yield return character;
                i++;
            }
        }

        public static IEnumerable<Character> Scale(this IEnumerable<Character> characters, double scaleX, double scaleY, Progress progress, int count = -1)
        {
            var center = characters.Bounds().Center();
            int length = progress.Mode == ProgressMode.OneAtATime ? 0 : count < 0 ? characters.Count() : count;
            int i = 0;
            foreach (var character in characters)
            {
                double p = progress.Get(i, length);
                character.Transform.ScaleAt(1 + (scaleX - 1) * p, 1 + (scaleY - 1) * p, center.X, center.Y);
                yield return character;
                i++;
            }
        }

        public static IEnumerable<Character> ScaleAt(this IEnumerable<Character> characters, double scaleX, double scaleY, Point center, Progress progress, int count = -1)
        {
            int length = progress.Mode == ProgressMode.OneAtATime ? 0 : count < 0 ? characters.Count() : count;
            int i = 0;
            foreach (var character in characters)
            {
                double p = progress.Get(i, length);
                character.Transform.ScaleAt(1 + (scaleX - 1) * p, 1 + (scaleY - 1) * p, center.X, center.Y);
                yield return character;
                i++;
            }
        }

        public static IEnumerable<Character> Rotate(this IEnumerable<Character> characters, double angle, Progress progress, int count = -1)
        {
            var center = characters.Bounds().Center();
            int length = progress.Mode == ProgressMode.OneAtATime ? 0 : count < 0 ? characters.Count() : count;
            int i = 0;
            foreach (var character in characters)
            {
                double p = progress.Get(i, length);
                character.Transform.RotateAt(angle * p, center.X, center.Y);
                yield return character;
                i++;
            }
        }

        public static IEnumerable<Character> RotateAt(this IEnumerable<Character> characters, double angle, Point center, Progress progress, int count = -1)
        {
            int length = progress.Mode == ProgressMode.OneAtATime ? 0 : count < 0 ? characters.Count() : count;
            int i = 0;
            foreach (var character in characters)
            {
                double p = progress.Get(i, length);
                character.Transform.RotateAt(angle * p, center.X, center.Y);
                yield return character;
                i++;
            }
        }

        public static GeometryGroup Geometry(this IEnumerable<Character> characters) => new GeometryGroup() { Children = new GeometryCollection(characters.Select(character => character.Geometry)) };
        public static Rect Bounds(this IEnumerable<Character> characters)
        {
            var (left, top, right, bottom) = (double.PositiveInfinity, double.PositiveInfinity, double.NegativeInfinity, double.NegativeInfinity);
            foreach (var character in characters)
            {
                var bounds = character.Geometry.Bounds;
                if (left > bounds.Left) left = bounds.Left;
                if (top > bounds.Top) top = bounds.Top;
                if (right < bounds.Right) right = bounds.Right;
                if (bottom < bounds.Bottom) bottom = bounds.Bottom;
            }
            return (left + right + top + bottom).IsNaN() ? Rect.Empty : new Rect(left, top, right - left, bottom - top);
        }

        public static Character ToCharacter(this Geometry geometry, Brush fill, Pen stroke) => new Character(geometry, fill, stroke);
        public static Character ToCharacter(this Geometry geometry, Pen stroke) => new Character(geometry, null, stroke);
        public static Character ToCharacter(this Geometry geometry, Brush fill) => new Character(geometry, fill, null);
        public static Character ToCharacter(this Geometry geometry) => new Character(geometry, null, null);

        public static Character ToCharacter(this Shape shape)
        {
            var geometry = shape.ToGeometry();
            var transform = shape.RenderTransform.Value;
            var result = new Character(geometry, shape.Fill, new Pen(shape.Stroke, shape.StrokeThickness) { DashCap = shape.StrokeDashCap, DashStyle = new DashStyle(shape.StrokeDashArray, shape.StrokeDashOffset), EndLineCap = shape.StrokeEndLineCap, LineJoin = shape.StrokeLineJoin, MiterLimit = shape.StrokeMiterLimit, StartLineCap = shape.StrokeStartLineCap });
            result.Transform *= transform;
            return result;
        }

        public static Character ToCharacter(this GeometryDrawing geometryDrawing) => new Character(geometryDrawing.Geometry, geometryDrawing.Brush, geometryDrawing.Pen);

        public static IEnumerable<Character> ToCharacters(this Geometry geometry) => Character.FromGeometry(geometry);
        public static IEnumerable<Character> ToCharacters(this Geometry geometry, Brush fill) => Character.FromGeometry(geometry, fill);
        public static IEnumerable<Character> ToCharacters(this Geometry geometry, Pen stroke) => Character.FromGeometry(geometry, null, stroke);
        public static IEnumerable<Character> ToCharacters(this Geometry geometry, Brush fill, Pen stroke) => Character.FromGeometry(geometry, fill, stroke);

        public static IEnumerable<Character> ToCharacters(this Canvas canvas)
        {
            foreach (var c in canvas.Children.OfType<Canvas>().SelectMany(ca => ca.ToCharacters())) yield return c;
            foreach (var shape in canvas.Children.OfType<Shape>()) yield return shape.ToCharacter();
        }

        public static IEnumerable<Character> ToCharacters(this DrawingGroup drawingGroup)
        {
            foreach (var drawing in drawingGroup.Children)
            {
                if (drawing is DrawingGroup group) { foreach (var character in group.ToCharacters()) yield return character; }
                else if (drawing is GeometryDrawing geometryDrawing) yield return geometryDrawing.ToCharacter();
            }
        }

        public static IEnumerable<Character> Color(this IEnumerable<Character> characters, Brush fill, Pen stroke) => characters.Select(character => character.Color(fill, stroke));
        public static IEnumerable<Character> Color(this IEnumerable<Character> characters, Brush fill) => characters.Select(character => character.Color(fill));
        public static IEnumerable<Character> Color(this IEnumerable<Character> characters, Pen stroke) => characters.Select(character => character.Color(stroke));

        public static IEnumerable<Character> ColorSelection(this IEnumerable<Character> characters, Brush fill, Pen stroke) => characters.Select(character => character.ColorSelection(fill, stroke));
        public static IEnumerable<Character> ColorSelection(this IEnumerable<Character> characters, Brush fill) => characters.Select(character => character.ColorSelection(fill));
        public static IEnumerable<Character> ColorSelection(this IEnumerable<Character> characters, Pen stroke) => characters.Select(character => character.ColorSelection(stroke));
        public static IEnumerable<Character> HideSelection(this IEnumerable<Character> characters) => characters.Select(character => character.HideSelection());
        public static IEnumerable<Character> HideHitTest(this IEnumerable<Character> characters) => characters.Select(character => character.HideHitTest());

    }
}
