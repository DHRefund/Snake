using System.Collections.Generic;
using NUnit.Framework;
using SnakeApple.Application.GameSession;
using SnakeApple.Domain.Board;
using SnakeApple.Domain.Commands;
using SnakeApple.Domain.Entities;
using SnakeApple.Domain.Rules;

namespace SnakeApple.Tests.EditMode
{
    public sealed class GameSessionTests
    {
        [Test]
        public void Undo_RestoresSnapshotAndCommandHistory()
        {
            var session = CreateSession();

            session.Move(Direction.Up);
            var didUndo = session.Undo();

            Assert.That(didUndo, Is.True);
            Assert.That(session.CurrentState.Snake.Head, Is.EqualTo(new GridPos(3, 2)));
            Assert.That(session.CurrentState.TurnIndex, Is.Zero);
            Assert.That(session.AcceptedCommands, Is.Empty);
            Assert.That(session.CanUndo, Is.False);
        }

        [Test]
        public void RejectedMove_DoesNotCreateUndoSnapshot()
        {
            var session = CreateSession(new[] { new GridPos(3, 3) });

            var result = session.Move(Direction.Up);

            Assert.That(result.IsAccepted, Is.False);
            Assert.That(session.CanUndo, Is.False);
            Assert.That(session.AcceptedCommands, Is.Empty);
        }

        [Test]
        public void Restart_RestoresInitialStateAndClearsHistory()
        {
            var session = CreateSession();
            session.Move(Direction.Up);

            session.Restart();

            Assert.That(session.CurrentState.Snake.Head, Is.EqualTo(new GridPos(3, 2)));
            Assert.That(session.CurrentState.TurnIndex, Is.Zero);
            Assert.That(session.CanUndo, Is.False);
            Assert.That(session.AcceptedCommands, Is.Empty);
        }

        private static GameSession CreateSession(IEnumerable<GridPos> walls = null)
        {
            var state = new BoardState(
                new BoardSize(8, 8),
                walls ?? new GridPos[0],
                new SnakeState(new[] { new GridPos(3, 2), new GridPos(2, 2), new GridPos(1, 2) }),
                new Dictionary<GridPos, CollectibleKind>(),
                new GridPos(6, 6));

            return new GameSession(state, new SnakeMovementRules());
        }
    }
}

