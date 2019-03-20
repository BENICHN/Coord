using BenLib;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Coord
{
    /// <summary>
    /// Met progressivement à l'échelle une sous-collection de <see cref="Character"/> par un coefficient spécifié
    /// </summary>
    public class ScaleCharacterEffect : CharacterEffect
    {
        public ScaleCharacterEffect(IntInterval interval, double scaleX, double scaleY, RectPoint rectPoint, Progress progress, params SynchronizedProgress[] synchronizedProgresses) : this(interval, scaleX, scaleY, rectPoint, progress, true, synchronizedProgresses) { }
        public ScaleCharacterEffect(IntInterval interval, double scaleX, double scaleY, RectPoint rectPoint, Progress progress, bool withTransforms, params SynchronizedProgress[] synchronizedProgresses) : base(interval, progress, withTransforms, synchronizedProgresses)
        {
            ScaleX = scaleX;
            ScaleY = scaleY;
            RectPoint = rectPoint;
        }

        private double m_scaleX;
        public double ScaleX
        {
            get => m_scaleX;
            set
            {
                m_scaleX = value;
                NotifyChanged();
            }
        }

        private double m_scaleY;
        public double ScaleY
        {
            get => m_scaleY;
            set
            {
                m_scaleY = value;
                NotifyChanged();
            }
        }

        private RectPoint m_rectPoint;
        public RectPoint RectPoint
        {
            get => m_rectPoint;
            set
            {
                m_rectPoint = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Applique l'effet à une collection de <see cref="Character"/>
        /// </summary>
        /// <param name="characters">Collection de <see cref="Character"/> sur qui appliquer l'effet</param>
        protected override void ApplyCore(IReadOnlyCollection<Character> characters, ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            var chars = characters.SubCollection(Interval, true).ToArray();
            chars.ScaleAt(ScaleX, ScaleY, RectPoint.GetPoint(chars.Geometry().Bounds), EasedProgress).Enumerate();
        }

        /// <summary>
        /// Crée un une copie des valeurs de l'instance actuelle de <see cref="ScaleCharacterEffect"/>
        /// </summary>
        /// <returns>Copie des valeurs de l'instance actuelle</returns>
        public override CharacterEffect Clone() => new ScaleCharacterEffect(Interval, ScaleX, ScaleY, RectPoint, Progress, WithTransforms);
    }
}
