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
    public class ColorCharacterEffect : CharacterEffect
    {
        protected override Freezable CreateInstanceCore() => new ColorCharacterEffect();

        /// <summary>
        /// Remplissage des <see cref="Character"/>
        /// </summary>
        public Brush Fill { get => (Brush)GetValue(FillProperty); set => SetValue(FillProperty, value); }
        public static readonly DependencyProperty FillProperty = CreateProperty<ColorCharacterEffect, Brush>(true, true, true, "Fill");

        /// <summary>
        /// Contour des <see cref="Character"/>
        /// </summary>
        public Pen Stroke { get => (Pen)GetValue(StrokeProperty); set => SetValue(StrokeProperty, value); }
        public static readonly DependencyProperty StrokeProperty = CreateProperty<ColorCharacterEffect, Pen>(true, true, true, "Stroke");

        /// <summary>
        /// Applique l'effet à une collection de <see cref="Character"/>
        /// </summary>
        /// <param name="characters">Collection de <see cref="Character"/> sur qui appliquer l'effet</param>
        protected override void ApplyCore(IReadOnlyCollection<Character> characters, Interval<int> interval, in ReadOnlyCoordinatesSystemManager coordinatesSystemManager) => characters.SubCollection(interval, true).ForEach((character, i) => ApplyOn(character, Fill, Stroke, EasedProgress.Get(i, RealLength)));

        public static void ApplyOn(Character character, Brush fill, Pen stroke, double progress)
        {
            var baseBrush = fill?.CloneCurrentValue();
            var basePen = stroke?.CloneCurrentValue();

            var brush = baseBrush ?? Brushes.Transparent;
            var pen = basePen ?? new Pen(Brushes.Transparent, 0);

            if (progress == 0) return;
            if (progress == 1)
            {
                character.Fill = baseBrush;
                character.Stroke = basePen;
                return;
            }

            var newFill = character.Fill?.CloneCurrentValue() ?? Brushes.Transparent;
            var newStroke = character.Stroke?.CloneCurrentValue() ?? new Pen(Brushes.Transparent, 0);

            if (newFill is SolidColorBrush fillC && brush is SolidColorBrush brushC) character.Fill = new SolidColorBrush(Interpolate(fillC.Color, brushC.Color, progress));
            if (newStroke.Brush is SolidColorBrush strokeC && pen.Brush is SolidColorBrush penC) character.Stroke = new Pen(new SolidColorBrush(Interpolate(strokeC.Color, penC.Color, progress)), Num.Interpolate(newStroke.Thickness, pen.Thickness, progress));
        }
    }
}
