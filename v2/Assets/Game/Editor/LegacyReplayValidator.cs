using System;
using System.Collections.Generic;
using System.IO;
using SnakeApple.Application.GameSession;
using SnakeApple.Domain.Board;
using SnakeApple.Domain.Commands;
using SnakeApple.Domain.Rules;
using SnakeApple.Infrastructure.Levels;
using UnityEditor;
using UnityEngine;

namespace SnakeApple.Editor
{
    public static class LegacyReplayValidator
    {
        [InitializeOnLoadMethod]
        private static void QueueValidation()
        {
            EditorApplication.delayCall += ValidateWhenReady;
        }

        [MenuItem("Snake Apple V2/Validate All V1 Hint Replays")]
        public static void ValidateFromMenu()
        {
            ValidateAll();
        }

        public static void ValidateAll()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            var catalog = LegacyLevelCatalogLoader.Load();
            var won = 0;
            var incomplete = 0;
            var lost = 0;
            var rejected = 0;
            var exceptions = 0;
            var examples = new List<string>();

            foreach (var record in catalog.levels)
            {
                try
                {
                    var session = new PuzzleGameSession(LegacyLevelAdapter.ToDefinition(record), new PuzzleRules());
                    var hadRejectedMove = false;
                    var processedSteps = 0;
                    for (var index = 0; index < record.solution.Length; index++)
                    {
                        var result = session.Move(ParseDirection(record.solution[index]));
                        if (!result.IsAccepted)
                        {
                            rejected++;
                            hadRejectedMove = true;
                            AddExample(examples,
                                $"{record.id}: rejected step {index + 1} '{record.solution[index]}' " +
                                $"({result.RejectionReason}), head={session.CurrentState.Head}, " +
                                $"snake={string.Join(" ", session.CurrentState.Snake)}, " +
                                $"movables={string.Join(" ", session.CurrentState.Movables)}");
                            break;
                        }

                        processedSteps = index + 1;
                        if (session.CurrentState.Status != SessionStatus.Playing) break;
                    }

                    if (hadRejectedMove) continue;
                    if (session.CurrentState.Status == SessionStatus.Won)
                    {
                        won++;
                    }
                    else if (session.CurrentState.Status == SessionStatus.Lost)
                    {
                        lost++;
                        AddExample(examples,
                            $"{record.id}: lost at step {processedSteps} " +
                            $"'{record.solution[Math.Max(0, processedSteps - 1)]}' ({session.CurrentState.LossReason}), " +
                            $"head={session.CurrentState.Head}, snake={string.Join(" ", session.CurrentState.Snake)}");
                    }
                    else
                    {
                        incomplete++;
                        AddExample(examples, $"{record.id}: hint ended before Win");
                    }
                }
                catch (Exception exception)
                {
                    exceptions++;
                    AddExample(examples, $"{record.id}: {exception.GetType().Name} - {exception.Message}");
                }
            }

            var summary = $"[Snake Apple V2] V1 replay validation: {won}/{catalog.levels.Length} won, " +
                          $"{incomplete} incomplete, {lost} lost, {rejected} rejected, {exceptions} exceptions.";
            var report = new List<string> { summary };
            report.AddRange(examples);
            File.WriteAllLines(Path.GetFullPath("Library/LegacyReplayValidation.txt"), report);
            WriteTrace(catalog, "normal-064");
            WriteTrace(catalog, "normal-084");
            WriteTrace(catalog, "normal-090");
            WriteTrace(catalog, "normal-121");
            WriteTrace(catalog, "normal-123");
            WriteTrace(catalog, "normal-126");
            WriteTrace(catalog, "normal-128");
            WriteTrace(catalog, "normal-130");
            WriteTrace(catalog, "normal-057");
            WriteTrace(catalog, "normal-079");
            WriteTrace(catalog, "normal-093");
            WriteTrace(catalog, "normal-111");
            WriteTrace(catalog, "normal-115");
            WriteTrace(catalog, "normal-119");
            WriteTrace(catalog, "normal-136");
            WriteTrace(catalog, "normal-150");
            WriteTrace(catalog, "normal-175");
            WriteTrace(catalog, "normal-178");
            WriteTrace(catalog, "normal-179");
            WriteTrace(catalog, "normal-177");
            WriteTrace(catalog, "challenge-009");
            WriteTrace(catalog, "challenge-011");
            WriteTrace(catalog, "challenge-025");
            WriteTrace(catalog, "challenge-028");
            WriteTrace(catalog, "challenge-030");
            WriteTrace(catalog, "challenge-039");
            WriteTrace(catalog, "challenge-040");
            WriteTrace(catalog, "challenge-041");
            WriteTrace(catalog, "challenge-043");
            if (won == catalog.levels.Length)
            {
                Debug.Log(summary);
            }
            else
            {
                Debug.LogWarning(summary + "\n" + string.Join("\n", examples));
            }
        }

        private static void ValidateWhenReady()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating ||
                EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.delayCall += ValidateWhenReady;
                return;
            }

            ValidateAll();
        }

        private static Direction ParseDirection(char value)
        {
            switch (value)
            {
                case 'U': return Direction.Up;
                case 'D': return Direction.Down;
                case 'L': return Direction.Left;
                case 'R': return Direction.Right;
                default: throw new FormatException($"Unknown hint direction '{value}'.");
            }
        }

        private static void AddExample(ICollection<string> examples, string value)
        {
            examples.Add(value);
        }

        private static void WriteTrace(LegacyLevelCatalog catalog, string levelId)
        {
            LegacyLevelRecord record = null;
            for (var index = 0; index < catalog.levels.Length; index++)
            {
                if (catalog.levels[index].id == levelId)
                {
                    record = catalog.levels[index];
                    break;
                }
            }

            if (record == null) return;
            var session = new PuzzleGameSession(LegacyLevelAdapter.ToDefinition(record), new PuzzleRules());
            var lines = new List<string>
            {
                $"{record.id} initial: head={session.CurrentState.Head}, " +
                $"snake={string.Join(" ", session.CurrentState.Snake)}, " +
                $"apples={session.CurrentState.RemainingApples}, " +
                $"movables={string.Join(" ", session.CurrentState.Movables)}, " +
                $"platforms={FormatPlatforms(session.CurrentState)}"
            };
            for (var index = 0; index < record.solution.Length; index++)
            {
                var command = record.solution[index];
                var result = session.Move(ParseDirection(command));
                lines.Add(
                    $"{index + 1:00} {command}: accepted={result.IsAccepted}, " +
                    $"reason={result.RejectionReason}, status={session.CurrentState.Status}, " +
                    $"head={session.CurrentState.Head}, apples={session.CurrentState.RemainingApples}, " +
                    $"snake={string.Join(" ", session.CurrentState.Snake)}, " +
                    $"movables={string.Join(" ", session.CurrentState.Movables)}, " +
                    $"platforms={FormatPlatforms(session.CurrentState)}");
                if (!result.IsAccepted || session.CurrentState.Status != SessionStatus.Playing) break;
            }

            File.WriteAllLines(Path.GetFullPath($"Library/LegacyReplayTrace-{levelId}.txt"), lines);
        }

        private static string FormatPlatforms(PuzzleState state)
        {
            var values = new List<string>();
            for (var index = 0; index < state.SlidePlatforms.Count; index++)
            {
                var platform = state.SlidePlatforms[index];
                values.Add($"{platform.Current}/{(platform.Horizontal ? "H" : "V")}");
            }

            return string.Join(" ", values);
        }
    }
}
