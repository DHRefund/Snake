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
        private const string GeneratedVersion = "17";
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

                var logoSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/Content/Sprites/UI/Branding/logo_main.png");
                var appleSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/Content/Sprites/Items/apple_normal.png");
                var trackSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/Content/Sprites/UI/Progress/progress_track.png");
                var fillSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/Content/Sprites/UI/Progress/progress_fill.png");

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

                // With the logo the subtitle sits just under it; the text fallback title needs more room.
                var subtitleY = logoSprite != null ? 30f : -55f;
                Text(safe, "SubtitleText", "A CLEVER PUZZLE ADVENTURE", 28, FontStyle.Bold,
                    new Color32(220, 245, 235, 220), new Vector2(0, subtitleY), new Vector2(900, 50));

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

                Text(safe, "StatusText", "Loading your profile...", 28, FontStyle.Bold,
                    Cream, new Vector2(0, -375), new Vector2(760, 50));

                var retry = Button(safe, "RetryButton", "RETRY", Orange, Cream,
                    new Vector2(0, -490), new Vector2(360, 88), 26);
                retry.gameObject.SetActive(false);

                Text(safe, "VersionText", "V2.0 • OFFICIAL BUILD", 19, FontStyle.Bold,
                    new Color(1, 1, 1, 0.40f), new Vector2(0, -780), new Vector2(500, 40));
            });
        }

        private const string UiSpritesRoot = "Assets/Game/Content/Sprites/UI/";
        private static readonly Color LabelOutline = new Color32(96, 44, 10, 255);
        private static readonly Color WarmInk = new Color32(70, 40, 20, 255);

        private static void CreateHomeScene()
        {
            CreateScene(HomeScenePath, "HomeScreen", root =>
            {
                root.AddComponent<HomeScreenController>();
                root.AddComponent<ScreenNavigator>();
                var safe = BuildBase(root.transform, false);
                ApplyBackgroundArt(safe, "Backgrounds/bg_desert_day.png");

                // Top bar: anchored to the top edge so it follows the safe area on every aspect ratio.
                var settings = SpriteButton(safe, "SettingsButton", string.Empty, "Buttons/btn_SETTINGS.png",
                    Vector2.zero, new Vector2(124, 126), 0);
                AnchorTop(settings.GetComponent<RectTransform>(), -440f, 100f);
                var gear = Panel(settings.transform, "GearIcon", Color.white, Vector2.zero, new Vector2(64, 64),
                    LoadUiSprite("Icons/icon_settings.png"));
                gear.type = Image.Type.Simple;
                gear.raycastTarget = false;

                var pill = Panel(safe, "CoinPill", Color.white, Vector2.zero, new Vector2(300, 89),
                    LoadUiSprite("Currency/coin_pill.png"));
                pill.type = Image.Type.Simple;
                pill.raycastTarget = false;
                AnchorTop(pill.rectTransform, 360f, 100f);
                var coinIcon = Panel(pill.transform, "CoinIcon", Color.white, new Vector2(-92, 0), new Vector2(56, 56),
                    LoadUiSprite("Currency/coin.png"));
                coinIcon.type = Image.Type.Simple;
                coinIcon.raycastTarget = false;
                var coinText = Text(pill.transform, "CoinText", "0", 34, FontStyle.Bold, Cream,
                    new Vector2(20, 0), new Vector2(150, 60));
                AddLabelOutline(coinText);

                var logo = Panel(safe, "MainLogo", Color.white, new Vector2(0, 470), new Vector2(880, 379),
                    LoadUiSprite("Branding/logo_main.png"));
                logo.type = Image.Type.Simple;
                logo.preserveAspect = true;
                logo.raycastTarget = false;

                Text(safe, "HeroSubtitle", "Eat every apple. Find the exit.", 34, FontStyle.Bold, WarmInk,
                    new Vector2(0, 225), new Vector2(880, 60));

                SpriteButton(safe, "PlayButton", "PLAY", "Buttons/btn_level.png",
                    new Vector2(0, -20), new Vector2(640, 284), 84);
                SpriteButton(safe, "LevelsButton", "LEVELS", "Buttons/btn_select_level.png",
                    new Vector2(-170, -400), new Vector2(290, 291), 38);
                var cosmetics = SpriteButton(safe, "CosmeticsButton", "SKINS", "Buttons/btn_no_ads.png",
                    new Vector2(170, -400), new Vector2(290, 252), 38);
                var skinIcon = Panel(cosmetics.transform, "SkinIcon", Color.white, new Vector2(0, 35), new Vector2(92, 86),
                    LoadUiSprite("Icons/icon_skin.png"));
                skinIcon.type = Image.Type.Simple;
                skinIcon.raycastTarget = false;
                skinIcon.preserveAspect = true;
                MoveLabelDown(cosmetics, -52f);

                Text(safe, "FooterText", "Swipe or use arrow keys • Undo is always free", 26,
                    FontStyle.Bold, WarmInk, new Vector2(0, -820), new Vector2(940, 50));
            });
        }

        private static Sprite LoadUiSprite(string relativePath)
        {
            var path = UiSpritesRoot + relativePath;
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                Debug.LogWarning($"[Snake Apple V2] UI sprite not found or not imported as Sprite: {path}");
            }
            return sprite;
        }

        // Fullscreen art that covers the canvas at any aspect ratio without stretching.
        private static void ApplyBackgroundArt(Transform safe, string spritePath)
        {
            var sprite = LoadUiSprite(spritePath);
            var background = safe.parent.Find("Background");
            if (sprite == null || background == null) return;
            for (var index = 0; index < background.childCount; index++)
            {
                background.GetChild(index).gameObject.SetActive(false); // decorative blobs clash with art
            }

            var art = Node("BackgroundArt", background);
            art.transform.SetAsFirstSibling();
            var image = art.AddComponent<Image>();
            image.sprite = sprite;
            image.type = Image.Type.Simple;
            image.raycastTarget = false;
            var rect = image.rectTransform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(sprite.rect.width, sprite.rect.height);
            var fitter = art.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fitter.aspectRatio = sprite.rect.width / sprite.rect.height;
        }

        private static void AnchorTop(RectTransform rect, float x, float distanceFromTop)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(x, -distanceFromTop);
        }

        private static void AddLabelOutline(Text text)
        {
            var outline = text.gameObject.AddComponent<Outline>();
            outline.effectColor = LabelOutline;
            outline.effectDistance = new Vector2(3f, -3f);
        }

        private static void MoveLabelDown(Button button, float offsetY)
        {
            var label = button.GetComponentInChildren<Text>();
            if (label != null) label.rectTransform.anchoredPosition = new Vector2(0f, offsetY);
        }

        // Button drawn by a pre-made sprite (no tint-swapping background); the label is overlaid on top.
        private static Button SpriteButton(Transform parent, string name, string label, string spritePath,
            Vector2 position, Vector2 size, int fontSize)
        {
            var node = Node(name, parent);
            var image = node.AddComponent<Image>();
            image.sprite = LoadUiSprite(spritePath);
            image.type = Image.Type.Simple;
            image.preserveAspect = true;
            Place(image.rectTransform, new Vector2(0.5f, 0.5f), position, size);
            var button = node.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = Color.white;
            colors.pressedColor = new Color(0.78f, 0.78f, 0.78f, 1f);
            colors.disabledColor = new Color(1f, 1f, 1f, 0.5f);
            button.colors = colors;
            var text = Text(node.transform, "Label", label, fontSize, FontStyle.Bold, Cream,
                Vector2.zero, size);
            AddLabelOutline(text);
            return button;
        }

        private static void CreateLevelSelectScene()
        {
            CreateScene(LevelSelectScenePath, "LevelSelectScreen", root =>
            {
                root.AddComponent<LevelSelectScreenController>();
                root.AddComponent<ScreenNavigator>();
                var safe = BuildBase(root.transform, false);
                ApplyBackgroundArt(safe, "Backgrounds/bg_desert_day.png");

                // Header: anchored to the top edge.
                var title = Text(safe, "ScreenTitle", "CHOOSE A LEVEL", 60, FontStyle.Bold, Cream,
                    Vector2.zero, new Vector2(640, 90));
                AnchorTop(title.rectTransform, 40f, 100f);
                AddLabelOutline(title);
                var back = SpriteButton(safe, "BackButton", "‹", "Buttons/btn_CANCEL.png",
                    Vector2.zero, new Vector2(116, 118), 72);
                AnchorTop(back.GetComponent<RectTransform>(), -430f, 100f);

                // Mode tabs: one sprite, state shown by tint (see LevelSelectScreenController.RefreshTab).
                SpriteTab(safe, "NormalTab", "NORMAL", -330f);
                SpriteTab(safe, "HardTab", "HARD", 0f);
                SpriteTab(safe, "ChallengeTab", "CHALLENGE", 330f);

                // Level card: 960 x 1230, top edge 380 below the screen top.
                var card = Panel(safe, "LevelCard", Color.white, Vector2.zero, new Vector2(960, 1230),
                    LoadUiSprite("Panels/box_popup.png"));
                card.type = Image.Type.Sliced;
                card.raycastTarget = false;
                AnchorTop(card.rectTransform, 0f, 380f + 615f);

                var chapter = Text(card.transform, "ChapterText", "NORMAL  -  LEVELS 1-20", 38, FontStyle.Bold, Ink,
                    new Vector2(0, 520), new Vector2(860, 64));
                var subtext = Text(card.transform, "ChapterSubtext", "Complete levels to unlock the path ahead", 28,
                    FontStyle.Normal, MutedInk, new Vector2(0, 460), new Vector2(860, 50));
                chapter.color = WarmInk;
                subtext.color = new Color32(110, 80, 50, 255);

                var gridObject = Node("LevelGrid", card.transform);
                var gridRect = gridObject.GetComponent<RectTransform>();
                Place(gridRect, new Vector2(0.5f, 0.5f), new Vector2(0, 40), new Vector2(896, 712));
                var grid = gridObject.AddComponent<GridLayoutGroup>();
                grid.cellSize = new Vector2(160, 160);
                grid.spacing = new Vector2(24, 24);
                grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                grid.constraintCount = 5;
                grid.childAlignment = TextAnchor.UpperCenter;

                var available = LoadUiSprite("Buttons/btn_select_level.png");
                var completed = LoadUiSprite("Buttons/btn_clear_level.png");
                var locked = LoadUiSprite("Buttons/btn_lock_level.png");
                for (var level = 1; level <= 20; level++)
                {
                    var slot = SpriteButton(gridObject.transform, $"Level_{level:000}", level.ToString(),
                        "Buttons/btn_select_level.png", Vector2.zero, new Vector2(160, 160), 60);
                    var slotColors = slot.colors;
                    slotColors.disabledColor = Color.white; // locked look comes from the sprite, not alpha
                    slot.colors = slotColors;
                    var view = slot.gameObject.AddComponent<LevelSlotView>();
                    view.Configure(slot.GetComponent<Image>(), slot.GetComponentInChildren<Text>(),
                        available, completed, locked);
                    view.Apply(level == 1 ? LevelSlotState.Available : LevelSlotState.Locked, level);
                    slot.interactable = level == 1;
                }

                SpriteButton(card.transform, "PrevPageButton", "‹ PREV", "Buttons/btn_NEXT.png",
                    new Vector2(-320, -500), new Vector2(260, 116), 36);
                var pageText = Text(card.transform, "PageText", "PAGE 1 / 9", 30, FontStyle.Bold, WarmInk,
                    new Vector2(0, -500), new Vector2(280, 70));
                pageText.color = WarmInk;
                SpriteButton(card.transform, "NextPageButton", "NEXT ›", "Buttons/btn_level.png",
                    new Vector2(320, -500), new Vector2(260, 116), 36);
            });
        }

        private static void SpriteTab(Transform parent, string name, string label, float x, float top = 270f)
        {
            var tab = SpriteButton(parent, name, label, "Buttons/btn_NEXT.png",
                Vector2.zero, top > 270f ? new Vector2(300, 112) : new Vector2(310, 138), top > 270f ? 30 : 34);
            AnchorTop(tab.GetComponent<RectTransform>(), x, top);
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
                ApplyBackgroundArt(safe, "Backgrounds/bg_iceberg.png");
                // HUD: anchored to the top edge so it stays above the board on every aspect ratio.
                var levelText = Text(safe, "LevelText", "LEVEL 1", 56, FontStyle.Bold, Cream,
                    Vector2.zero, new Vector2(500, 90));
                AnchorTop(levelText.rectTransform, 0f, 100f);
                AddLabelOutline(levelText);
                var pause = SpriteButton(safe, "PauseButton", "II", "Buttons/btn_CANCEL.png",
                    Vector2.zero, new Vector2(116, 118), 56);
                AnchorTop(pause.GetComponent<RectTransform>(), -430f, 100f);

                // Exit-portal status icon replaces the apple counter: dim while apples remain (with a count
                // badge), bright once the exit is open. "ApplePill" keeps its name for the controller lookups.
                var exitRoot = Node("ApplePill", safe);
                Place(exitRoot.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(110, 122));
                AnchorTop(exitRoot.GetComponent<RectTransform>(), 440f, 105f);
                var exitFrame = Panel(exitRoot.transform, "ExitFrame", Color.white, Vector2.zero, new Vector2(104, 116),
                    AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/Content/Sprites/Environment/exit_portal_frame.png"));
                exitFrame.type = Image.Type.Simple;
                exitFrame.preserveAspect = true;
                exitFrame.raycastTarget = false;
                var exitPortal = Panel(exitRoot.transform, "ExitPortal", Color.white, new Vector2(0, -6), new Vector2(80, 80),
                    AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/Content/Sprites/Environment/exit_portal.png"));
                exitPortal.type = Image.Type.Simple;
                exitPortal.preserveAspect = true;
                exitPortal.raycastTarget = false;
                var badge = Panel(exitRoot.transform, "CountBadge", Color.white, new Vector2(40, -44), new Vector2(58, 58),
                    LoadUiSprite("Icons/badge_count.png"));
                badge.type = Image.Type.Simple;
                badge.raycastTarget = false;
                // The controller writes a glyph into AppleIcon; the portal sprite replaces it visually.
                var appleGlyph = Text(exitRoot.transform, "AppleIcon", "●", 30, FontStyle.Bold, Red,
                    Vector2.zero, new Vector2(40, 40));
                appleGlyph.enabled = false;
                var appleCount = Text(badge.transform, "AppleText", "1", 34, FontStyle.Bold, Cream,
                    new Vector2(0, 1), new Vector2(70, 56));
                AddLabelOutline(appleCount);
                exitRoot.AddComponent<ExitStatusView>().Configure(exitFrame, exitPortal, badge.gameObject);

                // Board sits directly on the themed background (no card). The invisible BoardCard keeps the
                // hierarchy the controller expects and must not swallow swipe input.
                var boardCard = Panel(safe, "BoardCard", new Color(1, 1, 1, 0), new Vector2(0, 170), new Vector2(1040, 940));
                boardCard.raycastTarget = false;
                var gridObject = Node("BoardGrid", boardCard.transform);
                var gridRect = gridObject.GetComponent<RectTransform>();
                Place(gridRect, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1040, 940));
                gridObject.AddComponent<GridLayoutGroup>();
                var snakeOverlay = Node("SnakeOverlay", boardCard.transform);
                var snakeOverlayRect = snakeOverlay.GetComponent<RectTransform>();
                Place(snakeOverlayRect, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1040, 940));
                snakeOverlay.transform.SetAsLastSibling();
                var feedback = Text(safe, "FeedbackText", "Guide the worm to the apple, then reach the exit.", 28,
                    FontStyle.Bold, Cream, new Vector2(0, -330), new Vector2(940, 70));
                AddLabelOutline(feedback);

                // Icon-only actions on the right edge, away from the D-pad: top = least reachable = Restart.
                IconButton(safe, "RestartButton", "Icons/icon_restart.png", new Vector2(455, -480));
                IconButton(safe, "HintButton", "Icons/icon_hint.png", new Vector2(455, -615));
                IconButton(safe, "UndoButton", "Icons/icon_undo.png", new Vector2(455, -750));
                CreateDpad(safe);

                CreatePausePopup(safe);
                CreateQuickSettingsPopup(safe);
                CreateConfirmPopup(safe);
                CreateWinPopup(safe);
                CreateLosePopup(safe);
            });
        }

        private static void CreateSettingsScene()
        {
            CreateScene(SettingsScenePath, "SettingsScreen", root =>
            {
                root.AddComponent<SettingsScreenController>();
                root.AddComponent<ScreenNavigator>();
                var safe = BuildBase(root.transform, false);
                ApplyBackgroundArt(safe, "Backgrounds/bg_desert_day.png");

                var title = Text(safe, "ScreenTitle", "SETTINGS", 60, FontStyle.Bold, Cream,
                    Vector2.zero, new Vector2(640, 90));
                AnchorTop(title.rectTransform, 40f, 100f);
                AddLabelOutline(title);
                var back = SpriteButton(safe, "BackButton", "‹", "Buttons/btn_CANCEL.png",
                    Vector2.zero, new Vector2(116, 118), 72);
                AnchorTop(back.GetComponent<RectTransform>(), -430f, 100f);

                // Card: 960 x 1150, top edge 300 below the screen top.
                var card = Panel(safe, "SettingsCard", Color.white, Vector2.zero, new Vector2(960, 1150),
                    LoadUiSprite("Panels/box_popup.png"));
                card.type = Image.Type.Sliced;
                card.raycastTarget = false;
                AnchorTop(card.rectTransform, 0f, 300f + 575f);

                SectionTitle(card.transform, "SoundTitle", "SOUND & FEEDBACK", 500);
                SpriteToggleRow(card.transform, "MusicToggle", "Music", "Background soundtrack", 380,
                    "Icons/icon_music_on.png", "Icons/icon_music_off.png");
                SpriteToggleRow(card.transform, "SfxToggle", "Sound effects", "Moves, apples and hazards", 180,
                    "Icons/icon_sound_on.png", "Icons/icon_sound_off.png");
                SpriteToggleRow(card.transform, "HapticsToggle", "Haptics", "Gentle vibration feedback", -20,
                    "Icons/icon_vibration_on.png", "Icons/icon_vibration_off.png");
                SectionTitle(card.transform, "AccessTitle", "ACCESSIBILITY", -160);
                SpriteToggleRow(card.transform, "ReducedMotionToggle", "Reduced motion",
                    "Shorter and simpler animations", -280, null, null);

                var privacy = Text(card.transform, "PrivacyText",
                    "Privacy options become available when advertising consent requires them.",
                    26, FontStyle.Normal, MutedInk, new Vector2(0, -460), new Vector2(820, 90));
                privacy.color = new Color32(110, 80, 50, 255);

                var build = Text(safe, "BuildText", "Snake & Apple: Puzzle Adventure • V2", 26, FontStyle.Bold,
                    WarmInk, Vector2.zero, new Vector2(800, 50));
                build.rectTransform.anchorMin = build.rectTransform.anchorMax = new Vector2(0.5f, 0f);
                build.rectTransform.anchoredPosition = new Vector2(0f, 70f);
            });
        }

        private static void SectionTitle(Transform parent, string name, string value, float y)
        {
            var text = Text(parent, name, value, 30, FontStyle.Bold, WarmInk,
                new Vector2(-40, y), new Vector2(760, 56), TextAnchor.MiddleLeft);
            text.color = new Color32(150, 100, 45, 255);
        }

        // Setting row: title + subtitle on the left, a sprite-drawn Toggle on the right.
        private static void SpriteToggleRow(Transform parent, string toggleName, string title, string subtitle,
            float y, string iconOnPath, string iconOffPath)
        {
            var row = Node(toggleName + "Row", parent);
            Place(row.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0, y), new Vector2(860, 170));

            var titleText = Text(row.transform, toggleName + "Title", title, 40, FontStyle.Bold, WarmInk,
                new Vector2(-130, 28), new Vector2(580, 60), TextAnchor.MiddleLeft);
            titleText.color = WarmInk;
            var sub = Text(row.transform, toggleName + "Subtitle", subtitle, 28, FontStyle.Normal, MutedInk,
                new Vector2(-130, -26), new Vector2(580, 50), TextAnchor.MiddleLeft);
            sub.color = new Color32(110, 80, 50, 255);

            var onSprite = LoadUiSprite("Buttons/btn_toggle_on.png");
            var offSprite = LoadUiSprite("Buttons/btn_toggle_off.png");
            var node = Node(toggleName, row.transform);
            var background = node.AddComponent<Image>();
            background.sprite = onSprite;
            background.type = Image.Type.Simple;
            background.preserveAspect = true;
            Place(background.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(340, 0), new Vector2(140, 141));
            var toggle = node.AddComponent<Toggle>();
            toggle.targetGraphic = background;
            toggle.isOn = true;
            var colors = toggle.colors;
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            toggle.colors = colors;

            Image icon = null;
            Text stateLabel = null;
            if (iconOnPath != null)
            {
                icon = Panel(node.transform, "Icon", Color.white, new Vector2(0, 4), new Vector2(76, 76),
                    LoadUiSprite(iconOnPath));
                icon.type = Image.Type.Simple;
                icon.preserveAspect = true;
                icon.raycastTarget = false;
            }
            else
            {
                stateLabel = Text(node.transform, "StateLabel", "ON", 38, FontStyle.Bold, Cream,
                    new Vector2(0, 4), new Vector2(130, 60));
                AddLabelOutline(stateLabel);
            }

            var view = node.AddComponent<SpriteToggleView>();
            view.Configure(background, onSprite, offSprite, icon,
                iconOnPath != null ? LoadUiSprite(iconOnPath) : null,
                iconOffPath != null ? LoadUiSprite(iconOffPath) : null, stateLabel);
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
                ApplyBackgroundArt(safe, "Backgrounds/bg_desert_day.png");

                var title = Text(safe, "ScreenTitle", "WORM CLOSET", 60, FontStyle.Bold, Cream,
                    Vector2.zero, new Vector2(560, 90));
                AnchorTop(title.rectTransform, 20f, 100f);
                AddLabelOutline(title);
                var back = SpriteButton(safe, "BackButton", "‹", "Buttons/btn_CANCEL.png",
                    Vector2.zero, new Vector2(116, 118), 72);
                AnchorTop(back.GetComponent<RectTransform>(), -430f, 100f);

                var pill = Panel(safe, "CoinPill", Color.white, Vector2.zero, new Vector2(300, 89),
                    LoadUiSprite("Currency/coin_pill.png"));
                pill.type = Image.Type.Simple;
                pill.raycastTarget = false;
                AnchorTop(pill.rectTransform, 360f, 205f);
                var coinIcon = Panel(pill.transform, "CoinIcon", Color.white, new Vector2(-92, 0), new Vector2(56, 56),
                    LoadUiSprite("Currency/coin.png"));
                coinIcon.type = Image.Type.Simple;
                coinIcon.raycastTarget = false;
                var coinText = Text(pill.transform, "CoinText", "0", 34, FontStyle.Bold, Cream,
                    new Vector2(20, 0), new Vector2(150, 60));
                AddLabelOutline(coinText);

                // Rewarded ad: +50 coins, capped per rolling 24 h (label is driven by CosmeticsScreenController).
                var watchAd = SpriteButton(safe, "WatchAdButton", "WATCH AD  +50", "Buttons/btn_GET_COIN.png",
                    Vector2.zero, new Vector2(470, 124), 32);
                AnchorTop(watchAd.GetComponent<RectTransform>(), -170f, 205f);

                SpriteTab(safe, "TabSkins", "SNAKES", -330f, 310f);
                SpriteTab(safe, "TabBlocks", "BLOCKS", 0f, 310f);
                SpriteTab(safe, "TabBackgrounds", "BACKDROPS", 330f, 310f);

                // Card: 980 x 1360, top edge 280 below the screen top.
                var card = Panel(safe, "ShopCard", Color.white, Vector2.zero, new Vector2(980, 1360),
                    LoadUiSprite("Panels/box_popup.png"));
                card.type = Image.Type.Sliced;
                card.raycastTarget = false;
                AnchorTop(card.rectTransform, 0f, 380f + 680f);

                var gridObject = Node("SkinGrid", card.transform);
                var gridRect = gridObject.GetComponent<RectTransform>();
                Place(gridRect, new Vector2(0.5f, 0.5f), new Vector2(0, 20), new Vector2(880, 1208));
                var grid = gridObject.AddComponent<GridLayoutGroup>();
                grid.cellSize = new Vector2(280, 290);
                grid.spacing = new Vector2(20, 16);
                grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                grid.constraintCount = 3;
                grid.childAlignment = TextAnchor.UpperCenter;

                for (var index = 0; index < 12; index++)
                {
                    CreateSkinCell(gridObject.transform, $"Skin_{index:00}");
                }

                var status = Text(card.transform, "ShopStatusText", "Choose a look for your worm.", 28, FontStyle.Bold,
                    WarmInk, new Vector2(0, -640), new Vector2(860, 56));
                status.color = WarmInk;
            });
        }

        private static void CreateSkinCell(Transform parent, string name)
        {
            var cardSprite = LoadUiSprite("Cosmetics/skin_card.png");
            var node = Node(name, parent);
            var background = node.AddComponent<Image>();
            background.sprite = cardSprite;
            background.type = Image.Type.Simple;
            Place(background.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(280, 290));
            var button = node.AddComponent<Button>();
            button.targetGraphic = background;
            var colors = button.colors;
            colors.pressedColor = new Color(0.82f, 0.82f, 0.82f, 1f);
            button.colors = colors;

            var preview = Panel(node.transform, "Preview", Color.white, new Vector2(0, 72), new Vector2(210, 137));
            preview.sprite = null;
            preview.type = Image.Type.Simple;
            preview.preserveAspect = true;
            preview.raycastTarget = false;

            var nameText = Text(node.transform, "SkinName", "Skin", 28, FontStyle.Bold, WarmInk,
                new Vector2(0, -22), new Vector2(264, 40));

            var face = Panel(node.transform, "ActionFace", Color.white, new Vector2(0, -98), new Vector2(200, 90),
                LoadUiSprite("Cosmetics/btn_skin_free.png"));
            face.type = Image.Type.Simple;
            face.raycastTarget = false;
            var coin = Panel(face.transform, "CoinIcon", Color.white, new Vector2(-62, 4), new Vector2(40, 40),
                LoadUiSprite("Currency/coin.png"));
            coin.type = Image.Type.Simple;
            coin.raycastTarget = false;
            var actionText = Text(face.transform, "ActionText", "USE", 34, FontStyle.Bold, Cream,
                new Vector2(0, 4), new Vector2(190, 60));
            AddLabelOutline(actionText);

            var frame = Panel(node.transform, "SelectedFrame", Color.white, Vector2.zero, new Vector2(296, 306),
                LoadUiSprite("Cosmetics/skin_card_selected.png"));
            frame.type = Image.Type.Simple;
            frame.raycastTarget = false;
            frame.gameObject.SetActive(false);
            var badge = Panel(node.transform, "SelectedBadge", Color.white, new Vector2(108, 112), new Vector2(60, 60),
                LoadUiSprite("Cosmetics/icon_skin_selected.png"));
            badge.type = Image.Type.Simple;
            badge.raycastTarget = false;
            badge.gameObject.SetActive(false);

            var view = node.AddComponent<SkinCellView>();
            view.Configure(preview, nameText, face, actionText, coin, frame.gameObject, badge.gameObject,
                LoadUiSprite("Cosmetics/btn_skin_free.png"), LoadUiSprite("Cosmetics/btn_skin_price.png"));
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

        private static void IconButton(Transform parent, string name, string iconPath, Vector2 position)
        {
            var button = SpriteButton(parent, name, string.Empty, "Buttons/btn_select_level.png", position,
                new Vector2(124, 124), 0);
            var icon = Panel(button.transform, "Icon", Color.white, new Vector2(0, 5), new Vector2(66, 66),
                LoadUiSprite(iconPath));
            icon.type = Image.Type.Simple;
            icon.preserveAspect = true;
            icon.raycastTarget = false;
        }

        private static void CreateDpad(Transform parent)
        {
            DpadButton(parent, "MoveUpButton", new Vector2(0, -500), 0f);
            DpadButton(parent, "MoveLeftButton", new Vector2(-165, -665), 90f);
            DpadButton(parent, "MoveDownButton", new Vector2(0, -665), 180f);
            DpadButton(parent, "MoveRightButton", new Vector2(165, -665), -90f);
        }

        private static void DpadButton(Transform parent, string name, Vector2 position, float arrowRotation)
        {
            var button = SpriteButton(parent, name, string.Empty, "Buttons/btn_MOVE.png", position,
                new Vector2(150, 161), 0);
            var arrow = Panel(button.transform, "Arrow", Color.white, new Vector2(0, 6), new Vector2(70, 48),
                LoadUiSprite("Icons/icon_dpad_arrow.png"));
            arrow.type = Image.Type.Simple;
            arrow.preserveAspect = true;
            arrow.raycastTarget = false;
            arrow.rectTransform.localRotation = Quaternion.Euler(0f, 0f, arrowRotation);
        }

        // ---- Gameplay popups (sprite style). Object names are the contract with GameplayScreenController. ----

        // Dimmed full-screen overlay + sprite card + ribbon title. Returns the card transform; the overlay starts hidden.
        private static Transform PopupShell(Transform parent, string overlayName, string cardSpritePath,
            Vector2 cardSize, string ribbonSpritePath, string title, int titleSize, float ribbonY)
        {
            var overlay = Panel(parent, overlayName, new Color(0.02f, 0.08f, 0.08f, 0.78f), Vector2.zero, Vector2.zero);
            Stretch(overlay.rectTransform, 0, 0, 0, 0);
            var card = Panel(overlay.transform, "ModalCard", Color.white, Vector2.zero, cardSize,
                LoadUiSprite(cardSpritePath));
            card.type = Image.Type.Sliced;
            // The card must swallow taps so the board underneath never receives them.
            card.raycastTarget = true;

            var ribbon = Panel(card.transform, "ModalRibbon", Color.white, new Vector2(0, ribbonY), new Vector2(660, 224),
                LoadUiSprite(ribbonSpritePath));
            ribbon.type = Image.Type.Simple;
            ribbon.preserveAspect = true;
            ribbon.raycastTarget = false;
            var titleText = Text(card.transform, "ModalTitle", title, titleSize, FontStyle.Bold, Cream,
                new Vector2(0, ribbonY + 4f), new Vector2(520, 100));
            AddLabelOutline(titleText);
            overlay.gameObject.SetActive(false);
            return card.transform;
        }

        private static Text PopupSubtitle(Transform card, string text, float y, float height = 90f)
        {
            var subtitle = Text(card, "ModalSubtitle", text, 30, FontStyle.Bold, WarmInk,
                new Vector2(0, y), new Vector2(700, height));
            subtitle.color = WarmInk;
            return subtitle;
        }

        // Round icon button with a small caption underneath (caption is outside the button hit area).
        private static void PopupIconButton(Transform card, string name, string iconPath, string caption, Vector2 position)
        {
            var button = SpriteButton(card, name, string.Empty, "Buttons/btn_select_level.png", position,
                new Vector2(150, 150), 0);
            var icon = Panel(button.transform, "Icon", Color.white, new Vector2(0, 5), new Vector2(80, 80),
                LoadUiSprite(iconPath));
            icon.type = Image.Type.Simple;
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            var label = Text(card, name + "Caption", caption, 28, FontStyle.Bold, WarmInk,
                position + new Vector2(0, -100), new Vector2(220, 44));
            label.color = WarmInk;
        }

        private static void CreatePausePopup(Transform safe)
        {
            var card = PopupShell(safe, "PauseOverlay", "Panels/box_popup.png", new Vector2(900, 900),
                "Ribbons/ribbon_WIN.png", "PAUSED", 56, 410f);
            PopupSubtitle(card, "Take a breath. The puzzle will wait.", 250f);
            SpriteButton(card, "PauseResumeButton", "RESUME", "Buttons/btn_NEXT.png", new Vector2(0, 90),
                new Vector2(520, 232), 50);
            PopupIconButton(card, "PauseSettingsButton", "Icons/icon_settings.png", "SETTINGS", new Vector2(-270, -250));
            PopupIconButton(card, "PauseRestartButton", "Icons/icon_restart.png", "RESTART", new Vector2(0, -250));
            PopupIconButton(card, "PauseHomeButton", "Icons/icon_home.png", "HOME", new Vector2(270, -250));
        }

        private static void CreateQuickSettingsPopup(Transform safe)
        {
            var card = PopupShell(safe, "PauseSettingsOverlay", "Panels/box_popup.png", new Vector2(960, 1180),
                "Ribbons/ribbon_WIN.png", "SETTINGS", 56, 520f);
            SpriteToggleRow(card, "PauseMusicToggle", "Music", "Background soundtrack", 290f,
                "Icons/icon_music_on.png", "Icons/icon_music_off.png");
            SpriteToggleRow(card, "PauseSfxToggle", "Sound effects", "Moves, apples and hazards", 115f,
                "Icons/icon_sound_on.png", "Icons/icon_sound_off.png");
            SpriteToggleRow(card, "PauseHapticsToggle", "Haptics", "Gentle vibration feedback", -60f,
                "Icons/icon_vibration_on.png", "Icons/icon_vibration_off.png");
            SpriteToggleRow(card, "PauseReducedMotionToggle", "Reduced motion", "Shorter animations", -235f,
                null, null);
            SpriteButton(card, "PauseSettingsBackButton", "BACK", "Buttons/btn_OK.png", new Vector2(0, -440),
                new Vector2(420, 187), 46);
        }

        private static void CreateConfirmPopup(Transform safe)
        {
            var card = PopupShell(safe, "ConfirmHomeOverlay", "Panels/box_popup.png", new Vector2(900, 760),
                "Ribbons/ribbon_LOSE.png", "LEAVE LEVEL?", 46, 330f);
            PopupSubtitle(card, "Current moves will be lost.", 170f);
            SpriteButton(card, "ConfirmStayButton", "KEEP PLAYING", "Buttons/btn_green.png", new Vector2(0, 20),
                new Vector2(480, 214), 38);
            SpriteButton(card, "ConfirmHomeButton", "LEAVE TO HOME", "Buttons/btn_OK.png", new Vector2(0, -215),
                new Vector2(460, 205), 36);
        }

        private static void CreateWinPopup(Transform safe)
        {
            var card = PopupShell(safe, "WinOverlay", "Panels/box_endgame_win.png", new Vector2(860, 1130),
                "Ribbons/ribbon_WIN.png", "LEVEL COMPLETE!", 46, 480f);
            // The worm peeks from behind the ribbon: draw it first so the ribbon covers its base.
            var worm = Panel(card, "WinWorm", Color.white, new Vector2(-250, 660), new Vector2(177, 185),
                LoadUiSprite("Characters/endgame_worm_win.png"));
            worm.type = Image.Type.Simple;
            worm.preserveAspect = true;
            worm.raycastTarget = false;
            worm.transform.SetAsFirstSibling();

            var moves = Text(card, "WinMoveText", "Solved in 0 moves", 38, FontStyle.Bold, WarmInk,
                new Vector2(0, 285), new Vector2(700, 60));
            moves.color = WarmInk;
            var reward = Text(card, "WinRewardText", "FIRST CLEAR  •  +0 COINS", 34, FontStyle.Bold,
                new Color32(181, 96, 14, 255), new Vector2(0, 225), new Vector2(740, 60));
            reward.color = new Color32(181, 96, 14, 255);

            SpriteButton(card, "WinNextButton", "NEXT LEVEL", "Buttons/btn_NEXT.png", new Vector2(0, 80),
                new Vector2(520, 232), 50);
            SpriteButton(card, "WinWatchAdButton", "WATCH AD  +50", "Buttons/btn_green.png", new Vector2(0, -160),
                new Vector2(500, 222), 38);
            SpriteButton(card, "WinLevelsButton", "LEVELS", "Buttons/btn_OK.png", new Vector2(-165, -370),
                new Vector2(300, 134), 36);
            SpriteButton(card, "WinHomeButton", "HOME", "Buttons/btn_OK.png", new Vector2(165, -370),
                new Vector2(300, 134), 36);
        }

        private static void CreateLosePopup(Transform safe)
        {
            var card = PopupShell(safe, "LoseOverlay", "Panels/box_endgame_lose.png", new Vector2(860, 860),
                "Ribbons/ribbon_LOSE.png", "TRY AGAIN", 54, 340f);
            PopupSubtitle(card, "The worm needs another route.", 170f, 110f);
            SpriteButton(card, "LoseUndoButton", "UNDO", "Buttons/btn_NEXT.png", new Vector2(0, -20),
                new Vector2(520, 232), 54);
            SpriteButton(card, "LoseRestartButton", "RESTART", "Buttons/btn_OK.png", new Vector2(-165, -250),
                new Vector2(300, 134), 34);
            SpriteButton(card, "LoseHomeButton", "HOME", "Buttons/btn_OK.png", new Vector2(165, -250),
                new Vector2(300, 134), 34);
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

    }
}
