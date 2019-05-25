using BenLib.Standard;
using BenLib.WPF;
using Coord;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace CoordAnimation
{
    public class CharacterEffectElement
    {
        public static Brush IsSelectedBackground { get; } = BenLib.WPF.Misc.BrushFromHex(0xFF007ACC);
        public static Brush IsSelectedForeground { get; } = Brushes.White;
        public static Brush IsMouseOverBackground { get; } = Brushes.Transparent;
        public static Brush IsMouseOverForeground { get; } = Brushes.White;

        public CharacterEffectElement(CharacterEffect characterEffect, AnimationData animationData, VisualObject effected)
        {
            CharacterEffect = characterEffect;
            AnimationData = animationData;
            Effected = effected;
        }

        public CharacterEffect CharacterEffect { get; set; }
        public AnimationData AnimationData { get; set; }
        public VisualObject Effected { get; set; }

        public string Type => CharacterEffect.GetType().Name;
    }

    public class AnimationData
    {
        public AnimationData(Range<int> range, IEasingFunction easingFunction)
        {
            Range = range;
            EasingFunction = easingFunction;
        }

        public Range<int> Range { get; set; }
        public IEasingFunction EasingFunction { get; set; }
    }

    public interface ICharacterEffectSerializer
    {
        IEnumerable<ISerializedProperty> Properties { get; }
    }

    public class CharacterEffectSerializer<TEffect> : ICharacterEffectSerializer where TEffect : CharacterEffect
    {
        public CharacterEffectSerializer(params ISerializedProperty[] properties) => Properties = new ISerializedProperty[]
        {
            //new SerializedProperty<TEffect, Interval<int>>("Interval", (c, v) => c.Interval = v),
            new SerializedProperty<TEffect, bool>("WithTransforms", (c, v) => c.WithTransforms = v)
        }.Concat(properties).ToArray();

        public ISerializedProperty[] Properties { get; }
        IEnumerable<ISerializedProperty> ICharacterEffectSerializer.Properties => Properties;
    }
}
