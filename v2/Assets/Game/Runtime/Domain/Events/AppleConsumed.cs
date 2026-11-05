using SnakeApple.Domain.Board;
using SnakeApple.Domain.Entities;

namespace SnakeApple.Domain.Events
{
    public sealed class AppleConsumed : TurnEvent
    {
        public AppleConsumed(int turnIndex, GridPos position, CollectibleKind kind, int remainingApples)
            : base(turnIndex)
        {
            Position = position;
            Kind = kind;
            RemainingApples = remainingApples;
        }

        public GridPos Position { get; }

        public CollectibleKind Kind { get; }

        public int RemainingApples { get; }
    }
}

