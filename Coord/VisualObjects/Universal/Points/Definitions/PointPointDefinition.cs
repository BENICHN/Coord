using System.Windows;

namespace Coord
{
    /// <summary>
    /// Détermine un point du plan grâce à un <see cref="Point"/>
    /// </summary>
    public class PointPointDefinition : PointDefinition
    {
        /// <summary>
        /// Point du plan
        /// </summary>
        public new Point InPoint
        {
            get => base.InPoint;
            set
            {
                base.InPoint = value;
                NotifyChanged();
            }
        }

        public PointPointDefinition(Point inPoint) => InPoint = inPoint;

        /// <summary>
        /// Calcule et met en cache les propriétés fondamentales du point du plan
        /// </summary>
        protected override void Compute() { }
    }
}
