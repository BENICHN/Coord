using BenLib;
using BenLib.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using static BenLib.Animating;
using static BenLib.IntInterval;
using static Coord.MainWindow;

namespace Coord
{
    /// <summary>
    /// Contient des méthodes statiques permettant de créer et modifier rapidement des <see cref="VisualObject"/>
    /// </summary>
    public static class VisualObjects
    {
        #region PointVisualObject

        public static PointVisualObject Point(double x, double y) => Point(new Point(x, y));
        public static PointVisualObject Point(Point inPoint) => new PointVisualObject(new PointPointDefinition(inPoint));
        public static PointVisualObject Point(PointVisualObject point, Func<Point, Point> operations) => new PointVisualObject(new OperationsPointDefinition(point, operations));
        public static PointVisualObject Point(NotifyObjectCollection<PointVisualObject> points, Func<Point[], Point> operations) => new PointVisualObject(new MultiOperationsPointDefinition(points, operations));
        public static PointVisualObject Point(IEnumerable<PointVisualObject> points, Func<Point[], Point> operations) => new PointVisualObject(new MultiOperationsPointDefinition(points, operations));
        public static PointVisualObject MiddlePoint(PointVisualObject pointA, PointVisualObject pointB) => new PointVisualObject(new MiddlePointDefinition(pointA, pointB));
        public static PointVisualObject LineIntersectionPoint(LineVisualObject lineA, LineVisualObject lineB) => new PointVisualObject(new LineIntersectionPointDefinition(lineA, lineB));
        public static PointVisualObject Translation(PointVisualObject point, VectorVisualObject vector) => new PointVisualObject(new TranslationPointDefinition(point, vector));

        public static PointVisualObject Extend(this PointVisualObject pointVisualObject, double? radius = null)
        {
            if (radius.HasValue) pointVisualObject.Radius = radius.Value;
            return pointVisualObject;
        }

        #endregion

        #region VectorVisualObject

        public static VectorVisualObject Vector(PointVisualObject inAnchorPoint, double x, double y) => Vector(inAnchorPoint, new Vector(x, y));
        public static VectorVisualObject Vector(PointVisualObject inAnchorPoint, Vector inVector) => new VectorVisualObject(inAnchorPoint, new VectorVectorDefinition(inVector));
        public static VectorVisualObject Vector(PointVisualObject inAnchorPoint, PointVisualObject pointA, PointVisualObject pointB) => new VectorVisualObject(inAnchorPoint, new PointPointVectorDefinition(pointA, pointB));
        public static VectorVisualObject Vector(PointVisualObject inAnchorPoint, VectorVisualObject vector, Func<Vector, Vector> operations) => new VectorVisualObject(inAnchorPoint, new OperationsVectorDefinition(vector, operations));
        public static VectorVisualObject Vector(PointVisualObject inAnchorPoint, NotifyObjectCollection<VectorVisualObject> vectors, Func<Vector[], Vector> operations) => new VectorVisualObject(inAnchorPoint, new MultiOperationsVectorDefinition(vectors, operations));
        public static VectorVisualObject Vector(PointVisualObject inAnchorPoint, IEnumerable<VectorVisualObject> vectors, Func<Vector[], Vector> operations) => new VectorVisualObject(inAnchorPoint, new MultiOperationsVectorDefinition(vectors, operations));

        public static VectorVisualObject Extend(this VectorVisualObject vectorVisualObject, PointVisualObject inAnchorPoint = null, Arrow arrow = null, ArrowEnd? arrowEnd = null)
        {
            if (inAnchorPoint != null) vectorVisualObject.InAnchorPoint = inAnchorPoint;
            if (arrow != null) vectorVisualObject.Arrow = arrow;
            if (arrowEnd.HasValue) vectorVisualObject.ArrowEnd = arrowEnd.Value;
            return vectorVisualObject;
        }

        #endregion

        #region CircleVisualObject

        public static CircleVisualObject Circle(PointVisualObject center, double radius) => new CircleVisualObject(new CenterRadiusCircleDefinition(center, radius));
        public static CircleVisualObject Circle(PointVisualObject center, PointVisualObject point) => new CircleVisualObject(new CenterPointCircleDefinition(center, point));

        #endregion

        #region CurveVisualObject

        public static CurveVisualObject Curve(Series series, bool closed, bool smooth, double smoothValue = 0.75) => new CurveVisualObject(series, closed, smooth, smoothValue);

        #endregion

        #region MorphingVisualObject

