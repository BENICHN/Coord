using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Coord
{
    public class PlaneZoom : VisualObject
    {
        protected override Freezable CreateInstanceCore() => new PlaneZoom();

        public override string Type => "PlaneZoom";

        private CoordinatesSystemManager m_coordinatesSystemManager;

        public MathRect InputRange { get => (MathRect)GetValue(InputRangeProperty); set => SetValue(InputRangeProperty, value); }
        public static readonly DependencyProperty InputRangeProperty = CreateProperty<PlaneZoom, MathRect>(true, true, true, "InputRange", (sender, e) => { if (sender is PlaneZoom owner) owner.m_coordinatesSystemManager = new CoordinatesSystemManager { InputRange = owner.InputRange, OutputRange = owner.OutputRange }; });

        public Rect OutputRange { get => (Rect)GetValue(OutputRangeProperty); set => SetValue(OutputRangeProperty, value); }
        public static readonly DependencyProperty OutputRangeProperty = CreateProperty<PlaneZoom, Rect>(true, true, true, "OutputRange", (sender, e) => { if (sender is PlaneZoom owner) owner.m_coordinatesSystemManager = new CoordinatesSystemManager { InputRange = owner.InputRange, OutputRange = owner.OutputRange }; });

        public Geometry Clip { get => (Geometry)GetValue(ClipProperty); set => SetValue(ClipProperty, value); }
        public static readonly DependencyProperty ClipProperty = CreateProperty<PlaneZoom, Geometry>(true, true, true, "Clip");

        protected override IEnumerable<Character> GetCharactersCore(ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            var outputRange = OutputRange;
            var inputRange = InputRange;

            var clip = Clip?.CloneCurrentValue() ?? new RectangleGeometry(outputRange);

            var baseClip = clip.CloneCurrentValue().ToCharacter(Fill, Stroke);
            baseClip.Transform *= outputRange.To(coordinatesSystemManager.ComputeOutCoordinates(inputRange));
            yield return baseClip;

            m_coordinatesSystemManager.CoordinatesSystem = coordinatesSystemManager.CoordinatesSystem;
            foreach (var character in GetChildrenTransformedCharacters(coordinatesSystemManager)) yield return character;

            yield return clip.ToCharacter(Fill, Stroke);
        }

        protected override void RenderCore(DrawingContext drawingContext, ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            var characters = (Character[])GetTransformedCharacters(m_coordinatesSystemManager.AsReadOnly());
            int lastIndex = characters.Length - 1;

            drawingContext.DrawCharacter(characters[0]);

            var clip = characters[lastIndex];
            clip.ApplyTransforms();
            drawingContext.PushClip(clip.Geometry);

            for (int i = 1; i < lastIndex; i++) drawingContext.DrawCharacter(characters[i]);

            drawingContext.Pop();
            drawingContext.DrawCharacter(clip);
        }
    }
}
