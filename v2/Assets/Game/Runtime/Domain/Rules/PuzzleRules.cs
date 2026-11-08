using System;
using System.Collections.Generic;
using System.Linq;
using SnakeApple.Domain.Board;
using SnakeApple.Domain.Commands;
using SnakeApple.Domain.Entities;

namespace SnakeApple.Domain.Rules
{
    public sealed class PuzzleRules
    {
        private static readonly GridPos Up = new GridPos(0, 1);
        private static readonly GridPos Down = new GridPos(0, -1);
        private static readonly GridPos Left = new GridPos(-1, 0);
        private static readonly GridPos Right = new GridPos(1, 0);
        private static readonly GridPos[] ExplosionOffsets =
        {
            new GridPos(0, 0),
            Up, Down, Left, Right,
            new GridPos(-1, 1), new GridPos(1, 1),
            new GridPos(-1, -1), new GridPos(1, -1)
        };

        public PuzzleState ResolveInitial(PuzzleState initial)
        {
            if (initial == null) throw new ArgumentNullException(nameof(initial));
            var state = initial.Clone();
            // Legacy levels are authored in their visible spawn pose. Environment
            // resolution begins after the first accepted command, not while loading.
            UpdateTriggers(state, false);
            return state;
        }

        public PuzzleTurnResult Execute(PuzzleState current, Direction direction)
        {
            if (current == null) throw new ArgumentNullException(nameof(current));
            if (current.Status != SessionStatus.Playing)
            {
                return PuzzleTurnResult.Rejected(current, MoveRejectionReason.SessionNotPlaying);
            }

            var offset = direction.ToOffset();
            var target = ResolvePortalTarget(current, offset, current.Head + offset, out var usedPortal);
            if (!current.Size.Contains(target))
            {
                return PuzzleTurnResult.Rejected(
                    current,
                    usedPortal ? MoveRejectionReason.TeleportBlocked : MoveRejectionReason.OutsideBoard);
            }

            if (current.MutableSnake.Contains(target))
            {
                return PuzzleTurnResult.Rejected(current, MoveRejectionReason.SnakeBody);
            }

            var entersPortalPocket = !usedPortal && IsTeleport(current.CellAt(current.Head));
            if (IsHardBlock(current, target) && !entersPortalPocket)
            {
                return PuzzleTurnResult.Rejected(
                    current,
                    usedPortal ? MoveRejectionReason.TeleportBlocked :
                    current.IsDoorClosed(target) ? MoveRejectionReason.ClosedDoor : MoveRejectionReason.Wall);
            }

            var next = current.Clone();
            if (next.TryGetMovable(target, out var movable))
            {
                var destination = target + offset;
                if (IsTeleport(next.CellAt(destination)))
                {
                    destination = ResolvePairedPortal(next, destination) + offset;
                }
                if (!CanMovableEnter(next, destination))
                {
                    return PuzzleTurnResult.Rejected(current, MoveRejectionReason.MovableBlocked);
                }

                next.MutableMovables.Remove(target);
                next.MutableMovables[destination] = movable;
            }

            var cell = next.CellAt(target);
            if (cell == '2')
            {
                next.RemainingApples--;
                next.PendingGrowth += 1;
                next.SetCell(target, '0');
            }
            else if (cell == '8')
            {
                next.CollectedVipApples++;
                next.PendingGrowth += 2;
                next.SetCell(target, '0');
            }

            next.MutableSnake.Insert(0, target);
            if (next.PendingGrowth > 0)
            {
                next.PendingGrowth--;
            }
            else
            {
                next.MutableSnake.RemoveAt(next.MutableSnake.Count - 1);
            }

            var ateApple = cell == '2' || cell == '8';
            var preGravitySnake = next.MutableSnake.ToArray();
            next.TurnIndex++;
            if (CheckExit(next) || CheckStaticHazards(next))
            {
                return PuzzleTurnResult.Accepted(next, preGravitySnake, ateApple);
            }

            UpdateTriggers(next, true);
            // When the head enters VIP fruit in the gravity direction, V1's
            // pre-move support check keeps the worm in place for that turn.
            var gravity = current.Definition.GravityReversed ? Up : Down;
            if (cell != '8' || offset != gravity) ResolveGravity(next);
            if (next.Status == SessionStatus.Playing)
            {
                UpdateTriggers(next, true);
                ResolveDynamicHazards(next);
            }

            return PuzzleTurnResult.Accepted(next, preGravitySnake, ateApple);
        }

