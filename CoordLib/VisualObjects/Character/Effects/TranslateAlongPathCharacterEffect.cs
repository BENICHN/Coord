using BenLib.Standard;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Coord
{
    /// <summary>
    /// Translate progressivement une sous-collection de <see cref="Character"/> sur un <see cref="PathGeometry"/>
    /// </summary>
    public class TranslateAlongPathCharacterEffect : CharacterEffect
    {
        /// <summary>
        /// <see cref="PathGeometry"/> sur lequel translater les <see cref="Character"/> de la sous-collection
        /// </summary>
        public PathGeometry PathGeometry { get => (PathGeometry)GetValue(PathGeometryProperty); set => SetValue(PathGeometryProperty, value); }
        public static readonly DependencyProperty PathGeometryProperty = CreateProperty<PathGeometry>(true, true, "PathGeometry", typeof(TranslateAlongPathCharacterEffect));

        /// <summary>
        /// Méthode permettant d'obtenir le vecteur de translation à partir d'un point de <see cref="Geometry"/>
        /// </summary>
        public Func<Point, Vector> Translation { get => (Func<Point, Vector>)GetValue(TranslationProperty); set => SetValue(TranslationProperty, value); }
        public static readonly DependencyProperty TranslationProperty = CreateProperty<Func<Point, Vector>>(true, true, "Translation", typeof(TranslateAlongPathCharacterEffect));

        /// <summary>
        /// Applique l'effet à une collection de <see cref="Character"/>
        /// </summary>
        /// <param name="characters">Collection de <see cref="Character"/> sur qui appliquer l'effet</param>
        protected override void ApplyCore(IReadOnlyCollection<Character> characters, Interval<int> interval, in ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            var chars = characters.SubCollection(interval, true);
            PathGeometry.GetPointAtFractionLength(EasedProgress.Value, out var point, out var tangent);
            var translation = Translation(point);
            chars.Translate(translation, 1.0).Enumerate();
        }
    }

    /// <summary>
    /// Contient des méthodes statiques permettant d'obtenir rapidement le vecteur de translation d'un <see cref="TranslateAlongPathCharacterEffect"/> à partir d'un point de <see cref="TranslateAlongPathCharacterEffect.Geometry"/>
    /// </summary>
    public static class Translations
    {
        /// <summary>
        /// Obtient le vecteur ayant pour abscisse celle d'un point et pour ordonnée 0
        /// </summary>
        /// <param name="point"><see cref="Point"/> utilisé</param>
        /// <returns>Vecteur ayant pour abscisse celle du point et pour ordonnée 0</returns>
        public static Vector X(Point point) => new Vector(point.X, 0);

        /// <summary>
        /// Obtient le vecteur ayant pour abscisse 0 et pour ordonnée celle d'un point
        /// </summary>
        /// <param name="point"><see cref="Point"/> utilisé</param>
        /// <returns>Vecteur ayant pour abscisse 0 et pour ordonnée celle du point</returns>
        public static Vector Y(Point point) => new Vector(0, point.Y);

        /// <summary>
        /// Obtient le vecteur ayant pour abscisse l'opposé de celle d'un point et pour ordonnée 0
        /// </summary>
        /// <param name="point"><see cref="Point"/> utilisé</param>
        /// <returns>Vecteur ayant pour abscisse l'opposé de celle du point et pour ordonnée 0</returns>
        public static Vector MinusX(Point point) => new Vector(-point.X, 0);

        /// <summary>
        /// Obtient le vecteur ayant pour abscisse 0 et pour ordonnée l'opposé de celle d'un point
        /// </summary>
        /// <param name="point"><see cref="Point"/> utilisé</param>
        /// <returns>Vecteur ayant pour abscisse 0 et pour ordonnée l'opposé de celle du point</returns>
        public static Vector MinusY(Point point) => new Vector(0, -point.Y);
    }
}
