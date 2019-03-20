using BenLib;
using System.Collections.Generic;
using System.Linq;

namespace Coord
{
    public class InCharactersVisualObject : VisualObject
    {
        public override string Type => "InCharacters";

        private VisualObject m_visualObject;
        public VisualObject VisualObject
        {
            get => m_visualObject;
            set
            {
                m_visualObject = value;
                Register(value);
                NotifyChanged();
            }
        }

        private IntInterval m_interval;
        public IntInterval Interval
        {
            get => m_interval;
            set
            {
                m_interval = value;
                NotifyChanged();
            }
        }

        public InCharactersVisualObject(VisualObject visualObject, IntInterval interval)
        {
            VisualObject = visualObject;
            Interval = interval;
        }

        public override IReadOnlyCollection<Character> GetCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager) => VisualObject.GetTransformedCharacters(coordinatesSystemManager, false).SubCollection(Interval, true).ToArray();
    }
}