        private static GridPos ResolvePortalTarget(
            PuzzleState state,
            GridPos direction,
            GridPos defaultTarget,
            out bool usedPortal)
        {
            usedPortal = false;
            if (state.MutableSnake.Count < 2) return defaultTarget;
            var entry = state.MutableSnake[1];
            var entryValue = state.CellAt(entry);
            // A teleport has a one-cell hidden pocket behind its visible tile. Once
            // the head is in that pocket, the next command chooses the exit direction;
            // it does not have to repeat the direction used to enter the pocket.
            if (!IsTeleport(entryValue) || state.Head.ManhattanDistance(entry) != 1)
            {
                return defaultTarget;
            }

            usedPortal = true;
            var destination = ResolvePairedPortal(state, entry);
            return destination.X >= 0 ? destination + direction : new GridPos(-1, -1);
        }

        private static GridPos ResolvePairedPortal(PuzzleState state, GridPos entry)
        {
            var entryUpper = char.IsUpper(state.CellAt(entry));
            var configured = state.Definition.Teleports;
            for (var index = 0; index < configured.Count; index++)
            {
                if (configured[index] != entry) continue;
                var pairedIndex = index % 2 == 0 ? index + 1 : index - 1;
                if (pairedIndex >= 0 && pairedIndex < configured.Count)
                {
                    var paired = configured[pairedIndex];
                    var pairedValue = state.CellAt(paired);
                    if (IsTeleport(pairedValue) && char.IsUpper(pairedValue) == entryUpper)
                    {
                        return paired;
                    }
                }

                break;
            }

            for (var y = 0; y < state.Size.Height; y++)
            {
                for (var x = 0; x < state.Size.Width; x++)
                {
                    var candidate = new GridPos(x, y);
                    var value = state.CellAt(candidate);
                    if (candidate != entry && IsTeleport(value) && char.IsUpper(value) == entryUpper)
                    {
                        return candidate;
                    }
                }
            }

            return new GridPos(-1, -1);
        }

        private static void ResolveGravity(PuzzleState state)
        {
            var gravity = state.Definition.GravityReversed ? Up : Down;
            var maxTicks = state.Size.Height + state.Size.Width + 4;

            // The released game resolves the worm first (CheckDrop), then resolves
            // rocks/bombs (CheckDropObjects). They are not interleaved per grid cell.
            for (var tick = 0; tick < maxTicks && state.Status == SessionStatus.Playing; tick++)
            {
                if (!CanSnakeFall(state, gravity, out var fallsOutside)) break;
                for (var index = 0; index < state.MutableSnake.Count; index++)
                {
                    state.MutableSnake[index] = state.MutableSnake[index] + gravity;
                }

                if (fallsOutside)
                {
                    Lose(state, PuzzleLossReason.FellOutside);
                    return;
                }

                if (CheckExit(state) || CheckStaticHazards(state)) return;
                UpdateTriggers(state, true);
            }

            for (var tick = 0; tick < maxTicks && state.Status == SessionStatus.Playing; tick++)
            {
                if (!MoveMovablesByGravity(state, gravity)) break;
            }
        }

        private static bool MoveMovablesByGravity(PuzzleState state, GridPos gravity)
        {
            var positions = state.MutableMovables.Keys.ToList();
            positions.Sort((left, right) => gravity.Y < 0
                ? left.Y.CompareTo(right.Y)
                : right.Y.CompareTo(left.Y));
            var moved = false;
            foreach (var position in positions)
            {
                if (!state.MutableMovables.TryGetValue(position, out var kind)) continue;
                if (state.CellAt(position) == 'A') continue;
                var destination = position + gravity;
                if (!state.Size.Contains(destination))
                {
                    state.MutableMovables.Remove(position);
                    moved = true;
                    continue;
                }

                if (state.MutableSnake.Contains(destination))
                {
                    // The worm is a support surface for falling rocks and bombs.
                    // V1 Normal 37 deliberately keeps an upper rock resting on the
                    // worm while it passes underneath; contact is not a crush loss.
                    continue;
                }

                var destinationCell = state.CellAt(destination);
                if (IsGravitySupportCell(destinationCell) ||
                    IsHardBlock(state, destination) || state.MutableMovables.ContainsKey(destination))
                {
                    continue;
                }
                state.MutableMovables.Remove(position);
                state.MutableMovables[destination] = kind;
                moved = true;
            }

            return moved;
        }

        private static bool CanSnakeFall(
            PuzzleState state,
            GridPos gravity,
            out bool fallsOutside)
        {
            fallsOutside = false;
            for (var index = 0; index < state.MutableSnake.Count; index++)
            {
                var position = state.MutableSnake[index];
                if (IsHardBlock(state, position)) return false;
                if (state.CellAt(position) == 'A') return false;
                var destination = position + gravity;
                if (!state.Size.Contains(destination))
                {
                    fallsOutside = true;
                    continue;
                }

                if (state.MutableSnake.Contains(destination)) continue;
                // Fruit is collectible only when the head deliberately moves into it.
                // It remains a solid support during gravity resolution, matching the
                // authored V1 hint paths (for example Normal 38).
                var destinationCell = state.CellAt(destination);
                if (IsGravitySupportCell(destinationCell) ||
                    IsHardBlock(state, destination) || state.MutableMovables.ContainsKey(destination))
                {
                    return false;
                }
            }

            return true;
        }

