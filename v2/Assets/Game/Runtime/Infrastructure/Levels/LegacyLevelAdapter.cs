using System;
using SnakeApple.Domain.Board;
using SnakeApple.Domain.Levels;

namespace SnakeApple.Infrastructure.Levels
{
    public static class LegacyLevelAdapter
    {
        public static PuzzleLevelDefinition ToDefinition(LegacyLevelRecord record)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));
            var size = new BoardSize(record.width, record.height);
            return new PuzzleLevelDefinition(
                record.id,
                record.mode,
                record.number,
                size,
                record.data,
                record.gravityReversed,
                Convert(record.slideBars, size),
                record.slideBarsHorizontal ?? Array.Empty<bool>(),
                Convert(record.doorButtons, size),
                Convert(record.doors, size),
                record.doorInitiallyClosed ?? Array.Empty<bool>(),
                Convert(record.teleports, size),
                record.solution);
        }

        private static GridPos[] Convert(LegacyPoint[] points, BoardSize size)
        {
            if (points == null || points.Length == 0) return Array.Empty<GridPos>();
            var result = new GridPos[points.Length];
            for (var index = 0; index < points.Length; index++)
            {
                result[index] = new GridPos(points[index].x, size.Height - 1 - points[index].y);
            }

            return result;
        }
    }
}
