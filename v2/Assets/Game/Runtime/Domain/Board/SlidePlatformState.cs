namespace SnakeApple.Domain.Board
{
    public sealed class SlidePlatformState
    {
        public SlidePlatformState(GridPos first, GridPos second, bool horizontal, bool atFirst = true)
        {
            First = first;
            Second = second;
            Horizontal = horizontal;
            AtFirst = atFirst;
        }

        public GridPos First { get; }
        public GridPos Second { get; }
        public bool Horizontal { get; }
        public bool AtFirst { get; set; }
        public GridPos Current => AtFirst ? First : Second;
        public GridPos Destination => AtFirst ? Second : First;

        public bool Occupies(GridPos position)
        {
            var center = Current;
            return Horizontal
                ? position.Y == center.Y && (position.X == center.X || position.X == center.X - 1)
                : position.X == center.X && (position.Y == center.Y || position.Y == center.Y - 1);
        }

        public SlidePlatformState Clone()
        {
            return new SlidePlatformState(First, Second, Horizontal, AtFirst);
        }
    }
}
