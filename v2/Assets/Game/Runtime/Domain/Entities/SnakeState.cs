using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SnakeApple.Domain.Board;

namespace SnakeApple.Domain.Entities
{
    public sealed class SnakeState
    {
        private readonly GridPos[] segments;
        private readonly ReadOnlyCollection<GridPos> readOnlySegments;

        public SnakeState(IEnumerable<GridPos> segments)
        {
            if (segments == null)
            {
                throw new ArgumentNullException(nameof(segments));
            }

            this.segments = new List<GridPos>(segments).ToArray();
            if (this.segments.Length == 0)
            {
                throw new ArgumentException("Snake must have at least one segment.", nameof(segments));
            }

            readOnlySegments = Array.AsReadOnly(this.segments);
        }

        public GridPos Head => segments[0];

        public int Length => segments.Length;

        public IReadOnlyList<GridPos> Segments => readOnlySegments;

        public bool Occupies(GridPos position)
        {
            for (var index = 0; index < segments.Length; index++)
            {
                if (segments[index] == position)
                {
                    return true;
                }
            }

            return false;
        }

        public SnakeState MoveTo(GridPos newHead, bool grows)
        {
            var nextLength = grows ? segments.Length + 1 : segments.Length;
            var next = new GridPos[nextLength];
            next[0] = newHead;

            var copiedSegments = grows ? segments.Length : segments.Length - 1;
            Array.Copy(segments, 0, next, 1, copiedSegments);
            return new SnakeState(next);
        }

        public SnakeState Clone()
        {
            return new SnakeState(segments);
        }
    }
}

