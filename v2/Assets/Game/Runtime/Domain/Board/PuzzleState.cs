using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SnakeApple.Domain.Entities;
using SnakeApple.Domain.Levels;

namespace SnakeApple.Domain.Board
{
    public sealed class PuzzleState
    {
        private readonly char[] cells;
        private readonly List<GridPos> snake;
        private readonly Dictionary<GridPos, MovableKind> movables;
        private readonly Dictionary<GridPos, bool> doorsClosed;
        private readonly List<SlidePlatformState> slidePlatforms;
        private readonly HashSet<GridPos> pressedButtons;
        private readonly HashSet<GridPos> spentRemotes;

        private PuzzleState(
            PuzzleLevelDefinition definition,
            char[] cells,
            List<GridPos> snake,
            Dictionary<GridPos, MovableKind> movables,
            Dictionary<GridPos, bool> doorsClosed,
            List<SlidePlatformState> slidePlatforms,
            HashSet<GridPos> pressedButtons,
            HashSet<GridPos> spentRemotes,
            int turnIndex,
            int collectedVipApples,
            SessionStatus status,
            PuzzleLossReason lossReason)
        {
            Definition = definition;
            this.cells = cells;
            this.snake = snake;
            this.movables = movables;
            this.doorsClosed = doorsClosed;
            this.slidePlatforms = slidePlatforms;
            this.pressedButtons = pressedButtons;
            this.spentRemotes = spentRemotes;
            TurnIndex = turnIndex;
            CollectedVipApples = collectedVipApples;
            Status = status;
            LossReason = lossReason;
        }

        public PuzzleLevelDefinition Definition { get; }
        public BoardSize Size => Definition.Size;
        public IReadOnlyList<GridPos> Snake => snake.AsReadOnly();
        public IReadOnlyDictionary<GridPos, MovableKind> Movables =>
            new ReadOnlyDictionary<GridPos, MovableKind>(movables);
        public IReadOnlyDictionary<GridPos, bool> DoorsClosed =>
            new ReadOnlyDictionary<GridPos, bool>(doorsClosed);
        public IReadOnlyList<SlidePlatformState> SlidePlatforms => slidePlatforms.AsReadOnly();
        public int TurnIndex { get; internal set; }
        public int CollectedVipApples { get; internal set; }
        public int PendingGrowth { get; internal set; }
        public SessionStatus Status { get; internal set; }
        public PuzzleLossReason LossReason { get; internal set; }
        public GridPos Head => snake[0];
        public int RemainingApples { get; internal set; }
        public bool IsExitOpen => RemainingApples == 0;

        public static PuzzleState Create(PuzzleLevelDefinition definition)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            var cells = definition.Cells.ToCharArray();
            var snakeCells = new HashSet<GridPos>();
            GridPos? head = null;
            var movables = new Dictionary<GridPos, MovableKind>();
            var apples = 0;

            for (var y = 0; y < definition.Size.Height; y++)
            {
                for (var x = 0; x < definition.Size.Width; x++)
                {
                    var position = new GridPos(x, y);
                    var value = cells[Index(definition.Size, position)];
                    if (value == '9')
                    {
                        head = position;
                        snakeCells.Add(position);
                        cells[Index(definition.Size, position)] = '0';
                    }
                    else if (value == '3')
                    {
                        snakeCells.Add(position);
                        cells[Index(definition.Size, position)] = '0';
                    }
                    else if (value == '5' || value == 'K' || value == 'y' || value == 'z')
                    {
                        movables[position] = value == 'K' ? MovableKind.Bomb : MovableKind.Rock;
                        cells[Index(definition.Size, position)] = value == 'y' ? 'g' : value == 'z' ? '6' : '0';
                    }
                    else if (value == '2')
                    {
                        apples++;
                    }
                }
            }

            if (!head.HasValue) throw new FormatException($"{definition.Id} has no snake head.");
            var orderedSnake = TraceSnake(head.Value, snakeCells);
            var doorStates = new Dictionary<GridPos, bool>();
            for (var index = 0; index < definition.Doors.Count; index++)
            {
                doorStates[definition.Doors[index]] = index >= definition.DoorInitiallyClosed.Count ||
                                                     definition.DoorInitiallyClosed[index];
            }

