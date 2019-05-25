using BenLib.Standard;
using BenLib.WPF;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using static BenLib.Framework.NumFramework;

namespace Coord
{
    public class OutMorphingVisualObject : VisualObject, IProgressive
    {
        public override string Type => "OutMorphing";

        private MorphingCharacter[] m_fromCharacters;
        private MorphingCharacter[] m_toCharacters;
        private (MorphingCharacter From, MorphingCharacter To)[] m_correspondingPoints;

        public IReadOnlyCollection<Character> From { get => (IReadOnlyCollection<Character>)GetValue(FromProperty); set => SetValue(FromProperty, value); }
        public static readonly DependencyProperty FromProperty = CreateProperty<IReadOnlyCollection<Character>>(true, true, "From", typeof(OutMorphingVisualObject), (sender, e) => { if (sender is OutMorphingVisualObject owner) owner.Compute(true, false); });

        public IReadOnlyCollection<Character> To { get => (IReadOnlyCollection<Character>)GetValue(ToProperty); set => SetValue(ToProperty, value); }
        public static readonly DependencyProperty ToProperty = CreateProperty<IReadOnlyCollection<Character>>(true, true, "To", typeof(OutMorphingVisualObject), (sender, e) => { if (sender is OutMorphingVisualObject owner) owner.Compute(false, true); });

        public CorrespondanceDictionary Correspondances { get => (CorrespondanceDictionary)GetValue(CorrespondancesProperty); set => SetValue(CorrespondancesProperty, value); }
        public static readonly DependencyProperty CorrespondancesProperty = CreateProperty<CorrespondanceDictionary>(true, true, "Correspondances", typeof(OutMorphingVisualObject), (sender, e) => { if (sender is OutMorphingVisualObject owner) owner.Compute(false, false); });

        public Progress Progress { get => (Progress)GetValue(ProgressProperty); set => SetValue(ProgressProperty, value); }
        public static readonly DependencyProperty ProgressProperty = CreateProperty<Progress>(true, true, "Progress", typeof(OutMorphingVisualObject));

        public PointVisualObject InAnchorPoint { get => (PointVisualObject)GetValue(InAnchorPointProperty); set => SetValue(InAnchorPointProperty, value); }
        public static readonly DependencyProperty InAnchorPointProperty = CreateProperty<PointVisualObject>(true, true, "InAnchorPoint", typeof(OutMorphingVisualObject));

        private void Compute(bool from, bool to)
        {
            if (from) m_fromCharacters = From.Select(character => MorphingCharacter.FromCharacter(character)).ToArray();
            if (to) m_toCharacters = To.Select(character => MorphingCharacter.FromCharacter(character)).ToArray();
            m_correspondingPoints = MorphingCharacter.ComputeCorrespondances(m_fromCharacters, m_toCharacters, Correspondances).ToArray();
        }

        protected override IReadOnlyCollection<Character> GetCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            var outAnchorPoint = coordinatesSystemManager.ComputeOutCoordinates(InAnchorPoint);
            double progress = Progress.Value;

            return
                progress == 0 ? From.CloneCharacters().ToArray() :
                progress == 1 ? To.CloneCharacters().ToArray() :
                m_correspondingPoints.Select(correspondance =>
                {
                    var (from, to) = correspondance;
                    var fromPen = from.Stroke.GetOutPen(coordinatesSystemManager);
                    var toPen = to.Stroke.GetOutPen(coordinatesSystemManager);
                    return GeometryHelper.GetCurve(Interpolate(from.Points, to.Points, progress).ToArray(), false, false).ToCharacter(Interpolate(from.Fill, to.Fill, progress) ?? Fill, Interpolate(fromPen, toPen, progress) ?? Stroke);
                }).Translate((Vector)outAnchorPoint, 1).ToArray();
        }
    }

    public class InMorphingVisualObject : VisualObject, IProgressive
    {
        public override string Type => "InMorphing";

        public VisualObject From { get => (VisualObject)GetValue(FromProperty); set => SetValue(FromProperty, value); }
        public static readonly DependencyProperty FromProperty = CreateProperty<VisualObject>(true, true, "From", typeof(InMorphingVisualObject));

        public VisualObject To { get => (VisualObject)GetValue(ToProperty); set => SetValue(ToProperty, value); }
        public static readonly DependencyProperty ToProperty = CreateProperty<VisualObject>(true, true, "To", typeof(InMorphingVisualObject));

        public CorrespondanceDictionary Correspondances { get => (CorrespondanceDictionary)GetValue(CorrespondancesProperty); set => SetValue(CorrespondancesProperty, value); }
        public static readonly DependencyProperty CorrespondancesProperty = CreateProperty<CorrespondanceDictionary>(true, true, "Correspondances", typeof(InMorphingVisualObject));

        public Progress Progress { get => (Progress)GetValue(ProgressProperty); set => SetValue(ProgressProperty, value); }
        public static readonly DependencyProperty ProgressProperty = CreateProperty<Progress>(true, true, "Progress", typeof(InMorphingVisualObject));

        protected override IReadOnlyCollection<Character> GetCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            double progress = Progress.Value;
            return
                progress == 0 ? From.GetTransformedCharacters(coordinatesSystemManager) :
                progress == 1 ? To.GetTransformedCharacters(coordinatesSystemManager) :
                MorphingCharacter.ComputeCorrespondances(From.GetTransformedCharacters(coordinatesSystemManager).Select(c => MorphingCharacter.FromCharacter(c)).ToArray(), To.GetTransformedCharacters(coordinatesSystemManager).Select(c => MorphingCharacter.FromCharacter(c)).ToArray(), Correspondances).Select(correspondance =>
                {
                    var (from, to) = correspondance;
                    var fromPen = from.Stroke.GetOutPen(coordinatesSystemManager);
                    var toPen = to.Stroke.GetOutPen(coordinatesSystemManager);
                    return GeometryHelper.GetCurve(Interpolate(from.Points, to.Points, progress).ToArray(), false, false).ToCharacter(Interpolate(from.Fill, to.Fill, progress) ?? Fill, Interpolate(fromPen, toPen, progress) ?? Stroke);
                }).ToArray();
        }
    }

    public readonly struct MorphingCharacter
    {
        public MorphingCharacter(Point[] points, Brush fill, PlanePen stroke) : this()
        {
            Points = points;
            Fill = fill;
            Stroke = stroke;
        }

        public Point[] Points { get; }
        public Brush Fill { get; }
        public PlanePen Stroke { get; }

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
            return i >= 0 ? ValuesList[i] == item.Value : false;
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
