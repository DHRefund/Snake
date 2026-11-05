using System;
using System.Collections.Generic;
using SnakeApple.Domain.Board;

namespace SnakeApple.Domain.Rules
{
    public sealed class PuzzleTurnResult
    {
        private PuzzleTurnResult(
            bool accepted,
            PuzzleState state,
            MoveRejectionReason rejectionReason,
            IReadOnlyList<GridPos> preGravitySnake = null,
            bool ateApple = false)
        {
            IsAccepted = accepted;
            State = state ?? throw new ArgumentNullException(nameof(state));
            RejectionReason = rejectionReason;
            PreGravitySnake = preGravitySnake ?? state.Snake;
            AteApple = ateApple;
        }

        public bool IsAccepted { get; }
        public PuzzleState State { get; }
        public MoveRejectionReason RejectionReason { get; }
        public IReadOnlyList<GridPos> PreGravitySnake { get; }
        public bool AteApple { get; }

        public static PuzzleTurnResult Accepted(
            PuzzleState state,
            IReadOnlyList<GridPos> preGravitySnake = null,
            bool ateApple = false)
        {
            return new PuzzleTurnResult(true, state, MoveRejectionReason.None, preGravitySnake, ateApple);
        }

        public static PuzzleTurnResult Rejected(PuzzleState state, MoveRejectionReason reason)
        {
            return new PuzzleTurnResult(false, state, reason);
        }
    }
}
