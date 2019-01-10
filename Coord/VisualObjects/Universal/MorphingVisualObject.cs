using BenLib;
using BenLib.WPF;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Coord
{
    public class OutMorphingVisualObject : VisualObject, IProgressive
    {
        private MorphingCharacter[] m_fromCharacters;
        private IReadOnlyCollection<Character> m_from;
        public IReadOnlyCollection<Character> From
        {
            get => m_from;
            set
            {
                m_from = value;
                m_fromCharacters = value.Select(character => MorphingCharacter.FromCharacter(character)).ToArray();
                m_correspondingPoints = MorphingCharacter.ComputeCorrespondances(m_fromCharacters, m_toCharacters, Correspondances).ToArray();
                NotifyChanged();
            }
        }

        private MorphingCharacter[] m_toCharacters;
        private IReadOnlyCollection<Character> m_to;
        public IReadOnlyCollection<Character> To
        {
            get => m_to;
            set
            {
                m_to = value;
                m_toCharacters = value.Select(character => MorphingCharacter.FromCharacter(character)).ToArray();
                m_correspondingPoints = MorphingCharacter.ComputeCorrespondances(m_fromCharacters, m_toCharacters, Correspondances).ToArray();
                NotifyChanged();
            }
        }

        private (MorphingCharacter From, MorphingCharacter To)[] m_correspondingPoints;

        private CorrespondanceDictionary m_correspondances;
        public CorrespondanceDictionary Correspondances
        {
            get => m_correspondances;
            set
            {
                m_correspondances = value;
                m_correspondingPoints = MorphingCharacter.ComputeCorrespondances(m_fromCharacters, m_toCharacters, Correspondances).ToArray();
                NotifyChanged();
            }
        }

        private Progress m_progress;
        public Progress Progress
        {
            get => m_progress;
            set
            {
                m_progress = value;
                NotifyChanged();
            }
        }

        private PointVisualObject m_inAnchorPoint;
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

        public OutMorphingVisualObject(PointVisualObject inAnchorPoint, IReadOnlyCollection<Character> from, IReadOnlyCollection<Character> to, CorrespondanceDictionary correspondances, params SynchronizedProgress[] synchronizedProgresses)
        {
            InAnchorPoint = inAnchorPoint ?? new Point(0.0, 0.0);
            From = from;
            To = to;
            Correspondances = correspondances;
            foreach (var synchronizedProgress in synchronizedProgresses) synchronizedProgress.Objects.Add(this);
        }

        public override IReadOnlyCollection<Character> GetCharacters(CoordinatesSystemManager coordinatesSystemManager)
        {
            double progress = Progress.Value;

            if (progress == 0) return From.CloneCharacters().ToArray();
            if (progress == 1) return To.CloneCharacters().ToArray();

            return m_correspondingPoints.Select(correspondance =>
            {
                var (from, to) = correspondance;
                return new Character(GeometryHelper.GetCurve(Num.Interpolate(from.Points, to.Points, progress).ToArray(), false, false), Num.Interpolate(from.Fill, to.Fill, progress) ?? Fill, Num.Interpolate(from.Stroke, to.Stroke, progress) ?? Stroke);
            }).ToArray();
        }

        public override IReadOnlyCollection<Character> GetTransformedCharacters(CoordinatesSystemManager coordinatesSystemManager, bool applyTransforms)
        {
            var outAnchorPoint = coordinatesSystemManager.ComputeOutCoordinates(InAnchorPoint);
            var characters = GetCharacters(coordinatesSystemManager);
            if (!Effects.IsNullOrEmpty()) foreach (var effect in Effects) effect.Apply(characters, coordinatesSystemManager);
            characters.Translate((Vector)outAnchorPoint, 1).Enumerate();
            if (applyTransforms) foreach (var character in characters) character.ApplyTransforms();
            return characters;
        }
    }

    public class InMorphingVisualObject : VisualObject, IProgressive
    {
        private VisualObject m_from;
        public VisualObject From
        {
            get => m_from;
            set
            {
                m_from = value;
                Register(value);
                NotifyChanged();
            }
        }

        private VisualObject m_to;
        public VisualObject To
        {
            get => m_to;
            set
            {
                m_to = value;
                Register(value);
                NotifyChanged();
            }
        }

        private CorrespondanceDictionary m_correspondances;
        public CorrespondanceDictionary Correspondances
        {
            get => m_correspondances;
            set
            {
                m_correspondances = value;
                NotifyChanged();
            }
        }

        private Progress m_progress;
        public Progress Progress
        {
            get => m_progress;
            set
            {
                m_progress = value;
                NotifyChanged();
            }
        }

        public InMorphingVisualObject(VisualObject from, VisualObject to, CorrespondanceDictionary correspondances, params SynchronizedProgress[] synchronizedProgresses)
        {
            From = from;
            To = to;
            Correspondances = correspondances;
            foreach (var synchronizedProgress in synchronizedProgresses) synchronizedProgress.Objects.Add(this);
        }

        public override IReadOnlyCollection<Character> GetCharacters(CoordinatesSystemManager coordinatesSystemManager)
        {
            double progress = Progress.Value;

            if (progress == 0) return From.GetTransformedCharacters(coordinatesSystemManager, true);
            if (progress == 1) return To.GetTransformedCharacters(coordinatesSystemManager, true);

            return MorphingCharacter.ComputeCorrespondances(
                From.GetTransformedCharacters(coordinatesSystemManager, true).Select(c => MorphingCharacter.FromCharacter(c)).ToArray(), 
                To.GetTransformedCharacters(coordinatesSystemManager, true).Select(c => MorphingCharacter.FromCharacter(c)).ToArray(), 
                Correspondances).Select(correspondance =>
            {
                var (from, to) = correspondance;
                return new Character(GeometryHelper.GetCurve(Num.Interpolate(from.Points, to.Points, progress).ToArray(), false, false), Num.Interpolate(from.Fill, to.Fill, progress) ?? Fill, Num.Interpolate(from.Stroke, to.Stroke, progress) ?? Stroke);
            }).ToArray();
        }
    }

    public struct MorphingCharacter
    {
        public MorphingCharacter(Point[] points, Brush fill, Pen stroke) : this()
        {
            Points = points;
            Fill = fill;
            Stroke = stroke;
        }

        public Point[] Points { get; set; }
        public Brush Fill { get; set; }
        public Pen Stroke { get; set; }

        public static IEnumerable<(MorphingCharacter From, MorphingCharacter To)> ComputeCorrespondances(MorphingCharacter[] from, MorphingCharacter[] to, CorrespondanceDictionary correspondances)
        {
            var enumerator = (correspondances.IsNullOrEmpty() ? new CorrespondanceDictionary() : correspondances).GetKeyValuePairs(from.Length, to.Length).GetEnumerator();
            enumerator.MoveNext();

            while (true)
            {
                var start = enumerator.Current;

                if (!enumerator.MoveNext()) break;
                var end = enumerator.Current;

                foreach (var correspondance in from.ExpandOrContract((start.Key + 1, end.Key), to, (start.Value + 1, end.Value))) yield return correspondance;
            }
        }

        public static MorphingCharacter FromCharacter(Character character, int pointsCount) => new MorphingCharacter(character.Geometry.GetPoints(pointsCount).ToArray(), character.Fill, character.Stroke);
        public static MorphingCharacter FromCharacter(Character character, double tolerance = -1, ToleranceType type = ToleranceType.Absolute) => new MorphingCharacter(character.Geometry.GetPoints(tolerance, type).ToArray(), character.Fill, character.Stroke);
    }

    public class CorrespondanceDictionary : IDictionary<int, int>
    {
        public int this[int key] { get => ValuesList[KeysList.IndexOf(key)]; set => ValuesList[KeysList.IndexOf(key)] = value; }

        public List<int> KeysList { get; } = new List<int>();
        public List<int> ValuesList { get; } = new List<int>();

        public ICollection<int> Keys => KeysList;
        public ICollection<int> Values => ValuesList;

        private IEnumerable<KeyValuePair<int, int>> GetKeyValuePairs() { for (int i = 0; i < Count; i++) yield return new KeyValuePair<int, int>(KeysList[i], ValuesList[i]); }
        public IEnumerable<KeyValuePair<int, int>> GetKeyValuePairs(int fromCount, int toCount)
        {
            yield return new KeyValuePair<int, int>(-1, -1);

            for (int i = 0; i < Count; i++)
            {
                int key = KeysList[i];
                int value = ValuesList[i];
                if (key < fromCount && value <= toCount) yield return new KeyValuePair<int, int>(key - 1, value - 1);
            }

            yield return new KeyValuePair<int, int>(fromCount - 1, toCount - 1);
        }

        public int Count => KeysList.Count;

        public bool IsReadOnly => false;

        public void Add(int key, int value)
        {
            if (key <= 0) throw new ArgumentException("Impossible d'ajouter un élément dont la clé est inférieure ou égale à 0");
            if (value < 0) throw new ArgumentException("Impossible d'ajouter un élément dont la valeur est inférieure à 0");

            int i = KeysList.IndexOf(key);
            if (i >= 0) throw new ArgumentException("Un élément avec la même clé a déjà été ajouté.");

            KeysList.Add(key);
            ValuesList.Add(value);

            KeysList.Sort();
            ValuesList.Sort();

            i = KeysList.IndexOf(key);
            if (ValuesList[i] != value)
            {
                KeysList.RemoveAt(i);
                ValuesList.Remove(value);
                throw new ArgumentException("Les clés ne sont pas triées dans le même ordre que les valeurs");
            }
        }

        public void Add(KeyValuePair<int, int> item) => Add(item.Key, item.Value);

        public void Clear()
        {
            KeysList.Clear();
            ValuesList.Clear();
        }

        public bool Contains(KeyValuePair<int, int> item)
        {
            int i = KeysList.IndexOf(item.Key);
            if (i >= 0) return ValuesList[i] == item.Value;
            else return false;
        }

        public bool ContainsKey(int key) => KeysList.Contains(key);

        public void CopyTo(KeyValuePair<int, int>[] array, int arrayIndex) { for (int i = arrayIndex; i < Count; i++) array[i] = new KeyValuePair<int, int>(KeysList[i], ValuesList[i]); }

        public IEnumerator<KeyValuePair<int, int>> GetEnumerator() => GetKeyValuePairs().GetEnumerator();

        public bool Remove(int key)
        {
            int i = KeysList.IndexOf(key);
            if (i >= 0)
            {
                KeysList.RemoveAt(i);
                ValuesList.RemoveAt(i);
                return true;
            }
            else return false;
        }

        public bool Remove(KeyValuePair<int, int> item)
        {
            int i = KeysList.IndexOf(item.Key);
            if (i >= 0)
            {
                KeysList.RemoveAt(i);
                ValuesList.RemoveAt(i);
                return true;
            }
            else return false;
        }

        public bool TryGetValue(int key, out int value)
        {
            int i = KeysList.IndexOf(key);
            if (i >= 0)
            {
                value = ValuesList[i];
                return true;
            }
            else
            {
                value = 0;
                return false;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
