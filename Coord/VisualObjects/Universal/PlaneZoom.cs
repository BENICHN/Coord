using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Coord
{
    public class PlaneZoom : VisualObjectGroupBase
    {
        public override string Type => "PlaneZoom";

        private MathRect m_inputRange;
        private Rect m_outputRange;
        private Geometry m_clip;
        private CoordinatesSystemManager m_coordinatesSystemManager;

        public PlaneZoom(MathRect inputRange, Rect outputRange, Geometry clip)
        {
            InputRange = inputRange;
            OutputRange = outputRange;
            Clip = clip;
        }
        public PlaneZoom(MathRect inputRange, Rect outputRange, Geometry clip, NotifyObjectCollection<VisualObject> children) : this(inputRange, outputRange, clip) => Children = children;
        public PlaneZoom(MathRect inputRange, Rect outputRange, Geometry clip, IEnumerable<VisualObject> children) : this(inputRange, outputRange, clip, new NotifyObjectCollection<VisualObject>(children)) { }
        public PlaneZoom(MathRect inputRange, Rect outputRange, Geometry clip, params VisualObject[] children) : this(inputRange, outputRange, clip, new NotifyObjectCollection<VisualObject>(children)) { }

        public MathRect InputRange
        {
            get => m_inputRange;
            set
            {
                m_inputRange = value;
                m_coordinatesSystemManager = new CoordinatesSystemManager(InputRange, OutputRange);
                NotifyChanged();
            }
        }
        public Rect OutputRange
        {
            get => m_outputRange;
            set
            {
                m_outputRange = value;
                m_coordinatesSystemManager = new CoordinatesSystemManager(InputRange, OutputRange);
                NotifyChanged();
            }
        }

        public Geometry Clip
        {
            get => m_clip;
            set
            {
                UnRegister(m_clip);
                m_clip = value;
                Register(m_clip);
                NotifyChanged();
            }
        }

        public override IReadOnlyCollection<Character> GetCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            return GetCharactersCore().ToArray();
            IEnumerable<Character> GetCharactersCore()
            {
                var outRange = coordinatesSystemManager.OutputRange;
                var inRange = coordinatesSystemManager.InputRange;
                var outputRange = OutputRange;
                var inputRange = InputRange;

                var clip = Clip?.CloneCurrentValue() ?? new RectangleGeometry(outputRange);

                var baseClip = new Character(clip.CloneCurrentValue(), Fill, Stroke);
                baseClip.Matrix *= outputRange.To(coordinatesSystemManager.ComputeOutCoordinates(inputRange));
                yield return baseClip;

                m_coordinatesSystemManager.CoordinatesSystem = coordinatesSystemManager.CoordinatesSystem;
                foreach (var character in Children.SelectMany(visualObject => visualObject.GetTransformedCharacters(coordinatesSystemManager, false))) yield return character;

                yield return new Character(clip, Fill, Stroke);
            }
        }

        protected override void RenderCore(DrawingContext drawingContext, ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            var characters = GetTransformedCharacters(m_coordinatesSystemManager.AsReadOnly(), false) as Character[];
            int lastIndex = characters.Length - 1;

            drawingContext.DrawCharacter(characters[0]);

            var clip = characters[lastIndex];
            clip.ApplyTransforms();
            drawingContext.PushClip(clip.Geometry);

            for (int i = 1; i < lastIndex; i++) drawingContext.DrawCharacter(characters[i]);

            drawingContext.Pop();
            drawingContext.DrawCharacter(clip, false);
        }
    }
}
