namespace SnakeApple.Domain.Events
{
    public abstract class TurnEvent
    {
        protected TurnEvent(int turnIndex)
        {
            TurnIndex = turnIndex;
        }

        public int TurnIndex { get; }
    }
}

