using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SnakeApple.Domain.Entities;

namespace SnakeApple.Domain.Board
{
    public sealed class BoardState
    {
        private readonly HashSet<GridPos> walls;
        private readonly Dictionary<GridPos, CollectibleKind> collectibles;
        private readonly ReadOnlyCollection<GridPos> readOnlyWalls;
        private readonly ReadOnlyDictionary<GridPos, CollectibleKind> readOnlyCollectibles;

        public BoardState(
            BoardSize size,
            IEnumerable<GridPos> walls,
            SnakeState snake,
            IDictionary<GridPos, CollectibleKind> collectibles,
            GridPos exitPosition,
            int turnIndex = 0,
            int collectedVipApples = 0,
            SessionStatus status = SessionStatus.Playing)
        {
            Size = size;
            this.walls = new HashSet<GridPos>(walls ?? throw new ArgumentNullException(nameof(walls)));
            Snake = snake ?? throw new ArgumentNullException(nameof(snake));
            this.collectibles = new Dictionary<GridPos, CollectibleKind>(
                collectibles ?? throw new ArgumentNullException(nameof(collectibles)));
            ExitPosition = exitPosition;
            TurnIndex = turnIndex;
            CollectedVipApples = collectedVipApples;
            Status = status;
            readOnlyWalls = Array.AsReadOnly(new List<GridPos>(this.walls).ToArray());
            readOnlyCollectibles = new ReadOnlyDictionary<GridPos, CollectibleKind>(this.collectibles);
        }

        public BoardSize Size { get; }

        public IReadOnlyCollection<GridPos> Walls => readOnlyWalls;

        public SnakeState Snake { get; }

        public IReadOnlyDictionary<GridPos, CollectibleKind> Collectibles => readOnlyCollectibles;

        public GridPos ExitPosition { get; }

        public int TurnIndex { get; }

        public int CollectedVipApples { get; }

        public SessionStatus Status { get; }

        public int RemainingApples => collectibles.Count;

        public bool IsExitOpen => RemainingApples == 0;

        public bool IsWall(GridPos position)
        {
            return walls.Contains(position);
        }

        public bool TryGetCollectible(GridPos position, out CollectibleKind kind)
        {
            return collectibles.TryGetValue(position, out kind);
        }

        public BoardState ApplyAcceptedMove(SnakeState nextSnake, GridPos? consumedAt, CollectibleKind? consumedKind)
        {
            var nextCollectibles = new Dictionary<GridPos, CollectibleKind>(collectibles);
            if (consumedAt.HasValue)
            {
                nextCollectibles.Remove(consumedAt.Value);
            }

            var nextVipCount = CollectedVipApples;
            if (consumedKind == CollectibleKind.VipApple)
            {
                nextVipCount++;
            }

            var nextStatus = nextCollectibles.Count == 0 && nextSnake.Head == ExitPosition
                ? SessionStatus.Won
                : SessionStatus.Playing;

            return new BoardState(
                Size,
                walls,
                nextSnake,
                nextCollectibles,
                ExitPosition,
                TurnIndex + 1,
                nextVipCount,
                nextStatus);
        }

        public BoardState Clone()
        {
            return new BoardState(
                Size,
                walls,
                Snake.Clone(),
                collectibles,
                ExitPosition,
                TurnIndex,
                CollectedVipApples,
                Status);
        }
    }
}

