using System;
using System.Collections.Generic;
using SnakeApple.Domain.Board;
using SnakeApple.Domain.Entities;
using SnakeApple.Domain.Levels;
using UnityEngine;

namespace SnakeApple.Infrastructure.Levels
{
    public sealed class LevelJsonParser
    {
        public LevelDefinition Parse(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new ArgumentException("Level JSON is empty.", nameof(json));
            }

            var data = JsonUtility.FromJson<LevelJsonData>(json);
            if (data == null)
            {
                throw new FormatException("Level JSON could not be parsed.");
            }

            if (data.schemaVersion != 1)
            {
                throw new FormatException($"Unsupported schemaVersion {data.schemaVersion}.");
            }

            if (data.size == null || data.tiles == null)
            {
                throw new FormatException("Level requires size and tiles.");
            }

            var size = new BoardSize(data.size.width, data.size.height);
            if (data.tiles.Length != size.Height)
            {
                throw new FormatException($"Expected {size.Height} tile rows, got {data.tiles.Length}.");
            }

            var walls = new List<GridPos>();
            var snakeCells = new HashSet<GridPos>();
            var collectibles = new Dictionary<GridPos, CollectibleKind>();
            GridPos? head = null;
            GridPos? tail = null;
            GridPos? exit = null;

            for (var row = 0; row < data.tiles.Length; row++)
            {
                var line = data.tiles[row];
                if (line == null || line.Length != size.Width)
                {
                    throw new FormatException($"Row {row} must contain exactly {size.Width} symbols.");
                }

                var y = size.Height - 1 - row;
                for (var x = 0; x < line.Length; x++)
                {
                    var position = new GridPos(x, y);
                    switch (line[x])
                    {
                        case '.':
                            break;
                        case '#':
                            walls.Add(position);
                            break;
                        case 'H':
                            EnsureUnset(head, "head");
                            head = position;
                            snakeCells.Add(position);
                            break;
                        case 'o':
                            snakeCells.Add(position);
                            break;
                        case 't':
                            EnsureUnset(tail, "tail");
                            tail = position;
                            snakeCells.Add(position);
                            break;
                        case 'a':
                            collectibles.Add(position, CollectibleKind.Apple);
                            break;
                        case 'A':
                            collectibles.Add(position, CollectibleKind.VipApple);
                            break;
                        case 'E':
                            EnsureUnset(exit, "exit");
                            exit = position;
                            break;
                        default:
                            throw new FormatException($"Unknown symbol '{line[x]}' at ({x},{y}).");
                    }
                }
            }

            if (!head.HasValue)
            {
                throw new FormatException("Level requires exactly one snake head.");
            }

            if (!exit.HasValue)
            {
                throw new FormatException("Level requires exactly one exit.");
            }

            var orderedSnake = TraceSnake(head.Value, tail, snakeCells);
            var level = new LevelDefinition(data.id, size, walls, orderedSnake, collectibles, exit.Value);
            var validation = new LevelValidator().Validate(level);
            if (!validation.IsValid)
            {
                throw new FormatException(string.Join(" | ", validation.Errors));
            }

            return level;
        }

        private static void EnsureUnset(GridPos? current, string semantic)
        {
            if (current.HasValue)
            {
                throw new FormatException($"Level contains more than one {semantic}.");
            }
        }

        private static IReadOnlyList<GridPos> TraceSnake(
            GridPos head,
            GridPos? declaredTail,
            HashSet<GridPos> snakeCells)
        {
            var ordered = new List<GridPos> { head };
            var visited = new HashSet<GridPos> { head };
            var current = head;

            while (ordered.Count < snakeCells.Count)
            {
                var candidates = new List<GridPos>();
                AddCandidate(current + new GridPos(0, 1), snakeCells, visited, candidates);
                AddCandidate(current + new GridPos(0, -1), snakeCells, visited, candidates);
                AddCandidate(current + new GridPos(-1, 0), snakeCells, visited, candidates);
                AddCandidate(current + new GridPos(1, 0), snakeCells, visited, candidates);

                if (candidates.Count != 1)
                {
                    throw new FormatException(
                        $"Snake is branched or disconnected near {current}; expected one unvisited neighbor, got {candidates.Count}.");
                }

                current = candidates[0];
                visited.Add(current);
                ordered.Add(current);
            }

            if (declaredTail.HasValue && ordered[ordered.Count - 1] != declaredTail.Value)
            {
                throw new FormatException("Declared tail is not the final segment traced from the head.");
            }

            return ordered;
        }

        private static void AddCandidate(
            GridPos position,
            HashSet<GridPos> snakeCells,
            HashSet<GridPos> visited,
            ICollection<GridPos> candidates)
        {
            if (snakeCells.Contains(position) && !visited.Contains(position))
            {
                candidates.Add(position);
            }
        }

        [Serializable]
        private sealed class LevelJsonData
        {
            public int schemaVersion;
            public string id;
            public SizeData size;
            public string[] tiles;
        }

        [Serializable]
        private sealed class SizeData
        {
            public int width;
            public int height;
        }
    }
}

