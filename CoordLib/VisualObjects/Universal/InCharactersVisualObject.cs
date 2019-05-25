using BenLib.Standard;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Coord
{
    public class InCharactersVisualObject : VisualObject
    {
        public override string Type => "InCharacters";

        public VisualObject VisualObject { get => (VisualObject)GetValue(VisualObjectProperty); set => SetValue(VisualObjectProperty, value); }
        public static readonly DependencyProperty VisualObjectProperty = CreateProperty<VisualObject>(true, true, "VisualObject", typeof(InCharactersVisualObject));

        public Interval<int> Interval { get => (Interval<int>)GetValue(IntervalProperty); set => SetValue(IntervalProperty, value); }
        public static readonly DependencyProperty IntervalProperty = CreateProperty<Interval<int>>(true, true, "Interval", typeof(InCharactersVisualObject), Interval<int>.PositiveReals);

        protected override IReadOnlyCollection<Character> GetCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager) => VisualObject.GetTransformedCharacters(coordinatesSystemManager).SubCollection(Interval, true).Select(c => { c.Creator = this; return c; }).ToArray();
    }
}
