using SnakeApple.Infrastructure.Save;
using UnityEditor;
using UnityEngine;

namespace SnakeApple.Editor
{
    // Editor-only test helpers for the local profile (PlayerPrefs). Not included in builds.
    public static class DebugProfileCheats
    {
        // Every skin (3550) + block themes (2000) + backgrounds (400) = 5950, so this buys the whole shop.
        private const int TestCoins = 99999;

        [MenuItem("Snake Apple V2/Debug/Grant 99999 Coins")]
        public static void GrantCoins()
        {
            LocalProfileStore.Coins = TestCoins;
            Debug.Log($"[Snake Apple V2] Coins set to {LocalProfileStore.Coins} (enough to buy every skin, block theme and background).");
        }

        [MenuItem("Snake Apple V2/Debug/Reset Snake Skin Save")]
        public static void ResetSnakeSkinSave()
        {
            for (var index = 0; index < 32; index++)
            {
                PlayerPrefs.DeleteKey($"profile.cosmetics.skin.{index}.unlocked");
            }

            LocalProfileStore.SelectedSkin = 0;
            Debug.Log("[Snake Apple V2] Snake skin unlocks and selection reset (needed after the catalog reorder).");
        }
    }
}
