using System.Collections.Generic;
using System.Windows;

namespace Coord
{
    /// <summary>
    /// Détermine un point du plan grâce à un <see cref="Point"/>
    /// </summary>
    public class PointPointDefinition : PointDefinition, ICoordEditable
    {
        public override string Type => "PointPointDefinition";
        IEnumerable<(string Description, DependencyProperty Property)> ICoordEditable.Properties { get { yield return ("InPoint", InPointProperty); } }

        /// <summary>
        /// Point du plan
        /// </summary>
        public new Point InPoint { get => (Point)GetValue(InPointProperty); set => SetValue(InPointProperty, value); }
        public static readonly DependencyProperty InPointProperty = CreateProperty<Point>(true, true, "InPoint", typeof(PointPointDefinition));

        protected override void OnChanged() => base.InPoint = InPoint;
    }
}
