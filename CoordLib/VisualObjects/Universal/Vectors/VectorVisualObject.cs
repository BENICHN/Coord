using BenLib.WPF;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using static Coord.VisualObjects;

namespace Coord
{
    /// <summary>
    /// Objet visuel représentant un vecteur du plan
    /// </summary>
    public class VectorVisualObject : VisualObject
    {
        public override string Type => Definition switch
        {
            VectorVectorDefinition _ => "VectorVector",
            PointPointVectorDefinition _ => "PointPointVector",
            OperationsVectorDefinition _ => "OperationsVector",
            MultiOperationsVectorDefinition _ => "MultiOperationsVector",
            _ => "Vector"
        };

        /// <summary>
        /// Détermine les propriétés fondamentales de ce <see cref="VectorVisualObject"/>
        /// </summary>
        public VectorDefinition Definition { get => (VectorDefinition)GetValue(DefinitionProperty); set => SetValue(DefinitionProperty, value); }
        public static readonly DependencyProperty DefinitionProperty = CreateProperty<VectorDefinition>(true, true, "Definition", typeof(VectorVisualObject));

        /// <summary>
        /// Origine dans le plan du représentant de ce vecteur
        /// </summary>
        public PointVisualObject InAnchorPoint { get => (PointVisualObject)GetValue(InAnchorPointProperty); set => SetValue(InAnchorPointProperty, value); }
        public static readonly DependencyProperty InAnchorPointProperty = CreateProperty<PointVisualObject>(true, true, "InAnchorPoint", typeof(VectorVisualObject));

        /// <summary>
        /// Dessine une flèche à l'extrémité d'une ligne à l'aide d'un <see cref="DrawingContext"/>
        /// </summary>
        public Arrow Arrow { get => (Arrow)GetValue(ArrowProperty); set => SetValue(ArrowProperty, value); }
        public static readonly DependencyProperty ArrowProperty = CreateProperty<Arrow>(true, true, "Arrow", typeof(VectorVisualObject), new TriangleArrow());

        /// <summary>
        /// Extrémité de la ligne possède une flèche
        /// </summary>
        public ArrowEnd ArrowEnd { get => (ArrowEnd)GetValue(ArrowEndProperty); set => SetValue(ArrowEndProperty, value); }
        public static readonly DependencyProperty ArrowEndProperty = CreateProperty(true, true, "ArrowEnd", typeof(VectorVisualObject), ArrowEnd.End);

        public VectorVisualObject() => Arrow = new TriangleArrow { Closed = true };

        /// <summary>
        /// Dans le cas où <see cref="Definition"/> est une <see cref="VectorVectorDefinition"/>, définit <see cref="VectorVectorDefinition.InVector"/> de cette définition
        /// </summary>
        /// <param name="inVector">Vecteur du plan</param>
        public void SetInVector(Vector inVector) { if (Definition is VectorVectorDefinition definition) definition.InVector = inVector; }

        protected override IReadOnlyCollection<Character> GetCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager) => GetCharacters(InAnchorPoint?.Definition.InPoint ?? default, Definition.InVector, Arrow, ArrowEnd, Fill, Stroke, coordinatesSystemManager);

        public static Character[] GetCharacters(Point inAnchorPoint, Vector inVector, Arrow arrow, ArrowEnd arrowEnd, Brush fill, PlanePen stroke, ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            return GetCharactersCore().ToArray();
            IEnumerable<Character> GetCharactersCore()
            {
                var outAnchorPoint = coordinatesSystemManager.ComputeOutCoordinates(inAnchorPoint);
                var outVector = coordinatesSystemManager.ComputeOutCoordinates(inVector);
                yield return Character.Line(outAnchorPoint, outAnchorPoint + outVector).Color(stroke);
                if (arrow != null) { foreach (var character in (arrow.In ? arrow.GetInGeometry(inAnchorPoint, inVector, arrowEnd, coordinatesSystemManager) : arrow.GetOutGeometry(outAnchorPoint, outVector, arrowEnd)).ToCharacters(fill, stroke)) yield return character; }
            }
        }
        public static Character[] GetCharacters(Point outAnchorPoint, Vector outVector, Arrow arrow, ArrowEnd arrowEnd, Brush fill, PlanePen stroke)
        {
            return GetCharactersCore().ToArray();
            IEnumerable<Character> GetCharactersCore()
            {
                yield return Character.Line(outAnchorPoint, outAnchorPoint + outVector).Color(null, stroke);
                if (arrow != null) { foreach (var character in arrow.GetOutGeometry(outAnchorPoint, outVector, arrowEnd).ToCharacters(fill, stroke)) yield return character; }
            }
        }

        public override string ToString() => Definition.InVector.ToString();

        public static implicit operator VectorVisualObject(Vector inVector) => Vector(null, inVector);
        public static implicit operator Vector(VectorVisualObject vectorVisualObject) => vectorVisualObject.Definition.InVector;
    }
}
