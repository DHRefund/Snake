using System;
using System.Collections.Generic;
using SnakeApple.Domain.Board;
using SnakeApple.Domain.Commands;
using SnakeApple.Domain.Entities;
using SnakeApple.Domain.Events;

namespace SnakeApple.Domain.Rules
{
    public sealed class SnakeMovementRules
    {
        public TurnResult Execute(BoardState current, Direction direction)
        {
            if (current == null)
            {
                throw new ArgumentNullException(nameof(current));
            }

            if (current.Status != SessionStatus.Playing)
            {
                return TurnResult.Rejected(current, MoveRejectionReason.SessionNotPlaying);
            }

            var target = current.Snake.Head + direction.ToOffset();
            if (!current.Size.Contains(target))
            {
                return TurnResult.Rejected(current, MoveRejectionReason.OutsideBoard);
            }

            if (current.IsWall(target))
            {
                return TurnResult.Rejected(current, MoveRejectionReason.Wall);
            }

            // V2 deliberately rejects the old tail cell even though it would vacate this turn.
            if (current.Snake.Occupies(target))
            {
                return TurnResult.Rejected(current, MoveRejectionReason.SnakeBody);
            }

            var consumes = current.TryGetCollectible(target, out var collectibleKind);
            var nextSnake = current.Snake.MoveTo(target, consumes);
            var next = current.ApplyAcceptedMove(
                nextSnake,
                consumes ? target : (GridPos?)null,
                consumes ? collectibleKind : (CollectibleKind?)null);

            var turnIndex = next.TurnIndex;
            var events = new List<TurnEvent>
            {
                new SnakeMoved(turnIndex, current.Snake.Segments, next.Snake.Segments)
            };

            if (consumes)
            {
                events.Add(new AppleConsumed(turnIndex, target, collectibleKind, next.RemainingApples));
                if (!current.IsExitOpen && next.IsExitOpen)
                {
                    events.Add(new ExitOpened(turnIndex));
                }
            }

            if (next.Status == SessionStatus.Won)
            {
                events.Add(new LevelWon(turnIndex, next.Snake.Length));
            }

            return TurnResult.Accepted(next, events);
        }
    }
}

