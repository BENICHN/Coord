using BenLib;
using System.Collections.Generic;
using System.Windows;

namespace Coord
{
    /// <summary>
    /// </summary>
    public class RotateCharacterEffect : CharacterEffect
    {
        private Point m_center;
        private bool m_in;
        private double m_angle;

        public RotateCharacterEffect(IntInterval interval, Point center, double angle, bool inRotation, Progress progress, params SynchronizedProgress[] synchronizedProgresses) : this(interval, center, angle, inRotation, progress, true, synchronizedProgresses) { }
        public RotateCharacterEffect(IntInterval interval, Point center, double angle, bool inRotation, Progress progress, bool withTransforms, params SynchronizedProgress[] synchronizedProgresses) : base(interval, progress, withTransforms, synchronizedProgresses)
        {
            Center = center;
            In = inRotation;
            Angle = angle;
        }

        /// <summary>
        /// </summary>
        public Point Center
        {
            get => m_center;
            set
            {
                m_center = value;
                NotifyChanged();
            }
        }
        public bool In
        {
            get => m_in;
            set
            {
                m_in = value;
                NotifyChanged();
            }
        }
        public double Angle
        {
            get => m_angle;
            set
            {
                m_angle = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Applique l'effet à une collection de <see cref="Character"/>
        /// </summary>
        /// <param name="characters">Collection de <see cref="Character"/> sur qui appliquer l'effet</param>
        protected override void ApplyCore(IReadOnlyCollection<Character> characters, ReadOnlyCoordinatesSystemManager coordinatesSystemManager) => characters.SubCollection(Interval, true).RotateAt(Angle, In ? coordinatesSystemManager.ComputeOutCoordinates(Center) : Center, EasedProgress).Enumerate();

        /// <summary>
        /// Crée un une copie des valeurs de l'instance actuelle de <see cref="RotateCharacterEffect"/>
        /// </summary>
        /// <returns>Copie des valeurs de l'instance actuelle</returns>
        public override CharacterEffect Clone() => new RotateCharacterEffect(Interval, Center, Angle, In, Progress, WithTransforms);
    }
}
