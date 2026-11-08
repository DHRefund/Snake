using System;
using System.Collections;
using SnakeApple.Infrastructure.Levels;
using SnakeApple.Infrastructure.Save;
using SnakeApple.Presentation.Navigation;
using SnakeApple.Presentation.UI;
using UnityEngine;
using UnityEngine.UI;

namespace SnakeApple.Presentation.Screens
{
    public sealed class BootScreenController : MonoBehaviour
    {
        private ScreenNavigator navigator;
        private Text status;
        private Image progress;
        private Button retryButton;
        private RectTransform mainLogo;
        private RectTransform appleMascot;
        private Vector2 appleMascotBase;
        private bool loading;

        private void Awake()
        {
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
            Screen.orientation = ScreenOrientation.Portrait;
            navigator = GetComponent<ScreenNavigator>();
            status = UiLookup.Find<Text>(transform, "StatusText");
            progress = UiLookup.Find<Image>(transform, "ProgressFill");
            retryButton = UiLookup.Find<Button>(transform, "RetryButton");
            mainLogo = UiLookup.FindOptional<RectTransform>(transform, "MainLogo") 
                    ?? UiLookup.FindOptional<RectTransform>(transform, "WormLogo");
            appleMascot = UiLookup.FindOptional<RectTransform>(transform, "AppleMascot");
            if (appleMascot != null) appleMascotBase = appleMascot.anchoredPosition;
            retryButton.onClick.AddListener(BeginLoad);
            retryButton.gameObject.SetActive(false);
        }

        private void Start() => BeginLoad();

        private void Update()
        {
            if (mainLogo != null)
            {
                var scale = 1f + Mathf.Sin(Time.unscaledTime * 2.2f) * 0.02f;
                mainLogo.localScale = new Vector3(scale, scale, 1f);
            }

            if (appleMascot != null)
            {
                var yOffset = Mathf.Sin(Time.unscaledTime * 3.2f) * 7f;
                appleMascot.anchoredPosition = appleMascotBase + new Vector2(0f, yOffset);
            }
        }

        private void BeginLoad()
        {
            if (!loading) StartCoroutine(LoadRoutine());
        }

        private static readonly string[] RequiredModes = { "normal", "hard", "challenge" };

        private IEnumerator LoadRoutine()
        {
            loading = true;
            retryButton.gameObject.SetActive(false);
            progress.fillAmount = 0.05f;

            // Stage 1: local profile and settings. A broken save must never block the game.
            status.text = "Loading your profile...";
            yield return AnimateProgress(0.3f, 0.12f);
            try
            {
                LoadProfile();
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[Snake Apple V2] Profile could not be read, using defaults: {exception.Message}");
            }

            // Stage 2: level catalog. This one is required to play.
            status.text = "Loading level catalog...";
            yield return AnimateProgress(0.7f, 0.12f);
            try
            {
                LegacyLevelCatalogLoader.ClearCache();
                var catalog = LegacyLevelCatalogLoader.Load();
                foreach (var mode in RequiredModes)
                {
                    if (catalog.Count(mode) <= 0)
                    {
                        throw new InvalidOperationException($"The level catalog has no '{mode}' levels.");
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.LogError($"[Snake Apple V2] Boot failed: {exception}");
                status.text = "Couldn't load the level catalog.";
                progress.fillAmount = 0f;
                retryButton.gameObject.SetActive(true);
                loading = false;
                yield break;
            }

            status.text = "Ready!";
            yield return AnimateProgress(1.0f, 0.12f);
            yield return new WaitForSecondsRealtime(0.08f);

            navigator.Load(ScreenNames.Home);
        }

        private static void LoadProfile()
        {
            // Touch each value once so bad data surfaces here instead of inside a later screen.
            _ = LocalProfileStore.Coins;
            _ = LocalProfileStore.SelectedSkin;
            _ = LocalProfileStore.Music;
            _ = LocalProfileStore.Sfx;
            _ = LocalProfileStore.Haptics;
            _ = LocalProfileStore.ReducedMotion;
        }

        private IEnumerator AnimateProgress(float target, float duration)
        {
            var start = progress.fillAmount;
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                progress.fillAmount = Mathf.Lerp(start, target, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }
            progress.fillAmount = target;
        }
    }
}
