using BenLib.Standard;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace Coord
{
    public class InCharactersVisualObject : VisualObject
    {
        protected override Freezable CreateInstanceCore() => new InCharactersVisualObject();

        public override string Type => (VisualObject?.Type ?? string.Empty) + "InCharacters";

        public VisualObject VisualObject { get => (VisualObject)GetValue(VisualObjectProperty); set => SetValue(VisualObjectProperty, value); }
        public static readonly DependencyProperty VisualObjectProperty = CreateProperty<InCharactersVisualObject, VisualObject>(true, true, true, "VisualObject");

        public Interval<int> Interval { get => (Interval<int>)GetValue(IntervalProperty); set => SetValue(IntervalProperty, value); }
        public static readonly DependencyProperty IntervalProperty = CreateProperty<InCharactersVisualObject, Interval<int>>(true, true, true, "Interval", Interval<int>.PositiveReals);

        protected override IReadOnlyCollection<Character> GetCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager) => VisualObject.GetTransformedCharacters(coordinatesSystemManager).SubCollection(Interval, true).ToArray();
    }

    public class InCharactersVisualObjectGroup : VisualObject
    {
        protected override Freezable CreateInstanceCore() => new InCharactersVisualObjectGroup();

        public override string Type => "InCharactersGroup";

        public ObservableRangeCollection<Interval<int>> Intervals { get => (ObservableRangeCollection<Interval<int>>)GetValue(IntervalsProperty); set => SetValue(IntervalsProperty, value); }
        public static readonly DependencyProperty IntervalsProperty = CreateProperty<InCharactersVisualObjectGroup, ObservableRangeCollection<Interval<int>>>(true, true, true, "Intervals");

        protected override IReadOnlyCollection<Character> GetCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager) => Children.SelectMany((visualObject, i) => visualObject.GetTransformedCharacters(coordinatesSystemManager).SubCollection(Intervals[i], true)).ToArray();
    }
}
