using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SnakeApple.Domain.Levels
{
    public sealed class LevelValidationResult
    {
        public LevelValidationResult(IList<string> errors)
        {
            Errors = new ReadOnlyCollection<string>(errors);
        }

        public IReadOnlyList<string> Errors { get; }

        public bool IsValid => Errors.Count == 0;
    }
}