        public static OutMorphingVisualObject OutMorphing(PointVisualObject inAnchorPoint, IReadOnlyCollection<Character> from, IReadOnlyCollection<Character> to, CorrespondanceDictionary correspondances, params SynchronizedProgress[] synchronizedProgresses) => new OutMorphingVisualObject(inAnchorPoint, from, to, correspondances, synchronizedProgresses);
        public static InMorphingVisualObject InMorphing(VisualObject from, VisualObject to, CorrespondanceDictionary correspondances, params SynchronizedProgress[] synchronizedProgresses) => new InMorphingVisualObject(from, to, correspondances, synchronizedProgresses);

        #endregion

        #region LineVisualObject

        public static LineVisualObject Line(LinearEquation equation) => new LineVisualObject(new EquationLineDefinition(equation));
        public static LineVisualObject Line(PointVisualObject pointA, PointVisualObject pointB) => new LineVisualObject(new PointPointLineDefinition(pointA, pointB));
        public static LineVisualObject Line(PointVisualObject point, VectorVisualObject vector) => new LineVisualObject(new PointVectorLineDefinition(point, vector));

        public static LineVisualObject ParallelLine(PointVisualObject point, LineVisualObjectBase line) => new LineVisualObject(new ParallelLineDefinition(point, line));
        public static LineVisualObject PerpendicularLine(PointVisualObject point, LineVisualObjectBase line) => new LineVisualObject(new PerpendicularLineDefinition(point, line));
        public static LineVisualObject PerpendicularBisectorLine(PointVisualObject pointA, PointVisualObject pointB) => new LineVisualObject(new PerpendicularBisectorLineDefinition(pointA, pointB));

        #endregion

        #region SegmentVisualObject

        public static SegmentVisualObject Segment(PointVisualObject start, PointVisualObject end) => new SegmentVisualObject(new PointPointSegmentDefinition(start, end));

        #endregion

        #region PolygonVisualObject

        public static PolygonVisualObject Polygon(params PointVisualObject[] inPoints) => new PolygonVisualObject(new PointsPolygonDefinition(inPoints));
        public static PolygonVisualObject Polygon(IEnumerable<PointVisualObject> inPoints) => new PolygonVisualObject(new PointsPolygonDefinition(inPoints));
        public static PolygonVisualObject Polygon(NotifyObjectCollection<PointVisualObject> inPoints) => new PolygonVisualObject(new PointsPolygonDefinition(inPoints));

        public static PolygonVisualObject RegularPolygon(double sideLength, int sideCount) => new PolygonVisualObject(new RegularPolygonDefinition(sideLength, sideCount));

        #endregion

        #region TextVisualObject

        public static TextVisualObject InTex(string text, double scale, PointVisualObject inAnchorPoint = null, RectPoint rectPoint = default) => new TextVisualObject(text, scale, true, inAnchorPoint, rectPoint);
        public static TextVisualObject InText(string text, double scale, PointVisualObject inAnchorPoint = null, RectPoint rectPoint = default, Typeface typeface = null) => new TextVisualObject(text, scale, true, inAnchorPoint, rectPoint, typeface);

        public static TextVisualObject OutTex(string text, double scale, PointVisualObject inAnchorPoint = null, RectPoint rectPoint = default) => new TextVisualObject(text, scale, false, inAnchorPoint, rectPoint);
        public static TextVisualObject OutText(string text, double scale, PointVisualObject inAnchorPoint = null, RectPoint rectPoint = default, Typeface typeface = null) => new TextVisualObject(text, scale, false, inAnchorPoint, rectPoint, typeface);

        #endregion

        #region VisualObjectGroup

        public static VisualObjectGroup Group(NotifyObjectCollection<VisualObject> children) => new VisualObjectGroup(children);
        public static VisualObjectGroup Group(params VisualObject[] children) => new VisualObjectGroup(children);
        public static VisualObjectGroup Group(IEnumerable<VisualObject> children) => new VisualObjectGroup(children);

        #endregion

        #region CharactersVisualObject

        public static InCharactersVisualObject InCharacters(VisualObject visualObject, IntInterval interval) => new InCharactersVisualObject(visualObject, interval);
        public static CharactersVisualObject Characters(PointVisualObject inAnchorPoint, RectPoint rectPoint, bool inText, IEnumerable<Character> characters, IntInterval interval) => new CharactersVisualObject(inAnchorPoint, rectPoint, inText, characters.SubCollection(interval));

        #endregion

        #region CharacterEffects

        #region Create

