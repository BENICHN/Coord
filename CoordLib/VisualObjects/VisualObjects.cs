using BenLib.Framework;
using BenLib.Standard;
using BenLib.WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using static BenLib.Framework.Animating;
using static BenLib.Standard.Interval<int>;

namespace Coord
{
    /// <summary>
    /// Contient des méthodes statiques permettant de créer et modifier rapidement des <see cref="VisualObject"/>
    /// </summary>
    public static partial class VisualObjects
    {
        public const int FPS = 60;

        #region PointVisualObject

        public static PointVisualObject Point(double x, double y) => Point(new Point(x, y));
        public static PointVisualObject Point(Point inPoint) => new PointVisualObject { Definition = new PointPointDefinition { InPoint = inPoint } };
        public static PointVisualObject Point(PointVisualObject point, Func<Point, Point> operations) => new PointVisualObject { Definition = new OperationsPointDefinition { Point = point, Operations = operations } };
        public static PointVisualObject Point(IEnumerable<PointVisualObject> points, Func<Point[], Point> operations) => new PointVisualObject { Definition = new MultiOperationsPointDefinition { Points = points as NotifyObjectCollection<PointVisualObject> ?? new NotifyObjectCollection<PointVisualObject>(points), Operations = operations } };
        public static PointVisualObject MiddlePoint(PointVisualObject pointA, PointVisualObject pointB) => new PointVisualObject { Definition = new MiddlePointDefinition { PointA = pointA, PointB = pointB } };
        public static PointVisualObject LineIntersectionPoint(LineVisualObject lineA, LineVisualObject lineB) => new PointVisualObject { Definition = new LineIntersectionPointDefinition { LineA = lineA, LineB = lineB } };
        public static PointVisualObject Translation(PointVisualObject point, VectorVisualObject vector) => new PointVisualObject { Definition = new TranslationPointDefinition { Point = point, Vector = vector } };

        public static PointVisualObject Extend(this PointVisualObject pointVisualObject, double? radius = null)
        {
            if (radius.HasValue) pointVisualObject.Radius = radius.Value;
            return pointVisualObject;
        }

        public static VisualObjectIntersection Intersection(VisualObject object1, VisualObject object2, double radius = 10) => new VisualObjectIntersection { Object1 = object1, Object2 = object2, Radius = radius };

        #endregion

        #region VectorVisualObject

        public static VectorVisualObject Vector(PointVisualObject inAnchorPoint, double x, double y) => Vector(inAnchorPoint, new Vector(x, y));
        public static VectorVisualObject Vector(PointVisualObject inAnchorPoint, Vector inVector) => new VectorVisualObject { InAnchorPoint = inAnchorPoint, Definition = new VectorVectorDefinition { InVector = inVector } };
        public static VectorVisualObject Vector(PointVisualObject inAnchorPoint, PointVisualObject pointA, PointVisualObject pointB) => new VectorVisualObject { InAnchorPoint = inAnchorPoint, Definition = new PointPointVectorDefinition { PointA = pointA, PointB = pointB } };
        public static VectorVisualObject Vector(PointVisualObject inAnchorPoint, VectorVisualObject vector, Func<Vector, Vector> operations) => new VectorVisualObject { InAnchorPoint = inAnchorPoint, Definition = new OperationsVectorDefinition { Vector = vector, Operations = operations } };
        public static VectorVisualObject Vector(PointVisualObject inAnchorPoint, IEnumerable<VectorVisualObject> vectors, Func<Vector[], Vector> operations) => new VectorVisualObject { InAnchorPoint = inAnchorPoint, Definition = new MultiOperationsVectorDefinition { Vectors = vectors as NotifyObjectCollection<VectorVisualObject> ?? new NotifyObjectCollection<VectorVisualObject>(vectors), Operations = operations } };

        public static VectorVisualObject Extend(this VectorVisualObject vectorVisualObject, PointVisualObject inAnchorPoint = null, Arrow arrow = null, ArrowEnd? arrowEnd = null)
        {
            if (inAnchorPoint != null) vectorVisualObject.InAnchorPoint = inAnchorPoint;
            if (arrow != null) vectorVisualObject.Arrow = arrow;
            if (arrowEnd.HasValue) vectorVisualObject.ArrowEnd = arrowEnd.Value;
            return vectorVisualObject;
        }

        #endregion

        #region CircleVisualObject

        public static CircleVisualObject Circle(PointVisualObject center, double radius) => new CircleVisualObject { Definition = new CenterRadiusCircleDefinition { Center = center, Radius = radius } };
        public static CircleVisualObject Circle(PointVisualObject center, PointVisualObject point) => new CircleVisualObject { Definition = new CenterPointCircleDefinition { Center = center, Point = point } };

        #endregion

        #region CurveVisualObject

        public static CurveVisualObject Curve(Series series, bool closed, bool smooth, double smoothValue = 0.75) => new CurveVisualObject { Series = series, Closed = closed, Smooth = smooth, SmoothValue = smoothValue };
        public static CurveVisualObject FunctionCurve(Func<double, double> function, bool smooth, double smoothValue = 0.75) => FunctionCurve(function, Interval<double>.Reals, SeriesType.Y, 0, false, smooth, smoothValue);
        public static CurveVisualObject FunctionCurve(Func<double, double> function, Interval<double> interval, bool smooth, double smoothValue = 0.75) => FunctionCurve(function, interval, SeriesType.Y, 0, false, smooth, smoothValue);
        public static CurveVisualObject FunctionCurve(Func<double, double> function, Interval<double> interval, SeriesType type, double density, bool closed, bool smooth, double smoothValue = 0.75) => Curve(new FunctionSeries { Function = function, Interval = interval, Type = type, Density = density }, closed, smooth, smoothValue);

        #endregion

        #region MorphingVisualObject

        public static OutMorphingVisualObject OutMorphing(PointVisualObject inAnchorPoint, IReadOnlyCollection<Character> from, IReadOnlyCollection<Character> to, CorrespondanceDictionary correspondances, params SynchronizedProgress[] synchronizedProgresses)
        {
            var result = new OutMorphingVisualObject { InAnchorPoint = inAnchorPoint, From = from, To = to, Correspondances = correspondances };
            foreach (var sp in synchronizedProgresses) sp.Objects.Add(result);
            return result;
        }
        public static InMorphingVisualObject InMorphing(VisualObject from, VisualObject to, CorrespondanceDictionary correspondances, params SynchronizedProgress[] synchronizedProgresses)
        {
            var result = new InMorphingVisualObject { From = from, To = to, Correspondances = correspondances };
            foreach (var sp in synchronizedProgresses) sp.Objects.Add(result);
            return result;
        }

