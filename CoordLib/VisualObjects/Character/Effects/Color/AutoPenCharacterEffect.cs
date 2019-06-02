using BenLib.Standard;
using System.Collections.Generic;
using System.Windows;

namespace Coord
{
    public class AutoPenCharacterEffect : CharacterEffect, ICoordEditable
    {
        IEnumerable<(string Description, DependencyProperty Property)> ICoordEditable.Properties
        {
            get
            {
                yield return ("Progress", ProgressProperty);
                yield return ("WithTransforms", WithTransformsProperty);
            }
        }

        public PlanePen Template { get => (PlanePen)GetValue(TemplateProperty); set => SetValue(TemplateProperty, value); }
        public static readonly DependencyProperty TemplateProperty = CreateProperty<PlanePen>(true, true, "Template", typeof(AutoPenCharacterEffect));

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
