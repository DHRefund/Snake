namespace SnakeApple.Domain.Events
{
    public sealed class ExitOpened : TurnEvent
    {
        public ExitOpened(int turnIndex) : base(turnIndex)
        {
        }
    }
}

