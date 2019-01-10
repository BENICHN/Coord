namespace Coord
{
    /// <summary>
    /// Détermine les propriétés fondamentales d'un <see cref="CircleVisualObject"/> à l'aide d'autres propriétés
    /// </summary>
    public abstract class CircleDefinition : VisualObjectDefinition
    {
        /// <summary>
        /// Centre du cercle du plan
        /// </summary>
        public PointVisualObject Center { get; protected set; }

        /// <summary>
        /// Rayon du cercle du plan
        /// </summary>
        public double Radius { get; protected set; } = double.NaN;

        public override string ToString() => $"{Center.ToString()}, {Radius}";
    }
}
