using BenLib.Standard;
using BenLib.WPF;
using System;
using System.Collections.Generic;
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
        public Character(Geometry geometry) => Geometry = geometry;
        public Character(Geometry geometry, Brush fill, PlanePen stroke) : this(geometry)
        {
            Fill = fill;
            Stroke = stroke;
        }
        private Character(VisualObject owner, VisualObject creator, int index, Geometry geometry, Brush fill, PlanePen stroke, Matrix matrix, bool transformed)
        {
            Owner = owner;
            Creator = creator;
            Index = index;
            Geometry = geometry;
            Fill = fill;
            Stroke = stroke;
            Matrix = matrix;
            if (transformed) ApplyTransforms();
        }

        /// <summary>
        /// Crée un une copie des valeurs de l'instance actuelle de <see cref="Character"/>
        /// </summary>
        /// <returns>Copie des valeurs de l'instance actuelle</returns>
        public Character Clone() => new Character(Owner, Creator, Index, Geometry?.CloneCurrentValue(), Fill?.CloneCurrentValue(), Stroke?.CloneCurrentValue(), Matrix, Transformed);
        public Character Clone(VisualObject owner, int index) => new Character(owner, Creator ?? Owner, index, Geometry?.CloneCurrentValue(), Fill?.CloneCurrentValue(), Stroke?.CloneCurrentValue(), Matrix, Transformed);
        public Character Attach(VisualObject owner, int index) => new Character(owner, Creator ?? owner, index, Geometry, Fill, Stroke, Matrix, Transformed);

        /// <summary>
        /// Remplissage de <see cref="Geometry"/>
        /// </summary>
        public Brush Fill { get; set; }

        /// <summary>
        /// Contour de <see cref="Geometry"/>
        /// </summary>
        public PlanePen Stroke { get; set; }

        /// <summary>
        /// Transformation qui va être appliquée à <see cref="Geometry"/>
        /// </summary>
        public Matrix Matrix = new Matrix();

        /// <summary>
        /// Indique si <see cref="Geometry"/> est actuellement transformée par <see cref="Matrix"/>
        /// </summary>
        public bool Transformed { get; private set; }

        /// <summary>
        /// Géométrie à dessiner
        /// </summary>
        public Geometry Geometry { get; }

        public VisualObject Owner { get; }
        public VisualObject Creator { get; set; }

        public int Index { get; }

        public bool IsSelected
        {
            get => Owner?.Selection >= Index;
            set { if (Owner != null) Owner.Selection = value ? Owner.Selection | Interval<int>.Single(Index) : Owner.Selection / Interval<int>.Single(Index); }
        }

        public event PropertyChangedExtendedEventHandler<bool> IsSelectedChanged;
        internal void NotifyIsSelectedChanged() { bool newValue = IsSelected; IsSelectedChanged?.Invoke(this, new PropertyChangedExtendedEventArgs<bool>("IsSelected", !newValue, newValue)); }

        /*/// <summary>
        /// Ajoute une translation des offsets spécifiés à <see cref="Matrix"/>
        /// </summary>
        /// <param name="offsetX">Décalage sur l'axe des abscisses</param>
        /// <param name="offsetY">Décalage sur l'axe des ordonnées</param>
        public void Translate(Vector offset)
        {
            var matrix = Matrix;
            matrix.Translate(offset.X, offset.Y);
            Matrix = matrix;
        }*/

        /// <summary>
        /// Ajoute le vecteur d'échelle spécifié à <see cref="Matrix"/>
        /// </summary>
        /// <param name="scaleX">Valeur d'échelle sur l'axe des abscisses</param>
        /// <param name="scaleY">Valeur d'échelle sur l'axe des ordonnées</param>
        public void Scale(double scaleX, double scaleY)
        {
            var matrix = Matrix;
            matrix.Scale(scaleX, scaleY);
            Matrix = matrix;
        }

        /// <summary>
        /// Met à l'échelle <see cref="Matrix"/> selon la valeur spécifiée au point spécifié
        /// </summary>
        /// <param name="scaleX">Valeur d'échelle sur l'axe des abscisses</param>
        /// <param name="scaleY">Valeur d'échelle sur l'axe des ordonnées</param>
        /// <param name="centerX">Coordonnée x du point central de mise à l'échelle</param>
        /// <param name="centerY">Coordonnée y du point central de mise à l'échelle</param>
        public void ScaleAt(double scaleX, double scaleY, Point center)
        {
            var matrix = Matrix;
            matrix.ScaleAt(scaleX, scaleY, center.X, center.Y);
            Matrix = matrix;
        }

        public void Rotate(double angle)
        {
            var matrix = Matrix;
            matrix.Rotate(angle * 180 / Math.PI);
            Matrix = matrix;
        }

        public void RotateAt(double angle, Point center)
        {
            var matrix = Matrix;
            matrix.RotateAt(angle * 180 / Math.PI, center.X, center.Y);
            Matrix = matrix;
        }

        /// <summary>
        /// Transforme <see cref="Geometry"/> par <see cref="Matrix"/>
        /// </summary>
        public void ApplyTransforms()
        {
            Geometry.Transform = new MatrixTransform(Matrix);
            Transformed = true;
        }

        /// <summary>
        /// Supprime les transformations de <see cref="Geometry"/>
        /// </summary>
        public void ReleaseTransforms()
        {
            Geometry.Transform = null;
            Transformed = false;
        }

        public override string ToString() => ToString(false);
        public string ToString(bool detail) => "{" + Matrix + "} => " + (detail ? Geometry.ToString() : Geometry.GetType().Name);

        /// <summary>
        /// Obtient une collection de <see cref="Character"/> à partir d'une <see cref="System.Windows.Media.Geometry"/> éventuellement d'un <see cref="GeometryGroup"/>
        /// </summary>
        /// <param name="geometry"><see cref="System.Windows.Media.Geometry"/> utilisée</param>
        /// <returns>Collection de <see cref="Character"/> obtenue à partir de la <see cref="System.Windows.Media.Geometry"/></returns>
        public static IEnumerable<Character> FromGeometry(Geometry geometry, Brush fill = null, PlanePen stroke = null)
        {
            if (geometry is GeometryGroup geometryGroup) { foreach (var c in geometryGroup.Children.SelectMany(g => FromGeometry(g, fill, stroke))) yield return c; }
            else yield return new Character(geometry, fill, stroke);
        }

        /// <summary>
        /// Obtient une collection de <see cref="Character"/> à partir d'une collection de <see cref="System.Windows.Media.Geometry"/> éventuellement de <see cref="GeometryGroup"/>
        /// </summary>
        /// <param name="geometries">Collection de <see cref="System.Windows.Media.Geometry"/> utilisée</param>
        /// <returns>Collection de <see cref="Character"/> obtenue à partir de la <see cref="System.Windows.Media.Geometry"/></returns>
        public static IEnumerable<Character> FromGeometry(IEnumerable<Geometry> geometries, Brush fill = null, PlanePen stroke = null) => geometries.SelectMany(g => FromGeometry(g, fill, stroke));

        public static IEnumerable<Character> FromCanvas(Canvas canvas)
        {
            foreach (var c in canvas.Children.OfType<Canvas>().SelectMany(ca => FromCanvas(ca))) yield return c;
            foreach (var shape in canvas.Children.OfType<Shape>())
            {
                var geometry = shape.ToGeometry();
                var transform = shape.RenderTransform.Value;
                var result = new Character(geometry, shape.Fill, new PlanePen(new Pen(shape.Stroke, shape.StrokeThickness) { DashCap = shape.StrokeDashCap, DashStyle = new DashStyle(shape.StrokeDashArray, shape.StrokeDashOffset), EndLineCap = shape.StrokeEndLineCap, LineJoin = shape.StrokeLineJoin, MiterLimit = shape.StrokeMiterLimit, StartLineCap = shape.StrokeStartLineCap }, false));
                result.Matrix *= transform;
                yield return result;
            }
        }

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
                bool fill = character.Fill != null && character.Fill.Opacity > 0 && character.Fill != Brushes.Transparent;
                bool transformed = character.Transformed;
                if (!transformed) character.ApplyTransforms();
                var result = rectangle.FillContainsWithDetail(character.Geometry);
                if (result != IntersectionDetail.Empty && result != IntersectionDetail.NotCalculated && (fill || result != IntersectionDetail.FullyInside)) yield return character;
                if (!transformed) character.ReleaseTransforms();
            }
        }

        public static IEnumerable<Character> HitTest(this IEnumerable<Character> characters, Point point)
        {
            foreach (var character in characters)
            {
                bool fill = character.Fill != null && character.Fill.Opacity > 0 && character.Fill != Brushes.Transparent;
                bool stroke = !character.Stroke.IsNull() && character.Stroke.Thickness > 0 && character.Stroke.Brush != null && character.Stroke.Brush.Opacity > 0 && character.Stroke.Brush != Brushes.Transparent;
                bool transformed = character.Transformed;
                if (!transformed) character.ApplyTransforms();
                if (fill && character.Geometry.FillContains(point) || stroke && character.Geometry.StrokeContains(character.Stroke, point)) yield return character;
                if (!transformed) character.ReleaseTransforms();
            }
        }

        /// <summary>
        /// Clone tous les <see cref="Character"/> d'une collection puis retourne le résultat
        /// </summary>
        /// <param name="characters">Collection qui contient les <see cref="Character"/> à cloner</param>
        /// <returns>Collection contenant les <see cref="Character"/> clonés</returns>
        public static IEnumerable<Character> CloneCharacters(this IEnumerable<Character> characters) => characters.Select(c => c.Clone());
        public static IEnumerable<Character> CloneCharacters(this IEnumerable<Character> characters, VisualObject owner) => characters.Select((c, i) => c.Clone(owner, i));
        public static IEnumerable<Character> AttachCharacters(this IEnumerable<Character> characters, VisualObject owner) => characters.Select((c, i) => c.Attach(owner, i));

        public static void Transform(this IEnumerable<Character> characters, Matrix matrix, bool replace) { foreach (var character in characters) character.Matrix = replace ? matrix : character.Matrix * matrix; }

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
                character.Matrix.Translate(vector.X, vector.Y);
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
                character.ScaleAt(1 + (scaleX - 1) * p, 1 + (scaleY - 1) * p, center);
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
                character.ScaleAt(1 + (scaleX - 1) * p, 1 + (scaleY - 1) * p, center);
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
                character.RotateAt(angle * p, center);
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
                character.RotateAt(angle * p, center);
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

        public static Character ToCharacter(this Geometry geometry, Brush fill, PlanePen stroke) => new Character(geometry, fill, stroke);
        public static Character ToCharacter(this Geometry geometry, PlanePen stroke) => new Character(geometry, null, stroke);
        public static Character ToCharacter(this Geometry geometry, Brush fill) => new Character(geometry, fill, null);
        public static Character ToCharacter(this Geometry geometry) => new Character(geometry, null, null);

        public static IEnumerable<Character> ToCharacters(this Geometry geometry) => Character.FromGeometry(geometry);
        public static IEnumerable<Character> ToCharacters(this Geometry geometry, Brush fill) => Character.FromGeometry(geometry, fill);
        public static IEnumerable<Character> ToCharacters(this Geometry geometry, PlanePen stroke) => Character.FromGeometry(geometry, null, stroke);
        public static IEnumerable<Character> ToCharacters(this Geometry geometry, Brush fill, PlanePen stroke) => Character.FromGeometry(geometry, fill, stroke);

        public static IEnumerable<Character> ToCharacters(this Canvas canvas) => Character.FromCanvas(canvas);

        public static IEnumerable<Character> Color(this IEnumerable<Character> characters, Brush fill, PlanePen stroke) => characters.Select(character => character.Color(fill, stroke));
        public static IEnumerable<Character> Color(this IEnumerable<Character> characters, Brush fill) => characters.Select(character => character.Color(fill));
        public static IEnumerable<Character> Color(this IEnumerable<Character> characters, PlanePen stroke) => characters.Select(character => character.Color(stroke));

        public static Character Color(this Character character, Brush fill, PlanePen stroke)
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
        public static Character Color(this Character character, PlanePen stroke)
        {
            if (character != null) character.Stroke = stroke;
            return character;
        }
    }
}