        private static void UpdateTriggers(PuzzleState state, bool allowSlideToggle)
        {
            for (var index = 0; index < state.Definition.Doors.Count; index++)
            {
                var door = state.Definition.Doors[index];
                var button = state.Definition.DoorButtons.Count == 0
                    ? door
                    : state.Definition.DoorButtons[Math.Min(index, state.Definition.DoorButtons.Count - 1)];
                var shouldOpen = IsOccupied(state, button);
                if (shouldOpen)
                {
                    state.MutableDoors[door] = false;
                }
            }

            var nowPressed = new HashSet<GridPos>();
            for (var y = 0; y < state.Size.Height; y++)
            {
                for (var x = 0; x < state.Size.Width; x++)
                {
                    var position = new GridPos(x, y);
                    if ("QRST".IndexOf(state.CellAt(position)) >= 0 && IsOccupied(state, position))
                    {
                        nowPressed.Add(position);
                    }
                }
            }

            if (allowSlideToggle && nowPressed.Any(position => !state.MutablePressedButtons.Contains(position)))
            {
                TogglePlatforms(state);
            }

            state.MutablePressedButtons.Clear();
            state.MutablePressedButtons.UnionWith(nowPressed);
        }

        private static void TogglePlatforms(PuzzleState state)
        {
            for (var index = 0; index < state.MutablePlatforms.Count; index++)
            {
                var platform = state.MutablePlatforms[index];
                var destination = platform.Destination;
                if (!state.Size.Contains(destination) || IsHardBlockExceptPlatforms(state, destination) ||
                    state.MutableMovables.ContainsKey(destination) || state.MutableSnake.Contains(destination))
                {
                    continue;
                }

                CarryMovablesOnPlatform(state, platform, destination);
                platform.AtFirst = !platform.AtFirst;
            }
        }

        // V1 slide platforms and elevators carry rocks and bombs resting on them. Without this several V1
        // levels (e.g. Normal 90, 136, 175) cannot be completed because the bomb is left behind.
        private static void CarryMovablesOnPlatform(PuzzleState state, SlidePlatformState platform, GridPos destination)
        {
            var current = platform.Current;
            var delta = new GridPos(destination.X - current.X, destination.Y - current.Y);
            var gravity = state.Definition.GravityReversed ? Up : Down;
            var covered = platform.Horizontal
                ? new[] { current, new GridPos(current.X - 1, current.Y) }
                : new[] { current, new GridPos(current.X, current.Y - 1) };

            var carried = new List<GridPos>();
            foreach (var position in state.MutableMovables.Keys)
            {
                for (var index = 0; index < covered.Length; index++)
                {
                    if (position.X == covered[index].X - gravity.X && position.Y == covered[index].Y - gravity.Y)
                    {
                        carried.Add(position);
                        break;
                    }
                }
            }

            if (carried.Count == 0) return;
            carried.Sort((left, right) => delta.Y > 0 ? right.Y.CompareTo(left.Y) : left.Y.CompareTo(right.Y));
            for (var index = 0; index < carried.Count; index++)
            {
                var target = carried[index] + delta;
                if (!state.Size.Contains(target) || IsHardBlockExceptPlatforms(state, target) ||
                    state.MutableMovables.ContainsKey(target) || state.MutableSnake.Contains(target))
                {
                    continue;
                }

                var kind = state.MutableMovables[carried[index]];
                state.MutableMovables.Remove(carried[index]);
                state.MutableMovables[target] = kind;
            }
        }

        private static void ResolveDynamicHazards(PuzzleState state)
        {
            var bombQueue = new Queue<GridPos>();
            TraceLasers(state, bombQueue);
            if (state.Status != SessionStatus.Playing) return;

            if (state.CellAt(state.Head) == 'k' && state.MutableSpentRemotes.Add(state.Head))
            {
                foreach (var movable in state.MutableMovables)
                {
                    if (movable.Value == MovableKind.Bomb) bombQueue.Enqueue(movable.Key);
                }
            }

            ResolveExplosions(state, bombQueue);
            if (state.Status == SessionStatus.Playing)
            {
                UpdateTriggers(state, false);
            }
        }

