using BenLib;

namespace Coord
{
    /// <summary>
    /// Objet visuel du plan décrit par une équation de droite
    /// </summary>
    public abstract class LineVisualObjectBase : VisualObject
    {
        /// <summary>
        /// Équation de droite qui décrit ce <see cref="LineVisualObjectBase"/>
        /// </summary>
        public abstract LinearEquation Equation { get; }
    }
}
