using System;
using System.Collections.Generic;
using SnakeApple.Domain.Board;
using SnakeApple.Domain.Commands;
using SnakeApple.Domain.Levels;
using SnakeApple.Domain.Rules;

namespace SnakeApple.Application.GameSession
{
    public sealed class PuzzleGameSession
    {
        private readonly PuzzleRules rules;
        private readonly PuzzleState initialState;
        private readonly Stack<PuzzleState> history = new Stack<PuzzleState>();

        public PuzzleGameSession(PuzzleLevelDefinition definition, PuzzleRules rules)
        {
            this.rules = rules ?? throw new ArgumentNullException(nameof(rules));
            initialState = this.rules.ResolveInitial(PuzzleState.Create(
                definition ?? throw new ArgumentNullException(nameof(definition))));
            CurrentState = initialState.Clone();
        }

        public PuzzleState CurrentState { get; private set; }
        public bool CanUndo => history.Count > 0;

        public PuzzleTurnResult Move(Direction direction)
        {
            var result = rules.Execute(CurrentState, direction);
            if (!result.IsAccepted) return result;
            history.Push(CurrentState.Clone());
            CurrentState = result.State;
            return result;
        }

        public bool Undo()
        {
            if (history.Count == 0) return false;
            CurrentState = history.Pop();
            return true;
        }

        public void Restart()
        {
            history.Clear();
            CurrentState = initialState.Clone();
        }
    }
}