        public static T Hide<T>(this T visualObject, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject => Hide(visualObject, NSet, 1, synchronizedProgresses);
        public static T Hide<T>(this T visualObject, IntInterval interval, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject => Hide(visualObject, interval, 1, synchronizedProgresses);
        public static T Hide<T>(this T visualObject, IntInterval interval, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        {
            visualObject.Effects.Add(new ColorCharacterEffect(interval, null, null, false, progress, synchronizedProgresses));
            return visualObject;
        }
        public static T Hide<T>(this T visualObject, IntInterval interval, Progress progress, bool withTransforms, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        {
            visualObject.Effects.Add(new ColorCharacterEffect(interval, null, null, false, progress, withTransforms, synchronizedProgresses));
            return visualObject;
        }

        public static T Color<T>(this T visualObject, Brush fill, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject => Color(visualObject, NSet, fill, null, false, 1, synchronizedProgresses);
        public static T Color<T>(this T visualObject, Pen stroke, bool inPen, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject => Color(visualObject, NSet, null, stroke, inPen, 1, synchronizedProgresses);
        public static T Color<T>(this T visualObject, Brush fill, Pen stroke, bool inPen, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject => Color(visualObject, NSet, fill, stroke, inPen, 1, synchronizedProgresses);
        public static T Color<T>(this T visualObject, Brush fill, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject => Color(visualObject, NSet, fill, null, false, progress, synchronizedProgresses);
        public static T Color<T>(this T visualObject, Pen stroke, bool inPen, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject => Color(visualObject, NSet, null, stroke, inPen, progress, synchronizedProgresses);
        public static T Color<T>(this T visualObject, Brush fill, Pen stroke, bool inPen, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject => Color(visualObject, NSet, fill, stroke, inPen, progress, synchronizedProgresses);
        public static T Color<T>(this T visualObject, IntInterval interval, Brush fill, Pen stroke, bool inPen, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject => Color(visualObject, interval, fill, stroke, inPen, 1, synchronizedProgresses);
        public static T Color<T>(this T visualObject, IntInterval interval, Brush fill, Pen stroke, bool inPen, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        {
            visualObject.Effects.Add(new ColorCharacterEffect(interval, fill, stroke, inPen, progress, synchronizedProgresses));
            return visualObject;
        }
        public static T Color<T>(this T visualObject, IntInterval interval, Brush fill, Pen stroke, bool inPen, Progress progress, bool withTransforms, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        {
            visualObject.Effects.Add(new ColorCharacterEffect(interval, fill, stroke, inPen, progress, withTransforms, synchronizedProgresses));
            return visualObject;
        }
        public static T Color<T>(this T visualObject, ColorCharacterEffect colorCharacterEffect) where T : VisualObject
        {
            visualObject.Effects.Add(colorCharacterEffect);
            return visualObject;
        }

        public static T Fit<T>(this T visualObject, IntInterval interval, IntInterval boundsInterval, VisualObject text, IntInterval textInterval, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        {
            visualObject.Effects.Add(new FitCharacterEffect(interval, boundsInterval, text, textInterval, progress, synchronizedProgresses));
            return visualObject;
        }
        public static T Fit<T>(this T visualObject, IntInterval interval, IntInterval boundsInterval, VisualObject text, IntInterval textInterval, Progress progress, bool withTransforms, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        {
            visualObject.Effects.Add(new FitCharacterEffect(interval, boundsInterval, text, textInterval, progress, withTransforms, synchronizedProgresses));
            return visualObject;
        }
        public static T Fit<T>(this T visualObject, IntInterval interval, IntInterval boundsInterval, VisualObject text, IntInterval textInterval, Progress progress, bool scaleX, bool scaleY, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        {
            visualObject.Effects.Add(new FitCharacterEffect(interval, boundsInterval, text, textInterval, progress, scaleX, scaleY, synchronizedProgresses));
            return visualObject;
        }
        public static T Fit<T>(this T visualObject, IntInterval interval, IntInterval boundsInterval, VisualObject text, IntInterval textInterval, Progress progress, bool withTransforms, bool scaleX, bool scaleY, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        {
            visualObject.Effects.Add(new FitCharacterEffect(interval, boundsInterval, text, textInterval, progress, withTransforms, scaleX, scaleY, synchronizedProgresses));
            return visualObject;
        }
        public static T Fit<T>(this T visualObject, FitCharacterEffect fitCharacterEffect) where T : VisualObject
        {
            visualObject.Effects.Add(fitCharacterEffect);
            return visualObject;
        }

        public static T Insert<T>(this T visualObject, IntInterval interval, IntInterval boundsInterval, VisualObject text, IntInterval textInterval, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        {
            visualObject.Effects.Add(new InsertCharacterEffect(interval, boundsInterval, text, textInterval, progress, synchronizedProgresses));
            return visualObject;
        }
        public static T Insert<T>(this T visualObject, IntInterval interval, IntInterval boundsInterval, VisualObject text, IntInterval textInterval, Progress progress, bool withTransforms, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        {
            visualObject.Effects.Add(new InsertCharacterEffect(interval, boundsInterval, text, textInterval, progress, withTransforms, synchronizedProgresses));
            return visualObject;
        }
        public static T Insert<T>(this T visualObject, IntInterval interval, IntInterval boundsInterval, VisualObject text, IntInterval textInterval, Progress progress, bool translateX, bool translateY, RectPoint rectPoint, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        {
            visualObject.Effects.Add(new InsertCharacterEffect(interval, boundsInterval, text, textInterval, progress, translateX, translateY, rectPoint, synchronizedProgresses));
            return visualObject;
        }
        public static T Insert<T>(this T visualObject, IntInterval interval, IntInterval boundsInterval, VisualObject text, IntInterval textInterval, Progress progress, bool withTransforms, bool translateX, bool translateY, RectPoint rectPoint, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        {
            visualObject.Effects.Add(new InsertCharacterEffect(interval, boundsInterval, text, textInterval, progress, withTransforms, translateX, translateY, rectPoint, synchronizedProgresses));
            return visualObject;
        }
        public static T Insert<T>(this T visualObject, InsertCharacterEffect insertCharacterEffect) where T : VisualObject
        {
            visualObject.Effects.Add(insertCharacterEffect);
            return visualObject;
        }

        public static T Align<T>(this T visualObject, IntInterval interval, VisualObject text, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        {
            visualObject.Effects.Add(new AlignCharacterEffect(interval, text, progress, synchronizedProgresses));
            return visualObject;
        }
        public static T Align<T>(this T visualObject, IntInterval interval, VisualObject text, Progress progress, bool withTransforms, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        {
            visualObject.Effects.Add(new AlignCharacterEffect(interval, text, progress, withTransforms, synchronizedProgresses));
            return visualObject;
        }
        public static T Align<T>(this T visualObject, IntInterval interval, VisualObject text, Progress progress, bool translateX, bool translateY, RectPoint rectPoint, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        {
            visualObject.Effects.Add(new AlignCharacterEffect(interval, text, progress, translateX, translateY, rectPoint, synchronizedProgresses));
            return visualObject;
        }
        public static T Align<T>(this T visualObject, IntInterval interval, VisualObject text, Progress progress, bool withTransforms, bool translateX, bool translateY, RectPoint rectPoint, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        {
            visualObject.Effects.Add(new AlignCharacterEffect(interval, text, progress, withTransforms, translateX, translateY, rectPoint, synchronizedProgresses));
            return visualObject;
        }
        public static T Align<T>(this T visualObject, AlignCharacterEffect alignCharacterEffect) where T : VisualObject
        {
            visualObject.Effects.Add(alignCharacterEffect);
            return visualObject;
        }

        public static T Mask<T>(this T visualObject, IntInterval interval, Size smallSize, Size bigSize, RectPoint rectPoint, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        {
            visualObject.Effects.Add(new MaskCharacterEffect(interval, smallSize, bigSize, rectPoint, progress, synchronizedProgresses));
            return visualObject;
        }
        public static T Mask<T>(this T visualObject, IntInterval interval, Size smallSize, Size bigSize, RectPoint rectPoint, Progress progress, bool withTransforms, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        {
            visualObject.Effects.Add(new MaskCharacterEffect(interval, smallSize, bigSize, rectPoint, progress, withTransforms, synchronizedProgresses));
            return visualObject;
        }
        public static T Mask<T>(this T visualObject, MaskCharacterEffect maskCharacterEffect) where T : VisualObject
        {
            visualObject.Effects.Add(maskCharacterEffect);
            return visualObject;
        }

        public static T Opacity<T>(this T visualObject, IntInterval interval, double fillOpacity, double strokeOpacity, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        {
            visualObject.Effects.Add(new OpacityCharacterEffect(interval, fillOpacity, strokeOpacity, progress, synchronizedProgresses));
            return visualObject;
        }
        public static T Opacity<T>(this T visualObject, IntInterval interval, double fillOpacity, double strokeOpacity, Progress progress, bool withTransforms, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        {
            visualObject.Effects.Add(new OpacityCharacterEffect(interval, fillOpacity, strokeOpacity, progress, withTransforms, synchronizedProgresses));
            return visualObject;
        }
        public static T Opacity<T>(this T visualObject, OpacityCharacterEffect opacityCharacterEffect) where T : VisualObject
        {
            visualObject.Effects.Add(opacityCharacterEffect);
            return visualObject;
        }

        public static T Size<T>(this T visualObject, IntInterval interval, Size newSize, RectPoint rectPoint, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        {
            visualObject.Effects.Add(new SizeCharacterEffect(interval, newSize, rectPoint, progress, synchronizedProgresses));
            return visualObject;
        }
        public static T Size<T>(this T visualObject, IntInterval interval, Size newSize, RectPoint rectPoint, Progress progress, bool scaleX, bool scaleY, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        {
            visualObject.Effects.Add(new SizeCharacterEffect(interval, newSize, rectPoint, progress, scaleX, scaleY, synchronizedProgresses));
            return visualObject;
        }
        public static T Size<T>(this T visualObject, IntInterval interval, Size newSize, RectPoint rectPoint, Progress progress, bool scaleX, bool scaleY, bool withTransforms, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        {
            visualObject.Effects.Add(new SizeCharacterEffect(interval, newSize, rectPoint, progress, scaleX, scaleY, withTransforms, synchronizedProgresses));
            return visualObject;
        }
        public static T Size<T>(this T visualObject, SizeCharacterEffect sizeCharacterEffect) where T : VisualObject
        {
            visualObject.Effects.Add(sizeCharacterEffect);
            return visualObject;
        }

        public static T StrokeThickness<T>(this T visualObject, IntInterval interval, double strokeThickness, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        {
            visualObject.Effects.Add(new StrokeThicknessCharacterEffect(interval, strokeThickness, progress, synchronizedProgresses));
            return visualObject;
        }
        public static T StrokeThickness<T>(this T visualObject, IntInterval interval, double strokeThickness, Progress progress, bool withTransforms, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        {
            visualObject.Effects.Add(new StrokeThicknessCharacterEffect(interval, strokeThickness, progress, withTransforms, synchronizedProgresses));
            return visualObject;
        }
        public static T StrokeThickness<T>(this T visualObject, StrokeThicknessCharacterEffect strokeThicknessCharacterEffect) where T : VisualObject
        {
            visualObject.Effects.Add(strokeThicknessCharacterEffect);
            return visualObject;
        }

        public static T TranslateAlongPath<T>(this T visualObject, IntInterval interval, Geometry geometry, Func<Point, Vector> translation, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        {
            visualObject.Effects.Add(new TranslateAlongPathCharacterEffect(interval, geometry, translation, progress, synchronizedProgresses));
            return visualObject;
        }
        public static T TranslateAlongPath<T>(this T visualObject, IntInterval interval, Geometry geometry, Func<Point, Vector> translation, Progress progress, bool withTransforms, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        {
            visualObject.Effects.Add(new TranslateAlongPathCharacterEffect(interval, geometry, translation, progress, withTransforms, synchronizedProgresses));
            return visualObject;
        }
        public static T TranslateAlongPath<T>(this T visualObject, IntInterval interval, PathGeometry geometry, Func<Point, Vector> translation, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        {
            visualObject.Effects.Add(new TranslateAlongPathCharacterEffect(interval, geometry, translation, progress, synchronizedProgresses));
            return visualObject;
        }
        public static T TranslateAlongPath<T>(this T visualObject, IntInterval interval, PathGeometry geometry, Func<Point, Vector> translation, Progress progress, bool withTransforms, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        {
            visualObject.Effects.Add(new TranslateAlongPathCharacterEffect(interval, geometry, translation, progress, withTransforms, synchronizedProgresses));
            return visualObject;
        }
        public static T TranslateAlongPath<T>(this T visualObject, TranslateAlongPathCharacterEffect translateAlongPathCharacterEffect) where T : VisualObject
        {
            visualObject.Effects.Add(translateAlongPathCharacterEffect);
            return visualObject;
        }

        public static T Translate<T>(this T visualObject, IntInterval interval, Vector vector, bool inTranslation, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        {
            visualObject.Effects.Add(new TranslateCharacterEffect(interval, vector, inTranslation, progress, synchronizedProgresses));
            return visualObject;
        }
        public static T Translate<T>(this T visualObject, IntInterval interval, Vector vector, bool inTranslation, Progress progress, bool withTransforms, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        {
            visualObject.Effects.Add(new TranslateCharacterEffect(interval, vector, inTranslation, progress, withTransforms, synchronizedProgresses));
            return visualObject;
        }
        public static T Translate<T>(this T visualObject, TranslateCharacterEffect translateCharacterEffect) where T : VisualObject
        {
            visualObject.Effects.Add(translateCharacterEffect);
            return visualObject;
        }

        public static T Scale<T>(this T visualObject, IntInterval interval, double scaleX, double scaleY, RectPoint rectPoint, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        {
            visualObject.Effects.Add(new ScaleCharacterEffect(interval, scaleX, scaleY, rectPoint, progress, synchronizedProgresses));
            return visualObject;
        }
        public static T Scale<T>(this T visualObject, IntInterval interval, double scaleX, double scaleY, RectPoint rectPoint, Progress progress, bool withTransforms, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        {
            visualObject.Effects.Add(new ScaleCharacterEffect(interval, scaleX, scaleY, rectPoint, progress, withTransforms, synchronizedProgresses));
            return visualObject;
        }
        public static T Scale<T>(this T visualObject, ScaleCharacterEffect scaleCharacterEffect) where T : VisualObject
        {
            visualObject.Effects.Add(scaleCharacterEffect);
            return visualObject;
        }

        public static T Write<T>(this T visualObject, IntInterval interval, bool reverse, double strokeThickness, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        {
            visualObject.Effects.Add(new WriteCharacterEffect(interval, reverse, strokeThickness, progress, synchronizedProgresses));
            return visualObject;
        }
        public static T Write<T>(this T visualObject, IntInterval interval, bool reverse, double strokeThickness, Progress progress, bool withTransforms, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        {
            visualObject.Effects.Add(new WriteCharacterEffect(interval, reverse, strokeThickness, progress, withTransforms, synchronizedProgresses));
            return visualObject;
        }
        public static T Write<T>(this T visualObject, WriteCharacterEffect writeCharacterEffect) where T : VisualObject
        {
            visualObject.Effects.Add(writeCharacterEffect);
            return visualObject;
        }

        public static T Brace<T>(this T visualObject, int openingBraceIndex, int contentLength, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        {
            int closingBraceIndex = openingBraceIndex + contentLength + 1;
            var rectPoint = new RectPoint(0, 0.5);
            return visualObject.Size((openingBraceIndex, openingBraceIndex + 1), default, rectPoint, progress, synchronizedProgresses).Size((closingBraceIndex, closingBraceIndex + 1), default, rectPoint, progress, synchronizedProgresses);
        }

        public static T Stroke<T>(this T visualObject, IntInterval interval, bool reverse, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        {
            visualObject.Effects.Add(new StrokeCharacterEffect(interval, reverse, progress, synchronizedProgresses));
            return visualObject;
        }
        public static T Stroke<T>(this T visualObject, IntInterval interval, bool reverse, Progress progress, bool withTransforms, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        {
            visualObject.Effects.Add(new StrokeCharacterEffect(interval, reverse, progress, withTransforms, synchronizedProgresses));
            return visualObject;
        }
        public static T Stroke<T>(this T visualObject, StrokeCharacterEffect strokeCharacterEffect) where T : VisualObject
        {
            visualObject.Effects.Add(strokeCharacterEffect);
            return visualObject;
        }

        #endregion

        #region Animate

        public static T AddTo<T>(this T characterEffect, SynchronizedProgress synchronizedProgress) where T : CharacterEffect
        {
            synchronizedProgress.Objects.Add(characterEffect);
            return characterEffect;
        }

        //public static Task<T> Animate<T>(this T visualObject, bool destroy, TimeSpan duration, IEasingFunction easingFunction, CharacterEffect characterEffect, RepeatBehavior repeatBehavior = default, bool autoReverse = false, bool isCumulative = false, int? fps = FPS) where T : VisualObject => Animate(visualObject, destroy, duration, 0, 1, easingFunction, characterEffect, repeatBehavior, autoReverse, isCumulative, fps);
        //public static async Task<T> Animate<T>(this T visualObject, bool destroy, TimeSpan duration, double from, double to, IEasingFunction easingFunction, CharacterEffect characterEffect, RepeatBehavior repeatBehavior = default, bool autoReverse = false, bool isCumulative = false, int? fps = FPS) where T : VisualObject
        //{
        //    var progress = characterEffect.Progress;
        //    visualObject.Effects.Add(characterEffect);

        //    await AnimateDouble(null, value => characterEffect.Progress = progress.ChangeValue(value), from, to, duration, repeatBehavior, autoReverse, isCumulative, easingFunction, fps);

        //    if (destroy) characterEffect.Destroy();
        //    return visualObject;
        //}

        public static Task<T> Animate<T>(this T visualObject, bool destroy, TimeSpan duration, IEasingFunction easingFunction, IList<CharacterEffect> characterEffects, RepeatBehavior repeatBehavior = default, bool autoReverse = false, bool isCumulative = false, int? fps = FPS) where T : VisualObject => Animate(visualObject, destroy, duration, 0, 1, easingFunction, characterEffects, repeatBehavior, autoReverse, isCumulative, fps);
        public static async Task<T> Animate<T>(this T visualObject, bool destroy, TimeSpan duration, double from, double to, IEasingFunction easingFunction, IList<CharacterEffect> characterEffects, RepeatBehavior repeatBehavior = default, bool autoReverse = false, bool isCumulative = false, int? fps = FPS) where T : VisualObject
        {
            var progress = characterEffects.Select(effect =>
            {
                visualObject.Effects.Add(effect);
                return effect.Progress;
            }).ToArray();

            await AnimateDouble(null, value => characterEffects.ForEach((i, effect) => effect.Progress = progress[i].ChangeValue(value)), from, to, duration, repeatBehavior, autoReverse, isCumulative, easingFunction, fps);

            if (destroy) characterEffects.ForEach(effect => effect.Destroy());
            return visualObject;
        }
        public static Task<T> Animate<T>(this T visualObject, bool destroy, TimeSpan duration, IEasingFunction easingFunction, params CharacterEffect[] characterEffects) where T : VisualObject => Animate(visualObject, destroy, duration, 0, 1, null, characterEffects);
        public static Task<T> Animate<T>(this T visualObject, bool destroy, TimeSpan duration, double from, double to, IEasingFunction easingFunction, params CharacterEffect[] characterEffects) where T : VisualObject => Animate(visualObject, destroy, duration, from, to, null, characterEffects, default, false, false, FPS);

        public static async Task ReColor(this VisualObject visualObject, IntInterval interval, Brush fill, Pen stroke, bool inPen, double secondsDelay = 0.5)
        {
            var clr = new ColorCharacterEffect(interval, fill, stroke, inPen, 0);
            visualObject.Color(clr);

            await AnimateDouble(null, value => clr.Progress = value, 0.0, 1.0, TimeSpan.FromSeconds(secondsDelay), default, false, false, null, FPS);
        }

        public static Task<T> Write<T>(this T visualObject, bool small, bool reverse, double strokeThickness = 1, double seconds = double.NaN, double lagFactor = double.NaN) where T : VisualObject => Write(visualObject, NSet, small, reverse, strokeThickness, seconds, lagFactor);
        public static async Task<T> Write<T>(this T visualObject, IntInterval interval, bool small, bool reverse, double strokeThickness = 1, double seconds = double.NaN, double lagFactor = double.NaN) where T : VisualObject
        {
            if (double.IsNaN(seconds)) seconds = small ? 1.15 : 1.6;
            if (double.IsNaN(lagFactor))
            {
                double minLagFactor = small ? 1.5 : 2;
                lagFactor = Math.Max(seconds - 1, minLagFactor);
            }

            await visualObject.Animate(true, TimeSpan.FromSeconds(seconds), null, new WriteCharacterEffect(interval, reverse, strokeThickness, new Progress(0, ProgressMode.LaggedStart, lagFactor)));
            return visualObject;
        }

        public static Task<T> UnWrite<T>(this T visualObject, bool small, bool reverse, double strokeThickness = 1, double seconds = double.NaN, double lagFactor = double.NaN) where T : VisualObject => UnWrite(visualObject, NSet, small, reverse, strokeThickness, seconds, lagFactor);
        public static async Task<T> UnWrite<T>(this T visualObject, IntInterval interval, bool small, bool reverse, double strokeThickness = 1, double seconds = double.NaN, double lagFactor = double.NaN) where T : VisualObject
        {
            if (double.IsNaN(seconds)) seconds = small ? 1.15 : 1.6;
            if (double.IsNaN(lagFactor))
            {
                double minLagFactor = small ? 1.5 : 2;
                lagFactor = Math.Max(seconds - 1, minLagFactor);
            }

            await visualObject.Animate(false, TimeSpan.FromSeconds(seconds), 1, 0, null, new WriteCharacterEffect(interval, reverse, strokeThickness, new Progress(1, ProgressMode.LaggedStart, lagFactor)));
            return visualObject;
        }

        #region TextBase

        public static Task Group(params TextVisualObjectBase[] textVisualObjects) => Group(true, true, default, textVisualObjects);
        public static Task Group(bool translateX, bool translateY, params TextVisualObjectBase[] textVisualObjects) => Group(translateX, translateY, default, textVisualObjects);
        public static async Task Group(bool translateX, bool translateY, RectPoint rectPoint, params TextVisualObjectBase[] textVisualObjects)
        {
            if (textVisualObjects.Length == 0) return;
            var ins = new InsertCharacterEffect(NSet, null, textVisualObjects[0], NSet, 0, translateX, translateY, rectPoint);
            textVisualObjects.Skip(1).ForEach(t => t.Insert(ins));
            await AnimateDouble(null, value => ins.Progress = value, 0, 1, TimeSpan.FromSeconds(1), default, false, false, new CubicEase { EasingMode = EasingMode.EaseInOut }, FPS);
        }

        /*public static Task<T> Write<T>(this T textVisualObject, double strokeThickness = 1, double seconds = double.NaN, double lagFactor = double.NaN) where T : TextVisualObjectBase => Write(textVisualObject, NSet, textVisualObject.CharactersCount < 5, strokeThickness, seconds, lagFactor);
        public static Task<T> Write<T>(this T textVisualObject, IntInterval interval, double strokeThickness = 1, double seconds = double.NaN, double lagFactor = double.NaN) where T : TextVisualObjectBase => Write(textVisualObject, interval, (interval * (0, textVisualObject.CharactersCount)).Length < 5, strokeThickness, seconds, lagFactor);

        public static Task<T> UnWrite<T>(this T textVisualObject, double strokeThickness = 1, double seconds = double.NaN, double lagFactor = double.NaN) where T : TextVisualObjectBase => UnWrite(textVisualObject, NSet, textVisualObject.CharactersCount < 5, strokeThickness, seconds, lagFactor);
        public static Task<T> UnWrite<T>(this T textVisualObject, IntInterval interval, double strokeThickness = 1, double seconds = double.NaN, double lagFactor = double.NaN) where T : TextVisualObjectBase => UnWrite(textVisualObject, interval, (interval * (0, textVisualObject.CharactersCount)).Length < 5, strokeThickness, seconds, lagFactor);*/

        #region Math

        public static async Task PowerFrom(this TextVisualObjectBase textVisualObject, params TextVisualObjectBase[] textVisualObjects)
        {
            int length = textVisualObjects.Length;
            int newCount = textVisualObject.CharactersCount;

            var op0 = new OpacityCharacterEffect(NSet, 0, 0, 1);
            var op1 = new OpacityCharacterEffect((0, newCount - 1), 0, 0, 1);
            var op2 = new OpacityCharacterEffect((newCount - 1, newCount), 0, 0, 1);
            textVisualObject.Opacity(op1);
            textVisualObject.Opacity(op2);

            async Task GroupAndColor()
            {
                await Group(textVisualObjects);
                foreach (var t in textVisualObjects) t.Opacity(op0);
                op1.Progress = 0;
            }

            await GroupAndColor().AtMost(40);
            op2.Progress = 0;
            await textVisualObject.Write((newCount - 1, newCount), true, false);

            op1.Destroy();
            op2.Destroy();
        }

        #endregion

        #endregion

        #endregion

        #endregion

        #region PlaneZoom

        public static PlaneZoom Zoom(this Plane plane, bool overAxesNumbers, MathRect inputRange, Rect outputRange, Geometry clip, params VisualObject[] children) => ZoomCore(plane, overAxesNumbers, inputRange, outputRange, clip, children);
        public static PlaneZoom Zoom(this Plane plane, bool overAxesNumbers, MathRect inputRange, Rect outputRange, Geometry clip, IEnumerable<VisualObject> children) => ZoomCore(plane, overAxesNumbers, inputRange, outputRange, clip, children);

        private static PlaneZoom ZoomCore(this Plane plane, bool overAxesNumbers, MathRect inputRange, Rect outputRange, Geometry clip, IEnumerable<VisualObject> children)
        {
            var visualObjects = new List<VisualObject>(new VisualObject[] { plane.Grid, plane.Axes });

            bool axesNumbersAdded = false;
            foreach (var visualObject in children)
            {
                if (plane.OverAxesNumbers == visualObject)
                {
                    visualObjects.Add(plane.AxesNumbers);
                    axesNumbersAdded = true;
                }
                visualObjects.Add(visualObject);
            }
            if (!axesNumbersAdded) visualObjects.Add(plane.AxesNumbers);

            var zoom = new PlaneZoom(inputRange, outputRange, clip, visualObjects);
            plane.VisualObjects.Add(zoom);
            if (overAxesNumbers) plane.OverAxesNumbers = zoom;

            return zoom;
        }

        #endregion
    }

    public static partial class Extensions
    {
        public static T Style<T>(this T visualObject, Brush fill, Pen stroke) where T : VisualObject
        {
            visualObject.Fill = fill;
            visualObject.Stroke = stroke;
            return visualObject;
        }
        public static T Style<T>(this T visualObject, Pen stroke) where T : VisualObject
        {
            visualObject.Stroke = stroke;
            return visualObject;
        }
        public static T Style<T>(this T visualObject, Brush fill) where T : VisualObject
        {
            visualObject.Fill = fill;
            return visualObject;
        }
    }
}
