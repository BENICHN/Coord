using BenLib.Standard;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace Coord
{
    public class InCharactersVisualObject : VisualObject
    {
        public override string Type => VisualObject.Type + "InCharacters";

        public VisualObject VisualObject { get => (VisualObject)GetValue(VisualObjectProperty); set => SetValue(VisualObjectProperty, value); }
        public static readonly DependencyProperty VisualObjectProperty = CreateProperty<VisualObject>(true, true, "VisualObject", typeof(InCharactersVisualObject));

        public Interval<int> Interval { get => (Interval<int>)GetValue(IntervalProperty); set => SetValue(IntervalProperty, value); }
        public static readonly DependencyProperty IntervalProperty = CreateProperty<Interval<int>>(true, true, "Interval", typeof(InCharactersVisualObject), Interval<int>.PositiveReals);

        protected override IReadOnlyCollection<Character> GetCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager) => VisualObject.GetTransformedCharacters(coordinatesSystemManager).SubCollection(Interval, true)/*.Select(c => { c.Creator = this; return c; })*/.ToArray();
    }

    public class InCharactersVisualObjectGroup : VisualObjectGroupBase
    {
        public override string Type => "InCharactersGroup";

        public ObservableRangeCollection<Interval<int>> Intervals { get => (ObservableRangeCollection<Interval<int>>)GetValue(IntervalsProperty); set => SetValue(IntervalsProperty, value); }
        public int Select { get; set; }

        public static readonly DependencyProperty IntervalsProperty = CreateProperty<ObservableRangeCollection<Interval<int>>>(true, true, "Intervals", typeof(InCharactersVisualObjectGroup));

        protected override IReadOnlyCollection<Character> GetCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager) => Children.SelectMany((visualObject, i) => visualObject.GetTransformedCharacters(coordinatesSystemManager).SubCollection(Intervals[i], true)).ToArray();
    }
}