        private static void TraceLasers(PuzzleState state, Queue<GridPos> bombQueue)
        {
            for (var y = 0; y < state.Size.Height; y++)
            {
                for (var x = 0; x < state.Size.Width; x++)
                {
                    var emitter = new GridPos(x, y);
                    var value = state.CellAt(emitter);
                    var direction = value == 'q' ? Up : value == 'r' ? Down : value == 's' ? Right :
                        value == 't' ? Left : new GridPos(0, 0);
                    if (direction == new GridPos(0, 0)) continue;
                    // A rock/bomb pushed onto the gun occupies its first laser
                    // block slot and disables the beam at the source.
                    if (state.MutableMovables.ContainsKey(emitter)) continue;

                    var ray = emitter + direction;
                    while (state.Size.Contains(ray))
                    {
                        if (IsLaserBlock(state, ray)) break;
                        if (state.TryGetMovable(ray, out var movable))
                        {
                            if (movable == MovableKind.Bomb) bombQueue.Enqueue(ray);
                            break;
                        }

                        if (state.MutableSnake.Contains(ray))
                        {
                            Lose(state, PuzzleLossReason.Laser);
                            return;
                        }

                        ray += direction;
                    }
                }
            }
        }

        private static void ResolveExplosions(PuzzleState state, Queue<GridPos> queue)
        {
            var exploded = new HashSet<GridPos>();
            while (queue.Count > 0 && state.Status == SessionStatus.Playing)
            {
                var center = queue.Dequeue();
                if (!exploded.Add(center)) continue;
                if (!state.TryGetMovable(center, out var centerKind) || centerKind != MovableKind.Bomb) continue;
                state.MutableMovables.Remove(center);

                for (var index = 0; index < ExplosionOffsets.Length; index++)
                {
                    var position = center + ExplosionOffsets[index];
                    if (!state.Size.Contains(position)) continue;
                    if (index > 0 && (IsWallValue(state.CellAt(position)) || state.IsDoorClosed(position) ||
                                      state.IsPlatformAt(position)))
                    {
                        if (IsWallValue(state.CellAt(position))) state.SetCell(position, '0');
                        if (state.MutableDoors.ContainsKey(position))
                        {
                            state.MutableDoors[position] = false;
                            state.SetCell(position, '0');
                        }
                        continue;
                    }

                    if (state.MutableSnake.Contains(position))
                    {
                        Lose(state, PuzzleLossReason.Explosion);
                        return;
                    }

                    if (state.TryGetMovable(position, out var movable) && movable == MovableKind.Bomb)
                    {
                        queue.Enqueue(position);
                    }

                    if (state.CellAt(position) == 'A') state.SetCell(position, '0');
                }
            }
        }

        private static bool CheckStaticHazards(PuzzleState state)
        {
            for (var index = 0; index < state.MutableSnake.Count; index++)
            {
                var value = state.CellAt(state.MutableSnake[index]);
                if ("ghij".IndexOf(value) >= 0)
                {
                    Lose(state, PuzzleLossReason.Spike);
                    return true;
                }

                if (value == '6')
                {
                    Lose(state, PuzzleLossReason.Toxic);
                    return true;
                }
            }

            return false;
        }

        private static bool CheckExit(PuzzleState state)
        {
            var exitUnlocked = state.RemainingApples == 0 ||
                               string.Equals(state.Definition.Mode, "challenge", StringComparison.OrdinalIgnoreCase);
            if (exitUnlocked && state.CellAt(state.Head) == 'X')
            {
                state.Status = SessionStatus.Won;
                return true;
            }

            return false;
        }

        private static bool CanMovableEnter(PuzzleState state, GridPos position)
        {
            return state.Size.Contains(position) && !IsHardBlock(state, position) &&
                   !state.MutableMovables.ContainsKey(position) && !state.MutableSnake.Contains(position);
        }

        private static bool IsHardBlock(PuzzleState state, GridPos position)
        {
            return IsHardBlockExceptPlatforms(state, position) || state.IsPlatformAt(position);
        }

        private static bool IsLaserBlock(PuzzleState state, GridPos position)
        {
            if (IsHardBlock(state, position)) return true;
            return state.Definition.Doors.Contains(position) && state.CellAt(position) != '0';
        }

        private static bool IsHardBlockExceptPlatforms(PuzzleState state, GridPos position)
        {
            return !state.Size.Contains(position) || IsWallValue(state.CellAt(position)) || state.IsDoorClosed(position);
        }

        private static bool IsWallValue(char value)
        {
            return value == '1' || value == '7' || value == 'p' || value == 'B';
        }

        private static bool IsTeleport(char value)
        {
            return "cdefCDEF".IndexOf(value) >= 0;
        }

        private static bool IsGravitySupportCell(char value)
        {
            return value == '2' || value == '8' || value == 'k';
        }

        private static bool IsOccupied(PuzzleState state, GridPos position)
        {
            return state.MutableSnake.Contains(position) || state.MutableMovables.ContainsKey(position);
        }

        private static void Lose(PuzzleState state, PuzzleLossReason reason)
        {
            state.Status = SessionStatus.Lost;
            state.LossReason = reason;
        }
    }
}
