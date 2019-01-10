namespace Coord
{
    /// <summary>
    /// Détermine le centre et le rayon d'un cercle de centre et de rayon donnés
    /// </summary>
    public class CenterRadiusCircleDefinition : CircleDefinition
    {
        /// <summary>
        /// Centre du cercle du plan
        /// </summary>
        public new PointVisualObject Center
        {
            get => base.Center;
            set
            {
                base.Center = value;
                RegisterCompute(base.Center);
                ComputeAndNotifyChanged();
            }
        }

        /// <summary>
        /// Rayon du cercle du plan
        /// </summary>
        public new double Radius
        {
            get => base.Radius;
            set
            {
                base.Radius = value;
                ComputeAndNotifyChanged();
            }
        }

        public CenterRadiusCircleDefinition(PointVisualObject center, double radius)
        {
            Center = center;
            Radius = radius;
        }

        /// <summary>
        /// Calcule et met en cache les propriétés fondamentales du cercle du plan
        /// </summary>
        protected override void Compute() { }
    }
}
