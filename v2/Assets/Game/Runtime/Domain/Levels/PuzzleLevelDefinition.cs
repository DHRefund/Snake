using System;
using System.Collections.Generic;
using SnakeApple.Domain.Board;

namespace SnakeApple.Domain.Levels
{
    public sealed class PuzzleLevelDefinition
    {
        public PuzzleLevelDefinition(
            string id,
            string mode,
            int number,
            BoardSize size,
            string cells,
            bool gravityReversed,
            IReadOnlyList<GridPos> slideBarEndpoints,
            IReadOnlyList<bool> slideBarsHorizontal,
            IReadOnlyList<GridPos> doorButtons,
            IReadOnlyList<GridPos> doors,
            IReadOnlyList<bool> doorInitiallyClosed,
            IReadOnlyList<GridPos> teleports,
            string solution)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Level ID is required.", nameof(id));
            if (string.IsNullOrWhiteSpace(mode)) throw new ArgumentException("Mode is required.", nameof(mode));
            if (cells == null || cells.Length != size.Width * size.Height)
            {
                throw new ArgumentException("Cell payload does not match board dimensions.", nameof(cells));
            }

            Id = id;
            Mode = mode;
            Number = number;
            Size = size;
            Cells = cells;
            GravityReversed = gravityReversed;
            SlideBarEndpoints = slideBarEndpoints ?? Array.Empty<GridPos>();
            SlideBarsHorizontal = slideBarsHorizontal ?? Array.Empty<bool>();
            DoorButtons = doorButtons ?? Array.Empty<GridPos>();
            Doors = doors ?? Array.Empty<GridPos>();
            DoorInitiallyClosed = doorInitiallyClosed ?? Array.Empty<bool>();
            Teleports = teleports ?? Array.Empty<GridPos>();
            Solution = solution ?? string.Empty;
        }

        public string Id { get; }
        public string Mode { get; }
        public int Number { get; }
        public BoardSize Size { get; }
        public string Cells { get; }
        public bool GravityReversed { get; }
        public IReadOnlyList<GridPos> SlideBarEndpoints { get; }
        public IReadOnlyList<bool> SlideBarsHorizontal { get; }
        public IReadOnlyList<GridPos> DoorButtons { get; }
        public IReadOnlyList<GridPos> Doors { get; }
        public IReadOnlyList<bool> DoorInitiallyClosed { get; }
        public IReadOnlyList<GridPos> Teleports { get; }
        public string Solution { get; }
    }
}
