using System;
using System.Collections.Generic;
using SnakeApple.Presentation.Blocks;
using UnityEngine;

namespace SnakeApple.Presentation.Cosmetics
{
    // Ordered block themes and gameplay backgrounds. Index = saved selection, so entries are append-only.
    // Entries whose art is not authored yet stay in the list (stable indexes) but are hidden in the shop.
    [CreateAssetMenu(menuName = "Snake & Apple/Theme Catalog", fileName = "ThemeCatalog")]
    public sealed class ThemeCatalog : ScriptableObject
    {
        public const string ResourcePath = "Themes/ThemeCatalog";

        [Serializable]
        public sealed class BlockTheme
        {
            public string id;
            public string displayName;
            [Min(0)] public int price;
            public AutotileBlockSkin skin;
            public Sprite preview;

            public bool IsAvailable => skin != null && skin.IsConfigured;
        }

        [Serializable]
        public sealed class BackgroundTheme
        {
            public string id;
            public string displayName;
            [Min(0)] public int price;
            public Sprite art;

            public bool IsAvailable => art != null;
        }

        [SerializeField] private List<BlockTheme> blockThemes = new List<BlockTheme>();
        [SerializeField] private List<BackgroundTheme> backgrounds = new List<BackgroundTheme>();

        public int BlockCount => blockThemes.Count;
        public int BackgroundCount => backgrounds.Count;

        public BlockTheme FindBlock(int index)
        {
            return index >= 0 && index < blockThemes.Count ? blockThemes[index] : null;
        }

        public BackgroundTheme FindBackground(int index)
        {
            return index >= 0 && index < backgrounds.Count ? backgrounds[index] : null;
        }

        public void SetEntries(List<BlockTheme> blocks, List<BackgroundTheme> backgroundList)
        {
            blockThemes = blocks;
            backgrounds = backgroundList;
        }

        public static ThemeCatalog Load()
        {
            return Resources.Load<ThemeCatalog>(ResourcePath);
        }
    }
}
