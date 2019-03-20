using BenLib.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Coord
{
    /// <summary>
    /// Objet visuel représentant un vecteur du plan
    /// </summary>
    public class VectorVisualObject : VisualObject
    {
        public override string Type
        {
            get
            {
                var definitionType = Definition?.GetType();
                if (definitionType == typeof(VectorVectorDefinition)) return "VectorVector";
                if (definitionType == typeof(PointPointVectorDefinition)) return "PointPointVector";
                if (definitionType == typeof(OperationsVectorDefinition)) return "OperationsVector";
                if (definitionType == typeof(MultiOperationsVectorDefinition)) return "MultiOperationsVector";
                else return "Vector";
            }
        }

        private VectorDefinition m_definition;
        private PointVisualObject m_inAnchorPoint = new Point(0.0, 0.0);
        private Arrow m_arrow = new TriangleArrow(true, false);
        private ArrowEnd m_arrowEnd = ArrowEnd.End;

        public VectorVisualObject(PointVisualObject inAnchorPoint, Vector inVector)
        {
            InAnchorPoint = inAnchorPoint;
            Definition = new VectorVectorDefinition(inVector);
        }

        public VectorVisualObject(PointVisualObject inAnchorPoint, VectorDefinition definition)
        {
            InAnchorPoint = inAnchorPoint;
            Definition = definition;
        }

        /// <summary>
        /// Détermine les propriétés fondamentales de ce <see cref="VectorVisualObject"/>
        /// </summary>
        public VectorDefinition Definition
        {
            get => m_definition;
            set
            {
                UnRegister(m_definition);
                m_definition = value;
                Register(m_definition);
                NotifyChanged();
            }
        }

        /// <summary>
        /// Origine dans le plan du représentant de ce vecteur
        /// </summary>
        public PointVisualObject InAnchorPoint
        {
            get => m_inAnchorPoint;
            set
            {
                UnRegister(m_inAnchorPoint);
                m_inAnchorPoint = value;
                Register(m_inAnchorPoint);
                NotifyChanged();
            }
        }

        /// <summary>
        /// Dessine une flèche à l'extrémité d'une ligne à l'aide d'un <see cref="DrawingContext"/>
        /// </summary>
        public Arrow Arrow
        {
            get => m_arrow;
            set
            {
                m_arrow = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Indique quelle extrémité de la ligne possède une flèche
        /// </summary>
        public ArrowEnd ArrowEnd
        {
            get => m_arrowEnd;
            set
            {
                m_arrowEnd = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Dans le cas où <see cref="Definition"/> est une <see cref="VectorVectorDefinition"/>, définit <see cref="VectorVectorDefinition.InVector"/> de cette définition
        /// </summary>
        /// <param name="inVector">Vecteur du plan</param>
        public void SetInVector(Vector inVector)
        {
            if (Definition is VectorVectorDefinition definition) definition.InVector = inVector;
        }

        public override string ToString() => Definition.InVector.ToString();

        public override IReadOnlyCollection<Character> GetCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager) => GetCharacters(InAnchorPoint, Definition.InVector, Arrow, ArrowEnd, Fill, Stroke, coordinatesSystemManager);

        public static Character[] GetCharacters(Point inAnchorPoint, Vector inVector, Arrow arrow, ArrowEnd arrowEnd, Brush fill, Pen stroke, ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            return GetCharactersCore().ToArray();
            IEnumerable<Character> GetCharactersCore()
            {
                var outAnchorPoint = coordinatesSystemManager.ComputeOutCoordinates(inAnchorPoint);
                var outVector = coordinatesSystemManager.ComputeOutCoordinates(inVector);
                yield return Character.Line(outAnchorPoint, outAnchorPoint + outVector, null, stroke);
                if (arrow != null)
                {
                    foreach (var character in Character.FromGeometry(arrow.GetInGeometry(inAnchorPoint, inVector, arrowEnd, coordinatesSystemManager)))
                    {
                        character.Fill = fill;
                        character.Stroke = stroke;
                        yield return character;
                    }
                }
            }
        }
        public static Character[] GetCharacters(Point outAnchorPoint, Vector outVector, Arrow arrow, ArrowEnd arrowEnd, Brush fill, Pen stroke)
        {
            return GetCharactersCore().ToArray();
            IEnumerable<Character> GetCharactersCore()
            {
                yield return Character.Line(outAnchorPoint, outAnchorPoint + outVector, null, stroke);
                if (arrow != null)
                {
                    foreach (var character in Character.FromGeometry(arrow.GetOutGeometry(outAnchorPoint, outVector, arrowEnd)))
                    {
                        character.Fill = fill;
                        character.Stroke = stroke;
                        yield return character;
                    }
                }
            }
        }

        public static implicit operator VectorVisualObject(Vector inVector) => new VectorVisualObject(null, inVector);
        public static implicit operator Vector(VectorVisualObject vectorVisualObject) => vectorVisualObject.Definition.InVector;
    }
}
