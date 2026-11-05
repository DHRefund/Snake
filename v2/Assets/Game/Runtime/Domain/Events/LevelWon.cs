namespace SnakeApple.Domain.Events
{
    public sealed class LevelWon : TurnEvent
    {
        public LevelWon(int turnIndex, int evacuatedSegmentCount) : base(turnIndex)
        {
            EvacuatedSegmentCount = evacuatedSegmentCount;
        }

        public int EvacuatedSegmentCount { get; }
    }
}

