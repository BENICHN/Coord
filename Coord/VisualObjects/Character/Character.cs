using BenLib;
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
        public Character(Geometry geometry)
        {
            Geometry = geometry;
            //StrokeLength = geometry.StrokeLength();
        }
        public Character(Geometry geometry, Brush fill, Pen stroke) : this(geometry)
        {
            Fill = fill;
            Stroke = stroke;
        }
        private Character(Geometry geometry, Brush fill, Pen stroke, /*double strokeLength,*/ Matrix matrix)
        {
            Geometry = geometry;
            Fill = fill;
            Stroke = stroke;
            //StrokeLength = strokeLength;
            Matrix = matrix;
        }

        /// <summary>
        /// Crée un une copie des valeurs de l'instance actuelle de <see cref="Character"/>
        /// </summary>
        /// <returns>Copie des valeurs de l'instance actuelle</returns>
        public Character Clone() => new Character(Geometry?.CloneCurrentValue(), Fill?.CloneCurrentValue(), Stroke?.CloneCurrentValue(), /*StrokeLength,*/ Matrix);

        /// <summary>
        /// Remplissage de <see cref="Geometry"/>
        /// </summary>
        public Brush Fill { get; set; }

        /// <summary>
        /// Contour de <see cref="Geometry"/>
        /// </summary>
        public Pen Stroke { get; set; }

        /// <summary>
        /// Longueur du contour de <see cref="Geometry"/>
        /// </summary>
        //public double StrokeLength { get; }

        /// <summary>
        /// Transformation qui va être appliquée à <see cref="Geometry"/>
        /// </summary>
        public Matrix Matrix { get; set; } = new Matrix();

        /// <summary>
        /// Indique si <see cref="Geometry"/> est actuellement transformée par <see cref="Matrix"/>
        /// </summary>
        public bool Transformed { get; private set; }

        /// <summary>
        /// Géométrie à dessiner
        /// </summary>
        public Geometry Geometry { get; }

        /// <summary>
        /// Ajoute une translation des offsets spécifiés à <see cref="Matrix"/>
        /// </summary>
        /// <param name="offsetX">Décalage sur l'axe des abscisses</param>
        /// <param name="offsetY">Décalage sur l'axe des ordonnées</param>
        public void Translate(Vector offset)
        {
            var matrix = Matrix;
            matrix.Translate(offset.X, offset.Y);
            Matrix = matrix;
        }

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
            Transformed = true;
        }

        public override string ToString() => $"{Matrix}, {Geometry}";

        /// <summary>
        /// Obtient une collection de <see cref="Character"/> à partir d'une <see cref="System.Windows.Media.Geometry"/> éventuellement d'un <see cref="GeometryGroup"/>
        /// </summary>
        /// <param name="geometry"><see cref="System.Windows.Media.Geometry"/> utilisée</param>
        /// <returns>Collection de <see cref="Character"/> obtenue à partir de la <see cref="System.Windows.Media.Geometry"/></returns>
        public static IEnumerable<Character> FromGeometry(Geometry geometry)
        {
            if (geometry is GeometryGroup geometryGroup) { foreach (var c in geometryGroup.Children.SelectMany(g => FromGeometry(g))) yield return c; }
            else yield return new Character(geometry);
        }

        /// <summary>
        /// Obtient une collection de <see cref="Character"/> à partir d'une collection de <see cref="System.Windows.Media.Geometry"/> éventuellement de <see cref="GeometryGroup"/>
        /// </summary>
        /// <param name="geometries">Collection de <see cref="System.Windows.Media.Geometry"/> utilisée</param>
        /// <returns>Collection de <see cref="Character"/> obtenue à partir de la <see cref="System.Windows.Media.Geometry"/></returns>
        public static IEnumerable<Character> FromGeometry(IEnumerable<Geometry> geometries) => geometries.SelectMany(g => FromGeometry(g));

        public static IEnumerable<Character> FromCanvas(Canvas canvas)
        {
            foreach (var c in canvas.Children.OfType<Canvas>().SelectMany(ca => FromCanvas(ca))) yield return c;
            foreach (var shape in canvas.Children.OfType<Shape>())
            {
                var geometry = shape.ToGeometry();
                var transform = shape.RenderTransform.Value;
                var result = new Character(geometry, shape.Fill, new Pen(shape.Stroke, shape.StrokeThickness) { DashCap = shape.StrokeDashCap, DashStyle = new DashStyle(shape.StrokeDashArray, shape.StrokeDashOffset), EndLineCap = shape.StrokeEndLineCap, LineJoin = shape.StrokeLineJoin, MiterLimit = shape.StrokeMiterLimit, StartLineCap = shape.StrokeStartLineCap });
                result.Matrix *= transform;
                yield return result;
            }
        }

        public static Character Line(Point startPoint, Point endPoint, Brush brush, Pen pen) => new Character(new LineGeometry(startPoint, endPoint), brush, pen);
        public static Character Ellipse(Point center, double radiusX, double radiusY, Brush brush, Pen pen) => new Character(new EllipseGeometry(center, radiusX, radiusY), brush, pen);
        public static Character Ellipse(Rect rect, Brush brush, Pen pen) => new Character(new EllipseGeometry(rect), brush, pen);
        public static Character Rectangle(Rect rect, Brush brush, Pen pen) => new Character(new RectangleGeometry(rect), brush, pen);
        public static Character Rectangle(Rect rect, double radiusX, double radiusY, Brush brush, Pen pen) => new Character(new RectangleGeometry(rect, radiusX, radiusY), brush, pen);
        public static Character Arc(Point startPoint, Point endPoint, Size size, double rotationAngle, bool isLargeArc, SweepDirection sweepDirection, Brush brush, Pen pen) => new Character(new PathGeometry(new[] { new PathFigure(startPoint, new[] { new ArcSegment(endPoint, size, rotationAngle, isLargeArc, sweepDirection, true) }, false) }), brush, pen);
    }

    public static partial class Extensions
    {
        /// <summary>
        /// Clone tous les <see cref="Character"/> d'une collection puis retourne le résultat
        /// </summary>
        /// <param name="characters">Collection qui contient les <see cref="Character"/> à cloner</param>
        /// <returns>Collection contenant les <see cref="Character"/> clonés</returns>
        public static IEnumerable<Character> CloneCharacters(this IEnumerable<Character> characters) => characters.Select(c => c.Clone());

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
                character.Translate(offset * p);
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
    }
}
