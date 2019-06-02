using System;
using System.Collections.Generic;
using System.Windows;

namespace Coord
{
    /// <summary>
    /// Détermine les propriétés fondamentales d'un <see cref="PointVisualObject"/> à l'aide d'autres propriétés
    /// </summary>
    public abstract class PointDefinition : NotifyObject, ICoordSelectable
    {
        public abstract string Type { get; }

        IEnumerable<string> ICoordSelectable.Types
        {
            get
            {
                yield return "PointPointDefinition";
                yield return "LineIntersectionPointDefinition";
                yield return "MiddlePointDefinition";
                yield return "TranslationPointDefinition";
            }
        }

        ICoordEditable ICoordSelectable.GetObject(string type) => type switch
        {
            "PointPointDefinition" => new PointPointDefinition(),
            "LineIntersectionPointDefinition" => new LineIntersectionPointDefinition(),
            "MiddlePointDefinition" => new MiddlePointDefinition(),
            "TranslationPointDefinition" => (ICoordEditable)new TranslationPointDefinition(),
            _ => throw new ArgumentException()
        };

        /// <summary>
        /// Point du plan
        /// </summary>
        public Point InPoint { get; protected set; }
    }
}
