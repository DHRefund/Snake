using System;
using SnakeApple.Infrastructure.Levels;
using UnityEngine;

namespace SnakeApple.Infrastructure.Save
{
    public static class LevelProgressStore
    {
        public const string NormalMode = "normal";
        public const string HardMode = "hard";
        public const string ChallengeMode = "challenge";

        private const string SelectedModeKey = "progress.selectedMode";
        private const string SelectedLevelKey = "progress.selectedLevel";

        public static string SelectedMode
        {
            get
            {
                var mode = PlayerPrefs.GetString(SelectedModeKey, NormalMode);
                return IsKnownMode(mode) ? mode : NormalMode;
            }
            set
            {
                PlayerPrefs.SetString(SelectedModeKey, IsKnownMode(value) ? value : NormalMode);
                PlayerPrefs.Save();
            }
        }

        public static int SelectedLevel
        {
            get => Mathf.Max(1, PlayerPrefs.GetInt(SelectedLevelKey, 1));
            set
            {
                PlayerPrefs.SetInt(SelectedLevelKey, Mathf.Max(1, value));
                PlayerPrefs.Save();
            }
        }

        public static int GetUnlockedLevel(string mode)
        {
            return Mathf.Max(1, PlayerPrefs.GetInt(UnlockedKey(mode), 1));
        }

        public static bool IsUnlocked(string mode, int level)
        {
            return level >= 1 && level <= GetUnlockedLevel(mode);
        }

        public static bool IsCompleted(string mode, int level)
        {
            return PlayerPrefs.GetInt(CompletedKey(mode, level), 0) == 1;
        }

        public static int Complete(string mode, int level, int vipApples)
        {
            var firstCompletion = !IsCompleted(mode, level);
            PlayerPrefs.SetInt(CompletedKey(mode, level), 1);
            var total = LegacyLevelCatalogLoader.Load().Count(mode);
            var next = Mathf.Min(total, level + 1);
            if (level < total)
            {
                PlayerPrefs.SetInt(UnlockedKey(mode), Mathf.Max(GetUnlockedLevel(mode), next));
            }

            SelectedMode = mode;
            SelectedLevel = next;
            var reward = firstCompletion ? 10 + Mathf.Max(0, vipApples) * 5 : 0;
            if (reward > 0) LocalProfileStore.Coins += reward;
            PlayerPrefs.Save();
            return reward;
        }

        public static void Select(string mode, int level)
        {
            if (!IsKnownMode(mode)) throw new ArgumentException("Unknown game mode.", nameof(mode));
            if (!IsUnlocked(mode, level)) throw new InvalidOperationException("Level is locked.");
            SelectedMode = mode;
            SelectedLevel = level;
        }

        public static bool TrySelectNext(out int level)
        {
            var total = LegacyLevelCatalogLoader.Load().Count(SelectedMode);
            var candidate = SelectedLevel;
            if (candidate > total || !IsUnlocked(SelectedMode, candidate))
            {
                level = Mathf.Clamp(candidate, 1, total);
                return false;
            }

            level = candidate;
            return true;
        }

        public static bool IsModeUnlocked(string mode)
        {
            if (mode == NormalMode) return true;
            if (mode == HardMode)
            {
                var totalNormal = LegacyLevelCatalogLoader.Load().Count(NormalMode);
                return IsCompleted(NormalMode, totalNormal);
            }

            var totalHard = LegacyLevelCatalogLoader.Load().Count(HardMode);
            return mode == ChallengeMode && IsCompleted(HardMode, totalHard);
        }

        private static bool IsKnownMode(string mode)
        {
            return mode == NormalMode || mode == HardMode || mode == ChallengeMode;
        }

        private static string UnlockedKey(string mode)
        {
            return $"progress.{mode}.unlocked";
        }

        private static string CompletedKey(string mode, int level)
        {
            return $"progress.{mode}.completed.{level}";
        }
    }
}
