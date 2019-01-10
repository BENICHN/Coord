using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace Coord
{
    /// <summary>
    /// Objet visuel composé d'autres objets visuels
    /// </summary>
    public abstract class VisualObjectGroupBase : VisualObject
    {
        private NotifyObjectCollection<VisualObject> m_children;

        public VisualObjectGroupBase() => Children = new NotifyObjectCollection<VisualObject>();
        public VisualObjectGroupBase(NotifyObjectCollection<VisualObject> children) => Children = children;
        public VisualObjectGroupBase(params VisualObject[] children) : this(new NotifyObjectCollection<VisualObject>(children)) { }
        public VisualObjectGroupBase(IEnumerable<VisualObject> children) : this(new NotifyObjectCollection<VisualObject>(children)) { }

        /// <summary>
        /// <see cref="NotifyObjectCollection{T}"/> contenant les objets visuels qui définissent ce <see cref="VisualObjectGroup"/>
        /// </summary>
        public NotifyObjectCollection<VisualObject> Children
        {
            get => m_children;
            set
            {
                UnRegister(m_children);
                m_children = value;
                Register(m_children);
                NotifyChanged();
            }
        }
    }

    public class VisualObjectGroup : VisualObjectGroupBase
    {
        public VisualObjectGroup() { }
        public VisualObjectGroup(NotifyObjectCollection<VisualObject> children) : base(children) { }
        public VisualObjectGroup(params VisualObject[] children) : base(children) { }
        public VisualObjectGroup(IEnumerable<VisualObject> children) : base(children) { }

        public override IReadOnlyCollection<Character> GetCharacters(CoordinatesSystemManager coordinatesSystemManager) => Children.SelectMany(visualObject => visualObject.GetTransformedCharacters(coordinatesSystemManager, false)).ToArray();
    }

    public class VisualObjectRenderer : VisualObjectGroupBase
    {
        public VisualObjectRenderer() { }
        public VisualObjectRenderer(NotifyObjectCollection<VisualObject> children) : base(children) { }
        public VisualObjectRenderer(params VisualObject[] children) : base(children) { }
        public VisualObjectRenderer(IEnumerable<VisualObject> children) : base(children) { }

        public override IReadOnlyCollection<Character> GetCharacters(CoordinatesSystemManager coordinatesSystemManager) => throw new System.NotImplementedException();

        /// <summary>
        /// Appelle successivement la méthode <see cref="VisualObject.Render(DrawingContext, CoordinatesSystemManager)"/> de tous les éléments de <see cref="Children"/>
        /// </summary>
        /// <param name="drawingContext">Dessine dans le <see cref="DrawingVisual"/> associé à l'objet</param>
        /// <param name="coordinatesSystemManager">Système de coordonnées du <see cref="Plane"/></param>
        protected override void RenderCore(DrawingContext drawingContext, CoordinatesSystemManager coordinatesSystemManager) { foreach (var visualObject in Children) visualObject.Render(drawingContext, coordinatesSystemManager); }
    }
}
