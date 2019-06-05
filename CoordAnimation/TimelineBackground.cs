using BenLib.WPF;
using Coord;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace CoordAnimation
{
    public class TimelineBackground : VisualObject
    {
        public override string Type => "TimelineBackground";

        public double HeaderHeight { get => (double)GetValue(HeaderHeightProperty); set => SetValue(HeaderHeightProperty, value); }
        public static readonly DependencyProperty HeaderHeightProperty = CreateProperty(true, true, "HeaderHeight", typeof(TimelineBackground), 1.0);

        protected override IReadOnlyCollection<Character> GetCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            return GetCharactersCore().ToArray();
            IEnumerable<Character> GetCharactersCore()
            {
                yield return Character.Rectangle(coordinatesSystemManager.ComputeOutCoordinates(new Rect(coordinatesSystemManager.InputRange.Left, -HeaderHeight, coordinatesSystemManager.InputRange.Width, HeaderHeight))).Color(FlatBrushes.WetAsphalt);
                foreach (double i in coordinatesSystemManager.GetHorizontalSteps()) yield return Character.Line(coordinatesSystemManager.ComputeOutCoordinates(new Point(i, 0)), coordinatesSystemManager.ComputeOutCoordinates(new Point(i, -HeaderHeight))).Color(new PlanePen(Brushes.White, 1));
            }
        }
    }
}
