using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SnakeApple.Domain.Board;
using SnakeApple.Domain.Events;

namespace SnakeApple.Domain.Rules
{
    public sealed class TurnResult
    {
        private readonly ReadOnlyCollection<TurnEvent> events;

        private TurnResult(
            bool isAccepted,
            BoardState state,
            MoveRejectionReason rejectionReason,
            IList<TurnEvent> events)
        {
            IsAccepted = isAccepted;
            State = state ?? throw new ArgumentNullException(nameof(state));
            RejectionReason = rejectionReason;
            this.events = new ReadOnlyCollection<TurnEvent>(events ?? throw new ArgumentNullException(nameof(events)));
        }

        public bool IsAccepted { get; }

        public BoardState State { get; }

        public MoveRejectionReason RejectionReason { get; }

        public IReadOnlyList<TurnEvent> Events => events;

        public static TurnResult Rejected(BoardState state, MoveRejectionReason reason)
        {
            return new TurnResult(false, state, reason, new List<TurnEvent>());
        }

        public static TurnResult Accepted(BoardState state, IList<TurnEvent> events)
        {
            return new TurnResult(true, state, MoveRejectionReason.None, events);
        }
    }
}

