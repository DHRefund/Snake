using SnakeApple.Infrastructure.Levels;
using SnakeApple.Infrastructure.Save;
using SnakeApple.Presentation.Navigation;
using SnakeApple.Presentation.UI;
using UnityEngine;
using UnityEngine.UI;

namespace SnakeApple.Presentation.Screens
{
    public sealed class LevelSelectScreenController : MonoBehaviour
    {
        private const int PageSize = 20;
        private ScreenNavigator navigator;
        private LegacyLevelCatalog catalog;
        private string mode;
        private int page;
        private Text chapterText;
        private Text chapterSubtext;
        private Text pageText;
        private Button previousButton;
        private Button nextButton;
        private Button normalTab;
        private Button hardTab;
        private Button challengeTab;

        private void Awake()
        {
            navigator = GetComponent<ScreenNavigator>();
            catalog = LegacyLevelCatalogLoader.Load();
            chapterText = UiLookup.Find<Text>(transform, "ChapterText");
            chapterSubtext = UiLookup.Find<Text>(transform, "ChapterSubtext");
            pageText = UiLookup.Find<Text>(transform, "PageText");
            previousButton = UiLookup.Find<Button>(transform, "PrevPageButton");
            nextButton = UiLookup.Find<Button>(transform, "NextPageButton");

            UiLookup.Find<Button>(transform, "BackButton").onClick.AddListener(() => navigator.Load(ScreenNames.Home));
            normalTab = UiLookup.Find<Button>(transform, "NormalTab");
            hardTab = UiLookup.Find<Button>(transform, "HardTab");
            challengeTab = UiLookup.Find<Button>(transform, "ChallengeTab");
            normalTab.onClick.AddListener(() => SelectMode(LevelProgressStore.NormalMode));
            hardTab.onClick.AddListener(() => SelectMode(LevelProgressStore.HardMode));
            challengeTab.onClick.AddListener(() => SelectMode(LevelProgressStore.ChallengeMode));
            previousButton.onClick.AddListener(() => ChangePage(-1));
            nextButton.onClick.AddListener(() => ChangePage(1));

            mode = LevelProgressStore.IsModeUnlocked(LevelProgressStore.SelectedMode)
                ? LevelProgressStore.SelectedMode
                : LevelProgressStore.NormalMode;
            page = Mathf.Max(0, (LevelProgressStore.SelectedLevel - 1) / PageSize);
            Refresh();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) navigator.Load(ScreenNames.Home);
        }

        private void SelectMode(string selectedMode)
        {
            if (!LevelProgressStore.IsModeUnlocked(selectedMode))
            {
                chapterSubtext.text = selectedMode == LevelProgressStore.HardMode
                    ? "Complete Normal mode to unlock Hard."
                    : "Complete Hard mode to unlock Challenge.";
                return;
            }

            mode = selectedMode;
            page = 0;
            Refresh();
        }

        private void ChangePage(int delta)
        {
            var pageCount = Mathf.CeilToInt(catalog.Count(mode) / (float)PageSize);
            page = Mathf.Clamp(page + delta, 0, Mathf.Max(0, pageCount - 1));
            Refresh();
        }

        private void Refresh()
        {
            var total = catalog.Count(mode);
            var pageCount = Mathf.Max(1, Mathf.CeilToInt(total / (float)PageSize));
            page = Mathf.Clamp(page, 0, pageCount - 1);
            chapterText.text = $"{mode.ToUpperInvariant()}  -  LEVELS {page * PageSize + 1}-{Mathf.Min(total, (page + 1) * PageSize)}";
            chapterSubtext.text = $"{Mathf.Min(LevelProgressStore.GetUnlockedLevel(mode), total)} of {total} levels unlocked";
            pageText.text = $"PAGE {page + 1} / {pageCount}";
            previousButton.interactable = page > 0;
            nextButton.interactable = page + 1 < pageCount;

            for (var slot = 1; slot <= PageSize; slot++)
            {
                var button = UiLookup.Find<Button>(transform, $"Level_{slot:000}");
                button.onClick.RemoveAllListeners();
                var level = page * PageSize + slot;
                var exists = level <= total;
                var unlocked = exists && LevelProgressStore.IsUnlocked(mode, level);
                button.gameObject.SetActive(exists);
                button.interactable = unlocked;
                var state = !unlocked ? LevelSlotState.Locked
                    : LevelProgressStore.IsCompleted(mode, level) ? LevelSlotState.Completed
                    : LevelSlotState.Available;
                var view = button.GetComponent<LevelSlotView>();
                if (view != null) view.Apply(state, level);
                if (unlocked)
                {
                    var selectedLevel = level;
                    button.onClick.AddListener(() => Play(selectedLevel));
                }
            }

            RefreshTab(normalTab, "NORMAL", true, mode == LevelProgressStore.NormalMode);
            RefreshTab(hardTab, "HARD", LevelProgressStore.IsModeUnlocked(LevelProgressStore.HardMode),
                mode == LevelProgressStore.HardMode);
            RefreshTab(challengeTab, "CHALLENGE", LevelProgressStore.IsModeUnlocked(LevelProgressStore.ChallengeMode),
                mode == LevelProgressStore.ChallengeMode);
        }

        private static void RefreshTab(Button button, string label, bool unlocked, bool selected)
        {
            button.interactable = true;
            button.GetComponentInChildren<Text>().text = unlocked ? label : label + "\nLOCKED";
            // Tabs are sprites, so state is a tint: bright when selected, dimmed otherwise.
            var tint = selected ? Color.white
                : unlocked ? new Color(0.62f, 0.62f, 0.62f, 1f) : new Color(0.38f, 0.4f, 0.42f, 1f);
            var colors = button.colors;
            colors.normalColor = tint;
            colors.highlightedColor = Color.Lerp(tint, Color.white, 0.2f);
            colors.pressedColor = Color.Lerp(tint, Color.black, 0.15f);
            button.colors = colors;
            button.GetComponent<Image>().color = tint;
        }

        private void Play(int level)
        {
            LevelProgressStore.Select(mode, level);
            navigator.Load(ScreenNames.Gameplay);
        }
    }
}