        #endregion

        #region LineVisualObject

        public static LineVisualObject Line(LinearEquation equation) => new LineVisualObject { Definition = new EquationLineDefinition { Equation = equation } };
        public static LineVisualObject Line(PointVisualObject pointA, PointVisualObject pointB) => new LineVisualObject { Definition = new PointPointLineDefinition { PointA = pointA, PointB = pointB } };
        public static LineVisualObject Line(PointVisualObject point, VectorVisualObject vector) => new LineVisualObject { Definition = new PointVectorLineDefinition { Point = point, Vector = vector } };

        public static LineVisualObject ParallelLine(PointVisualObject point, LineVisualObjectBase line) => new LineVisualObject { Definition = new ParallelLineDefinition { Point = point, Line = line } };
        public static LineVisualObject PerpendicularLine(PointVisualObject point, LineVisualObjectBase line) => new LineVisualObject { Definition = new PerpendicularLineDefinition { Point = point, Line = line } };
        public static LineVisualObject PerpendicularBisectorLine(SegmentVisualObject segment) => new LineVisualObject { Definition = new PerpendicularBisectorLineDefinition { Segment = segment } };

        #endregion

        #region SegmentVisualObject

        public static SegmentVisualObject Segment(PointVisualObject start, PointVisualObject end) => new SegmentVisualObject { Definition = new PointPointSegmentDefinition { Start = start, End = end } };

        #endregion

        #region PolygonVisualObject

        public static PolygonVisualObject Polygon(IEnumerable<PointVisualObject> inPoints) => new PolygonVisualObject { Definition = new PointsPolygonDefinition { InPoints = inPoints as NotifyObjectCollection<PointVisualObject> ?? new NotifyObjectCollection<PointVisualObject>(inPoints) } };
        public static PolygonVisualObject RegularPolygon(double sideLength, int sideCount) => new PolygonVisualObject { Definition = new RegularPolygonDefinition { SideCount = sideCount, SideLength = sideLength } };

        #endregion

        #region TextVisualObject

        public static TextVisualObject InTex(string text, double scale, PointVisualObject inAnchorPoint = null, RectPoint rectPoint = default) => new TextVisualObject { Text = text, Scale = scale, LaTex = true, In = true, InAnchorPoint = inAnchorPoint, RectPoint = rectPoint };
        public static TextVisualObject InText(string text, double scale, PointVisualObject inAnchorPoint = null, RectPoint rectPoint = default, Typeface typeface = null) => new TextVisualObject { Text = text, Scale = scale, LaTex = false, In = true, InAnchorPoint = inAnchorPoint, RectPoint = rectPoint, Typeface = typeface };

        public static TextVisualObject OutTex(string text, double scale, PointVisualObject inAnchorPoint = null, RectPoint rectPoint = default) => new TextVisualObject { Text = text, Scale = scale, LaTex = true, In = false, InAnchorPoint = inAnchorPoint, RectPoint = rectPoint };
        public static TextVisualObject OutText(string text, double scale, PointVisualObject inAnchorPoint = null, RectPoint rectPoint = default, Typeface typeface = null) => new TextVisualObject { Text = text, Scale = scale, LaTex = false, In = false, InAnchorPoint = inAnchorPoint, RectPoint = rectPoint, Typeface = typeface };

        #endregion

        #region VisualObjectGroup

        public static VisualObjectContainer Container(VisualObjectChildrenRenderingMode childrenRenderingMode, IEnumerable<VisualObject> children) => new VisualObjectContainer { ChildrenRenderingMode = childrenRenderingMode, Children = children as VisualObjectCollection ?? new VisualObjectCollection(children) };
        public static VisualObjectContainer Container(VisualObjectChildrenRenderingMode childrenRenderingMode, params VisualObject[] children) => Container(childrenRenderingMode, (IEnumerable<VisualObject>)children);

        public static VisualObjectContainer Group(IEnumerable<VisualObject> children) => new VisualObjectContainer { ChildrenRenderingMode = VisualObjectChildrenRenderingMode.Embedded, Children = children as VisualObjectCollection ?? new VisualObjectCollection(children) };
        public static VisualObjectContainer Group(params VisualObject[] children) => Group((IEnumerable<VisualObject>)children);

        public static VisualObjectContainer Renderer(IEnumerable<VisualObject> children) => new VisualObjectContainer { ChildrenRenderingMode = VisualObjectChildrenRenderingMode.Independent, Children = children as VisualObjectCollection ?? new VisualObjectCollection(children) };
        public static VisualObjectContainer Renderer(params VisualObject[] children) => Renderer((IEnumerable<VisualObject>)children);

        #endregion

        #region CharactersVisualObject

        public static InCharactersVisualObjectGroup InCharactersGroup(IEnumerable<VisualObject> visualObjects, IEnumerable<Interval<int>> intervals) => new InCharactersVisualObjectGroup { Children = visualObjects as VisualObjectCollection ?? new VisualObjectCollection(visualObjects), Intervals = new ObservableRangeCollection<Interval<int>>(intervals) };
        public static InCharactersVisualObjectGroup InCharactersGroup(IEnumerable<VisualObject> visualObjects) => new InCharactersVisualObjectGroup { Children = visualObjects as VisualObjectCollection ?? new VisualObjectCollection(visualObjects), Intervals = new ObservableRangeCollection<Interval<int>>(visualObjects.Select(vo => vo.Selection)) };
        public static InCharactersVisualObject InCharacters(VisualObject visualObject) => new InCharactersVisualObject { VisualObject = visualObject, Interval = visualObject.Selection };
        public static InCharactersVisualObject InCharacters(VisualObject visualObject, Interval<int> interval) => new InCharactersVisualObject { VisualObject = visualObject, Interval = interval };
        public static CharactersVisualObject Characters(PointVisualObject inAnchorPoint, RectPoint rectPoint, IEnumerable<Character> characters, Interval<int> interval) => new CharactersVisualObject { InAnchorPoint = inAnchorPoint, RectPoint = rectPoint, Characters = characters.SubCollection(interval, true).ToArray() };

        #endregion

        #region CharacterEffects

        #region Create

