using System;
using System.Collections.Generic;
using UnityEngine;

namespace SnakeApple.Presentation.Snake
{
    // Ordered list of every snake skin. The index is what LocalProfileStore.SelectedSkin stores,
    // so entries must only ever be appended, never reordered.
    [CreateAssetMenu(menuName = "Snake & Apple/Snake Skin Catalog", fileName = "SnakeSkinCatalog")]
    public sealed class SnakeSkinCatalog : ScriptableObject
    {
        public const string ResourcePath = "SnakeSkins/SnakeSkinCatalog";

        [Serializable]
        public sealed class Entry
        {
            public string id;
            public string displayName;
            [Min(0)] public int price;
            public SnakeSpriteSkin skin;
            public Sprite preview;
        }

        [SerializeField] private List<Entry> entries = new List<Entry>();

        public int Count => entries.Count;
        public Entry this[int index] => entries[index];

        public Entry Find(int index)
        {
            return index >= 0 && index < entries.Count ? entries[index] : null;
        }

        public SnakeSpriteSkin SkinAt(int index)
        {
            var entry = Find(index);
            return entry != null && entry.skin != null && entry.skin.IsConfigured ? entry.skin : null;
        }

        public void SetEntries(List<Entry> newEntries)
        {
            entries = newEntries;
        }

        public static SnakeSkinCatalog Load()
        {
            return Resources.Load<SnakeSkinCatalog>(ResourcePath);
        }
    }
}
