using System.Collections.Generic;
using SnakeApple.Infrastructure.Save;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SnakeApple.Presentation.Audio
{
    /// <summary>
    /// Presentation-only audio. Reads Sfx/Music toggles from LocalProfileStore at play time,
    /// so Settings changes apply immediately without touching gameplay state.
    /// </summary>
    public sealed class AudioManager : MonoBehaviour
    {
        private const int VoiceCount = 10;
        private const string BankPath = "Audio/SoundBank";

        private static AudioManager instance;

        private readonly Dictionary<SfxCue, SoundBank.Entry> entries = new Dictionary<SfxCue, SoundBank.Entry>();
        private readonly Dictionary<SfxCue, float> lastPlayed = new Dictionary<SfxCue, float>();
        private readonly HashSet<int> hookedWidgets = new HashSet<int>();
        private AudioSource[] voices;
        private AudioSource musicSource;
        private SoundBank bank;
        private int nextVoice;

        public static void Play(SfxCue cue)
        {
            if (instance != null) instance.PlayInternal(cue);
        }

        /// <summary>Hooks click sounds onto buttons/toggles created after scene load.</summary>
        public static void RescanWidgets()
        {
            if (instance != null) instance.HookWidgets();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (instance != null) return;
            var go = new GameObject("AudioManager");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<AudioManager>();
        }

        private void Awake()
        {
            // Unity plays nothing without an AudioListener and the UI-only scenes have none.
            if (GetComponent<AudioListener>() == null) gameObject.AddComponent<AudioListener>();

            bank = Resources.Load<SoundBank>(BankPath);
            if (bank == null)
            {
                Debug.LogWarning($"AudioManager: missing Resources/{BankPath}. Audio disabled.");
                return;
            }

            foreach (var entry in bank.Entries)
            {
                if (entry.clip != null) entries[entry.cue] = entry;
            }

            voices = new AudioSource[VoiceCount];
            for (var i = 0; i < VoiceCount; i++)
            {
                voices[i] = gameObject.AddComponent<AudioSource>();
                voices[i].playOnAwake = false;
            }

            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
            musicSource.clip = bank.MenuMusic;

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (instance == this) instance = null;
        }

        private void Update()
        {
            if (musicSource == null || musicSource.clip == null) return;
            var want = LocalProfileStore.Music;
            if (want && !musicSource.isPlaying)
            {
                musicSource.volume = bank.MusicVolume;
                musicSource.Play();
            }
            else if (!want && musicSource.isPlaying)
            {
                musicSource.Pause();
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            HookWidgets();
        }

        private void HookWidgets()
        {
            foreach (var button in FindObjectsOfType<Button>(true))
            {
                if (!hookedWidgets.Add(button.GetInstanceID())) continue;
                var buttonName = button.name;
                // D-pad buttons already produce Move/Blocked cues.
                if (buttonName.StartsWith("Move")) continue;
                var cue = buttonName.Contains("Back") || buttonName.Contains("Stay") || buttonName.Contains("Resume")
                    ? SfxCue.UiBack
                    : SfxCue.UiClick;
                button.onClick.AddListener(() => Play(cue));
            }

            foreach (var toggle in FindObjectsOfType<Toggle>(true))
            {
                if (!hookedWidgets.Add(toggle.GetInstanceID())) continue;
                toggle.onValueChanged.AddListener(_ => Play(SfxCue.UiClick));
            }
        }

        private void PlayInternal(SfxCue cue)
        {
            if (voices == null || !LocalProfileStore.Sfx) return;
            if (!entries.TryGetValue(cue, out var entry)) return;

            var now = Time.unscaledTime;
            if (entry.cooldown > 0f && lastPlayed.TryGetValue(cue, out var last) && now - last < entry.cooldown) return;
            lastPlayed[cue] = now;

            var voice = voices[nextVoice];
            nextVoice = (nextVoice + 1) % voices.Length;
            voice.pitch = 1f + Random.Range(-entry.pitchJitter, entry.pitchJitter);
            voice.PlayOneShot(entry.clip, entry.volume <= 0f ? 1f : entry.volume);
        }
    }
}
