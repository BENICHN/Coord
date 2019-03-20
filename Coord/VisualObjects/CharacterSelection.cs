using BenLib;
using System.Collections.Generic;
using System.Linq;

namespace Coord
{
    public struct CharacterSelection
    {
        public Dictionary<VisualObject, IntInterval> Selection { get; }

        public CharacterSelection(params (VisualObject VisualObject, IntInterval Selection)[] selection) : this(selection as IEnumerable<(VisualObject VisualObject, IntInterval Selection)>) { }
        public CharacterSelection(IEnumerable<(VisualObject VisualObject, IntInterval Selection)> selection) => Selection = selection.ToDictionary(s => s.VisualObject, s => s.Selection) ?? new Dictionary<VisualObject, IntInterval>();
        public CharacterSelection(params HitTestCharacterResult[] selection) : this(selection as IEnumerable<HitTestCharacterResult>) { }
        public CharacterSelection(IEnumerable<HitTestCharacterResult> selection) => Selection = selection.GroupBy(hr => hr.Owner).ToDictionary(group => group.Key, group => group.Sum(hr => (hr.Index, hr.Index + 1))) ?? new Dictionary<VisualObject, IntInterval>();

        public bool ContainsAny(IEnumerable<HitTestCharacterResult> characters)
        {
            foreach (var hr in characters) { if (Selection.TryGetValue(hr.Owner, out IntInterval selection) && selection.Contains(hr.Index)) return true; }
            return false;
        }

        public bool Contains(IEnumerable<HitTestCharacterResult> characters)
        {
            foreach (var hr in characters) { if (!Selection.TryGetValue(hr.Owner, out IntInterval selection) || !selection.Contains(hr.Index)) return false; }
            return true;
        }

        public static CharacterSelection operator +(CharacterSelection selection1, CharacterSelection selection2) => new CharacterSelection(selection1.Selection.Concat(selection2.Selection).GroupBy(kvp => kvp.Key).Select(group => (group.Key, group.Sum(kvp => kvp.Value))));
    }
}
