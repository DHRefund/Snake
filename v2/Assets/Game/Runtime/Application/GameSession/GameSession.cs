using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SnakeApple.Domain.Board;
using SnakeApple.Domain.Commands;
using SnakeApple.Domain.Rules;

namespace SnakeApple.Application.GameSession
{
    public sealed class GameSession
    {
        private readonly SnakeMovementRules movementRules;
        private readonly BoardState initialState;
        private readonly Stack<BoardState> undoHistory = new Stack<BoardState>();
        private readonly List<Direction> acceptedCommands = new List<Direction>();
        private readonly ReadOnlyCollection<Direction> readOnlyCommands;

        public GameSession(BoardState initialState, SnakeMovementRules movementRules)
        {
            this.initialState = initialState?.Clone() ?? throw new ArgumentNullException(nameof(initialState));
            this.movementRules = movementRules ?? throw new ArgumentNullException(nameof(movementRules));
            CurrentState = this.initialState.Clone();
            readOnlyCommands = acceptedCommands.AsReadOnly();
        }

        public BoardState CurrentState { get; private set; }

        public bool CanUndo => undoHistory.Count > 0;

        public IReadOnlyList<Direction> AcceptedCommands => readOnlyCommands;

        public TurnResult Move(Direction direction)
        {
            var result = movementRules.Execute(CurrentState, direction);
            if (!result.IsAccepted)
            {
                return result;
            }

            undoHistory.Push(CurrentState.Clone());
            acceptedCommands.Add(direction);
            CurrentState = result.State;
            return result;
        }

        public bool Undo()
        {
            if (undoHistory.Count == 0)
            {
                return false;
            }

            CurrentState = undoHistory.Pop();
            acceptedCommands.RemoveAt(acceptedCommands.Count - 1);
            return true;
        }

        public void Restart()
        {
            undoHistory.Clear();
            acceptedCommands.Clear();
            CurrentState = initialState.Clone();
        }
    }
}

