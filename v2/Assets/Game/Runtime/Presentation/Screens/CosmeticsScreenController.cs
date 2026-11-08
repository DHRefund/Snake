using System.Collections.Generic;
using SnakeApple.Infrastructure.Save;
using SnakeApple.Presentation.Ads;
using SnakeApple.Presentation.Cosmetics;
using SnakeApple.Presentation.Navigation;
using SnakeApple.Presentation.Snake;
using SnakeApple.Presentation.UI;
using UnityEngine;
using UnityEngine.UI;

namespace SnakeApple.Presentation.Screens
{
    // Shop for the three cosmetic categories (snake skins, block themes, backdrops) plus the rewarded-ad coin offer.
    public sealed class CosmeticsScreenController : MonoBehaviour
    {
        private enum Tab
        {
            Snakes,
            Blocks,
            Backdrops
        }

        private struct Item
        {
            public int Index;
            public string Name;
            public Sprite Preview;
            public int Price;
        }

        private const int SlotCount = 12;
        private readonly List<Item> items = new List<Item>();
        private Text coinText;
        private Text statusText;
        private Button watchAdButton;
        private Text watchAdLabel;
        private Button tabSnakes;
        private Button tabBlocks;
        private Button tabBackdrops;
        private ScreenNavigator navigator;
        private SnakeSkinCatalog snakeCatalog;
        private ThemeCatalog themeCatalog;
        private RewardedAdController rewardedAd;
        private Tab tab = Tab.Snakes;
        private bool adInProgress;

        private void Awake()
        {
            navigator = GetComponent<ScreenNavigator>();
            UiLookup.Find<Button>(transform, "BackButton").onClick.AddListener(() => navigator.Load(ScreenNames.Home));
            coinText = UiLookup.Find<Text>(transform, "CoinText");
            statusText = UiLookup.Find<Text>(transform, "ShopStatusText");
            snakeCatalog = SnakeSkinCatalog.Load();
            themeCatalog = ThemeCatalog.Load();
            if (snakeCatalog == null)
            {
                Debug.LogError("[Snake Apple V2] Snake skin catalog is missing. Run Snake Apple V2 > Build Snake Skin Library.");
            }

            if (themeCatalog == null)
            {
                Debug.LogError("[Snake Apple V2] Theme catalog is missing. Run Snake Apple V2 > Build Theme Catalog.");
            }

            tabSnakes = UiLookup.Find<Button>(transform, "TabSkins");
            tabBlocks = UiLookup.Find<Button>(transform, "TabBlocks");
            tabBackdrops = UiLookup.Find<Button>(transform, "TabBackgrounds");
            tabSnakes.onClick.AddListener(() => SwitchTab(Tab.Snakes));
            tabBlocks.onClick.AddListener(() => SwitchTab(Tab.Blocks));
            tabBackdrops.onClick.AddListener(() => SwitchTab(Tab.Backdrops));

            watchAdButton = UiLookup.Find<Button>(transform, "WatchAdButton");
            watchAdLabel = watchAdButton.GetComponentInChildren<Text>();
            watchAdButton.onClick.AddListener(WatchAd);
            rewardedAd = GetComponent<RewardedAdController>();
            if (rewardedAd == null) rewardedAd = gameObject.AddComponent<RewardedAdController>();

            for (var slot = 0; slot < SlotCount; slot++)
            {
                var capturedSlot = slot;
                UiLookup.Find<Button>(transform, $"Skin_{slot:00}").onClick.AddListener(() => SelectOrBuy(capturedSlot));
            }

            Refresh();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) navigator.Load(ScreenNames.Home);
            RefreshAdButton();
        }

        private void SwitchTab(Tab next)
        {
            tab = next;
            statusText.text = next == Tab.Snakes ? "Choose a look for your worm."
                : next == Tab.Blocks ? "Pick the blocks your puzzles are built from."
                : "Pick the backdrop behind your puzzles.";
            Refresh();
        }

        private void WatchAd()
        {
            if (adInProgress) return;
            if (RewardLedger.RemainingGrants(RewardLedger.NowUnix) <= 0)
            {
                statusText.text = "You have watched all of today's ads. Come back later!";
                return;
            }

            adInProgress = rewardedAd.TryShow(committed =>
            {
                adInProgress = false;
                statusText.text = committed
                    ? $"+{RewardLedger.RewardCoins} coins!"
                    : "No reward this time. Try again.";
                Refresh();
            });
            if (!adInProgress) statusText.text = "No ad is ready yet. Try again in a moment.";
        }

