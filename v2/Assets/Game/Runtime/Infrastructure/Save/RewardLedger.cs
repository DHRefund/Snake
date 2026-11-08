using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace SnakeApple.Infrastructure.Save
{
    // Rewarded-ad payout bookkeeping: an idempotent TryCommit per ad attempt plus a rolling 24 h cap.
    public static class RewardLedger
    {
        public const int RewardCoins = 50;
        public const int MaxGrantsPerWindow = 5;
        public const long WindowSeconds = 24L * 60L * 60L;
        private const int MaxRememberedAttempts = 100;
        private const string GrantsKey = "profile.rewards.grants";
        private const string AttemptsKey = "profile.rewards.attempts";

        public static long NowUnix => DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        public static int RemainingGrants(long nowUnix)
        {
            return Mathf.Max(0, MaxGrantsPerWindow - ActiveGrants(nowUnix).Count);
        }

        public static long SecondsUntilNextGrant(long nowUnix)
        {
            var grants = ActiveGrants(nowUnix);
            if (grants.Count < MaxGrantsPerWindow) return 0;
            return Math.Max(1, grants[0] + WindowSeconds - nowUnix);
        }

        public static string NewAttemptId()
        {
            return Guid.NewGuid().ToString("N");
        }

        // Returns true only the first time an attempt id is committed and only while under the cap.
        public static bool TryCommit(string attemptId, long nowUnix)
        {
            if (string.IsNullOrEmpty(attemptId)) return false;
            var attempts = ReadStrings(AttemptsKey);
            if (attempts.Contains(attemptId)) return false;

            var grants = ActiveGrants(nowUnix);
            if (grants.Count >= MaxGrantsPerWindow) return false;

            grants.Add(nowUnix);
            attempts.Add(attemptId);
            while (attempts.Count > MaxRememberedAttempts) attempts.RemoveAt(0);

            PlayerPrefs.SetString(GrantsKey, string.Join(",", grants));
            PlayerPrefs.SetString(AttemptsKey, string.Join(",", attempts));
            LocalProfileStore.Coins += RewardCoins;
            return true;
        }

        private static List<long> ActiveGrants(long nowUnix)
        {
            var result = new List<long>();
            foreach (var part in ReadStrings(GrantsKey))
            {
                if (long.TryParse(part, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
                    && nowUnix - value < WindowSeconds)
                {
                    result.Add(value);
                }
            }

            result.Sort();
            return result;
        }

        private static List<string> ReadStrings(string key)
        {
            var raw = PlayerPrefs.GetString(key, string.Empty);
            return string.IsNullOrEmpty(raw)
                ? new List<string>()
                : new List<string>(raw.Split(','));
        }
    }
}
