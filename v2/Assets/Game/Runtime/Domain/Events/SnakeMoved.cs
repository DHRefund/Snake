using System.Collections.Generic;
using System.Collections.ObjectModel;
using SnakeApple.Domain.Board;

namespace SnakeApple.Domain.Events
{
    public sealed class SnakeMoved : TurnEvent
    {
        private readonly ReadOnlyCollection<GridPos> from;
        private readonly ReadOnlyCollection<GridPos> to;

        public SnakeMoved(int turnIndex, IEnumerable<GridPos> from, IEnumerable<GridPos> to)
            : base(turnIndex)
        {
            this.from = new List<GridPos>(from).AsReadOnly();
            this.to = new List<GridPos>(to).AsReadOnly();
        }

        public IReadOnlyList<GridPos> From => from;

        public IReadOnlyList<GridPos> To => to;
    }
}

