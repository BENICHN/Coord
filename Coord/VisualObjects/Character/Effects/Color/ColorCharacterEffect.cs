using BenLib;
using BenLib.WPF;
using System.Collections.Generic;
using System.Windows.Media;

namespace Coord
{
    /// <summary>
    /// Colore progressivement une sous-collection de <see cref="CharacterEffect"/>
    /// </summary>
    public class ColorCharacterEffect : CharacterEffect
    {
        private Brush m_fill;
        private Pen m_stroke;

        /// <summary>
        /// Remplissage des <see cref="Character"/>
        /// </summary>
        public Brush Fill
        {
            get => m_fill;
            set
            {
                UnRegister(m_fill);
                m_fill = value;
                Register(m_fill);
                NotifyChanged();
            }
        }

        /// <summary>
        /// Contour des <see cref="Character"/>
        /// </summary>
        public Pen Stroke
        {
            get => m_stroke; set
            {
                UnRegister(m_stroke);
                m_stroke = value;
                Register(m_stroke);
                NotifyChanged();
            }
        }

        private bool m_inPen;
        public bool InPen
        {
            get => m_inPen;
            set
            {
                m_inPen = value;
                NotifyChanged();
            }
        }

        public ColorCharacterEffect(IntInterval interval, Brush fill, Pen stroke, bool inPen, Progress progress, params SynchronizedProgress[] synchronizedProgresses) : this(interval, fill, stroke, inPen, progress, false, synchronizedProgresses) { }
        public ColorCharacterEffect(IntInterval interval, Brush fill, Pen stroke, bool inPen, Progress progress, bool withTransforms, params SynchronizedProgress[] synchronizedProgresses) : base(interval, progress, withTransforms, synchronizedProgresses)
        {
            Fill = fill;
            Stroke = stroke;
            InPen = inPen;
        }

        /// <summary>
        /// Applique l'effet à une collection de <see cref="Character"/>
        /// </summary>
        /// <param name="characters">Collection de <see cref="Character"/> sur qui appliquer l'effet</param>
        protected override void ApplyCore(IReadOnlyCollection<Character> characters, CoordinatesSystemManager coordinatesSystemManager)
        {
            var stroke = InPen ? Stroke?.Edit(pen => pen.Thickness = Stroke?.Thickness * coordinatesSystemManager.WidthRatio ?? 0) : Stroke;
            characters.SubCollection(Interval).ForEach((i, character) => ApplyOn(character, Fill, stroke, EasedProgress.Get(i, RealLength)));
        }

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

            if (newFill is SolidColorBrush fillC && brush is SolidColorBrush brushC) character.Fill = new SolidColorBrush(Num.Interpolate(fillC.Color, brushC.Color, progress));
            if (newStroke.Brush is SolidColorBrush strokeC && pen.Brush is SolidColorBrush penC) character.Stroke = new Pen(new SolidColorBrush(Num.Interpolate(strokeC.Color, penC.Color, progress)), Num.Interpolate(newStroke.Thickness, pen.Thickness, progress));
        }

        /// <summary>
        /// Crée un une copie des valeurs de l'instance actuelle de <see cref="ColorCharacterEffect"/>
        /// </summary>
        /// <returns>Copie des valeurs de l'instance actuelle</returns>
        public override CharacterEffect Clone() => new ColorCharacterEffect(Interval, Fill?.CloneCurrentValue(), Stroke?.CloneCurrentValue(), InPen, Progress);
    }
}
