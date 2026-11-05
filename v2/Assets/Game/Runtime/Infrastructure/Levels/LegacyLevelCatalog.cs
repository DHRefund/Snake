using System;
using UnityEngine;

namespace SnakeApple.Infrastructure.Levels
{
    [Serializable]
    public sealed class LegacyLevelCatalog
    {
        public int schemaVersion;
        public LegacyLevelRecord[] levels;

        public LegacyLevelRecord Find(string mode, int number)
        {
            if (levels == null)
            {
                return null;
            }

            for (var index = 0; index < levels.Length; index++)
            {
                var level = levels[index];
                if (level != null && level.number == number &&
                    string.Equals(level.mode, mode, StringComparison.OrdinalIgnoreCase))
                {
                    return level;
                }
            }

            return null;
        }

        public int Count(string mode)
        {
            var count = 0;
            if (levels == null)
            {
                return count;
            }

            for (var index = 0; index < levels.Length; index++)
            {
                if (levels[index] != null &&
                    string.Equals(levels[index].mode, mode, StringComparison.OrdinalIgnoreCase))
                {
                    count++;
                }
            }

            return count;
        }
    }

    [Serializable]
    public sealed class LegacyLevelRecord
    {
        public string id;
        public string mode;
        public int number;
        public int width;
        public int height;
        public string data;
        public bool gravityReversed;
        public LegacyPoint[] slideBars;
        public LegacyPoint[] doorButtons;
        public LegacyPoint[] doorButtonData;
        public LegacyPoint[] doors;
        public LegacyPoint[] teleports;
        public string teleportDirections;
        public bool[] doorInitiallyClosed;
        public bool[] slideBarsHorizontal;
        public string solution;
        public string sourcePath;
        public string sourceHash;
    }

    [Serializable]
    public struct LegacyPoint
    {
        public int x;
        public int y;

        public LegacyPoint(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public static class LegacyLevelCatalogLoader
    {
        private const string ResourcePath = "Levels/legacy-catalog";
        private static LegacyLevelCatalog cached;

        public static LegacyLevelCatalog Load()
        {
            if (cached != null)
            {
                return cached;
            }

            var asset = Resources.Load<TextAsset>(ResourcePath);
            if (asset == null)
            {
                throw new InvalidOperationException($"Missing Resources/{ResourcePath}.json.");
            }

            cached = JsonUtility.FromJson<LegacyLevelCatalog>(asset.text);
            if (cached == null || cached.schemaVersion != 1 || cached.levels == null)
            {
                throw new FormatException("Legacy level catalog is missing or has an unsupported schema.");
            }

            return cached;
        }

        public static void ClearCache()
        {
            cached = null;
        }
    }
}