            var platforms = new List<SlidePlatformState>();
            for (var index = 0; index + 1 < definition.SlideBarEndpoints.Count; index += 2)
            {
                var pairIndex = index / 2;
                platforms.Add(new SlidePlatformState(
                    definition.SlideBarEndpoints[index],
                    definition.SlideBarEndpoints[index + 1],
                    pairIndex < definition.SlideBarsHorizontal.Count && definition.SlideBarsHorizontal[pairIndex]));
            }

            return new PuzzleState(
                definition,
                cells,
                orderedSnake,
                movables,
                doorStates,
                platforms,
                new HashSet<GridPos>(),
                new HashSet<GridPos>(),
                0,
                0,
                SessionStatus.Playing,
                PuzzleLossReason.None)
            {
                RemainingApples = apples
            };
        }

        public char CellAt(GridPos position)
        {
            return Size.Contains(position) ? cells[Index(Size, position)] : '\0';
        }

        public bool TryGetMovable(GridPos position, out MovableKind kind)
        {
            return movables.TryGetValue(position, out kind);
        }

        public bool IsDoorClosed(GridPos position)
        {
            return doorsClosed.TryGetValue(position, out var closed) && closed;
        }

        public bool IsPlatformAt(GridPos position)
        {
            for (var index = 0; index < slidePlatforms.Count; index++)
            {
                if (slidePlatforms[index].Occupies(position)) return true;
            }

            return false;
        }

        public bool IsButtonPressed(GridPos position)
        {
            return pressedButtons != null && pressedButtons.Contains(position);
        }

        internal char[] MutableCells => cells;
        internal List<GridPos> MutableSnake => snake;
        internal Dictionary<GridPos, MovableKind> MutableMovables => movables;
        internal Dictionary<GridPos, bool> MutableDoors => doorsClosed;
        internal List<SlidePlatformState> MutablePlatforms => slidePlatforms;
        internal HashSet<GridPos> MutablePressedButtons => pressedButtons;
        internal HashSet<GridPos> MutableSpentRemotes => spentRemotes;

        public PuzzleState Clone()
        {
            var platformClones = new List<SlidePlatformState>(slidePlatforms.Count);
            for (var index = 0; index < slidePlatforms.Count; index++)
            {
                platformClones.Add(slidePlatforms[index].Clone());
            }

            return new PuzzleState(
                Definition,
                (char[])cells.Clone(),
                new List<GridPos>(snake),
                new Dictionary<GridPos, MovableKind>(movables),
                new Dictionary<GridPos, bool>(doorsClosed),
                platformClones,
                new HashSet<GridPos>(pressedButtons),
                new HashSet<GridPos>(spentRemotes),
                TurnIndex,
                CollectedVipApples,
                Status,
                LossReason)
            {
                RemainingApples = RemainingApples,
                PendingGrowth = PendingGrowth
            };
        }

        internal void SetCell(GridPos position, char value)
        {
            cells[Index(Size, position)] = value;
        }

        private static int Index(BoardSize size, GridPos position)
        {
            // Canonical domain Y grows upward; legacy rows in the source string grow downward.
            var sourceY = size.Height - 1 - position.Y;
            return sourceY * size.Width + position.X;
        }

        private static List<GridPos> TraceSnake(GridPos head, HashSet<GridPos> cells)
        {
            var result = new List<GridPos> { head };
            var remaining = new HashSet<GridPos>(cells);
            remaining.Remove(head);
            var current = head;
            var offsets = new[]
            {
                new GridPos(0, 1), new GridPos(0, -1), new GridPos(-1, 0), new GridPos(1, 0)
            };

            while (remaining.Count > 0)
            {
                GridPos? next = null;
                for (var index = 0; index < offsets.Length; index++)
                {
                    var candidate = current + offsets[index];
                    if (!remaining.Contains(candidate)) continue;
                    if (next.HasValue)
                    {
                        throw new FormatException("Snake body is branched and cannot be ordered.");
                    }

                    next = candidate;
                }

                if (!next.HasValue)
                {
                    throw new FormatException("Snake body is disconnected.");
                }

                current = next.Value;
                result.Add(current);
                remaining.Remove(current);
            }

            return result;
        }
    }
}
