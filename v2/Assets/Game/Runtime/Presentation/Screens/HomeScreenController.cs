using SnakeApple.Infrastructure.Save;
using SnakeApple.Presentation.Navigation;
using SnakeApple.Presentation.UI;
using UnityEngine;
using UnityEngine.UI;

namespace SnakeApple.Presentation.Screens
{
    public sealed class HomeScreenController : MonoBehaviour
    {
        private ScreenNavigator navigator;

        private void Awake()
        {
            navigator = GetComponent<ScreenNavigator>();
            UiLookup.Find<Button>(transform, "PlayButton").onClick.AddListener(ContinueGame);
            UiLookup.Find<Button>(transform, "LevelsButton").onClick.AddListener(() => navigator.Load(ScreenNames.LevelSelect));
            UiLookup.Find<Button>(transform, "CosmeticsButton").onClick.AddListener(() => navigator.Load(ScreenNames.Cosmetics));
            UiLookup.Find<Button>(transform, "SettingsButton").onClick.AddListener(() => navigator.Load(ScreenNames.Settings));
            UiLookup.Find<Text>(transform, "CoinText").text = LocalProfileStore.Coins.ToString();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) UnityEngine.Application.Quit();
        }

        private void ContinueGame()
        {
            var mode = LevelProgressStore.SelectedMode;
            var level = LevelProgressStore.SelectedLevel;
            if (!LevelProgressStore.IsModeUnlocked(mode) || !LevelProgressStore.IsUnlocked(mode, level))
            {
                LevelProgressStore.Select(LevelProgressStore.NormalMode, 1);
            }

            navigator.Load(ScreenNames.Gameplay);
        }
    }
}
