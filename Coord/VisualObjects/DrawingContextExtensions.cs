using BenLib.WPF;
using System;
using System.Windows;
using System.Windows.Media;

namespace Coord
{
    public static class DrawingContextExtensions
    {
        public static void DrawCharacter(this DrawingContext drawingContext, Character character, bool transform = true, bool? release = false)
        {
            bool transformed = character.Transformed;
            if (transform) character.ApplyTransforms();
            drawingContext.DrawGeometry(character.Fill, character.Stroke, character.Geometry);
            if (release == true || release == null && transform) character.ReleaseTransforms();
        }

        public static void DrawPoint(this DrawingContext drawingContext, Brush brush, Pen pen, Point center, double radius) => drawingContext.DrawEllipse(brush, pen, center, radius, radius);
        public static void DrawCurve(this DrawingContext drawingContext, Brush brush, Pen pen, Point[] points, bool closed, bool smooth, double smoothValue = 0.75) => drawingContext.DrawGeometry(brush, pen, GeometryHelper.GetCurve(points, closed, smooth, smoothValue));

        //public static void DrawArrow(this DrawingContext drawingContext, Brush brush, Pen pen, Point point, Vector vector, Arrow arrow, ArrowEnd arrowEnd)
        //{
        //    drawingContext.DrawLine(pen, point, point + vector);
        //    drawingContext.DrawGeometry(brush, pen, arrow?.GetGeometry(point, vector, arrowEnd));
        //}
        //public static void DrawArrow(this DrawingContext drawingContext, Brush brush, Pen pen, Point point1, Point point2, Arrow arrow, ArrowEnd arrowEnd) => DrawArrow(drawingContext, brush, pen, point1, point2 - point1, arrow, arrowEnd);
    }
}
