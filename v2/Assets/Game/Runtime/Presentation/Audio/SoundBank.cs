using System;
using UnityEngine;

namespace SnakeApple.Presentation.Audio
{
    [CreateAssetMenu(menuName = "Snake Apple/Sound Bank")]
    public sealed class SoundBank : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public SfxCue cue;
            public AudioClip clip;
            [Range(0f, 1f)] public float volume;
            [Range(0f, 0.3f)] public float pitchJitter;
            public float cooldown;
        }

        [SerializeField] private Entry[] entries = Array.Empty<Entry>();
        [SerializeField] private AudioClip menuMusic;
        [SerializeField, Range(0f, 1f)] private float musicVolume = 0.4f;

        public AudioClip MenuMusic => menuMusic;
        public float MusicVolume => musicVolume;
        public Entry[] Entries => entries;
    }
}
