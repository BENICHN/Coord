using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Coord
{
    public class AxesVisualObject : VisualObject
    {
        protected override Freezable CreateInstanceCore() => new AxesVisualObject();

        public override string Type => "Axes";

        public AxesNumbers Numbers { get => (AxesNumbers)GetValue(NumbersProperty); set => SetValue(NumbersProperty, value); }
        public static readonly DependencyProperty NumbersProperty = CreateProperty<AxesVisualObject, AxesNumbers>(true, true, true, "Numbers");

        public AxesDirection Direction { get => (AxesDirection)GetValue(DirectionProperty); set => SetValue(DirectionProperty, value); }
        public static readonly DependencyProperty DirectionProperty = CreateProperty<AxesVisualObject, AxesDirection>(true, true, true, "Direction");

        protected override IEnumerable<Character> GetCharactersCore(ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            var direction = Direction;
            var outRange = coordinatesSystemManager.OutputRange;
            var inRange = coordinatesSystemManager.InputRange;

            var center = coordinatesSystemManager.OrthonormalOrigin;
            double demiThickness = Stroke == null ? 0 : Stroke.Thickness / 2;

            if ((direction == AxesDirection.Horizontal || direction == AxesDirection.Both) && outRange.HeightContainsRange(center.Y - demiThickness, center.Y + demiThickness, false)) yield return Character.Line(coordinatesSystemManager.ComputeOutCoordinates(new Point(inRange.Left, 0)), coordinatesSystemManager.ComputeOutCoordinates(new Point(inRange.Right, 0))).Color(Fill, Stroke);
            if ((direction == AxesDirection.Vertical || direction == AxesDirection.Both) && outRange.WidthContainsRange(center.X - demiThickness, center.X + demiThickness, false)) yield return Character.Line(coordinatesSystemManager.ComputeOutCoordinates(new Point(0, inRange.Bottom)), coordinatesSystemManager.ComputeOutCoordinates(new Point(0, inRange.Top))).Color(Fill, Stroke);
        }
    }

    public enum AxesDirection { None, Horizontal, Vertical, Both }
}
