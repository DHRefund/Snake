using System;
using System.IO;
using System.Linq;
using SnakeApple.Presentation.Blocks;
using SnakeApple.Presentation.Items;
using SnakeApple.Presentation.Navigation;
using SnakeApple.Presentation.Screens;
using SnakeApple.Presentation.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SnakeApple.Editor
{
    public static class UiSceneGenerator
    {
        private const string ScenesFolder = "Assets/Game/Scenes";
        private const string PrefabsFolder = "Assets/Game/Prefabs/UI";
        private const string BootScenePath = ScenesFolder + "/Boot.unity";
        private const string HomeScenePath = ScenesFolder + "/Home.unity";
        private const string LevelSelectScenePath = ScenesFolder + "/LevelSelect.unity";
        private const string GameplayScenePath = ScenesFolder + "/Gameplay.unity";
        private const string SettingsScenePath = ScenesFolder + "/Settings.unity";
        private const string CosmeticsScenePath = ScenesFolder + "/Cosmetics.unity";
        private const string GeneratedVersionPath = "Assets/Game/UI_GENERATED_VERSION.txt";
        private const string GeneratedVersion = "8";
        private const string LegacySnakeFolder = "Assets/Game/Content/Sprites/Snake/LegacyDemo";

        private static readonly Color Teal = new Color32(21, 103, 105, 255);
        private static readonly Color DeepTeal = new Color32(12, 65, 69, 255);
        private static readonly Color Mint = new Color32(115, 211, 166, 255);
        private static readonly Color Cream = new Color32(255, 249, 226, 255);
        private static readonly Color Orange = new Color32(255, 132, 42, 255);
        private static readonly Color Yellow = new Color32(255, 197, 55, 255);
        private static readonly Color Red = new Color32(229, 73, 63, 255);
        private static readonly Color Ink = new Color32(38, 52, 54, 255);
        private static readonly Color MutedInk = new Color32(91, 108, 108, 255);

        private static Font font;
        private static Sprite uiSprite;
        private static Sprite knobSprite;

        [InitializeOnLoadMethod]
        private static void GenerateOnFirstImport()
        {
            var bootPath = Path.Combine(UnityEngine.Application.dataPath, "Game/Scenes/Boot.unity");
            var versionFile = Path.Combine(UnityEngine.Application.dataPath, "Game/UI_GENERATED_VERSION.txt");
            var requiresGeneration = !File.Exists(bootPath) ||
                                     new FileInfo(bootPath).Length == 0 ||
                                     !File.Exists(versionFile) ||
                                     File.ReadAllText(versionFile).Trim() != GeneratedVersion;
            if (requiresGeneration)
            {
                EditorApplication.delayCall += GenerateWhenReady;
            }
        }

        [MenuItem("Snake Apple V2/Rebuild All UI Scenes")]
        public static void RebuildFromMenu()
        {
            GenerateAll();
        }

        private static void GenerateWhenReady()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.delayCall += GenerateWhenReady;
                return;
            }

            GenerateAll();
        }

        private static void GenerateAll()
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            knobSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

            EnsureFolder("Assets/Game", "Scenes");
            EnsureFolder("Assets/Game", "Prefabs");
            EnsureFolder("Assets/Game/Prefabs", "UI");
            CreateSharedPrefabs();
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;

            var originalScene = SceneManager.GetActiveScene();
            var originalScenePath = originalScene.path;
            CreateBootScene();
            CreateHomeScene();
            CreateLevelSelectScene();
            CreateGameplayScene();
            CreateSettingsScene();
            CreateCosmeticsScene();
            if (originalScene.IsValid() && originalScene.isLoaded)
            {
                SceneManager.SetActiveScene(originalScene);
            }
            else if (!string.IsNullOrEmpty(originalScenePath))
            {
                EditorSceneManager.OpenScene(originalScenePath, OpenSceneMode.Single);
            }

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(BootScenePath, true),
                new EditorBuildSettingsScene(HomeScenePath, true),
                new EditorBuildSettingsScene(LevelSelectScenePath, true),
                new EditorBuildSettingsScene(GameplayScenePath, true),
                new EditorBuildSettingsScene(SettingsScenePath, true),
                new EditorBuildSettingsScene(CosmeticsScenePath, true)
            };

            File.WriteAllText(
                Path.Combine(UnityEngine.Application.dataPath, "Game/UI_GENERATED_VERSION.txt"),
                GeneratedVersion);
            AssetDatabase.ImportAsset(GeneratedVersionPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            UiSceneValidator.ValidateFromMenu();
            Debug.Log("[Snake Apple V2] Generated six UI scenes, two UI prefabs and build settings.");
        }

        private static void CreateBootScene()
        {
            CreateScene(BootScenePath, "BootScreen", root =>
            {
                root.AddComponent<BootScreenController>();
                root.AddComponent<ScreenNavigator>();
                var safe = BuildBase(root.transform, false);

                var logoSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/Content/Sprites/UI/logo_main.png");
                var appleSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/Content/Sprites/Items/apple_normal.png");
                var trackSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/Content/Sprites/UI/progress_track.png");
                var fillSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/Content/Sprites/UI/progress_fill.png");

                // Ambient golden glow behind the logo
                Blob(safe, "LogoGlow", new Vector2(0, 220), 560, new Color(1f, 0.78f, 0.25f, 0.12f));

                // Official high-res title logo
                if (logoSprite != null)
                {
                    var logoImg = Panel(safe, "MainLogo", Color.white, new Vector2(0, 220), new Vector2(760, 328), logoSprite);
                    logoImg.preserveAspect = true;
                    logoImg.type = Image.Type.Simple;
                }
                else
                {
                    CreateLogo(safe, new Vector2(0, 280), 1.25f);
                    Text(safe, "TitleText", "SNAKE & APPLE", 74, FontStyle.Bold, Cream,
                        new Vector2(0, 35), new Vector2(900, 110));
                }

                Text(safe, "SubtitleText", "A CLEVER PUZZLE ADVENTURE", 24, FontStyle.Bold,
                    new Color32(220, 245, 235, 220), new Vector2(0, 30), new Vector2(900, 50));

                if (appleSprite != null)
                {
                    var mascot = Panel(safe, "AppleMascot", Color.white, new Vector2(0, -150), new Vector2(110, 110), appleSprite);
                    mascot.preserveAspect = true;
                    mascot.type = Image.Type.Simple;
                }

                // Modern 9-sliced rounded capsule progress bar
                var bar = Panel(safe, "ProgressTrack", Color.white, new Vector2(0, -310), new Vector2(640, 32), trackSprite);
                bar.type = Image.Type.Sliced;

                var fill = Panel(bar.transform, "ProgressFill", Color.white, Vector2.zero, Vector2.zero, fillSprite);
                Stretch(fill.rectTransform, 3, 3, 3, 3);
                fill.type = Image.Type.Filled;
                fill.fillMethod = Image.FillMethod.Horizontal;
                fill.fillAmount = 0f;

                Text(safe, "StatusText", "Loading level catalog...", 23, FontStyle.Bold,
                    Cream, new Vector2(0, -375), new Vector2(760, 50));

                var retry = Button(safe, "RetryButton", "RETRY", Orange, Cream,
                    new Vector2(0, -490), new Vector2(360, 88), 26);
                retry.gameObject.SetActive(false);

                Text(safe, "VersionText", "V2.0 • OFFICIAL BUILD", 19, FontStyle.Bold,
                    new Color(1, 1, 1, 0.40f), new Vector2(0, -780), new Vector2(500, 40));
            });
        }

        private static void CreateHomeScene()
        {
            CreateScene(HomeScenePath, "HomeScreen", root =>
            {
                root.AddComponent<HomeScreenController>();
                root.AddComponent<ScreenNavigator>();
                var safe = BuildBase(root.transform, true);
                CreateCoinPill(safe, new Vector2(355, -85));
                Text(safe, "TinyTitle", "SNAKE & APPLE", 26, FontStyle.Bold,
                    new Color(1, 1, 1, 0.72f), new Vector2(-280, -85), new Vector2(430, 60));
                CreateLogo(safe, new Vector2(0, 300), 1.05f);
                Text(safe, "HeroTitle", "READY TO\nWIGGLE?", 78, FontStyle.Bold, Cream,
                    new Vector2(0, 30), new Vector2(900, 220));
                Text(safe, "HeroSubtitle", "Eat every apple. Find the exit. Outsmart the board.", 27,
                    FontStyle.Normal, new Color(1, 1, 1, 0.78f), new Vector2(0, -120), new Vector2(880, 70));

                Button(safe, "PlayButton", "PLAY", Orange, Cream, new Vector2(0, -315), new Vector2(720, 126), 38);
                Button(safe, "LevelsButton", "LEVELS", Cream, Ink, new Vector2(0, -470), new Vector2(720, 110), 31);
                Button(safe, "CosmeticsButton", "COSMETICS", Mint, DeepTeal, new Vector2(0, -610), new Vector2(720, 100), 28);
                Button(safe, "SettingsButton", "SETTINGS", new Color(1, 1, 1, 0.16f), Cream,
                    new Vector2(0, -735), new Vector2(720, 94), 27);
                Text(safe, "FooterText", "Swipe or use arrow keys • Undo is always free", 21,
                    FontStyle.Normal, new Color(1, 1, 1, 0.5f), new Vector2(0, -850), new Vector2(900, 50));
            });
        }

        private static void CreateLevelSelectScene()
        {
            CreateScene(LevelSelectScenePath, "LevelSelectScreen", root =>
            {
                root.AddComponent<LevelSelectScreenController>();
                root.AddComponent<ScreenNavigator>();
                var safe = BuildBase(root.transform, false);
                Header(safe, "CHOOSE A LEVEL");
                Button(safe, "BackButton", "‹", new Color(1, 1, 1, 0.17f), Cream,
                    new Vector2(-430, -88), new Vector2(92, 92), 48);

                var tabs = Panel(safe, "ModeTabs", new Color(1, 1, 1, 0.10f),
                    new Vector2(0, -230), new Vector2(900, 92));
                Button(tabs.transform, "NormalTab", "NORMAL", Orange, Cream,
                    new Vector2(-285, 0), new Vector2(275, 68), 23);
                Button(tabs.transform, "HardTab", "HARD", new Color(1, 1, 1, 0.09f),
                    new Color(1, 1, 1, 0.72f), Vector2.zero, new Vector2(275, 68), 23);
                Button(tabs.transform, "ChallengeTab", "CHALLENGE", new Color(1, 1, 1, 0.09f),
                    new Color(1, 1, 1, 0.72f), new Vector2(285, 0), new Vector2(275, 68), 21);

                var card = Panel(safe, "LevelCard", Cream, new Vector2(0, -780), new Vector2(930, 950));
                Text(card.transform, "ChapterText", "CHAPTER 1  •  FIRST BITES", 28, FontStyle.Bold, Ink,
                    new Vector2(0, 402), new Vector2(820, 60));
                Text(card.transform, "ChapterSubtext", "Complete levels to unlock the path ahead", 22,
                    FontStyle.Normal, MutedInk, new Vector2(0, 350), new Vector2(820, 50));
                var gridObject = Node("LevelGrid", card.transform);
                var gridRect = gridObject.GetComponent<RectTransform>();
                Place(gridRect, new Vector2(0.5f, 0.5f), new Vector2(0, -45), new Vector2(790, 690));
                var grid = gridObject.AddComponent<GridLayoutGroup>();
                grid.cellSize = new Vector2(142, 116);
                grid.spacing = new Vector2(20, 22);
                grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                grid.constraintCount = 5;
                grid.childAlignment = TextAnchor.UpperCenter;
                for (var level = 1; level <= 20; level++)
                {
                    var available = level == 1;
                    var button = Button(gridObject.transform, $"Level_{level:000}", available ? level.ToString() : $"{level}\nLOCKED",
                        available ? Orange : new Color32(220, 224, 210, 255), available ? Cream : MutedInk,
                        Vector2.zero, new Vector2(142, 116), available ? 36 : 17);
                    button.interactable = available;
                }
                Button(card.transform, "PrevPageButton", "< PREV", new Color32(220, 224, 210, 255), Ink,
                    new Vector2(-285, -420), new Vector2(220, 62), 20);
                Text(card.transform, "PageText", "PAGE 1 / 9", 20, FontStyle.Bold, MutedInk,
                    new Vector2(0, -420), new Vector2(240, 62));
                Button(card.transform, "NextPageButton", "NEXT >", Orange, Cream,
                    new Vector2(285, -420), new Vector2(220, 62), 20);
            });
        }

        private static void CreateGameplayScene()
        {
            CreateScene(GameplayScenePath, "GameplayScreen", root =>
            {
                var controller = root.AddComponent<GameplayScreenController>();
                controller.ConfigureLegacySnakeSkin(
                    AssetDatabase.LoadAssetAtPath<SnakeApple.Presentation.Snake.SnakeSpriteSkin>(
                        "Assets/Game/Resources/SnakeSkins/LegacyV1SnakeSkin.asset"));
                controller.ConfigureAutotileBlockSkin(
                    AssetDatabase.LoadAssetAtPath<AutotileBlockSkin>(
                        "Assets/Game/Resources/BlockSkins/IceBlockSkin.asset"));
                controller.ConfigureItemSkin(
                    AssetDatabase.LoadAssetAtPath<GameplayItemSkin>(
                        "Assets/Game/Resources/ItemSkins/DefaultItemSkin.asset"));
                root.AddComponent<ScreenNavigator>();
                var safe = BuildBase(root.transform, false);
                Header(safe, "LEVEL 1", "LevelText");
                Button(safe, "PauseButton", "Ⅱ", new Color(1, 1, 1, 0.17f), Cream,
                    new Vector2(-430, -88), new Vector2(92, 92), 38);
                var applePill = Panel(safe, "ApplePill", new Color(1, 1, 1, 0.16f),
                    new Vector2(390, -88), new Vector2(155, 82));
                Text(applePill.transform, "AppleIcon", "●", 38, FontStyle.Bold, Red,
                    new Vector2(-40, 0), new Vector2(60, 60));
                Text(applePill.transform, "AppleText", "1", 30, FontStyle.Bold, Cream,
                    new Vector2(33, 0), new Vector2(65, 60));

                var boardCard = Panel(safe, "BoardCard", Cream, new Vector2(0, 155), new Vector2(960, 860));
                var gridObject = Node("BoardGrid", boardCard.transform);
                var gridRect = gridObject.GetComponent<RectTransform>();
                Place(gridRect, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(820, 640));
                gridObject.AddComponent<GridLayoutGroup>();
                var snakeOverlay = Node("SnakeOverlay", boardCard.transform);
                var snakeOverlayRect = snakeOverlay.GetComponent<RectTransform>();
                Place(snakeOverlayRect, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(820, 640));
                snakeOverlay.transform.SetAsLastSibling();
                Text(safe, "FeedbackText", "Guide the worm to the apple, then reach the exit.", 23,
                    FontStyle.Bold, Cream, new Vector2(0, -330), new Vector2(900, 70));

                Button(safe, "UndoButton", "↶  UNDO", Cream, Ink,
                    new Vector2(-300, -465), new Vector2(270, 96), 24);
                Button(safe, "RestartButton", "↻  RESTART", new Color(1, 1, 1, 0.14f), Cream,
                    new Vector2(0, -465), new Vector2(270, 96), 22);
                Button(safe, "HintButton", "?  HINT", Mint, DeepTeal,
                    new Vector2(300, -465), new Vector2(270, 96), 24);
                CreateDpad(safe);

                CreateGameplayModal(safe, "PauseOverlay", "PAUSED", "Take a breath. The puzzle will wait.",
                    new[]
                    {
                        new ModalButton("PauseResumeButton", "RESUME", Orange),
                        new ModalButton("PauseSettingsButton", "SETTINGS", Yellow),
                        new ModalButton("PauseRestartButton", "RESTART", Mint),
                        new ModalButton("PauseHomeButton", "HOME", new Color32(209, 218, 207, 255))
                    });
                CreateQuickSettingsModal(safe);
                CreateGameplayModal(safe, "ConfirmHomeOverlay", "LEAVE LEVEL?", "Current moves will be lost.",
                    new[]
                    {
                        new ModalButton("ConfirmStayButton", "KEEP PLAYING", Orange),
                        new ModalButton("ConfirmHomeButton", "LEAVE TO HOME", new Color32(209, 218, 207, 255))
                    });
                var win = CreateGameplayModal(safe, "WinOverlay", "LEVEL COMPLETE!", "That was a smart route.",
                    new[]
                    {
                        new ModalButton("WinNextButton", "NEXT LEVEL", Orange),
                        new ModalButton("WinLevelsButton", "LEVELS", Mint),
                        new ModalButton("WinHomeButton", "HOME", new Color32(209, 218, 207, 255))
                    });
                Text(win.transform.Find("ModalCard"), "WinMoveText", "Solved in 0 moves", 25,
                    FontStyle.Bold, MutedInk, new Vector2(0, 88), new Vector2(600, 55));
                Text(win.transform.Find("ModalCard"), "WinRewardText", "FIRST CLEAR  •  +0 COINS", 21,
                    FontStyle.Bold, new Color32(181, 116, 24, 255), new Vector2(0, 132), new Vector2(600, 45));
                CreateGameplayModal(safe, "LoseOverlay", "TRY AGAIN", "The worm needs another route.",
                    new[]
                    {
                        new ModalButton("LoseUndoButton", "UNDO", Orange),
                        new ModalButton("LoseRestartButton", "RESTART", Mint),
                        new ModalButton("LoseHomeButton", "HOME", new Color32(209, 218, 207, 255))
                    });
            });
        }

        private static void CreateSettingsScene()
        {
            CreateScene(SettingsScenePath, "SettingsScreen", root =>
            {
                root.AddComponent<SettingsScreenController>();
                root.AddComponent<ScreenNavigator>();
                var safe = BuildBase(root.transform, false);
                Header(safe, "SETTINGS");
                Button(safe, "BackButton", "‹", new Color(1, 1, 1, 0.17f), Cream,
                    new Vector2(-430, -88), new Vector2(92, 92), 48);
                var card = Panel(safe, "SettingsCard", Cream, new Vector2(0, -270), new Vector2(910, 1050));
                Text(card.transform, "SoundTitle", "SOUND & FEEDBACK", 25, FontStyle.Bold, MutedInk,
                    new Vector2(-245, 440), new Vector2(360, 55), TextAnchor.MiddleLeft);
                ToggleRow(card.transform, "MusicToggle", "Music", "Background soundtrack", 320);
                ToggleRow(card.transform, "SfxToggle", "Sound effects", "Moves, apples and hazards", 130);
                ToggleRow(card.transform, "HapticsToggle", "Haptics", "Gentle vibration feedback", -60);
                Text(card.transform, "AccessTitle", "ACCESSIBILITY", 25, FontStyle.Bold, MutedInk,
                    new Vector2(-245, -190), new Vector2(360, 55), TextAnchor.MiddleLeft);
                ToggleRow(card.transform, "ReducedMotionToggle", "Reduced motion", "Shorter and simpler animations", -310);
                Text(card.transform, "PrivacyText", "Privacy options become available when advertising consent requires them.",
                    20, FontStyle.Normal, MutedInk, new Vector2(0, -455), new Vector2(760, 80));
                Text(safe, "BuildText", "Snake & Apple: Puzzle Adventure • V2", 20, FontStyle.Normal,
                    new Color(1, 1, 1, 0.48f), new Vector2(0, -850), new Vector2(700, 50));
            });
        }

        private static Sprite LoadLegacySnakeSprite(string fileName)
        {
            var path = $"{LegacySnakeFolder}/{fileName}";
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                throw new InvalidOperationException($"Legacy snake demo sprite is missing or not imported: {path}");
            }

            return sprite;
        }

        private static void CreateCosmeticsScene()
        {
            CreateScene(CosmeticsScenePath, "CosmeticsScreen", root =>
            {
                root.AddComponent<CosmeticsScreenController>();
                root.AddComponent<ScreenNavigator>();
                var safe = BuildBase(root.transform, false);
                Header(safe, "WORM CLOSET");
                Button(safe, "BackButton", "‹", new Color(1, 1, 1, 0.17f), Cream,
                    new Vector2(-430, -88), new Vector2(92, 92), 48);
                CreateCoinPill(safe, new Vector2(365, -88));
                Text(safe, "ShopSubtitle", "Cosmetics never change puzzle rules", 23, FontStyle.Normal,
                    new Color(1, 1, 1, 0.67f), new Vector2(0, -190), new Vector2(820, 55));
                var card = Panel(safe, "ShopCard", Cream, new Vector2(0, -750), new Vector2(940, 1030));
                var gridObject = Node("SkinGrid", card.transform);
                var gridRect = gridObject.GetComponent<RectTransform>();
                Place(gridRect, new Vector2(0.5f, 0.5f), new Vector2(0, 35), new Vector2(820, 830));
                var grid = gridObject.AddComponent<GridLayoutGroup>();
                grid.cellSize = new Vector2(188, 245);
                grid.spacing = new Vector2(20, 30);
                grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                grid.constraintCount = 4;
                grid.childAlignment = TextAnchor.UpperCenter;
                Color[] skinColors = { Orange, Mint, Yellow, Red, new Color32(102, 168, 255, 255),
                    new Color32(190, 116, 231, 255), new Color32(250, 130, 170, 255), new Color32(84, 192, 184, 255),
                    new Color32(147, 197, 73, 255), new Color32(69, 82, 104, 255), new Color32(209, 139, 83, 255),
                    new Color32(237, 226, 202, 255) };
                for (var index = 0; index < skinColors.Length; index++)
                {
                    var button = Button(gridObject.transform, $"Skin_{index:00}", index == 0 ? "SELECTED" : "LOCKED",
                        skinColors[index], index == 9 ? Cream : Ink, Vector2.zero, new Vector2(188, 245), 17);
                    var label = button.GetComponentInChildren<Text>();
                    label.alignment = TextAnchor.LowerCenter;
                    label.rectTransform.offsetMin = new Vector2(8, 12);
                    label.rectTransform.offsetMax = new Vector2(-8, -168);
                }
                Text(card.transform, "ShopStatusText", "Choose a look for your worm.", 22, FontStyle.Bold,
                    MutedInk, new Vector2(0, -455), new Vector2(760, 60));
            });
        }

        private static void CreateScene(string path, string rootName, Action<GameObject> build)
        {
            var previous = SceneManager.GetActiveScene();
            var batchMode = UnityEngine.Application.isBatchMode;
            var scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                batchMode ? NewSceneMode.Single : NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);

            // A scene cannot safely be overwritten while an older copy of that same path is loaded.
            // Close the old copy only after the replacement scene exists, so Unity always has a loaded scene.
            var loadedCopy = SceneManager.GetSceneByPath(path);
            if (loadedCopy.IsValid() && loadedCopy.isLoaded && loadedCopy != scene)
            {
                EditorSceneManager.CloseScene(loadedCopy, true);
            }

            var root = new GameObject(rootName);
            build(root);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, path);
            if (!batchMode)
            {
                EditorSceneManager.CloseScene(scene, true);
                if (previous.IsValid() && previous.isLoaded)
                {
                    SceneManager.SetActiveScene(previous);
                }
            }
        }

        private static Transform BuildBase(Transform root, bool addHeroDecorations)
        {
            var canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(root, false);
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = false;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            // Portrait compromise: preserve enough width on tall phones and enough height on tablets.
            scaler.matchWidthOrHeight = 0.45f;

            var background = Panel(canvasObject.transform, "Background", Teal, Vector2.zero, Vector2.zero);
            Stretch(background.rectTransform, 0, 0, 0, 0);
            background.transform.SetAsFirstSibling();
            Blob(background.transform, "BlobTop", new Vector2(420, 760), 520, new Color(0.34f, 0.82f, 0.63f, 0.15f));
            Blob(background.transform, "BlobBottom", new Vector2(-430, -730), 680, new Color(1f, 0.65f, 0.22f, 0.10f));
            if (addHeroDecorations)
            {
                Blob(background.transform, "AppleGlow", new Vector2(-390, 420), 220, new Color(1f, 0.8f, 0.2f, 0.10f));
            }

            var safeObject = Node("SafeArea", canvasObject.transform);
            Stretch(safeObject.GetComponent<RectTransform>(), 0, 0, 0, 0);
            safeObject.AddComponent<SafeAreaPanel>();

            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            eventSystem.transform.SetParent(root, false);

            var cameraObject = new GameObject("UICamera", typeof(Camera));
            cameraObject.transform.SetParent(root, false);
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = DeepTeal;
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.cullingMask = 0;
            return safeObject.transform;
        }

        private static void Header(Transform parent, string title, string objectName = "ScreenTitle")
        {
            Text(parent, objectName, title, 39, FontStyle.Bold, Cream,
                new Vector2(0, -88), new Vector2(650, 80));
        }

        private static void CreateLogo(Transform parent, Vector2 position, float scale)
        {
            var logo = Node("WormLogo", parent);
            Place(logo.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), position, new Vector2(520, 220) * scale);
            for (var index = 0; index < 4; index++)
            {
                var segment = Panel(logo.transform, $"Segment_{index}", index == 3 ? Yellow : Orange,
                    new Vector2((-150 + index * 92) * scale, 0), new Vector2(132, 132) * scale, knobSprite);
                if (index == 3)
                {
                    Text(segment.transform, "Face", "••", Mathf.RoundToInt(38 * scale), FontStyle.Bold, Ink,
                        new Vector2(13, 16), new Vector2(100, 60));
                    Text(segment.transform, "Smile", "⌣", Mathf.RoundToInt(35 * scale), FontStyle.Bold, Ink,
                        new Vector2(18, -26), new Vector2(80, 45));
                }
            }
            var apple = Panel(logo.transform, "Apple", Red, new Vector2(210 * scale, -60 * scale),
                new Vector2(92, 92) * scale, knobSprite);
            apple.transform.SetAsFirstSibling();
        }

        private static void CreateCoinPill(Transform parent, Vector2 position)
        {
            var pill = Panel(parent, "CoinPill", new Color(1, 1, 1, 0.16f), position, new Vector2(250, 78));
            Text(pill.transform, "CoinIcon", "●", 34, FontStyle.Bold, Yellow,
                new Vector2(-72, 0), new Vector2(55, 55));
            Text(pill.transform, "CoinText", "0", 28, FontStyle.Bold, Cream,
                new Vector2(22, 0), new Vector2(110, 55));
        }

        private static void CreateDpad(Transform parent)
        {
            Button(parent, "MoveUpButton", "▲", new Color(1, 1, 1, 0.18f), Cream,
                new Vector2(0, -620), new Vector2(112, 100), 30);
            Button(parent, "MoveLeftButton", "◀", new Color(1, 1, 1, 0.18f), Cream,
                new Vector2(-120, -730), new Vector2(112, 100), 30);
            Button(parent, "MoveDownButton", "▼", new Color(1, 1, 1, 0.18f), Cream,
                new Vector2(0, -730), new Vector2(112, 100), 30);
            Button(parent, "MoveRightButton", "▶", new Color(1, 1, 1, 0.18f), Cream,
                new Vector2(120, -730), new Vector2(112, 100), 30);
        }

        private static GameObject CreateGameplayModal(
            Transform parent,
            string name,
            string title,
            string subtitle,
            ModalButton[] buttons)
        {
            var overlay = Panel(parent, name, new Color(0.02f, 0.08f, 0.08f, 0.78f), Vector2.zero, Vector2.zero);
            Stretch(overlay.rectTransform, 0, 0, 0, 0);
            var cardHeight = buttons.Length > 3 ? 900f : 760f;
            var card = Panel(overlay.transform, "ModalCard", Cream, Vector2.zero, new Vector2(790, cardHeight));
            Text(card.transform, "ModalTitle", title, 48, FontStyle.Bold, Ink,
                new Vector2(0, cardHeight * 0.5f - 120f), new Vector2(680, 90));
            Text(card.transform, "ModalSubtitle", subtitle, 23, FontStyle.Normal, MutedInk,
                new Vector2(0, cardHeight * 0.5f - 200f), new Vector2(650, 70));
            var startY = buttons.Length > 3 ? 55f : 25f;
            for (var index = 0; index < buttons.Length; index++)
            {
                Button(card.transform, buttons[index].Name, buttons[index].Label, buttons[index].Color,
                    buttons[index].Color == Orange ? Cream : Ink,
                    new Vector2(0, startY - index * 135), new Vector2(610, 104), 27);
            }
            overlay.gameObject.SetActive(false);
            return overlay.gameObject;
        }

        private static void CreateQuickSettingsModal(Transform parent)
        {
            var overlay = Panel(parent, "PauseSettingsOverlay", new Color(0.02f, 0.08f, 0.08f, 0.78f),
                Vector2.zero, Vector2.zero);
            Stretch(overlay.rectTransform, 0, 0, 0, 0);
            var card = Panel(overlay.transform, "QuickSettingsCard", Cream, Vector2.zero, new Vector2(850, 1040));
            Text(card.transform, "QuickSettingsTitle", "QUICK SETTINGS", 44, FontStyle.Bold, Ink,
                new Vector2(0, 430), new Vector2(700, 80));
            ToggleRow(card.transform, "PauseMusicToggle", "Music", "Background soundtrack", 285);
            ToggleRow(card.transform, "PauseSfxToggle", "Sound effects", "Moves, apples and hazards", 110);
            ToggleRow(card.transform, "PauseHapticsToggle", "Haptics", "Gentle vibration feedback", -65);
            ToggleRow(card.transform, "PauseReducedMotionToggle", "Reduced motion", "Shorter animations", -240);
            Button(card.transform, "PauseSettingsBackButton", "BACK", Orange, Cream,
                new Vector2(0, -420), new Vector2(610, 96), 27);
            overlay.gameObject.SetActive(false);
        }

        private static void ToggleRow(Transform parent, string objectName, string title, string subtitle, float y)
        {
            var row = Panel(parent, objectName + "Row", new Color32(239, 242, 229, 255),
                new Vector2(0, y), new Vector2(790, 155));
            Text(row.transform, objectName + "Title", title, 27, FontStyle.Bold, Ink,
                new Vector2(-205, 25), new Vector2(380, 50), TextAnchor.MiddleLeft);
            Text(row.transform, objectName + "Subtitle", subtitle, 19, FontStyle.Normal, MutedInk,
                new Vector2(-205, -25), new Vector2(380, 45), TextAnchor.MiddleLeft);

            var toggleObject = DefaultControls.CreateToggle(new DefaultControls.Resources
            {
                standard = uiSprite,
                background = uiSprite,
                checkmark = uiSprite,
                knob = knobSprite
            });
            toggleObject.name = objectName;
            toggleObject.transform.SetParent(row.transform, false);
            var toggleRect = toggleObject.GetComponent<RectTransform>();
            Place(toggleRect, new Vector2(0.5f, 0.5f), new Vector2(292, 0), new Vector2(105, 70));
            var defaultLabel = toggleObject.transform.Find("Label");
            if (defaultLabel != null)
            {
                UnityEngine.Object.DestroyImmediate(defaultLabel.gameObject);
            }
            var background = toggleObject.transform.Find("Background").GetComponent<Image>();
            background.color = new Color32(195, 205, 193, 255);
            background.type = Image.Type.Sliced;
            var checkmark = toggleObject.transform.Find("Background/Checkmark").GetComponent<Image>();
            checkmark.color = Orange;
        }

        private static void CreateSharedPrefabs()
        {
            var primary = Button(null, "PrimaryButton", "BUTTON", Orange, Cream, Vector2.zero, new Vector2(720, 112), 31);
            PrefabUtility.SaveAsPrefabAsset(primary.gameObject, PrefabsFolder + "/PrimaryButton.prefab");
            UnityEngine.Object.DestroyImmediate(primary.gameObject);

            var modal = Panel(null, "ModalCard", Cream, Vector2.zero, new Vector2(790, 760));
            Text(modal.transform, "Title", "MODAL TITLE", 48, FontStyle.Bold, Ink,
                new Vector2(0, 250), new Vector2(680, 90));
            Text(modal.transform, "Body", "Modal content", 24, FontStyle.Normal, MutedInk,
                new Vector2(0, 150), new Vector2(650, 100));
            PrefabUtility.SaveAsPrefabAsset(modal.gameObject, PrefabsFolder + "/ModalCard.prefab");
            UnityEngine.Object.DestroyImmediate(modal.gameObject);
        }

        private static GameObject Node(string name, Transform parent)
        {
            var node = new GameObject(name, typeof(RectTransform));
            if (parent != null)
            {
                node.transform.SetParent(parent, false);
            }
            return node;
        }

        private static Image Panel(
            Transform parent,
            string name,
            Color color,
            Vector2 position,
            Vector2 size,
            Sprite sprite = null)
        {
            var node = Node(name, parent);
            var image = node.AddComponent<Image>();
            image.sprite = sprite != null ? sprite : uiSprite;
            image.type = sprite == knobSprite ? Image.Type.Simple : Image.Type.Sliced;
            image.color = color;
            Place(image.rectTransform, new Vector2(0.5f, 0.5f), position, size);
            return image;
        }

        private static Text Text(
            Transform parent,
            string name,
            string value,
            int size,
            FontStyle style,
            Color color,
            Vector2 position,
            Vector2 dimensions,
            TextAnchor alignment = TextAnchor.MiddleCenter)
        {
            var node = Node(name, parent);
            var text = node.AddComponent<Text>();
            text.font = font;
            text.text = value;
            text.fontSize = size;
            text.fontStyle = style;
            text.alignment = alignment;
            text.color = color;
            text.raycastTarget = false;
            text.resizeTextForBestFit = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            Place(text.rectTransform, new Vector2(0.5f, 0.5f), position, dimensions);
            return text;
        }

        private static Button Button(
            Transform parent,
            string name,
            string label,
            Color background,
            Color foreground,
            Vector2 position,
            Vector2 size,
            int fontSize)
        {
            var resources = new DefaultControls.Resources { standard = uiSprite };
            var node = DefaultControls.CreateButton(resources);
            node.name = name;
            if (parent != null)
            {
                node.transform.SetParent(parent, false);
            }
            var rect = node.GetComponent<RectTransform>();
            Place(rect, new Vector2(0.5f, 0.5f), position, size);
            var image = node.GetComponent<Image>();
            image.sprite = uiSprite;
            image.type = Image.Type.Sliced;
            image.color = background;
            var button = node.GetComponent<Button>();
            var colors = button.colors;
            colors.normalColor = background;
            colors.highlightedColor = Color.Lerp(background, Color.white, 0.16f);
            colors.pressedColor = Color.Lerp(background, Color.black, 0.12f);
            colors.disabledColor = new Color(background.r, background.g, background.b, 0.48f);
            button.colors = colors;
            var text = node.GetComponentInChildren<Text>();
            text.font = font;
            text.text = label;
            text.fontSize = fontSize;
            text.fontStyle = FontStyle.Bold;
            text.color = foreground;
            text.alignment = TextAnchor.MiddleCenter;
            return button;
        }

        private static void Blob(Transform parent, string name, Vector2 position, float size, Color color)
        {
            Panel(parent, name, color, position, new Vector2(size, size), knobSprite).raycastTarget = false;
        }

        private static void Place(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            rect.localScale = Vector3.one;
        }

        private static void Stretch(RectTransform rect, float left, float bottom, float right, float top)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
        }

        private static void EnsureFolder(string parent, string child)
        {
            var path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }

        private readonly struct ModalButton
        {
            public ModalButton(string name, string label, Color color)
            {
                Name = name;
                Label = label;
                Color = color;
            }

            public string Name { get; }
            public string Label { get; }
            public Color Color { get; }
        }
    }
}