        private void RefreshAdButton()
        {
            var now = RewardLedger.NowUnix;
            var remaining = RewardLedger.RemainingGrants(now);
            if (remaining <= 0)
            {
                var seconds = RewardLedger.SecondsUntilNextGrant(now);
                watchAdLabel.text = $"BACK IN {seconds / 3600}h {seconds % 3600 / 60:00}m";
                watchAdButton.interactable = false;
                return;
            }

            watchAdLabel.text = adInProgress ? "WATCHING..." : $"WATCH AD  +{RewardLedger.RewardCoins}";
            watchAdButton.interactable = !adInProgress && rewardedAd.IsReady;
        }

        private void SelectOrBuy(int slot)
        {
            if (slot < 0 || slot >= items.Count) return;
            var item = items[slot];
            if (!IsUnlocked(item.Index))
            {
                if (LocalProfileStore.Coins < item.Price)
                {
                    statusText.text = $"You need {item.Price - LocalProfileStore.Coins} more coins for {item.Name}.";
                    return;
                }

                LocalProfileStore.Coins -= item.Price;
                Unlock(item.Index);
                statusText.text = $"{item.Name} unlocked!";
            }
            else
            {
                statusText.text = $"{item.Name} selected.";
            }

            Select(item.Index);
            Refresh();
        }

        private bool IsUnlocked(int index)
        {
            return tab == Tab.Snakes ? LocalProfileStore.IsSkinUnlocked(index)
                : tab == Tab.Blocks ? LocalProfileStore.IsBlockThemeUnlocked(index)
                : LocalProfileStore.IsBackgroundUnlocked(index);
        }

        private void Unlock(int index)
        {
            if (tab == Tab.Snakes) LocalProfileStore.UnlockSkin(index);
            else if (tab == Tab.Blocks) LocalProfileStore.UnlockBlockTheme(index);
            else LocalProfileStore.UnlockBackground(index);
        }

        private void Select(int index)
        {
            if (tab == Tab.Snakes) LocalProfileStore.SelectedSkin = index;
            else if (tab == Tab.Blocks) LocalProfileStore.SelectedBlockTheme = index;
            else LocalProfileStore.SelectedBackground = index;
        }

        private int SelectedIndex()
        {
            return tab == Tab.Snakes ? LocalProfileStore.SelectedSkin
                : tab == Tab.Blocks ? LocalProfileStore.SelectedBlockTheme
                : LocalProfileStore.SelectedBackground;
        }

        private void CollectItems()
        {
            items.Clear();
            if (tab == Tab.Snakes && snakeCatalog != null)
            {
                for (var index = 0; index < snakeCatalog.Count; index++)
                {
                    var entry = snakeCatalog.Find(index);
                    items.Add(new Item { Index = index, Name = entry.displayName, Preview = entry.preview, Price = entry.price });
                }
            }
            else if (tab == Tab.Blocks && themeCatalog != null)
            {
                for (var index = 0; index < themeCatalog.BlockCount; index++)
                {
                    var entry = themeCatalog.FindBlock(index);
                    if (!entry.IsAvailable) continue; // art not authored yet
                    items.Add(new Item { Index = index, Name = entry.displayName, Preview = entry.preview, Price = entry.price });
                }
            }
            else if (tab == Tab.Backdrops && themeCatalog != null)
            {
                for (var index = 0; index < themeCatalog.BackgroundCount; index++)
                {
                    var entry = themeCatalog.FindBackground(index);
                    if (!entry.IsAvailable) continue;
                    items.Add(new Item { Index = index, Name = entry.displayName, Preview = entry.art, Price = entry.price });
                }
            }
        }

        private void Refresh()
        {
            coinText.text = LocalProfileStore.Coins.ToString();
            RefreshTabs();
            CollectItems();
            var selected = SelectedIndex();
            for (var slot = 0; slot < SlotCount; slot++)
            {
                var button = UiLookup.Find<Button>(transform, $"Skin_{slot:00}");
                var has = slot < items.Count;
                button.gameObject.SetActive(has);
                if (!has) continue;

                var item = items[slot];
                var state = selected == item.Index ? SkinCellState.Selected
                    : IsUnlocked(item.Index) ? SkinCellState.Owned
                    : SkinCellState.Locked;
                var view = button.GetComponent<SkinCellView>();
                if (view != null) view.Apply(item.Name, item.Preview, state, item.Price);
            }
        }

        private void RefreshTabs()
        {
            SetTabTint(tabSnakes, tab == Tab.Snakes);
            SetTabTint(tabBlocks, tab == Tab.Blocks);
            SetTabTint(tabBackdrops, tab == Tab.Backdrops);
        }

        private static void SetTabTint(Button button, bool selected)
        {
            var tint = selected ? Color.white : new Color(0.62f, 0.62f, 0.62f, 1f);
            var colors = button.colors;
            colors.normalColor = tint;
            colors.highlightedColor = Color.Lerp(tint, Color.white, 0.2f);
            colors.pressedColor = Color.Lerp(tint, Color.black, 0.15f);
            button.colors = colors;
            button.GetComponent<Image>().color = tint;
        }
    }
}
