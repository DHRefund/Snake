using GoogleMobileAds.Api;
using SnakeApple.Presentation.Screens;
using UnityEngine;
using UnityEngine.UI;

namespace SnakeApple.Presentation.UI
{
    // Scene-owned banner. The layout stays reserved while an ad is loading or hidden.
    public sealed class GameplayBanner : MonoBehaviour
    {
        private const string AndroidTestUnit = "ca-app-pub-3940256099942544/9214589741";
        private const string IosTestUnit = "ca-app-pub-3940256099942544/2435281174";
        private const float ControlGapDp = 16f;
        private SafeAreaPanel safeArea;
        private GameplayScreenController gameplay;
        private BannerView banner;
        private Rect screenSafeArea;
        private Vector2Int screenSize;
        private RectTransform preview;
        private volatile bool sdkReady;
        private volatile bool adLoaded;
        private volatile bool adFailed;
        private bool visible;
        private bool adGeometryApplied;
        private float retryAt;

        private void Start()
        {
            safeArea = GetComponentInChildren<SafeAreaPanel>();
            gameplay = GetComponent<GameplayScreenController>();
            if (safeArea == null) { enabled = false; return; }
#if UNITY_EDITOR
            CreatePreview();
#elif UNITY_ANDROID || UNITY_IOS
            MobileAds.Initialize(_ => sdkReady = true);
#endif
            UpdateLayout();
        }

        private void Update()
        {
            if (safeArea == null) return;
            if (screenSafeArea != Screen.safeArea || screenSize.x != Screen.width || screenSize.y != Screen.height)
            {
                DestroyBanner();
                UpdateLayout();
            }
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
            if (sdkReady && banner == null && Time.unscaledTime >= retryAt) LoadBanner();
            if (adFailed)
            {
                DestroyBanner();
                retryAt = Time.unscaledTime + 30f;
            }
#endif
            if (adLoaded && !adGeometryApplied)
            {
                banner.Hide();
                UpdateLayout();
                adGeometryApplied = true;
            }
            var shouldShow = !gameplay.IsGameplayOverlayOpen;
            if (preview != null) preview.gameObject.SetActive(shouldShow);
            if (banner != null && adLoaded && visible != shouldShow)
            {
                if (shouldShow) banner.Show(); else banner.Hide();
                visible = shouldShow;
            }
        }

        private float Density => UnityEngine.Application.isEditor
            ? Mathf.Max(1f, Screen.width / 360f)
            : Mathf.Max(1f, MobileAds.Utils.GetDeviceScale());

        private void UpdateLayout()
        {
            screenSafeArea = Screen.safeArea;
            screenSize = new Vector2Int(Screen.width, Screen.height);
            var height = banner != null && adLoaded ? banner.GetHeightInPixels() : 60f * Density;
            safeArea.SetBottomReservedPixels(height + ControlGapDp * Density);
            if (preview != null)
            {
                var min = screenSafeArea.position;
                var max = min + screenSafeArea.size;
                preview.anchorMin = new Vector2(min.x / Screen.width, min.y / Screen.height);
                preview.anchorMax = new Vector2(max.x / Screen.width, (min.y + height) / Screen.height);
                preview.offsetMin = preview.offsetMax = Vector2.zero;
            }
            if (banner != null && adLoaded)
            {
                var x = screenSafeArea.x + (screenSafeArea.width - banner.GetWidthInPixels()) * 0.5f;
                var y = Screen.height - screenSafeArea.y - height;
                banner.SetPosition(Mathf.RoundToInt(x / Density), Mathf.RoundToInt(y / Density));
            }
        }

        private void LoadBanner()
        {
            adLoaded = adFailed = false;
            var width = MobileAds.Utils.GetDeviceSafeWidth();
            var size = AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(width);
            var id = UnityEngine.Application.platform == RuntimePlatform.IPhonePlayer ? IosTestUnit : AndroidTestUnit;
            banner = new BannerView(id, size, AdPosition.Bottom);
            var requestedBanner = banner;
            banner.OnBannerAdLoaded += () =>
            {
                if (banner == requestedBanner) adLoaded = true;
            };
            banner.OnBannerAdLoadFailed += error =>
            {
                if (banner == requestedBanner) adFailed = true;
            };
            banner.Hide();
            banner.LoadAd(new AdRequest());
        }

        private void CreatePreview()
        {
            var canvas = safeArea.GetComponentInParent<Canvas>();
            var node = new GameObject("BannerPreview", typeof(RectTransform), typeof(Image));
            preview = (RectTransform)node.transform;
            preview.SetParent(canvas.transform, false);
            var image = node.GetComponent<Image>();
            image.color = new Color32(27, 48, 52, 255);
            image.raycastTarget = true;
            var label = new GameObject("Label", typeof(RectTransform), typeof(Text));
            label.transform.SetParent(preview, false);
            var rect = (RectTransform)label.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var text = label.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = "TEST BANNER";
            text.fontSize = 24;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.raycastTarget = false;
        }

        private void DestroyBanner()
        {
            var previous = banner;
            banner = null;
            adLoaded = adFailed = visible = adGeometryApplied = false;
            previous?.Destroy();
        }

        private void OnDestroy()
        {
            DestroyBanner();
            if (preview != null) Destroy(preview.gameObject);
            if (safeArea != null) safeArea.SetBottomReservedPixels(0f);
        }
    }
}
