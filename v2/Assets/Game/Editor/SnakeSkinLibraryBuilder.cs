using System.Collections.Generic;
using SnakeApple.Presentation.Snake;
using UnityEditor;
using UnityEngine;

namespace SnakeApple.Editor
{
    // Builds the SnakeSpriteSkin assets for the V1 skins and the ordered SnakeSkinCatalog.
    // Source sprites live in Content/Sprites/Snake/Skins/<key>/ (head, preview, body_0-7, corner_0-7, tail_0-7).
    [InitializeOnLoad]
    public static class SnakeSkinLibraryBuilder
    {
        private const string SpritesRoot = "Assets/Game/Content/Sprites/Snake/Skins/";
        private const string ResourcesFolder = "Assets/Game/Resources/SnakeSkins";
        private const int PhaseCount = 8;

        private readonly struct Definition
        {
            public Definition(string id, string displayName, int price, string spriteKey, string spineSkin, string existingAsset)
            {
                Id = id;
                DisplayName = displayName;
                Price = price;
                SpriteKey = spriteKey;
                SpineSkin = spineSkin;
                ExistingAsset = existingAsset;
            }

            public string Id { get; }
            public string DisplayName { get; }
            public int Price { get; }
            public string SpriteKey { get; }
            public string SpineSkin { get; }
            public string ExistingAsset { get; }
        }

        // Order is the saved SelectedSkin index: append only, never reorder.
        private static readonly Definition[] Definitions =
        {
            new Definition("nemo", "Clownfish", 0, "nemo", "skin_nemo", null),
            new Definition("dog", "Puppy", 150, "dog", "skin_dog", null),
            new Definition("bird", "Birdie", 200, "bird", "skin_bird", null),
            new Definition("frog", "Froggy", 250, "frog", "skin_frog", null),
            new Definition("tiger", "Tiger", 300, "tiger", "skin_tiger", null),
            new Definition("bear", "Bear", 350, "bear", "skin_bear", null),
            new Definition("unicorn", "Unicorn", 450, "unicorn", "skin_unicorn", null),
            new Definition("dragon", "Dragon", 550, "dragon", "skin_dragon", null),
            new Definition("sorcerer", "Sorcerer", 600, "doctor_strange", "skin_docter_strange", null),
            new Definition("thunder", "Thunder", 700, "thor", "skin_thor", null)
        };

        static SnakeSkinLibraryBuilder()
        {
            EditorApplication.delayCall += BuildIfNeeded;
        }

        [MenuItem("Snake Apple V2/Build Snake Skin Library")]
        public static void BuildFromMenu()
        {
            Build();
        }

        private static void BuildIfNeeded()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.delayCall += BuildIfNeeded;
                return;
            }

            var catalog = AssetDatabase.LoadAssetAtPath<SnakeSkinCatalog>(CatalogPath);
            if (catalog == null || catalog.Count != Definitions.Length) Build();
        }

        private static string CatalogPath => ResourcesFolder + "/SnakeSkinCatalog.asset";

        private static void Build()
        {
            var entries = new List<SnakeSkinCatalog.Entry>();
            foreach (var definition in Definitions)
            {
                var skin = definition.ExistingAsset != null
                    ? AssetDatabase.LoadAssetAtPath<SnakeSpriteSkin>($"{ResourcesFolder}/{definition.ExistingAsset}.asset")
                    : BuildSkin(definition);
                if (skin == null)
                {
                    Debug.LogWarning($"[Snake Apple V2] Skin '{definition.Id}' skipped: assets are missing or not imported yet.");
                    return; // never write a partial catalog: indexes must stay stable
                }

                entries.Add(new SnakeSkinCatalog.Entry
                {
                    id = definition.Id,
                    displayName = definition.DisplayName,
                    price = definition.Price,
                    skin = skin,
                    preview = definition.SpriteKey != null ? LoadSprite(definition.SpriteKey, "preview") : skin.Head
                });
            }

            var catalog = AssetDatabase.LoadAssetAtPath<SnakeSkinCatalog>(CatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<SnakeSkinCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }

            catalog.SetEntries(entries);
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            Debug.Log($"[Snake Apple V2] Snake skin catalog built with {entries.Count} skins.");
        }

        private static SnakeSpriteSkin BuildSkin(Definition definition)
        {
            var head = LoadSprite(definition.SpriteKey, "head");
            if (head == null) return null;
            var body = LoadPhases(definition.SpriteKey, "body");
            var corner = LoadPhases(definition.SpriteKey, "corner");
            var tail = LoadPhases(definition.SpriteKey, "tail");
            if (body == null || corner == null || tail == null) return null;

            var path = $"{ResourcesFolder}/Skin_{definition.Id}.asset";
            var skin = AssetDatabase.LoadAssetAtPath<SnakeSpriteSkin>(path);
            if (skin == null)
            {
                skin = ScriptableObject.CreateInstance<SnakeSpriteSkin>();
                AssetDatabase.CreateAsset(skin, path);
            }

            var serialized = new SerializedObject(skin);
            serialized.FindProperty("head").objectReferenceValue = head;
            serialized.FindProperty("headWidthInCells").floatValue = 1f;
            serialized.FindProperty("headHeightInCells").floatValue = 1f;
            AssignArray(serialized.FindProperty("bodyPhases"), body);
            AssignArray(serialized.FindProperty("cornerPhases"), corner);
            AssignArray(serialized.FindProperty("tailPhases"), tail);
            serialized.FindProperty("useSpineHead").boolValue = true;
            serialized.FindProperty("spineSkinName").stringValue = definition.SpineSkin;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(skin);
            return skin;
        }

        private static Sprite LoadSprite(string key, string name)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesRoot}{key}/{name}.png");
        }

        private static Sprite[] LoadPhases(string key, string prefix)
        {
            var sprites = new Sprite[PhaseCount];
            for (var index = 0; index < PhaseCount; index++)
            {
                sprites[index] = LoadSprite(key, $"{prefix}_{index}");
                if (sprites[index] == null) return null;
            }

            return sprites;
        }

        private static void AssignArray(SerializedProperty property, Sprite[] sprites)
        {
            property.arraySize = sprites.Length;
            for (var index = 0; index < sprites.Length; index++)
            {
                property.GetArrayElementAtIndex(index).objectReferenceValue = sprites[index];
            }
        }
    }
}
