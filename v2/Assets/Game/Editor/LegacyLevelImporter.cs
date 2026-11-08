using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using SnakeApple.Infrastructure.Levels;
using UnityEditor;
using UnityEngine;

namespace SnakeApple.Editor
{
    public static class LegacyLevelImporter
    {
        private const int ImporterVersion = 1;
        private const string OutputPath = "Assets/Game/Resources/Levels/legacy-catalog.json";
        private const string MarkerPath = "Assets/Game/Resources/Levels/legacy-catalog.version";
        private static readonly Regex PointPattern = new Regex(
            @"- \{x: (?<x>-?\d+), y: (?<y>-?\d+)\}",
            RegexOptions.Compiled);
        private static readonly Regex DirectionPattern = new Regex(
            @"^\s+-?\s*Direction:\s*(?<direction>[1-4])\s*$",
            RegexOptions.Compiled);

        [InitializeOnLoadMethod]
        private static void QueueInitialImport()
        {
            EditorApplication.delayCall += ImportWhenReady;
        }

        [MenuItem("Snake Apple V2/Import All V1 Levels")]
        public static void ImportFromMenu()
        {
            ImportAll();
        }

        private static void ImportWhenReady()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating ||
                EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.delayCall += ImportWhenReady;
                return;
            }

            var expectedMarker = ImporterVersion.ToString(CultureInfo.InvariantCulture);
            if (!File.Exists(ToAbsolute(MarkerPath)) ||
                File.ReadAllText(ToAbsolute(MarkerPath)).Trim() != expectedMarker ||
                !File.Exists(ToAbsolute(OutputPath)))
            {
                ImportAll();
            }
        }

        // Corrections for V1 data that cannot be completed (or hinted) under the V2 rules. Re-applied on every
        // import so "Import All V1 Levels" never reverts them. See Doc/03-Data/03-v1-level-fixes.md.
        private static readonly Dictionary<string, string> HintOverrides = new Dictionary<string, string>
        {
            { "normal-093", "LLLLLLLLUURRRRRRUURURRURULLLLLLLLUUUUL" },
            { "normal-119", "RRUURRRRRRRDDDLLLL" },
            { "normal-177", "RURRURURRURRUUUULLLDLLUULLLDDLLDDDD" }
        };

        private static void ApplyV2Fixes(List<LegacyLevelRecord> records)
        {
            foreach (var record in records)
            {
                if (HintOverrides.TryGetValue(record.id, out var hint)) record.solution = hint;
                // Normal 93: the slide platform flag is inverted in the V1 data; with it flipped the level is solvable.
                if (record.id == "normal-093" && record.slideBarsHorizontal != null && record.slideBarsHorizontal.Length > 0)
                {
                    record.slideBarsHorizontal[0] = true;
                }
            }
        }

        private static void ImportAll()
        {
            var resourcesRoot = Path.GetFullPath(Path.Combine(
                UnityEngine.Application.dataPath,
                "../../SnakeApple_ExportedProject/ExportedProject/Assets/Resources"));
            if (!Directory.Exists(resourcesRoot))
            {
                Debug.LogError($"[Snake Apple V2] V1 Resources folder was not found: {resourcesRoot}");
                return;
            }

            var records = new List<LegacyLevelRecord>(284);
            ImportMode(resourcesRoot, "snakeapple/easy", "normal", records);
            ImportMode(resourcesRoot, "snakeapple/hard", "hard", records);
            ImportMode(resourcesRoot, "challenge", "challenge", records);

            ApplyV2Fixes(records);

            records.Sort((left, right) =>
            {
                var modeComparison = ModeOrder(left.mode).CompareTo(ModeOrder(right.mode));
                return modeComparison != 0 ? modeComparison : left.number.CompareTo(right.number);
            });

            var catalog = new LegacyLevelCatalog
            {
                schemaVersion = 1,
                levels = records.ToArray()
            };
            Directory.CreateDirectory(Path.GetDirectoryName(ToAbsolute(OutputPath)) ?? UnityEngine.Application.dataPath);
            File.WriteAllText(ToAbsolute(OutputPath), JsonUtility.ToJson(catalog, true), new UTF8Encoding(false));
            File.WriteAllText(
                ToAbsolute(MarkerPath),
                ImporterVersion.ToString(CultureInfo.InvariantCulture),
                new UTF8Encoding(false));
            AssetDatabase.ImportAsset(OutputPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(MarkerPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.SaveAssets();
            LegacyLevelCatalogLoader.ClearCache();

            var normal = records.Count(record => record.mode == "normal");
            var hard = records.Count(record => record.mode == "hard");
            var challenge = records.Count(record => record.mode == "challenge");
            Debug.Log($"[Snake Apple V2] Imported {records.Count} V1 levels: {normal} Normal, {hard} Hard, {challenge} Challenge.");
        }

        private static void ImportMode(
            string resourcesRoot,
            string relativeDirectory,
            string mode,
            ICollection<LegacyLevelRecord> destination)
        {
            var directory = Path.Combine(resourcesRoot, relativeDirectory.Replace('/', Path.DirectorySeparatorChar));
            if (!Directory.Exists(directory))
            {
                throw new DirectoryNotFoundException(directory);
            }

            var files = Directory.GetFiles(directory, "Level *.prefab")
                .OrderBy(GetLevelNumber)
                .ToArray();
            foreach (var file in files)
            {
                destination.Add(ParsePrefab(file, relativeDirectory, mode));
            }
        }

        private static LegacyLevelRecord ParsePrefab(string path, string relativeDirectory, string mode)
        {
            var lines = File.ReadAllLines(path);
            var number = GetLevelNumber(path);
            var width = ParseIntField(lines, "SizeX");
            var height = ParseIntField(lines, "SizeY");
            var rawData = ParseStringField(lines, "Data");
            var data = DecodeUnityCharArray(rawData);
            if (data.Length != width * height)
            {
                throw new FormatException($"{path}: decoded {data.Length} cells, expected {width * height}.");
            }

            return new LegacyLevelRecord
            {
                id = $"{mode}-{number:000}",
                mode = mode,
                number = number,
                width = width,
                height = height,
                data = data,
                gravityReversed = ParseIntField(lines, "IsGravityReversed") != 0,
                slideBars = ParsePointList(lines, "listSlideBarPos"),
                doorButtons = ParsePointList(lines, "listButtonOpenDoorPos"),
                doorButtonData = ParsePointList(lines, "listButtonOpenDoorPosData"),
                doors = ParsePointList(lines, "listOpenDoorPos"),
                teleports = ParsePointList(lines, "listTeleportPos"),
                teleportDirections = ParseStringField(lines, "TeleportDirection", false),
                doorInitiallyClosed = DecodeBoolArray(ParseStringField(lines, "listOpenDoorStateClose", false)),
                slideBarsHorizontal = DecodeBoolArray(ParseStringField(lines, "listSlideBarDirectionHorzontal", false)),
                solution = ParseSolution(lines),
                sourcePath = $"Resources/{relativeDirectory}/Level {number}.prefab",
                sourceHash = ComputeSha256(path)
            };
        }

        private static string DecodeUnityCharArray(string hex)
        {
            if (hex.Length % 4 != 0)
            {
                throw new FormatException("Unity char[] payload must contain four hex digits per character.");
            }

            var result = new StringBuilder(hex.Length / 4);
            for (var index = 0; index < hex.Length; index += 4)
            {
                var lowByte = byte.Parse(hex.Substring(index, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                var highByte = byte.Parse(hex.Substring(index + 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                result.Append((char)(lowByte | highByte << 8));
            }

            return result.ToString();
        }

        private static LegacyPoint[] ParsePointList(string[] lines, string field)
        {
            var start = FindField(lines, field);
            if (start < 0 || lines[start].TrimEnd().EndsWith("[]", StringComparison.Ordinal))
            {
                return Array.Empty<LegacyPoint>();
            }

            var points = new List<LegacyPoint>();
            for (var index = start + 1; index < lines.Length; index++)
            {
                var line = lines[index];
                if (!line.StartsWith("    - ", StringComparison.Ordinal))
                {
                    break;
                }

                var match = PointPattern.Match(line.Trim());
                if (!match.Success)
                {
                    break;
                }

                points.Add(new LegacyPoint(
                    int.Parse(match.Groups["x"].Value, CultureInfo.InvariantCulture),
                    int.Parse(match.Groups["y"].Value, CultureInfo.InvariantCulture)));
            }

            return points.ToArray();
        }

        private static string ParseSolution(IEnumerable<string> lines)
        {
            var result = new StringBuilder();
            foreach (var line in lines)
            {
                var match = DirectionPattern.Match(line);
                if (!match.Success)
                {
                    continue;
                }

                var direction = match.Groups["direction"].Value[0];
                result.Append(direction == '1' ? 'U' : direction == '2' ? 'D' : direction == '3' ? 'L' : 'R');
            }

            return result.ToString();
        }

        private static bool[] DecodeBoolArray(string hex)
        {
            if (string.IsNullOrEmpty(hex))
            {
                return Array.Empty<bool>();
            }

            var result = new bool[hex.Length / 2];
            for (var index = 0; index < result.Length; index++)
            {
                result[index] = byte.Parse(
                    hex.Substring(index * 2, 2),
                    NumberStyles.HexNumber,
                    CultureInfo.InvariantCulture) != 0;
            }

            return result;
        }

        private static int ParseIntField(string[] lines, string field)
        {
            return int.Parse(ParseStringField(lines, field), CultureInfo.InvariantCulture);
        }

        private static string ParseStringField(string[] lines, string field, bool required = true)
        {
            var index = FindField(lines, field);
            if (index < 0)
            {
                if (required)
                {
                    throw new FormatException($"Missing field {field}.");
                }

                return string.Empty;
            }

            var separator = lines[index].IndexOf(':');
            return separator < 0 ? string.Empty : lines[index].Substring(separator + 1).Trim();
        }

        private static int FindField(IReadOnlyList<string> lines, string field)
        {
            var prefix = "    " + field + ":";
            for (var index = 0; index < lines.Count; index++)
            {
                if (lines[index].StartsWith(prefix, StringComparison.Ordinal))
                {
                    return index;
                }
            }

            return -1;
        }

        private static int GetLevelNumber(string path)
        {
            var name = Path.GetFileNameWithoutExtension(path);
            return int.Parse(name.Substring("Level ".Length), CultureInfo.InvariantCulture);
        }

        private static int ModeOrder(string mode)
        {
            return mode == "normal" ? 0 : mode == "hard" ? 1 : 2;
        }

        private static string ComputeSha256(string path)
        {
            using (var stream = File.OpenRead(path))
            using (var hash = SHA256.Create())
            {
                return string.Concat(hash.ComputeHash(stream).Select(value => value.ToString("x2")));
            }
        }

        private static string ToAbsolute(string assetPath)
        {
            return Path.GetFullPath(Path.Combine(
                UnityEngine.Application.dataPath,
                "..",
                assetPath.Replace('/', Path.DirectorySeparatorChar)));
        }
    }
}
