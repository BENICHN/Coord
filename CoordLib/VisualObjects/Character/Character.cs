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
    /// <summary>
    /// Contient une <see cref="System.Windows.Media.Geometry"/> et des propriétés permettant la mise en forme de cette dernière
    /// </summary>
    public class Character : ICloneable<Character>
    {
        public Character(Geometry geometry)
        {
            Geometry = geometry;
            Transform = geometry.Transform?.Value ?? new Matrix();
            if (geometry.IsFrozen) IsTransformed = true;
            else geometry.Transform = null;
        }

        public Character(Geometry geometry, Brush fill, Pen stroke) : this(geometry)
        {
            Fill = fill;
            Stroke = stroke;
        }

        private Character(VisualObject owner, VisualObject creator, int index, bool isSelectable, Geometry geometry, Brush fill, Pen stroke, Matrix transform, bool isTransformed, object data)
        {
            Owner = owner;
            Creator = creator;
            Index = index;
            IsSelectable = isSelectable;
            Geometry = geometry;
            Fill = fill;
            Stroke = stroke;
            Transform = transform;
            IsTransformed = isTransformed;
            Data = data;
        }

        /// <summary>
        /// Crée un une copie des valeurs de l'instance actuelle de <see cref="Character"/>
        /// </summary>
        /// <returns>Copie des valeurs de l'instance actuelle</returns>
        public Character Clone() => new Character(Owner, Creator, Index, IsSelectable, Geometry?.CloneCurrentValue(), Fill?.CloneCurrentValue(), Stroke?.CloneCurrentValue(), Transform, IsTransformed, Data);
        public Character Clone(VisualObject owner, int index) => new Character(owner, Creator ?? Owner, index, IsSelectable, Geometry?.CloneCurrentValue(), Fill?.CloneCurrentValue(), Stroke?.CloneCurrentValue(), Transform, IsTransformed, Data);
        public Character Attach(VisualObject owner, int index)
        {
            Owner = owner;
            Creator ??= owner;
            Index = index;
            return this;
        }
        public Character PreventSelection()
        {
            IsSelectable = false;
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

        /// <summary>
        /// Remplissage de <see cref="Geometry"/>
        /// </summary>
        public Brush Fill { get; set; }

        /// <summary>
        /// Contour de <see cref="Geometry"/>
        /// </summary>
        public Pen Stroke { get; set; }

        /// <summary>
        /// Transformation qui va être appliquée à <see cref="Geometry"/>
        /// </summary>
        public Matrix Transform;

        /// <summary>
        /// Indique si <see cref="Geometry"/> est actuellement transformée par <see cref="Matrix"/>
        /// </summary>
        public bool IsTransformed { get; private set; }

        /// <summary>
        /// Géométrie à dessiner
        /// </summary>
        public Geometry Geometry { get; }

        public object Data { get; set; }

        public VisualObject Owner { get; private set; }
        public VisualObject Creator { get; private set; }
        public int Index { get; private set; }

        private bool m_isSelectable = true;
        public bool IsSelectable
        {
            get => m_isSelectable;
            set
            {
                m_isSelectable = value;
                if (!value) IsSelected = false;
            }
        }

        public bool IsSelected
        {
            get => Owner?.Selection >= Index;
            set { if (Owner != null) Owner.Selection = value ? Owner.Selection | Interval<int>.Single(Index) : Owner.Selection / Interval<int>.Single(Index); }
        }

        public event PropertyChangedExtendedEventHandler<bool> IsSelectedChanged;
        internal void NotifyIsSelectedChanged() { bool newValue = IsSelected; IsSelectedChanged?.Invoke(this, new PropertyChangedExtendedEventArgs<bool>("IsSelected", !newValue, newValue)); }

        /// <summary>
        /// Transforme <see cref="Geometry"/> par <see cref="Matrix"/>
        /// </summary>
        public void ApplyTransforms()
        {
            if (!Geometry.IsFrozen)
            {
                Geometry.Transform = new MatrixTransform(Transform);
                IsTransformed = true;
            }
        }

        /// <summary>
        /// Supprime les transformations de <see cref="Geometry"/>
        /// </summary>
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

        /// <summary>
        /// Obtient une collection de <see cref="Character"/> à partir d'une <see cref="System.Windows.Media.Geometry"/> éventuellement d'un <see cref="GeometryGroup"/>
        /// </summary>
        /// <param name="geometry"><see cref="System.Windows.Media.Geometry"/> utilisée</param>
        /// <returns>Collection de <see cref="Character"/> obtenue à partir de la <see cref="System.Windows.Media.Geometry"/></returns>
        public static IEnumerable<Character> FromGeometry(Geometry geometry, Brush fill = null, Pen stroke = null)
        {
            if (geometry is GeometryGroup geometryGroup) { foreach (var c in geometryGroup.Children.SelectMany(g => FromGeometry(g, fill, stroke))) yield return c; }
            else yield return new Character(geometry, fill, stroke);
        }

        /// <summary>
        /// Obtient une collection de <see cref="Character"/> à partir d'une collection de <see cref="System.Windows.Media.Geometry"/> éventuellement de <see cref="GeometryGroup"/>
        /// </summary>
        /// <param name="geometries">Collection de <see cref="System.Windows.Media.Geometry"/> utilisée</param>
        /// <returns>Collection de <see cref="Character"/> obtenue à partir de la <see cref="System.Windows.Media.Geometry"/></returns>
        public static IEnumerable<Character> FromGeometry(IEnumerable<Geometry> geometries, Brush fill = null, Pen stroke = null) => geometries.SelectMany(g => FromGeometry(g, fill, stroke));

        public static IEnumerable<Character> FromCanvas(Canvas canvas)
        {
            foreach (var c in canvas.Children.OfType<Canvas>().SelectMany(ca => FromCanvas(ca))) yield return c;
            foreach (var shape in canvas.Children.OfType<Shape>()) yield return shape.ToCharacter();
        }

        public static Character Text(Point anchorPoint, string text, Typeface typeface, double fontSize, TextAlignment textAlignment = TextAlignment.Left) => new Character(new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, fontSize, Brushes.Transparent, 1) { TextAlignment = textAlignment }.BuildGeometry(anchorPoint));
        public static Character Line(Point startPoint, Point endPoint) => new Character(new LineGeometry(startPoint, endPoint));
        public static Character Ellipse(Point center, double radiusX, double radiusY) => new Character(new EllipseGeometry(center, radiusX, radiusY));
        public static Character Ellipse(Rect rect) => new Character(new EllipseGeometry(rect));
        public static Character Rectangle(Rect rect) => new Character(new RectangleGeometry(rect));
        public static Character Rectangle(Rect rect, double radiusX, double radiusY) => new Character(new RectangleGeometry(rect, radiusX, radiusY));
        public static Character Arc(Point startPoint, Point endPoint, Size size, double rotationAngle, bool isLargeArc, SweepDirection sweepDirection) => new Character(new PathGeometry(new[] { new PathFigure(startPoint, new[] { new ArcSegment(endPoint, size, rotationAngle, isLargeArc, sweepDirection, true) }, false) }));
        public static Character Path(IEnumerable<PathFigure> figures, FillRule fillRule, Transform transform) => new Character(new PathGeometry(figures, fillRule, transform));
        public static Character Path(params PathFigure[] figures) => new Character(new PathGeometry(figures));
    }

    public static partial class Extensions
    {
        public static IEnumerable<Character> HitTest(this IEnumerable<Character> characters, Rect rect)
        {
            var rectangle = new RectangleGeometry(rect);
            foreach (var character in characters)
            {
                bool fill = character.Fill != null && character.Fill.Opacity > 0;
                bool stroke = character.Stroke != null && character.Stroke.Thickness > 0 && character.Stroke.Brush != null && character.Stroke.Brush.Opacity > 0;
                bool transformed = character.IsTransformed;
                if (!transformed) character.ApplyTransforms();
                var result = rectangle.FillContainsWithDetail(character.Geometry);
                if (result != IntersectionDetail.Empty && result != IntersectionDetail.NotCalculated && result != IntersectionDetail.FullyInside) yield return character;
                if (!transformed) character.ReleaseTransforms();
            }
        }

        public static IEnumerable<Character> HitTest(this IEnumerable<Character> characters, Point point)
        {
            foreach (var character in characters)
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

        /// <summary>
        /// Clone tous les <see cref="Character"/> d'une collection puis retourne le résultat
        /// </summary>
        /// <param name="characters">Collection qui contient les <see cref="Character"/> à cloner</param>
        /// <returns>Collection contenant les <see cref="Character"/> clonés</returns>
        public static IEnumerable<Character> CloneCharacters(this IEnumerable<Character> characters) => characters.Select(c => c.Clone());
        public static IEnumerable<Character> CloneCharacters(this IEnumerable<Character> characters, VisualObject owner) => characters.Select((c, i) => c.Clone(owner, i));
        public static IEnumerable<Character> AttachCharacters(this IEnumerable<Character> characters, VisualObject owner) => characters.Select((c, i) => c.Attach(owner, i));

        public static void Transform(this IEnumerable<Character> characters, Matrix matrix, bool replace) { foreach (var character in characters) character.Transform = replace ? matrix : character.Transform * matrix; }

        /// <summary>
        /// Translate un groupe de <see cref="Character"/> par les valeurs spécifiées
        /// </summary>
        /// <param name="characters">Groupe de <see cref="Character"/> à translater</param>
        /// <param name="offsetX">Décalage sur l'axe des abscisses</param>
        /// <param name="offsetY">Décalage sur l'axe des ordonnées</param>
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

        /// <summary>
        /// Met à l'échele un groupe de <see cref="Character"/> par les valeurs spécifiées
        /// </summary>
        /// <param name="characters">Groupe de <see cref="Character"/> à mettre à l'échelle</param>
        /// <param name="scaleX">Valeur d'échelle sur l'axe des abscisses</param>
        /// <param name="scaleY">Valeur d'échelle sur l'axe des ordonnées</param>
        public static IEnumerable<Character> Scale(this IEnumerable<Character> characters, double scaleX, double scaleY, Progress progress, int count = -1)
        {
            var center = characters.Geometry().Bounds.Center();
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

        /// <summary>
        /// Met à l'échele un groupe de <see cref="Character"/> par les valeurs spécifiées au point spécifié
        /// </summary>
        /// <param name="characters">Groupe de <see cref="Character"/> à mettre à l'échelle</param>
        /// <param name="scaleX">Valeur d'échelle sur l'axe des abscisses</param>
        /// <param name="scaleY">Valeur d'échelle sur l'axe des ordonnées</param>
        /// <param name="centerX">Coordonnée x du point central de mise à l'échelle</param>
        /// <param name="centerY">Coordonnée y du point central de mise à l'échelle</param>
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
            var center = characters.Geometry().Bounds.Center();
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

        /// <summary>
        /// Obtient un <see cref="GeometryGroup"/> à partir d'une collection de <see cref="Character"/>
        /// </summary>
        /// <param name="characters">Collection de <see cref="Character"/> utilisée</param>
        /// <returns><see cref="GeometryGroup"/> obtenu à partir de la collection de <see cref="Character"/></returns>
        public static GeometryGroup Geometry(this IEnumerable<Character> characters) => new GeometryGroup() { Children = new GeometryCollection(characters.Select(character => character.Geometry)) };

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

        public static IEnumerable<Character> ToCharacters(this Geometry geometry) => Character.FromGeometry(geometry);
        public static IEnumerable<Character> ToCharacters(this Geometry geometry, Brush fill) => Character.FromGeometry(geometry, fill);
        public static IEnumerable<Character> ToCharacters(this Geometry geometry, Pen stroke) => Character.FromGeometry(geometry, null, stroke);
        public static IEnumerable<Character> ToCharacters(this Geometry geometry, Brush fill, Pen stroke) => Character.FromGeometry(geometry, fill, stroke);

        public static IEnumerable<Character> ToCharacters(this Canvas canvas) => Character.FromCanvas(canvas);

        public static IEnumerable<Character> PreventSelection(this IEnumerable<Character> characters)
        {
            foreach (var character in characters)
            {
                character.IsSelectable = false;
                yield return character;
            }
        }

        public static IEnumerable<Character> Color(this IEnumerable<Character> characters, Brush fill, Pen stroke) => characters.Select(character => character.Color(fill, stroke));
        public static IEnumerable<Character> Color(this IEnumerable<Character> characters, Brush fill) => characters.Select(character => character.Color(fill));
        public static IEnumerable<Character> Color(this IEnumerable<Character> characters, Pen stroke) => characters.Select(character => character.Color(stroke));

        public static Character Color(this Character character, Brush fill, Pen stroke)
        {
            if (character != null)
            {
                character.Fill = fill;
                character.Stroke = stroke;
            }
            return character;
        }
        public static Character Color(this Character character, Brush fill)
        {
            if (character != null) character.Fill = fill;
            return character;
        }
        public static Character Color(this Character character, Pen stroke)
        {
            if (character != null) character.Stroke = stroke;
            return character;
        }
    }

    public class PositionCharactersEqualityComparer : EqualityComparer<Character>
    {
        public override bool Equals(Character x, Character y) => x.Index == y.Index && x.Owner == y.Owner;
        public override int GetHashCode(Character obj)
        {
            int hashCode = 286831327;
            hashCode = hashCode * -1521134295 + EqualityComparer<VisualObject>.Default.GetHashCode(obj.Owner);
            hashCode = hashCode * -1521134295 + obj.Index.GetHashCode();
            return hashCode;
        }
    }
}
