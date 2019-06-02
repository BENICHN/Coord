using BenLib.Standard;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using static BenLib.Framework.NumFramework;

namespace Coord
{
    /// <summary>
    /// Colore progressivement une sous-collection de <see cref="CharacterEffect"/>
    /// </summary>
    public class ColorCharacterEffect : CharacterEffect, ICoordEditable
    {
        IEnumerable<(string Description, DependencyProperty Property)> ICoordEditable.Properties
        {
            get
            {
                yield return ("Fill", FillProperty);
                yield return ("Stroke", StrokeProperty);
                yield return ("Progress", ProgressProperty);
                yield return ("WithTransforms", WithTransformsProperty);
            }
        }

        /// <summary>
        /// Remplissage des <see cref="Character"/>
        /// </summary>
        public Brush Fill { get => (Brush)GetValue(FillProperty); set => SetValue(FillProperty, value); }
        public static readonly DependencyProperty FillProperty = CreateProperty<Brush>(true, true, "Fill", typeof(ColorCharacterEffect));

        /// <summary>
        /// Contour des <see cref="Character"/>
        /// </summary>
        public PlanePen Stroke { get => (PlanePen)GetValue(StrokeProperty); set => SetValue(StrokeProperty, value); }
        public static readonly DependencyProperty StrokeProperty = CreateProperty<PlanePen>(true, true, "Stroke", typeof(ColorCharacterEffect));

        /// <summary>
        /// Applique l'effet à une collection de <see cref="Character"/>
        /// </summary>
        /// <param name="characters">Collection de <see cref="Character"/> sur qui appliquer l'effet</param>
        protected override void ApplyCore(IReadOnlyCollection<Character> characters, Interval<int> interval, in ReadOnlyCoordinatesSystemManager coordinatesSystemManager) => characters.SubCollection(interval, true).ForEach((character, i) => ApplyOn(character, Fill, Stroke, EasedProgress.Get(i, RealLength)));

        public static void ApplyOn(Character character, Brush fill, PlanePen stroke, double progress)
        {
            var baseBrush = fill?.CloneCurrentValue();
            var basePen = stroke?.CloneCurrentValue();

            var brush = baseBrush ?? Brushes.Transparent;
            var pen = basePen ?? new PlanePen(Brushes.Transparent, 0);

            if (progress == 0) return;
            if (progress == 1)
            {
                character.Fill = baseBrush;
                character.Stroke = basePen;
                return;
            }

            var newFill = character.Fill?.CloneCurrentValue() ?? Brushes.Transparent;
            var newStroke = character.Stroke?.CloneCurrentValue() ?? new PlanePen(Brushes.Transparent, 0);

            if (newFill is SolidColorBrush fillC && brush is SolidColorBrush brushC) character.Fill = new SolidColorBrush(Interpolate(fillC.Color, brushC.Color, progress));
            if (newStroke.Brush is SolidColorBrush strokeC && pen.Brush is SolidColorBrush penC) character.Stroke = new PlanePen(new SolidColorBrush(Interpolate(strokeC.Color, penC.Color, progress)), Num.Interpolate(newStroke.Thickness, pen.Thickness, progress));
        }
    }
}
