using System.Collections.Generic;
using NUnit.Framework;
using SnakeApple.Domain.Board;
using SnakeApple.Domain.Commands;
using SnakeApple.Domain.Entities;
using SnakeApple.Domain.Events;
using SnakeApple.Domain.Rules;

namespace SnakeApple.Tests.EditMode
{
    public sealed class SnakeMovementRulesTests
    {
        private readonly SnakeMovementRules rules = new SnakeMovementRules();

        [Test]
        public void Move_PropagatesEverySegmentIntoPredecessorPosition()
        {
            var state = CreateState(
                new[] { new GridPos(3, 2), new GridPos(2, 2), new GridPos(1, 2) });

            var result = rules.Execute(state, Direction.Up);

            Assert.That(result.IsAccepted, Is.True);
            Assert.That(result.State.Snake.Segments, Is.EqualTo(new[]
            {
                new GridPos(3, 3), new GridPos(3, 2), new GridPos(2, 2)
            }));
            Assert.That(result.State.TurnIndex, Is.EqualTo(1));
            Assert.That(result.Events[0], Is.TypeOf<SnakeMoved>());
        }

        [Test]
        public void Move_IntoOldTail_IsRejectedByV2Rule()
        {
            var state = CreateState(new[]
            {
                new GridPos(2, 2),
                new GridPos(2, 3),
                new GridPos(1, 3),
                new GridPos(1, 2)
            });

            var result = rules.Execute(state, Direction.Left);

            Assert.That(result.IsAccepted, Is.False);
            Assert.That(result.RejectionReason, Is.EqualTo(MoveRejectionReason.SnakeBody));
            Assert.That(result.State, Is.SameAs(state));
            Assert.That(state.TurnIndex, Is.Zero);
        }

        [Test]
        public void EatApple_GrowsOnce_RemovesApple_AndOpensExit()
        {
            var apple = new GridPos(3, 3);
            var state = CreateState(
                new[] { new GridPos(3, 2), new GridPos(2, 2), new GridPos(1, 2) },
                new Dictionary<GridPos, CollectibleKind> { [apple] = CollectibleKind.Apple });

            var result = rules.Execute(state, Direction.Up);

            Assert.That(result.IsAccepted, Is.True);
            Assert.That(result.State.Snake.Length, Is.EqualTo(4));
            Assert.That(result.State.RemainingApples, Is.Zero);
            Assert.That(result.State.IsExitOpen, Is.True);
            Assert.That(result.Events, Has.Exactly(1).TypeOf<AppleConsumed>());
            Assert.That(result.Events, Has.Exactly(1).TypeOf<ExitOpened>());
        }

        [Test]
        public void EnterOpenExit_CommitsWinAndEmitsLevelWon()
        {
            var state = CreateState(
                new[] { new GridPos(3, 2), new GridPos(2, 2), new GridPos(1, 2) },
                exit: new GridPos(3, 3));

            var result = rules.Execute(state, Direction.Up);

            Assert.That(result.IsAccepted, Is.True);
            Assert.That(result.State.Status, Is.EqualTo(SessionStatus.Won));
            Assert.That(result.Events, Has.Exactly(1).TypeOf<LevelWon>());
        }

        [Test]
        public void RejectedMove_DoesNotCreateTurnOrEvents()
        {
            var state = CreateState(
                new[] { new GridPos(3, 2), new GridPos(2, 2) },
                walls: new[] { new GridPos(3, 3) });

            var result = rules.Execute(state, Direction.Up);

            Assert.That(result.IsAccepted, Is.False);
            Assert.That(result.RejectionReason, Is.EqualTo(MoveRejectionReason.Wall));
            Assert.That(result.State.TurnIndex, Is.Zero);
            Assert.That(result.Events, Is.Empty);
        }

        private static BoardState CreateState(
            IEnumerable<GridPos> snake,
            IDictionary<GridPos, CollectibleKind> collectibles = null,
            IEnumerable<GridPos> walls = null,
            GridPos? exit = null)
        {
            return new BoardState(
                new BoardSize(8, 8),
                walls ?? new GridPos[0],
                new SnakeState(snake),
                collectibles ?? new Dictionary<GridPos, CollectibleKind>(),
                exit ?? new GridPos(6, 6));
        }
    }
}

