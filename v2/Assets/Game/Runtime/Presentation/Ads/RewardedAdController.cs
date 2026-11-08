using System;
using GoogleMobileAds.Api;
using SnakeApple.Infrastructure.Save;
using UnityEngine;

namespace SnakeApple.Presentation.Ads
{
    public enum RewardedAdState
    {
        Idle,
        Loading,
        Ready,
        Showing
    }

    // Rewarded ad placement. The SDK reward callback is the only payout source (via RewardLedger.TryCommit);
    // the close callback never grants. Callbacks only set volatile flags, Update marshals them to the main thread.
    // The editor has no real ads: it simulates a successful ad so the flow can be tested.
    public sealed class RewardedAdController : MonoBehaviour
    {
        private const string AndroidTestUnit = "ca-app-pub-3940256099942544/5224354917";
        private const string IosTestUnit = "ca-app-pub-3940256099942544/1712485313";
        private static readonly float[] BackoffSeconds = { 5f, 15f, 30f, 60f };

        private RewardedAd ad;
        private volatile bool loadSucceeded;
        private volatile bool loadFailed;
        private volatile bool rewardEarned;
        private volatile bool closed;
        private string attemptId;
        private bool sdkReady;
        private int failures;
        private float retryAt;
        private float editorFakeEndsAt;
        private Action<bool> onFinished;

        public RewardedAdState State { get; private set; } = RewardedAdState.Idle;

        public bool IsReady => State == RewardedAdState.Ready;

        private void Start()
        {
#if UNITY_EDITOR
            State = RewardedAdState.Ready;
#elif UNITY_ANDROID || UNITY_IOS
            MobileAds.Initialize(_ => sdkReady = true);
#endif
        }

        private void OnDestroy()
        {
            DestroyAd();
        }

        // Starts a rewarded ad if one is Ready; onFinished(true) is raised only when coins were committed.
        public bool TryShow(Action<bool> finished)
        {
            if (State != RewardedAdState.Ready) return false;
            if (RewardLedger.RemainingGrants(RewardLedger.NowUnix) <= 0) return false;

            onFinished = finished;
            attemptId = RewardLedger.NewAttemptId();
            rewardEarned = closed = false;
            State = RewardedAdState.Showing;
#if UNITY_EDITOR
            editorFakeEndsAt = Time.unscaledTime + 1f;
#else
            ad.Show(_ => rewardEarned = true);
#endif
            return true;
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (State == RewardedAdState.Showing && Time.unscaledTime >= editorFakeEndsAt)
            {
                rewardEarned = closed = true;
            }
#elif UNITY_ANDROID || UNITY_IOS
            if (sdkReady && State == RewardedAdState.Idle && Time.unscaledTime >= retryAt) LoadAd();
            if (loadSucceeded)
            {
                loadSucceeded = false;
                failures = 0;
                if (State == RewardedAdState.Loading) State = RewardedAdState.Ready;
            }

            if (loadFailed)
            {
                loadFailed = false;
                DestroyAd();
                retryAt = Time.unscaledTime + BackoffSeconds[Mathf.Min(failures, BackoffSeconds.Length - 1)];
                failures++;
                State = RewardedAdState.Idle;
            }
#endif
            if (State != RewardedAdState.Showing || !closed) return;

            var committed = rewardEarned && RewardLedger.TryCommit(attemptId, RewardLedger.NowUnix);
            rewardEarned = closed = false;
            attemptId = null;
            DestroyAd();
#if UNITY_EDITOR
            State = RewardedAdState.Ready;
#else
            State = RewardedAdState.Idle;
            retryAt = Time.unscaledTime;
#endif
            var callback = onFinished;
            onFinished = null;
            callback?.Invoke(committed);
        }

        private void LoadAd()
        {
            State = RewardedAdState.Loading;
            var id = UnityEngine.Application.platform == RuntimePlatform.IPhonePlayer ? IosTestUnit : AndroidTestUnit;
            RewardedAd.Load(id, new AdRequest(), (loaded, error) =>
            {
                if (error != null || loaded == null)
                {
                    loadFailed = true;
                    return;
                }

                ad = loaded;
                ad.OnAdFullScreenContentClosed += () => closed = true;
                ad.OnAdFullScreenContentFailed += _ => closed = true;
                loadSucceeded = true;
            });
        }

        private void DestroyAd()
        {
            if (ad == null) return;
            ad.Destroy();
            ad = null;
        }
    }
}
