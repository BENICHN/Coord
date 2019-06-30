using BenLib.Standard;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Coord
{
    public class AutoPenCharacterEffect : CharacterEffect
    {
        protected override Freezable CreateInstanceCore() => new AutoPenCharacterEffect();

        public Pen Template { get => (Pen)GetValue(TemplateProperty); set => SetValue(TemplateProperty, value); }
        public static readonly DependencyProperty TemplateProperty = CreateProperty<AutoPenCharacterEffect, Pen>(true, true, true, "Template");

        protected override void ApplyCore(IReadOnlyCollection<Character> characters, Interval<int> interval, in ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            var sample = Template?.CloneCurrentValue();
            if (sample != null)
            {
                foreach (var character in characters.SubCollection(interval, true))
                {
                    if (character.Stroke == null)
                    {
                        sample.Brush = character.Fill;
                        character.Stroke = sample.CloneCurrentValue();
                    }
                }
            }
        }
    }
}
