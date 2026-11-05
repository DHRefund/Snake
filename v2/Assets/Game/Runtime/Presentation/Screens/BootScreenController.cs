using System;
using System.Collections;
using SnakeApple.Infrastructure.Levels;
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
                appleMascot.anchoredPosition = new Vector2(0f, -150f + yOffset);
            }
        }

        private void BeginLoad()
        {
            if (!loading) StartCoroutine(LoadRoutine());
        }

        private IEnumerator LoadRoutine()
        {
            loading = true;
            retryButton.gameObject.SetActive(false);
            progress.fillAmount = 0.05f;
            status.text = "Loading level catalog...";

            yield return AnimateProgress(0.35f, 0.25f);

            try
            {
                LegacyLevelCatalogLoader.ClearCache();
                var catalog = LegacyLevelCatalogLoader.Load();
                if (catalog.Count("normal") != 180 || catalog.Count("hard") != 50 || catalog.Count("challenge") != 54)
                {
                    throw new InvalidOperationException("The level catalog is incomplete.");
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

            status.text = "Preparing your adventure...";
            yield return AnimateProgress(0.85f, 0.25f);
            yield return AnimateProgress(1.0f, 0.20f);
            yield return new WaitForSecondsRealtime(0.12f);

            navigator.Load(ScreenNames.Home);
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
