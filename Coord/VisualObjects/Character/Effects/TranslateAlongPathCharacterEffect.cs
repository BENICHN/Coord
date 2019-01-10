using BenLib;
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
        private PathGeometry m_geometry;
        private Func<Point, Vector> m_translation;

        public TranslateAlongPathCharacterEffect(IntInterval interval, Geometry geometry, Func<Point, Vector> translation, Progress progress, params SynchronizedProgress[] synchronizedProgresses) : this(interval, PathGeometry.CreateFromGeometry(geometry), translation, progress, false, synchronizedProgresses) { }
        public TranslateAlongPathCharacterEffect(IntInterval interval, Geometry geometry, Func<Point, Vector> translation, Progress progress, bool withTransforms, params SynchronizedProgress[] synchronizedProgresses) : this(interval, PathGeometry.CreateFromGeometry(geometry), translation, progress, withTransforms, synchronizedProgresses) { }
        public TranslateAlongPathCharacterEffect(IntInterval interval, PathGeometry geometry, Func<Point, Vector> translation, Progress progress, params SynchronizedProgress[] synchronizedProgresses) : this(interval, geometry, translation, progress, false, synchronizedProgresses) { }
        public TranslateAlongPathCharacterEffect(IntInterval interval, PathGeometry geometry, Func<Point, Vector> translation, Progress progress, bool withTransforms, params SynchronizedProgress[] synchronizedProgresses) : base(interval, progress, withTransforms, synchronizedProgresses)
        {
            Geometry = PathGeometry.CreateFromGeometry(geometry);
            Translation = translation;
        }

        /// <summary>
        /// <see cref="PathGeometry"/> sur lequel translater les <see cref="Character"/> de la sous-collection
        /// </summary>
        public PathGeometry Geometry
        {
            get => m_geometry;
            set
            {
                UnRegister(m_geometry);
                m_geometry = value;
                Register(m_geometry);
                NotifyChanged();
            }
        }

        /// <summary>
        /// Méthode permettant d'obtenir le vecteur de translation à partir d'un point de <see cref="Geometry"/>
        /// </summary>
        public Func<Point, Vector> Translation
        {
            get => m_translation;
            set
            {
                m_translation = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Applique l'effet à une collection de <see cref="Character"/>
        /// </summary>
        /// <param name="characters">Collection de <see cref="Character"/> sur qui appliquer l'effet</param>
        protected override void ApplyCore(IReadOnlyCollection<Character> characters, CoordinatesSystemManager coordinatesSystemManager)
        {
            var chars = characters.SubCollection(Interval);
            Geometry.GetPointAtFractionLength(EasedProgress.Value, out var point, out var tangent);
            var translation = Translation(point);
            chars.Translate(translation, 1.0).Enumerate();
        }

        /// <summary>
        /// Crée un une copie des valeurs de l'instance actuelle de <see cref="TranslateAlongPathCharacterEffect"/>
        /// </summary>
        /// <returns>Copie des valeurs de l'instance actuelle</returns>
        public override CharacterEffect Clone() => new TranslateAlongPathCharacterEffect(Interval, Geometry, Translation, Progress, WithTransforms);
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
