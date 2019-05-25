using BenLib.Standard;
using System;
using System.Linq;
using System.Windows;

namespace Coord
{
    /// <summary>
    /// Détermine les points d'un polygone passant par des <see cref="PointVisualObject"/>
    /// </summary>
    public class PointsPolygonDefinition : PolygonDefinition
    {
        public new NotifyObjectCollection<PointVisualObject> InPoints { get => (NotifyObjectCollection<PointVisualObject>)GetValue(InPointsProperty); set => SetValue(InPointsProperty, value); }
        public static readonly DependencyProperty InPointsProperty = CreateProperty<NotifyObjectCollection<PointVisualObject>>(true, true, "InPoints", typeof(PointsPolygonDefinition));

        protected override void OnChanged() => base.InPoints = InPoints.IsNullOrEmpty() ? Array.Empty<Point>() : InPoints.Select(p => p.Definition.InPoint).ToArray();
    }
}
