using System.Windows.Media;

namespace Coord
{
    public static class DrawingContextExtensions
    {
        public static void DrawCharacter(this DrawingContext drawingContext, Character character, bool transform = false, bool? release = false)
        {
            bool transformed = character.Transformed;
            if (transform) character.ApplyTransforms();
            drawingContext.DrawGeometry(character.Fill, character.IsSelected ? VisualObject.SelectionStroke : character.Stroke, character.Geometry);
            if (release == true || release == null && transformed) character.ReleaseTransforms();
        }
    }
}
