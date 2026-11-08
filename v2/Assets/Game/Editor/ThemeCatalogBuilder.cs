using System.Collections.Generic;
using SnakeApple.Presentation.Blocks;
using SnakeApple.Presentation.Cosmetics;
using UnityEditor;
using UnityEngine;

namespace SnakeApple.Editor
{
    // Builds the ThemeCatalog (block themes + backgrounds) and one AutotileBlockSkin per authored theme.
    // A block theme is "available" once Sprites/Blocks/Skins/<Folder>/Runtime holds block_<id>_mXX_vYY.png for all
    // 16 masks. Re-runs on every editor load, so dropping new art in is enough.
    [InitializeOnLoad]
    public static class ThemeCatalogBuilder
    {
        private const string BlocksRoot = "Assets/Game/Content/Sprites/Blocks/Skins/";
        private const string BackgroundsRoot = "Assets/Game/Content/Sprites/UI/Backgrounds/";
        private const string ResourcesFolder = "Assets/Game/Resources/Themes";
        private const string BlockSkinFolder = "Assets/Game/Resources/BlockSkins";

        private readonly struct BlockDefinition
        {
            public BlockDefinition(string id, string displayName, int price, string folder, string existingAsset)
            {
                Id = id;
                DisplayName = displayName;
                Price = price;
                Folder = folder;
                ExistingAsset = existingAsset;
            }

            public string Id { get; }
            public string DisplayName { get; }
            public int Price { get; }
            public string Folder { get; }
            public string ExistingAsset { get; }
        }

        private readonly struct BackgroundDefinition
        {
            public BackgroundDefinition(string id, string displayName, int price, string file)
            {
                Id = id;
                DisplayName = displayName;
                Price = price;
                File = file;
            }

            public string Id { get; }
            public string DisplayName { get; }
            public int Price { get; }
            public string File { get; }
        }

        // Order is the saved selection index: append only, never reorder.
        private static readonly BlockDefinition[] Blocks =
        {
            new BlockDefinition("ice", "Frosty Ice", 0, "Ice", "IceBlockSkin"),
            new BlockDefinition("desert", "Sandstone", 200, "Desert", null),
            new BlockDefinition("forest", "Mossy Wood", 300, "Forest", null),
            new BlockDefinition("lava", "Volcano", 400, "Lava", null),
            new BlockDefinition("candy", "Candy Land", 500, "Candy", null),
            new BlockDefinition("night", "Midnight", 600, "Night", null)
        };

        private static readonly BackgroundDefinition[] Backgrounds =
        {
            new BackgroundDefinition("iceberg", "Iceberg", 0, "bg_iceberg.png"),
            new BackgroundDefinition("desert", "Desert Day", 150, "bg_desert_day.png"),
            new BackgroundDefinition("clouds", "Sky Clouds", 250, "bg_clouds_light.png")
        };

        static ThemeCatalogBuilder()
        {
            EditorApplication.delayCall += BuildWhenReady;
        }

        [MenuItem("Snake Apple V2/Build Theme Catalog")]
        public static void BuildFromMenu()
        {
            Build();
        }

        private static void BuildWhenReady()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.delayCall += BuildWhenReady;
                return;
            }

            Build();
        }

        private static void Build()
        {
            EnsureFolder("Assets/Game/Resources", "Themes");

            var blocks = new List<ThemeCatalog.BlockTheme>();
            foreach (var definition in Blocks)
            {
                var skin = LoadOrBuildBlockSkin(definition);
                blocks.Add(new ThemeCatalog.BlockTheme
                {
                    id = definition.Id,
                    displayName = definition.DisplayName,
                    price = definition.Price,
                    skin = skin,
                    preview = skin != null ? LoadSprite(definition, 0, 1) : null
                });
            }

            var backgrounds = new List<ThemeCatalog.BackgroundTheme>();
            foreach (var definition in Backgrounds)
            {
                backgrounds.Add(new ThemeCatalog.BackgroundTheme
                {
                    id = definition.Id,
                    displayName = definition.DisplayName,
                    price = definition.Price,
                    art = AssetDatabase.LoadAssetAtPath<Sprite>(BackgroundsRoot + definition.File)
                });
            }

            var path = ResourcesFolder + "/ThemeCatalog.asset";
            var catalog = AssetDatabase.LoadAssetAtPath<ThemeCatalog>(path);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<ThemeCatalog>();
                AssetDatabase.CreateAsset(catalog, path);
            }

            catalog.SetEntries(blocks, backgrounds);
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
        }

        private static AutotileBlockSkin LoadOrBuildBlockSkin(BlockDefinition definition)
        {
            if (definition.ExistingAsset != null)
            {
                return AssetDatabase.LoadAssetAtPath<AutotileBlockSkin>(
                    $"{BlockSkinFolder}/{definition.ExistingAsset}.asset");
            }

            var masks = new List<Sprite>[16];
            for (var mask = 0; mask < 16; mask++)
            {
                masks[mask] = new List<Sprite>();
                for (var variant = 1; variant <= 9; variant++)
                {
                    var sprite = LoadSprite(definition, mask, variant);
                    if (sprite != null) masks[mask].Add(sprite);
                }

                if (masks[mask].Count == 0) return null; // incomplete theme stays hidden
            }

            var assetPath = $"{BlockSkinFolder}/{definition.Folder}BlockSkin.asset";
            var skin = AssetDatabase.LoadAssetAtPath<AutotileBlockSkin>(assetPath);
            if (skin == null)
            {
                skin = ScriptableObject.CreateInstance<AutotileBlockSkin>();
                AssetDatabase.CreateAsset(skin, assetPath);
            }

            for (var mask = 0; mask < 16; mask++) skin.SetMaskVariants(mask, masks[mask]);
            EditorUtility.SetDirty(skin);
            return skin;
        }

        private static Sprite LoadSprite(BlockDefinition definition, int mask, int variant)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(
                $"{BlocksRoot}{definition.Folder}/Runtime/block_{definition.Id}_m{mask:00}_v{variant:00}.png");
        }

        private static void EnsureFolder(string parent, string child)
        {
            if (!AssetDatabase.IsValidFolder(parent + "/" + child)) AssetDatabase.CreateFolder(parent, child);
        }
    }
}
