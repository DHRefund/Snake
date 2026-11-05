using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SnakeApple.Domain.Board;
using SnakeApple.Domain.Entities;

namespace SnakeApple.Domain.Levels
{
    public sealed class LevelDefinition
    {
        private readonly ReadOnlyCollection<GridPos> walls;
        private readonly ReadOnlyCollection<GridPos> snakeSegments;
        private readonly ReadOnlyDictionary<GridPos, CollectibleKind> collectibles;

        public LevelDefinition(
            string id,
            BoardSize size,
            IEnumerable<GridPos> walls,
            IEnumerable<GridPos> snakeSegments,
            IDictionary<GridPos, CollectibleKind> collectibles,
            GridPos exitPosition)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Level ID is required.", nameof(id));
            }

            Id = id;
            Size = size;
            this.walls = new List<GridPos>(walls ?? throw new ArgumentNullException(nameof(walls))).AsReadOnly();
            this.snakeSegments = new List<GridPos>(snakeSegments ?? throw new ArgumentNullException(nameof(snakeSegments))).AsReadOnly();
            this.collectibles = new ReadOnlyDictionary<GridPos, CollectibleKind>(
                new Dictionary<GridPos, CollectibleKind>(collectibles ?? throw new ArgumentNullException(nameof(collectibles))));
            ExitPosition = exitPosition;
        }

        public string Id { get; }

        public BoardSize Size { get; }

        public IReadOnlyList<GridPos> Walls => walls;

        public IReadOnlyList<GridPos> SnakeSegments => snakeSegments;

        public IReadOnlyDictionary<GridPos, CollectibleKind> Collectibles => collectibles;

        public GridPos ExitPosition { get; }

        public BoardState CreateInitialState()
        {
            return new BoardState(
                Size,
                walls,
                new SnakeState(snakeSegments),
                new Dictionary<GridPos, CollectibleKind>(collectibles),
                ExitPosition);
        }
    }
}

