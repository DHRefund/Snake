using System;
using System.Collections.Generic;
using SnakeApple.Domain.Board;

namespace SnakeApple.Domain.Levels
{
    public sealed class LevelValidator
    {
        public LevelValidationResult Validate(LevelDefinition level)
        {
            if (level == null)
            {
                throw new ArgumentNullException(nameof(level));
            }

            var errors = new List<string>();
            var occupied = new HashSet<GridPos>();

            if (!level.Size.Contains(level.ExitPosition))
            {
                errors.Add($"Exit {level.ExitPosition} is outside the board.");
            }

            if (level.SnakeSegments.Count == 0)
            {
                errors.Add("Snake must have at least one segment.");
            }

            for (var index = 0; index < level.SnakeSegments.Count; index++)
            {
                var segment = level.SnakeSegments[index];
                if (!level.Size.Contains(segment))
                {
                    errors.Add($"Snake segment {index} at {segment} is outside the board.");
                }

                if (!occupied.Add(segment))
                {
                    errors.Add($"Snake overlaps itself at {segment}.");
                }

                if (index > 0 && segment.ManhattanDistance(level.SnakeSegments[index - 1]) != 1)
                {
                    errors.Add($"Snake segments {index - 1} and {index} are not adjacent.");
                }
            }

            foreach (var wall in level.Walls)
            {
                if (!level.Size.Contains(wall))
                {
                    errors.Add($"Wall {wall} is outside the board.");
                }

                if (occupied.Contains(wall))
                {
                    errors.Add($"Wall overlaps snake at {wall}.");
                }
            }

            foreach (var collectible in level.Collectibles)
            {
                if (!level.Size.Contains(collectible.Key))
                {
                    errors.Add($"Collectible {collectible.Key} is outside the board.");
                }

                if (occupied.Contains(collectible.Key))
                {
                    errors.Add($"Collectible overlaps snake at {collectible.Key}.");
                }
            }

            return new LevelValidationResult(errors);
        }
    }
}

