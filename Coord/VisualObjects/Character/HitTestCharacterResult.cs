namespace Coord
{
    public struct HitTestCharacterResult
    {
        public HitTestCharacterResult(VisualObject owner, int index, Character character) : this()
        {
            Owner = owner;
            Index = index;
            Character = character;
        }

        public VisualObject Owner { get; set; }
        public int Index { get; set; }
        public Character Character { get; set; }
    }
}