        public static T Align<T>(this T vo, Interval<int> interval, VisualObject visualObject, RectPoint rectPoint, bool translateX, bool translateY, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject => vo.Align(interval, visualObject, rectPoint, translateX, translateY, true, progress, synchronizedProgresses);
        public static T Align<T>(this T vo, Interval<int> interval, VisualObject visualObject, RectPoint rectPoint, bool translateX, bool translateY, bool withTransforms, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject => vo.Align(interval, new AlignCharacterEffect { VisualObject = visualObject, RectPoint = rectPoint, TranslateX = translateX, TranslateY = translateY, WithTransforms = withTransforms, Progress = progress }, synchronizedProgresses);
        public static T Align<T>(this T vo, Interval<int> interval, AlignCharacterEffect alignCharacterEffect, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        {
            vo.Effects.Add(alignCharacterEffect, interval);
            foreach (var p in synchronizedProgresses) p.Objects.Add(alignCharacterEffect);
            return vo;
        }

        public static T Fit<T>(this T vo, Interval<int> interval, Interval<int> boundsInterval, VisualObject visualObject, bool scaleX, bool scaleY, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject => vo.Fit(interval, boundsInterval, visualObject, scaleX, scaleY, true, progress, synchronizedProgresses);
        public static T Fit<T>(this T vo, Interval<int> interval, Interval<int> boundsInterval, VisualObject visualObject, bool scaleX, bool scaleY, bool withTransforms, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject => vo.Fit(interval, new FitCharacterEffect { BoundsInterval = boundsInterval, VisualObject = visualObject, ScaleX = scaleX, ScaleY = scaleY, WithTransforms = withTransforms, Progress = progress }, synchronizedProgresses);
        public static T Fit<T>(this T vo, Interval<int> interval, FitCharacterEffect fitCharacterEffect, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        {
            vo.Effects.Add(fitCharacterEffect, interval);
            foreach (var p in synchronizedProgresses) p.Objects.Add(fitCharacterEffect);
            return vo;
        }

        public static T Insert<T>(this T vo, Interval<int> interval, Interval<int> boundsInterval, VisualObject visualObject, RectPoint rectPoint, bool translateX, bool translateY, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject => vo.Insert(interval, boundsInterval, visualObject, rectPoint, translateX, translateY, true, progress, synchronizedProgresses);
        public static T Insert<T>(this T vo, Interval<int> interval, Interval<int> boundsInterval, VisualObject visualObject, RectPoint rectPoint, bool translateX, bool translateY, bool withTransforms, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject => vo.Insert(interval, new InsertCharacterEffect { BoundsInterval = boundsInterval, VisualObject = visualObject, RectPoint = rectPoint, TranslateX = translateX, TranslateY = translateY, WithTransforms = withTransforms, Progress = progress }, synchronizedProgresses);
        public static T Insert<T>(this T vo, Interval<int> interval, InsertCharacterEffect insertCharacterEffect, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        {
            vo.Effects.Add(insertCharacterEffect, interval);
            foreach (var p in synchronizedProgresses) p.Objects.Add(insertCharacterEffect);
            return vo;
        }

        public static T Mask<T>(this T vo, Interval<int> interval, Size smallSize, Size bigSize, RectPoint rectPoint, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject => vo.Mask(interval, smallSize, bigSize, rectPoint, true, progress, synchronizedProgresses);
        public static T Mask<T>(this T vo, Interval<int> interval, Size smallSize, Size bigSize, RectPoint rectPoint, bool withTransforms, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject => vo.Mask(interval, new MaskCharacterEffect { SmallSize = smallSize, BigSize = bigSize, RectPoint = rectPoint, WithTransforms = withTransforms, Progress = progress }, synchronizedProgresses);
        public static T Mask<T>(this T vo, Interval<int> interval, MaskCharacterEffect maskCharacterEffect, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        {
            vo.Effects.Add(maskCharacterEffect, interval);
            foreach (var p in synchronizedProgresses) p.Objects.Add(maskCharacterEffect);
            return vo;
        }

        public static T Rotate<T>(this T vo, Interval<int> interval, Point center, bool inEffect, double angle, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject => vo.Rotate(interval, center, inEffect, angle, true, progress, synchronizedProgresses);
        public static T Rotate<T>(this T vo, Interval<int> interval, Point center, bool inEffect, double angle, bool withTransforms, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject => vo.Rotate(interval, new RotateCharacterEffect { Center = center, In = inEffect, Angle = angle, WithTransforms = withTransforms, Progress = progress }, synchronizedProgresses);
        public static T Rotate<T>(this T vo, Interval<int> interval, RotateCharacterEffect rotateCharacterEffect, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        {
            vo.Effects.Add(rotateCharacterEffect, interval);
            foreach (var p in synchronizedProgresses) p.Objects.Add(rotateCharacterEffect);
            return vo;
        }

        public static T Scale<T>(this T vo, Interval<int> interval, double scaleX, double scaleY, RectPoint rectPoint, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject => vo.Scale(interval, scaleX, scaleY, rectPoint, new Point(double.NaN, double.NaN), false, true, progress, synchronizedProgresses);
        public static T Scale<T>(this T vo, Interval<int> interval, double scaleX, double scaleY, RectPoint rectPoint, bool withTransforms, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject => vo.Scale(interval, scaleX, scaleY, rectPoint, new Point(double.NaN, double.NaN), false, withTransforms, progress, synchronizedProgresses);
        public static T Scale<T>(this T vo, Interval<int> interval, double scaleX, double scaleY, RectPoint rectPoint, Point center, bool inEffect, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject => vo.Scale(interval, scaleX, scaleY, rectPoint, center, inEffect, true, progress, synchronizedProgresses);
        public static T Scale<T>(this T vo, Interval<int> interval, double scaleX, double scaleY, RectPoint rectPoint, Point center, bool inEffect, bool withTransforms, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject => vo.Scale(interval, new ScaleCharacterEffect { ScaleX = scaleX, ScaleY = scaleY, RectPoint = rectPoint, Center = center, In = inEffect, WithTransforms = withTransforms, Progress = progress }, synchronizedProgresses);
        public static T Scale<T>(this T vo, Interval<int> interval, ScaleCharacterEffect scaleCharacterEffect, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        {
            vo.Effects.Add(scaleCharacterEffect, interval);
            foreach (var p in synchronizedProgresses) p.Objects.Add(scaleCharacterEffect);
            return vo;
        }

        public static T Size<T>(this T vo, Interval<int> interval, Size size, RectPoint rectPoint, bool scaleX, bool scaleY, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject => vo.Size(interval, size, rectPoint, scaleX, scaleY, true, progress, synchronizedProgresses);
        public static T Size<T>(this T vo, Interval<int> interval, Size size, RectPoint rectPoint, bool scaleX, bool scaleY, bool withTransforms, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject => vo.Size(interval, new SizeCharacterEffect { Size = size, RectPoint = rectPoint, ScaleX = scaleX, ScaleY = scaleY, WithTransforms = withTransforms, Progress = progress }, synchronizedProgresses);
        public static T Size<T>(this T vo, Interval<int> interval, SizeCharacterEffect sizeCharacterEffect, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        {
            vo.Effects.Add(sizeCharacterEffect, interval);
            foreach (var p in synchronizedProgresses) p.Objects.Add(sizeCharacterEffect);
            return vo;
        }

        public static T TranslateAlongPath<T>(this T vo, Interval<int> interval, PathGeometry pathGeometry, Func<Point, Vector> translation, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject => vo.TranslateAlongPath(interval, pathGeometry, translation, true, progress, synchronizedProgresses);
        public static T TranslateAlongPath<T>(this T vo, Interval<int> interval, PathGeometry pathGeometry, Func<Point, Vector> translation, bool withTransforms, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject => vo.TranslateAlongPath(interval, new TranslateAlongPathCharacterEffect { PathGeometry = pathGeometry, Translation = translation, WithTransforms = withTransforms, Progress = progress }, synchronizedProgresses);
        public static T TranslateAlongPath<T>(this T vo, Interval<int> interval, TranslateAlongPathCharacterEffect translateAlongPathCharacterEffect, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        {
            vo.Effects.Add(translateAlongPathCharacterEffect, interval);
            foreach (var p in synchronizedProgresses) p.Objects.Add(translateAlongPathCharacterEffect);
            return vo;
        }

        public static T Translate<T>(this T vo, Interval<int> interval, Vector vector, bool inEffect, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject => vo.Translate(interval, vector, inEffect, true, progress, synchronizedProgresses);
        public static T Translate<T>(this T vo, Interval<int> interval, Vector vector, bool inEffect, bool withTransforms, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject => vo.Translate(interval, new TranslateCharacterEffect { Vector = vector, In = inEffect, WithTransforms = withTransforms, Progress = progress }, synchronizedProgresses);
        public static T Translate<T>(this T vo, Interval<int> interval, TranslateCharacterEffect translateCharacterEffect, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        {
            vo.Effects.Add(translateCharacterEffect, interval);
            foreach (var p in synchronizedProgresses) p.Objects.Add(translateCharacterEffect);
            return vo;
        }

        public static T AutoPen<T>(this T vo, Interval<int> interval, Pen template, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject => vo.AutoPen(interval, template, true, progress, synchronizedProgresses);
        public static T AutoPen<T>(this T vo, Interval<int> interval, Pen template, bool withTransforms, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject => vo.AutoPen(interval, new AutoPenCharacterEffect { Template = template, WithTransforms = withTransforms, Progress = progress }, synchronizedProgresses);
        public static T AutoPen<T>(this T vo, Interval<int> interval, AutoPenCharacterEffect autoPenCharacterEffect, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        {
            vo.Effects.Add(autoPenCharacterEffect, interval);
            foreach (var p in synchronizedProgresses) p.Objects.Add(autoPenCharacterEffect);
            return vo;
        }

        public static T Hide<T>(this T vo) where T : VisualObject => vo.Color(PositiveReals, null, null, 1);
        public static T Hide<T>(this T vo, Interval<int> interval) where T : VisualObject => vo.Color(interval, null, null, 1);
        public static T Hide<T>(this T vo, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject => vo.Color(PositiveReals, null, null, progress, synchronizedProgresses);
        public static T Hide<T>(this T vo, Interval<int> interval, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject => vo.Color(interval, null, null, false, progress, synchronizedProgresses);

        public static T Color<T>(this T vo, Pen stroke) where T : VisualObject => vo.Color(PositiveReals, null, stroke);
        public static T Color<T>(this T vo, Brush fill) where T : VisualObject => vo.Color(PositiveReals, fill, null);
        public static T Color<T>(this T vo, Brush fill, Pen stroke) where T : VisualObject => vo.Color(PositiveReals, fill, stroke, 1);

        public static T Color<T>(this T vo, Interval<int> interval, Pen stroke) where T : VisualObject => vo.Color(interval, null, stroke);
        public static T Color<T>(this T vo, Interval<int> interval, Brush fill) where T : VisualObject => vo.Color(interval, fill, null);
        public static T Color<T>(this T vo, Interval<int> interval, Brush fill, Pen stroke) where T : VisualObject => vo.Color(interval, fill, stroke, 1);

        public static T Color<T>(this T vo, Pen stroke, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject => vo.Color(PositiveReals, null, stroke, progress, synchronizedProgresses);
        public static T Color<T>(this T vo, Brush fill, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject => vo.Color(PositiveReals, fill, null, progress, synchronizedProgresses);
        public static T Color<T>(this T vo, Brush fill, Pen stroke, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject => vo.Color(PositiveReals, fill, stroke, progress, synchronizedProgresses);

        public static T Color<T>(this T vo, Interval<int> interval, Pen stroke, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject => vo.Color(interval, null, stroke, progress, synchronizedProgresses);
        public static T Color<T>(this T vo, Interval<int> interval, Brush fill, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject => vo.Color(interval, fill, null, progress, synchronizedProgresses);

        public static T Color<T>(this T vo, Interval<int> interval, Brush fill, Pen stroke, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject => vo.Color(interval, fill, stroke, true, progress, synchronizedProgresses);
        public static T Color<T>(this T vo, Interval<int> interval, Brush fill, Pen stroke, bool withTransforms, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject => vo.Color(interval, new ColorCharacterEffect { Fill = fill, Stroke = stroke, WithTransforms = withTransforms, Progress = progress }, synchronizedProgresses);
        public static T Color<T>(this T vo, Interval<int> interval, ColorCharacterEffect colorCharacterEffect, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        {
            vo.Effects.Add(colorCharacterEffect, interval);
            foreach (var p in synchronizedProgresses) p.Objects.Add(colorCharacterEffect);
            return vo;
        }

        public static T Opacity<T>(this T vo, Interval<int> interval, double fillOpacity, double strokeOpacity, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject => vo.Opacity(interval, fillOpacity, strokeOpacity, true, progress, synchronizedProgresses);
        public static T Opacity<T>(this T vo, Interval<int> interval, double fillOpacity, double strokeOpacity, bool withTransforms, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject => vo.Opacity(interval, new OpacityCharacterEffect { FillOpacity = fillOpacity, StrokeOpacity = strokeOpacity, WithTransforms = withTransforms, Progress = progress }, synchronizedProgresses);
        public static T Opacity<T>(this T vo, Interval<int> interval, OpacityCharacterEffect opacityCharacterEffect, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        {
            vo.Effects.Add(opacityCharacterEffect, interval);
            foreach (var p in synchronizedProgresses) p.Objects.Add(opacityCharacterEffect);
            return vo;
        }

        public static T Stroke<T>(this T vo, Interval<int> interval, bool reverse, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject => vo.Stroke(interval, reverse, true, progress, synchronizedProgresses);
        public static T Stroke<T>(this T vo, Interval<int> interval, bool reverse, bool withTransforms, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject => vo.Stroke(interval, new StrokeCharacterEffect { Reverse = reverse, WithTransforms = withTransforms, Progress = progress }, synchronizedProgresses);
        public static T Stroke<T>(this T vo, Interval<int> interval, StrokeCharacterEffect strokeCharacterEffect, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        {
            vo.Effects.Add(strokeCharacterEffect, interval);
            foreach (var p in synchronizedProgresses) p.Objects.Add(strokeCharacterEffect);
            return vo;
        }

        public static T StrokeThickness<T>(this T vo, Interval<int> interval, double strokeThickness, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject => vo.StrokeThickness(interval, strokeThickness, true, progress, synchronizedProgresses);
        public static T StrokeThickness<T>(this T vo, Interval<int> interval, double strokeThickness, bool withTransforms, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject => vo.StrokeThickness(interval, new StrokeThicknessCharacterEffect { StrokeThickness = strokeThickness, WithTransforms = withTransforms, Progress = progress }, synchronizedProgresses);
        public static T StrokeThickness<T>(this T vo, Interval<int> interval, StrokeThicknessCharacterEffect strokeThicknessCharacterEffect, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        {
            vo.Effects.Add(strokeThicknessCharacterEffect, interval);
            foreach (var p in synchronizedProgresses) p.Objects.Add(strokeThicknessCharacterEffect);
            return vo;
        }

        public static T Write<T>(this T vo, Interval<int> interval, double strokeThickness, bool reverse, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject => vo.Write(interval, strokeThickness, reverse, true, progress, synchronizedProgresses);
        public static T Write<T>(this T vo, Interval<int> interval, double strokeThickness, bool reverse, bool withTransforms, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject => vo.Write(interval, new WriteCharacterEffect { StrokeThickness = strokeThickness, Reverse = reverse, WithTransforms = withTransforms, Progress = progress }, synchronizedProgresses);
        public static T Write<T>(this T vo, Interval<int> interval, WriteCharacterEffect writeCharacterEffect, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        {
            vo.Effects.Add(writeCharacterEffect, interval);
            foreach (var p in synchronizedProgresses) p.Objects.Add(writeCharacterEffect);
            return vo;
        }

        #endregion

        //#region Create

        //public static T Hide<T>(this T visualObject, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject => Hide(visualObject, PositiveReals, 1, synchronizedProgresses);
        //public static T Hide<T>(this T visualObject, Interval<int> interval, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject => Hide(visualObject, interval, 1, synchronizedProgresses);
        //public static T Hide<T>(this T visualObject, Interval<int> interval, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        //{
        //    visualObject.Effects.Add(new ColorCharacterEffect(null, null, progress, synchronizedProgresses), interval);
        //    return visualObject;
        //}
        //public static T Hide<T>(this T visualObject, Interval<int> interval, Progress progress, bool withTransforms, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        //{
        //    visualObject.Effects.Add(new ColorCharacterEffect(null, null, progress, withTransforms, synchronizedProgresses), interval);
        //    return visualObject;
        //}

        //public static T Color<T>(this T visualObject, Brush fill, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject => Color(visualObject, PositiveReals, fill, null, 1, synchronizedProgresses);
        //public static T Color<T>(this T visualObject, Pen stroke, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject => Color(visualObject, PositiveReals, null, stroke, 1, synchronizedProgresses);
        //public static T Color<T>(this T visualObject, Brush fill, Pen stroke, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject => Color(visualObject, PositiveReals, fill, stroke, 1, synchronizedProgresses);
        //public static T Color<T>(this T visualObject, Brush fill, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject => Color(visualObject, PositiveReals, fill, null, progress, synchronizedProgresses);
        //public static T Color<T>(this T visualObject, Pen stroke, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject => Color(visualObject, PositiveReals, null, stroke, progress, synchronizedProgresses);
        //public static T Color<T>(this T visualObject, Brush fill, Pen stroke, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject => Color(visualObject, PositiveReals, fill, stroke, progress, synchronizedProgresses);
        //public static T Color<T>(this T visualObject, Interval<int> interval, Brush fill, Pen stroke, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject => Color(visualObject, interval, fill, stroke, 1, synchronizedProgresses);
        //public static T Color<T>(this T visualObject, Interval<int> interval, Brush fill, Pen stroke, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        //{
        //    visualObject.Effects.Add(new ColorCharacterEffect(fill, stroke, progress, synchronizedProgresses), interval);
        //    return visualObject;
        //}
        //public static T Color<T>(this T visualObject, Interval<int> interval, Brush fill, Pen stroke, Progress progress, bool withTransforms, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        //{
        //    visualObject.Effects.Add(new ColorCharacterEffect(fill, stroke, progress, withTransforms, synchronizedProgresses), interval);
        //    return visualObject;
        //}
        //public static T Color<T>(this T visualObject, ColorCharacterEffect colorCharacterEffect,  Interval<int> interval) where T : VisualObject
        //{
        //    visualObject.Effects.Add(colorCharacterEffect, interval);
        //    return visualObject;
        //}

        //public static T Fit<T>(this T visualObject, Interval<int> interval, Interval<int> boundsInterval, VisualObject text, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        //{
        //    visualObject.Effects.Add(new FitCharacterEffect(boundsInterval, text, progress, synchronizedProgresses), interval);
        //    return visualObject;
        //}
        //public static T Fit<T>(this T visualObject, Interval<int> interval, Interval<int> boundsInterval, VisualObject text, Progress progress, bool withTransforms, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        //{
        //    visualObject.Effects.Add(new FitCharacterEffect(boundsInterval, text, progress, withTransforms, synchronizedProgresses), interval);
        //    return visualObject;
        //}
        //public static T Fit<T>(this T visualObject, Interval<int> interval, Interval<int> boundsInterval, VisualObject text, Progress progress, bool scaleX, bool scaleY, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        //{
        //    visualObject.Effects.Add(new FitCharacterEffect(boundsInterval, text, progress, scaleX, scaleY, synchronizedProgresses), interval);
        //    return visualObject;
        //}
        //public static T Fit<T>(this T visualObject, Interval<int> interval, Interval<int> boundsInterval, VisualObject text, Progress progress, bool withTransforms, bool scaleX, bool scaleY, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        //{
        //    visualObject.Effects.Add(new FitCharacterEffect(boundsInterval, text, progress, withTransforms, scaleX, scaleY, synchronizedProgresses), interval);
        //    return visualObject;
        //}
        //public static T Fit<T>(this T visualObject, FitCharacterEffect fitCharacterEffect,  Interval<int> interval) where T : VisualObject
        //{
        //    visualObject.Effects.Add(fitCharacterEffect, interval);
        //    return visualObject;
        //}

        //public static T Insert<T>(this T visualObject, Interval<int> interval, Interval<int> boundsInterval, VisualObject text, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        //{
        //    visualObject.Effects.Add(new InsertCharacterEffect(boundsInterval, text, progress, synchronizedProgresses), interval);
        //    return visualObject;
        //}
        //public static T Insert<T>(this T visualObject, Interval<int> interval, Interval<int> boundsInterval, VisualObject text, Progress progress, bool withTransforms, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        //{
        //    visualObject.Effects.Add(new InsertCharacterEffect(boundsInterval, text, progress, withTransforms, synchronizedProgresses), interval);
        //    return visualObject;
        //}
        //public static T Insert<T>(this T visualObject, Interval<int> interval, Interval<int> boundsInterval, VisualObject text, Progress progress, bool translateX, bool translateY, RectPoint rectPoint, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        //{
        //    visualObject.Effects.Add(new InsertCharacterEffect(boundsInterval, text, progress, translateX, translateY, rectPoint, synchronizedProgresses), interval);
        //    return visualObject;
        //}
        //public static T Insert<T>(this T visualObject, Interval<int> interval, Interval<int> boundsInterval, VisualObject text, Progress progress, bool withTransforms, bool translateX, bool translateY, RectPoint rectPoint, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        //{
        //    visualObject.Effects.Add(new InsertCharacterEffect(boundsInterval, text, progress, withTransforms, translateX, translateY, rectPoint, synchronizedProgresses), interval);
        //    return visualObject;
        //}
        //public static T Insert<T>(this T visualObject, InsertCharacterEffect insertCharacterEffect,  Interval<int> interval) where T : VisualObject
        //{
        //    visualObject.Effects.Add(insertCharacterEffect, interval);
        //    return visualObject;
        //}

        //public static T Align<T>(this T visualObject, Interval<int> interval, VisualObject text, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        //{
        //    visualObject.Effects.Add(new AlignCharacterEffect(text, progress, synchronizedProgresses), interval);
        //    return visualObject;
        //}
        //public static T Align<T>(this T visualObject, Interval<int> interval, VisualObject text, Progress progress, bool withTransforms, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        //{
        //    visualObject.Effects.Add(new AlignCharacterEffect(text, progress, withTransforms, synchronizedProgresses), interval);
        //    return visualObject;
        //}
        //public static T Align<T>(this T visualObject, Interval<int> interval, VisualObject text, Progress progress, bool translateX, bool translateY, RectPoint rectPoint, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        //{
        //    visualObject.Effects.Add(new AlignCharacterEffect(text, progress, translateX, translateY, rectPoint, synchronizedProgresses), interval);
        //    return visualObject;
        //}
        //public static T Align<T>(this T visualObject, Interval<int> interval, VisualObject text, Progress progress, bool withTransforms, bool translateX, bool translateY, RectPoint rectPoint, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        //{
        //    visualObject.Effects.Add(new AlignCharacterEffect(text, progress, withTransforms, translateX, translateY, rectPoint, synchronizedProgresses), interval);
        //    return visualObject;
        //}
        //public static T Align<T>(this T visualObject, AlignCharacterEffect alignCharacterEffect,  Interval<int> interval) where T : VisualObject
        //{
        //    visualObject.Effects.Add(alignCharacterEffect, interval);
        //    return visualObject;
        //}

        //public static T Mask<T>(this T visualObject, Interval<int> interval, Size smallSize, Size bigSize, RectPoint rectPoint, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        //{
        //    visualObject.Effects.Add(new MaskCharacterEffect(smallSize, bigSize, rectPoint, progress, synchronizedProgresses), interval);
        //    return visualObject;
        //}
        //public static T Mask<T>(this T visualObject, Interval<int> interval, Size smallSize, Size bigSize, RectPoint rectPoint, Progress progress, bool withTransforms, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        //{
        //    visualObject.Effects.Add(new MaskCharacterEffect(smallSize, bigSize, rectPoint, progress, withTransforms, synchronizedProgresses), interval);
        //    return visualObject;
        //}
        //public static T Mask<T>(this T visualObject, MaskCharacterEffect maskCharacterEffect,  Interval<int> interval) where T : VisualObject
        //{
        //    visualObject.Effects.Add(maskCharacterEffect, interval);
        //    return visualObject;
        //}

        //public static T Opacity<T>(this T visualObject, Interval<int> interval, double fillOpacity, double strokeOpacity, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        //{
        //    visualObject.Effects.Add(new OpacityCharacterEffect(fillOpacity, strokeOpacity, progress, synchronizedProgresses), interval);
        //    return visualObject;
        //}
        //public static T Opacity<T>(this T visualObject, Interval<int> interval, double fillOpacity, double strokeOpacity, Progress progress, bool withTransforms, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        //{
        //    visualObject.Effects.Add(new OpacityCharacterEffect(fillOpacity, strokeOpacity, progress, withTransforms, synchronizedProgresses), interval);
        //    return visualObject;
        //}
        //public static T Opacity<T>(this T visualObject, OpacityCharacterEffect opacityCharacterEffect,  Interval<int> interval) where T : VisualObject
        //{
        //    visualObject.Effects.Add(opacityCharacterEffect, interval);
        //    return visualObject;
        //}

        //public static T Size<T>(this T visualObject, Interval<int> interval, Size newSize, RectPoint rectPoint, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        //{
        //    visualObject.Effects.Add(new SizeCharacterEffect(newSize, rectPoint, progress, synchronizedProgresses), interval);
        //    return visualObject;
        //}
        //public static T Size<T>(this T visualObject, Interval<int> interval, Size newSize, RectPoint rectPoint, Progress progress, bool scaleX, bool scaleY, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        //{
        //    visualObject.Effects.Add(new SizeCharacterEffect(newSize, rectPoint, progress, scaleX, scaleY, synchronizedProgresses), interval);
        //    return visualObject;
        //}
        //public static T Size<T>(this T visualObject, Interval<int> interval, Size newSize, RectPoint rectPoint, Progress progress, bool scaleX, bool scaleY, bool withTransforms, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        //{
        //    visualObject.Effects.Add(new SizeCharacterEffect(newSize, rectPoint, progress, scaleX, scaleY, withTransforms, synchronizedProgresses), interval);
        //    return visualObject;
        //}
        //public static T Size<T>(this T visualObject, SizeCharacterEffect sizeCharacterEffect,  Interval<int> interval) where T : VisualObject
        //{
        //    visualObject.Effects.Add(sizeCharacterEffect, interval);
        //    return visualObject;
        //}

        //public static T StrokeThickness<T>(this T visualObject, Interval<int> interval, double strokeThickness, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        //{
        //    visualObject.Effects.Add(new StrokeThicknessCharacterEffect(strokeThickness, progress, synchronizedProgresses), interval);
        //    return visualObject;
        //}
        //public static T StrokeThickness<T>(this T visualObject, Interval<int> interval, double strokeThickness, Progress progress, bool withTransforms, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        //{
        //    visualObject.Effects.Add(new StrokeThicknessCharacterEffect(strokeThickness, progress, withTransforms, synchronizedProgresses), interval);
        //    return visualObject;
        //}
        //public static T StrokeThickness<T>(this T visualObject, StrokeThicknessCharacterEffect strokeThicknessCharacterEffect,  Interval<int> interval) where T : VisualObject
        //{
        //    visualObject.Effects.Add(strokeThicknessCharacterEffect, interval);
        //    return visualObject;
        //}

        //public static T TranslateAlongPath<T>(this T visualObject, Interval<int> interval, Geometry geometry, Func<Point, Vector> translation, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        //{
        //    visualObject.Effects.Add(new TranslateAlongPathCharacterEffect(geometry, translation, progress, synchronizedProgresses), interval);
        //    return visualObject;
        //}
        //public static T TranslateAlongPath<T>(this T visualObject, Interval<int> interval, Geometry geometry, Func<Point, Vector> translation, Progress progress, bool withTransforms, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        //{
        //    visualObject.Effects.Add(new TranslateAlongPathCharacterEffect(geometry, translation, progress, withTransforms, synchronizedProgresses), interval);
        //    return visualObject;
        //}
        //public static T TranslateAlongPath<T>(this T visualObject, Interval<int> interval, PathGeometry geometry, Func<Point, Vector> translation, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        //{
        //    visualObject.Effects.Add(new TranslateAlongPathCharacterEffect(geometry, translation, progress, synchronizedProgresses), interval);
        //    return visualObject;
        //}
        //public static T TranslateAlongPath<T>(this T visualObject, Interval<int> interval, PathGeometry geometry, Func<Point, Vector> translation, Progress progress, bool withTransforms, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        //{
        //    visualObject.Effects.Add(new TranslateAlongPathCharacterEffect(geometry, translation, progress, withTransforms, synchronizedProgresses), interval);
        //    return visualObject;
        //}
        //public static T TranslateAlongPath<T>(this T visualObject, TranslateAlongPathCharacterEffect translateAlongPathCharacterEffect,  Interval<int> interval) where T : VisualObject
        //{
        //    visualObject.Effects.Add(translateAlongPathCharacterEffect, interval);
        //    return visualObject;
        //}

        //public static T Translate<T>(this T visualObject, Interval<int> interval, Vector vector, bool inTranslation, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        //{
        //    visualObject.Effects.Add(new TranslateCharacterEffect(vector, inTranslation, progress, synchronizedProgresses), interval);
        //    return visualObject;
        //}
        //public static T Translate<T>(this T visualObject, Interval<int> interval, Vector vector, bool inTranslation, Progress progress, bool withTransforms, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        //{
        //    visualObject.Effects.Add(new TranslateCharacterEffect(vector, inTranslation, progress, withTransforms, synchronizedProgresses), interval);
        //    return visualObject;
        //}
        //public static T Translate<T>(this T visualObject, TranslateCharacterEffect translateCharacterEffect,  Interval<int> interval) where T : VisualObject
        //{
        //    visualObject.Effects.Add(translateCharacterEffect, interval);
        //    return visualObject;
        //}

        //public static T Scale<T>(this T visualObject, Interval<int> interval, double scaleX, double scaleY, RectPoint rectPoint, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        //{
        //    visualObject.Effects.Add(new ScaleCharacterEffect(scaleX, scaleY, rectPoint, progress, synchronizedProgresses), interval);
        //    return visualObject;
        //}
        //public static T Scale<T>(this T visualObject, Interval<int> interval, double scaleX, double scaleY, RectPoint rectPoint, Progress progress, bool withTransforms, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        //{
        //    visualObject.Effects.Add(new ScaleCharacterEffect(scaleX, scaleY, rectPoint, progress, withTransforms, synchronizedProgresses), interval);
        //    return visualObject;
        //}
        //public static T Scale<T>(this T visualObject, ScaleCharacterEffect scaleCharacterEffect,  Interval<int> interval) where T : VisualObject
        //{
        //    visualObject.Effects.Add(scaleCharacterEffect, interval);
        //    return visualObject;
        //}

        //public static T Write<T>(this T visualObject, Interval<int> interval, bool reverse, double strokeThickness, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        //{
        //    visualObject.Effects.Add(new WriteCharacterEffect(reverse, strokeThickness, progress, synchronizedProgresses), interval);
        //    return visualObject;
        //}
        //public static T Write<T>(this T visualObject, Interval<int> interval, bool reverse, double strokeThickness, Progress progress, bool withTransforms, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        //{
        //    visualObject.Effects.Add(new WriteCharacterEffect(reverse, strokeThickness, progress, withTransforms, synchronizedProgresses), interval);
        //    return visualObject;
        //}
        //public static T Write<T>(this T visualObject, WriteCharacterEffect writeCharacterEffect,  Interval<int> interval) where T : VisualObject
        //{
        //    visualObject.Effects.Add(writeCharacterEffect, interval);
        //    return visualObject;
        //}


        //public static T Stroke<T>(this T visualObject, Interval<int> interval, bool reverse, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        //{
        //    visualObject.Effects.Add(new StrokeCharacterEffect(reverse, progress, synchronizedProgresses), interval);
        //    return visualObject;
        //}
        //public static T Stroke<T>(this T visualObject, Interval<int> interval, bool reverse, Progress progress, bool withTransforms, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        //{
        //    visualObject.Effects.Add(new StrokeCharacterEffect(reverse, progress, withTransforms, synchronizedProgresses), interval);
        //    return visualObject;
        //}
        //public static T Stroke<T>(this T visualObject, StrokeCharacterEffect strokeCharacterEffect,  Interval<int> interval) where T : VisualObject
        //{
        //    visualObject.Effects.Add(strokeCharacterEffect, interval);
        //    return visualObject;
        //}

        //#endregion

        public static T Brace<T>(this T visualObject, int openingBraceIndex, int contentLength, Progress progress, params SynchronizedProgress[] synchronizedProgresses) where T : VisualObject
        {
            int closingBraceIndex = openingBraceIndex + contentLength + 1;
            return visualObject.Size((openingBraceIndex, openingBraceIndex + 1), default, RectPoint.Left, true, true, progress, synchronizedProgresses).Size((closingBraceIndex, closingBraceIndex + 1), default, RectPoint.Left, true, true, progress, synchronizedProgresses);
        }

        #region Animate

        public static T AddTo<T>(this T characterEffect, SynchronizedProgress synchronizedProgress) where T : CharacterEffect
        {
            synchronizedProgress.Objects.Add(characterEffect);
            return characterEffect;
        }

        public static Task<T> Animate<T>(this T visualObject, bool destroy, TimeSpan duration, IEasingFunction easingFunction, IDictionary<CharacterEffect, Interval<int>> characterEffects, RepeatBehavior repeatBehavior = default, bool autoReverse = false, bool isCumulative = false, int? fps = FPS) where T : VisualObject => Animate(visualObject, destroy, duration, 0, 1, easingFunction, characterEffects, repeatBehavior, autoReverse, isCumulative, fps);
        public static async Task<T> Animate<T>(this T visualObject, bool destroy, TimeSpan duration, double from, double to, IEasingFunction easingFunction, IDictionary<CharacterEffect, Interval<int>> characterEffects, RepeatBehavior repeatBehavior = default, bool autoReverse = false, bool isCumulative = false, int? fps = FPS) where T : VisualObject
        {
            var progress = characterEffects.Select(effect =>
            {
                visualObject.Effects.Add(effect);
                return effect.Key.Progress;
            }).ToArray();

            await Animating.Animate(null, value => characterEffects.ForEach((effect, i) => effect.Key.Progress = progress[i].ChangeValue(value)), from, to, duration, repeatBehavior, autoReverse, isCumulative, easingFunction, fps);

            if (destroy) characterEffects.ForEach(effect => effect.Key.Destroy());
            return visualObject;
        }
        public static Task<T> Animate<T>(this T visualObject, bool destroy, TimeSpan duration, IEasingFunction easingFunction, params (CharacterEffect, Interval<int>)[] characterEffects) where T : VisualObject => Animate(visualObject, destroy, duration, 0, 1, easingFunction, characterEffects.ToDictionary());
        public static Task<T> Animate<T>(this T visualObject, bool destroy, TimeSpan duration, double from, double to, IEasingFunction easingFunction, params (CharacterEffect, Interval<int>)[] characterEffects) where T : VisualObject => Animate(visualObject, destroy, duration, from, to, easingFunction, characterEffects.ToDictionary(), default, false, false, FPS);

        public static async Task ReColor(this VisualObject visualObject, Interval<int> interval, Brush fill, Pen stroke, double secondsDelay = 0.5)
        {
            var effect = new ColorCharacterEffect { Fill = fill, Stroke = stroke };
            visualObject.Color(interval, effect);

            await Animating.Animate(null, value => effect.Progress = value, 0.0, 1.0, TimeSpan.FromSeconds(secondsDelay), default, false, false, null, FPS);
        }

        public static Task<T> Write<T>(this T visualObject, bool small, bool reverse, double strokeThickness = 1, double seconds = double.NaN, double lagFactor = double.NaN) where T : VisualObject => Write(visualObject, PositiveReals, small, reverse, strokeThickness, seconds, lagFactor);
        public static async Task<T> Write<T>(this T visualObject, Interval<int> interval, bool small, bool reverse, double strokeThickness = 1, double seconds = double.NaN, double lagFactor = double.NaN) where T : VisualObject
        {
            if (double.IsNaN(seconds)) seconds = small ? 1.15 : 1.6;
            if (double.IsNaN(lagFactor))
            {
                double minLagFactor = small ? 1.5 : 2;
                lagFactor = Math.Max(seconds - 1, minLagFactor);
            }

            await visualObject.Animate(true, TimeSpan.FromSeconds(seconds), null, (new WriteCharacterEffect { Reverse = reverse, StrokeThickness = strokeThickness, Progress = new Progress(0, ProgressMode.LaggedStart, lagFactor) }, interval));
            return visualObject;
        }

        public static Task<T> UnWrite<T>(this T visualObject, bool small, bool reverse, double strokeThickness = 1, double seconds = double.NaN, double lagFactor = double.NaN) where T : VisualObject => UnWrite(visualObject, PositiveReals, small, reverse, strokeThickness, seconds, lagFactor);
        public static async Task<T> UnWrite<T>(this T visualObject, Interval<int> interval, bool small, bool reverse, double strokeThickness = 1, double seconds = double.NaN, double lagFactor = double.NaN) where T : VisualObject
        {
            if (double.IsNaN(seconds)) seconds = small ? 1.15 : 1.6;
            if (double.IsNaN(lagFactor))
            {
                double minLagFactor = small ? 1.5 : 2;
                lagFactor = Math.Max(seconds - 1, minLagFactor);
            }

            await visualObject.Animate(false, TimeSpan.FromSeconds(seconds), 1, 0, null, (new WriteCharacterEffect { Reverse = reverse, StrokeThickness = strokeThickness, Progress = new Progress(1, ProgressMode.LaggedStart, lagFactor) }, interval));
            return visualObject;
        }

        #endregion

        #endregion

        #region PlaneZoom

        public static PlaneZoom Zoom(this Plane plane, bool overAxesNumbers, MathRect inputRange, Rect outputRange, Geometry clip, params VisualObject[] children) => ZoomCore(plane, overAxesNumbers, inputRange, outputRange, clip, children);
        public static PlaneZoom Zoom(this Plane plane, bool overAxesNumbers, MathRect inputRange, Rect outputRange, Geometry clip, IEnumerable<VisualObject> children) => ZoomCore(plane, overAxesNumbers, inputRange, outputRange, clip, children);

        private static PlaneZoom ZoomCore(this Plane plane, bool overAxesNumbers, MathRect inputRange, Rect outputRange, Geometry clip, IEnumerable<VisualObject> children)
        {
            var visualObjects = new VisualObjectCollection(new VisualObject[] { plane.Grid, plane.Axes });

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

            var zoom = new PlaneZoom { InputRange = inputRange, OutputRange = outputRange, Clip = clip, Children = visualObjects };
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
