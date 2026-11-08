using UnityEngine;

namespace SnakeApple.Infrastructure.Save
{
    public static class LocalProfileStore
    {
        private const string CoinsKey = "profile.wallet.softCurrency";
        private const string SelectedSkinKey = "profile.cosmetics.selectedSkin";
        private const string MusicKey = "settings.music";
        private const string SfxKey = "settings.sfx";
        private const string HapticsKey = "settings.haptics";
        private const string ReducedMotionKey = "settings.reducedMotion";

        public static int Coins
        {
            get => PlayerPrefs.GetInt(CoinsKey, 0);
            set
            {
                PlayerPrefs.SetInt(CoinsKey, Mathf.Max(0, value));
                PlayerPrefs.Save();
            }
        }

        public static int SelectedSkin
        {
            get => PlayerPrefs.GetInt(SelectedSkinKey, 0);
            set
            {
                PlayerPrefs.SetInt(SelectedSkinKey, Mathf.Max(0, value));
                PlayerPrefs.Save();
            }
        }

        public static int SelectedBlockTheme
        {
            get => PlayerPrefs.GetInt("profile.cosmetics.blockTheme", 0);
            set
            {
                PlayerPrefs.SetInt("profile.cosmetics.blockTheme", Mathf.Max(0, value));
                PlayerPrefs.Save();
            }
        }

        public static int SelectedBackground
        {
            get => PlayerPrefs.GetInt("profile.cosmetics.background", 0);
            set
            {
                PlayerPrefs.SetInt("profile.cosmetics.background", Mathf.Max(0, value));
                PlayerPrefs.Save();
            }
        }

        public static bool IsBlockThemeUnlocked(int index)
        {
            return index == 0 || PlayerPrefs.GetInt($"profile.cosmetics.blockTheme.{index}.unlocked", 0) == 1;
        }

        public static void UnlockBlockTheme(int index)
        {
            PlayerPrefs.SetInt($"profile.cosmetics.blockTheme.{index}.unlocked", 1);
            PlayerPrefs.Save();
        }

        public static bool IsBackgroundUnlocked(int index)
        {
            return index == 0 || PlayerPrefs.GetInt($"profile.cosmetics.background.{index}.unlocked", 0) == 1;
        }

        public static void UnlockBackground(int index)
        {
            PlayerPrefs.SetInt($"profile.cosmetics.background.{index}.unlocked", 1);
            PlayerPrefs.Save();
        }

        public static bool Music
        {
            get => GetBool(MusicKey, true);
            set => SetBool(MusicKey, value);
        }

        public static bool Sfx
        {
            get => GetBool(SfxKey, true);
            set => SetBool(SfxKey, value);
        }

        public static bool Haptics
        {
            get => GetBool(HapticsKey, true);
            set => SetBool(HapticsKey, value);
        }

        public static bool ReducedMotion
        {
            get => GetBool(ReducedMotionKey, false);
            set => SetBool(ReducedMotionKey, value);
        }

        public static bool IsSkinUnlocked(int skinIndex)
        {
            return skinIndex == 0 || PlayerPrefs.GetInt($"profile.cosmetics.skin.{skinIndex}.unlocked", 0) == 1;
        }

        public static void UnlockSkin(int skinIndex)
        {
            PlayerPrefs.SetInt($"profile.cosmetics.skin.{skinIndex}.unlocked", 1);
            PlayerPrefs.Save();
        }

        private static bool GetBool(string key, bool defaultValue)
        {
            return PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;
        }

        private static void SetBool(string key, bool value)
        {
            PlayerPrefs.SetInt(key, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}

