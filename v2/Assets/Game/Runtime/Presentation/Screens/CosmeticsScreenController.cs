using SnakeApple.Infrastructure.Save;
using SnakeApple.Presentation.Navigation;
using SnakeApple.Presentation.UI;
using UnityEngine;
using UnityEngine.UI;

namespace SnakeApple.Presentation.Screens
{
    public sealed class CosmeticsScreenController : MonoBehaviour
    {
        private static readonly int[] Prices = { 0, 150, 200, 250, 300, 350, 400, 450, 500, 550, 600, 700 };
        private Text coinText;
        private Text statusText;
        private ScreenNavigator navigator;

        private void Awake()
        {
            navigator = GetComponent<ScreenNavigator>();
            UiLookup.Find<Button>(transform, "BackButton").onClick.AddListener(() => navigator.Load(ScreenNames.Home));
            coinText = UiLookup.Find<Text>(transform, "CoinText");
            statusText = UiLookup.Find<Text>(transform, "ShopStatusText");

            for (var index = 0; index < Prices.Length; index++)
            {
                var capturedIndex = index;
                UiLookup.Find<Button>(transform, $"Skin_{index:00}").onClick.AddListener(() => SelectOrBuy(capturedIndex));
            }

            Refresh();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) navigator.Load(ScreenNames.Home);
        }

        private void SelectOrBuy(int index)
        {
            if (!LocalProfileStore.IsSkinUnlocked(index))
            {
                if (LocalProfileStore.Coins < Prices[index])
                {
                    statusText.text = $"You need {Prices[index] - LocalProfileStore.Coins} more coins.";
                    return;
                }

                LocalProfileStore.Coins -= Prices[index];
                LocalProfileStore.UnlockSkin(index);
            }

            LocalProfileStore.SelectedSkin = index;
            statusText.text = index == 0
                ? "Cat Snake selected."
                : index == 1
                    ? "Classic Worm selected."
                    : $"Skin {index + 1} selected.";
            Refresh();
        }

        private void Refresh()
        {
            coinText.text = LocalProfileStore.Coins.ToString();
            for (var index = 0; index < Prices.Length; index++)
            {
                var button = UiLookup.Find<Button>(transform, $"Skin_{index:00}");
                var label = button.GetComponentInChildren<Text>();
                if (LocalProfileStore.SelectedSkin == index)
                {
                    label.text = "SELECTED";
                }
                else if (LocalProfileStore.IsSkinUnlocked(index))
                {
                    label.text = "USE";
                }
                else
                {
                    label.text = $"{Prices[index]} COINS";
                }
            }
        }
    }
}
