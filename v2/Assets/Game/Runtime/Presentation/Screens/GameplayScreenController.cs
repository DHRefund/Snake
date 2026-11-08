using System.Collections;
using System.Collections.Generic;
using SnakeApple.Application.GameSession;
using SnakeApple.Domain.Board;
using SnakeApple.Domain.Commands;
using SnakeApple.Domain.Entities;
using SnakeApple.Domain.Rules;
using SnakeApple.Infrastructure.Levels;
using SnakeApple.Infrastructure.Save;
using SnakeApple.Presentation.Ads;
using SnakeApple.Presentation.Audio;
using SnakeApple.Presentation.Navigation;
using SnakeApple.Presentation.Blocks;
using SnakeApple.Presentation.Cosmetics;
using SnakeApple.Presentation.Items;
using SnakeApple.Presentation.Snake;
using SnakeApple.Presentation.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SnakeApple.Presentation.Screens
{
    public sealed class GameplayScreenController : MonoBehaviour
    {
        private const string DefaultSnakeSkinResource = "SnakeSkins/LegacyV1SnakeSkin";
        private const string DefaultAutotileBlockSkinResource = "BlockSkins/IceBlockSkin";
        private const string DefaultBlockSkinResource = "BlockSkins/LegacyDefaultBlockSkin";
        // Items sit slightly above the tile centre so they line up with the blocks' raised top faces.
        private const float ItemLift = 0.08f;
        private const string DefaultItemSkinResource = "ItemSkins/DefaultItemSkin";
        private static readonly Color FloorColor = Color.clear;
        private static readonly Color WallColor = new Color32(72, 89, 91, 255);
        private static readonly Color ExitClosedColor = new Color32(104, 124, 113, 255);
        private static readonly Color ExitOpenColor = new Color32(51, 190, 118, 255);
        private static readonly Color AppleColor = new Color32(235, 73, 65, 255);
        private static readonly Color VipAppleColor = new Color32(255, 193, 48, 255);
        private static readonly Color HeadColor = new Color32(255, 178, 39, 255);
        private static readonly Color BodyColor = new Color32(255, 125, 37, 255);
        private static readonly Color TailColor = new Color32(239, 96, 34, 255);
        private static readonly Color RockColor = new Color32(112, 126, 132, 255);
        private static readonly Color HazardColor = new Color32(224, 72, 66, 255);
        private static readonly Color PortalColor = new Color32(151, 92, 220, 255);
        private static readonly Color ButtonColor = new Color32(246, 197, 57, 255);
        private static readonly Color DoorColor = new Color32(139, 91, 52, 255);
        private static readonly Color LaserColor = new Color32(255, 78, 111, 255);
        private static readonly Color WebColor = new Color32(187, 211, 215, 255);

        private readonly Dictionary<GridPos, Image> cellImages = new Dictionary<GridPos, Image>();
        private readonly Dictionary<GridPos, Text> cellLabels = new Dictionary<GridPos, Text>();
        private readonly Dictionary<GridPos, RectTransform> cellRects = new Dictionary<GridPos, RectTransform>();
        private readonly Dictionary<GridPos, BoardAppleView> appleViews = new Dictionary<GridPos, BoardAppleView>();
        private readonly Dictionary<GridPos, RectTransform> itemVisuals = new Dictionary<GridPos, RectTransform>();
        private RectTransform itemOverlay;
        private RectTransform portalFrontOverlay;
        // Teleporter cell -> side of the cell the hidden pocket is on (-1 left, +1 right). Used to clip the snake.
        private readonly Dictionary<GridPos, float> portalSides = new Dictionary<GridPos, float>();
        private readonly Dictionary<GridPos, RectTransform> portalFronts = new Dictionary<GridPos, RectTransform>();
        private readonly List<Direction> acceptedMoves = new List<Direction>();

        [Header("Legacy V1 snake demo")]
        [SerializeField] private SnakeSpriteSkin legacySnakeSkin;

        [Header("Production Autotile block skin")]
        [SerializeField] private AutotileBlockSkin autotileBlockSkin;

        [Header("Legacy V1 default blocks")]
        [SerializeField] private BlockSpriteSkin legacyBlockSkin;

        [Header("Production item / object skin")]
        [SerializeField] private GameplayItemSkin itemSkin;

        private ScreenNavigator navigator;
        private PuzzleGameSession session;
        private LegacyLevelRecord levelRecord;
        private RectTransform boardGrid;
        private RectTransform snakeOverlay;
        private Text levelText;
        private Text appleText;
        private Text appleIconText;
        private ExitStatusView exitStatus;
        private Text feedbackText;
        private Text winRewardText;
        private Button winWatchAdButton;
        private Text winWatchAdLabel;
        private RewardedAdController rewardedAd;
        private bool winAdInProgress;
        private Text loseSubtitleText;
        private GameObject pauseOverlay;
        private GameObject pauseSettingsOverlay;
        private GameObject confirmHomeOverlay;
        private GameObject winOverlay;
        private GameObject loseOverlay;
        private Button undoButton;
        private Button loseUndoButton;
        private Vector2 pointerDown;
        private bool pointerTracking;
        private bool completionCommitted;
        private Coroutine activeMoveRoutine;
        private bool isAnimating;
        private Direction? queuedDirection;
        private readonly List<GameObject> snakeSegments = new List<GameObject>();

        private void Awake()
        {
            if (GetComponent<GameplayBanner>() == null) gameObject.AddComponent<GameplayBanner>();
            EnsureSnakeSkin();
            EnsureBlockSkin();
            EnsureItemSkin();
            ApplySelectedBackground();
            navigator = GetComponent<ScreenNavigator>();
            boardGrid = UiLookup.Find<RectTransform>(transform, "BoardGrid");
            snakeOverlay = UiLookup.Find<RectTransform>(transform, "SnakeOverlay");
            EnsureItemOverlay();
            levelText = UiLookup.Find<Text>(transform, "LevelText");
            appleText = UiLookup.Find<Text>(transform, "AppleText");
            appleIconText = UiLookup.Find<Text>(transform, "AppleIcon");
            exitStatus = GetComponentInChildren<ExitStatusView>(true);
            feedbackText = UiLookup.Find<Text>(transform, "FeedbackText");
            pauseOverlay = UiLookup.FindObject(transform, "PauseOverlay");
            pauseSettingsOverlay = UiLookup.FindObject(transform, "PauseSettingsOverlay");
            confirmHomeOverlay = UiLookup.FindObject(transform, "ConfirmHomeOverlay");
            winOverlay = UiLookup.FindObject(transform, "WinOverlay");
            loseOverlay = UiLookup.FindObject(transform, "LoseOverlay");
            winRewardText = UiLookup.Find<Text>(winOverlay.transform, "WinRewardText");
            loseSubtitleText = UiLookup.Find<Text>(loseOverlay.transform, "ModalSubtitle");
            undoButton = UiLookup.Find<Button>(transform, "UndoButton");
            loseUndoButton = UiLookup.Find<Button>(loseOverlay.transform, "LoseUndoButton");

            BindButton("PauseButton", ShowPause);
            BindButton("UndoButton", Undo);
            BindButton("RestartButton", Restart);
            BindButton("HintButton", ShowHint);
            BindButton("MoveUpButton", () => TryMove(Direction.Up));
            BindButton("MoveDownButton", () => TryMove(Direction.Down));
            BindButton("MoveLeftButton", () => TryMove(Direction.Left));
            BindButton("MoveRightButton", () => TryMove(Direction.Right));
            BindButton("PauseResumeButton", HidePause);
            BindButton("PauseSettingsButton", ShowPauseSettings);
            BindButton("PauseRestartButton", RestartFromPause);
            BindButton("PauseHomeButton", ShowHomeConfirmation);
            BindButton("PauseSettingsBackButton", HidePauseSettings);
            BindButton("ConfirmStayButton", HideHomeConfirmation);
            BindButton("ConfirmHomeButton", () => navigator.Load(ScreenNames.Home));
            BindButton("WinNextButton", NextLevel);
            BindButton("WinLevelsButton", () => navigator.Load(ScreenNames.LevelSelect));
            BindButton("WinHomeButton", () => navigator.Load(ScreenNames.Home));
            winWatchAdButton = UiLookup.Find<Button>(transform, "WinWatchAdButton");
            winWatchAdLabel = winWatchAdButton.GetComponentInChildren<Text>();
            winWatchAdButton.onClick.AddListener(WatchAdFromWin);
            rewardedAd = GetComponent<RewardedAdController>();
            if (rewardedAd == null) rewardedAd = gameObject.AddComponent<RewardedAdController>();
            BindButton("LoseUndoButton", UndoFromLose);
            BindButton("LoseRestartButton", RestartFromLose);
            BindButton("LoseHomeButton", () => navigator.Load(ScreenNames.Home));

            BindQuickSetting("PauseMusicToggle", LocalProfileStore.Music, value => LocalProfileStore.Music = value);
            BindQuickSetting("PauseSfxToggle", LocalProfileStore.Sfx, value => LocalProfileStore.Sfx = value);
            BindQuickSetting("PauseHapticsToggle", LocalProfileStore.Haptics, value => LocalProfileStore.Haptics = value);
            BindQuickSetting("PauseReducedMotionToggle", LocalProfileStore.ReducedMotion,
                value => LocalProfileStore.ReducedMotion = value);

            pauseOverlay.SetActive(false);
            pauseSettingsOverlay.SetActive(false);
            confirmHomeOverlay.SetActive(false);
            winOverlay.SetActive(false);
            loseOverlay.SetActive(false);
        }

        private void OnEnable()
        {
            // Also runs after a Unity domain reload, when an already-open scene can lose a newly
            // introduced serialized reference until it is reloaded from disk.
            EnsureSnakeSkin();
            EnsureBlockSkin();
            EnsureItemSkin();
        }

        private void ApplySelectedBackground()
        {
            var themes = ThemeCatalog.Load();
            var selected = LocalProfileStore.SelectedBackground;
            var background = themes != null ? themes.FindBackground(selected) : null;
            if (background == null || !background.IsAvailable || !LocalProfileStore.IsBackgroundUnlocked(selected)) return;

            var art = UiLookup.FindOptional<Image>(transform, "BackgroundArt");
            if (art == null) return;
            art.sprite = background.art;
            var fitter = art.GetComponent<AspectRatioFitter>();
            if (fitter != null) fitter.aspectRatio = background.art.rect.width / background.art.rect.height;
            art.rectTransform.sizeDelta = new Vector2(background.art.rect.width, background.art.rect.height);
        }

        private void EnsureItemSkin()
        {
            if (itemSkin != null && itemSkin.IsConfigured) return;

            itemSkin = Resources.Load<GameplayItemSkin>(DefaultItemSkinResource);
            if (itemSkin == null)
            {
                Debug.LogWarning($"Item skin resource was not found at {DefaultItemSkinResource}.", this);
            }
        }

        private void EnsureSnakeSkin()
        {
            var selectedSkin = LocalProfileStore.SelectedSkin;
            var catalog = SnakeSkinCatalog.Load();
            var loadedSkin = catalog != null ? catalog.SkinAt(selectedSkin) : null;
            if (loadedSkin == null)
            {
                // Saved index no longer exists (or catalog missing): fall back to the first catalog skin.
                loadedSkin = catalog != null ? catalog.SkinAt(0) : null;
            }

            if (loadedSkin != null && loadedSkin.IsConfigured)
            {
                legacySnakeSkin = loadedSkin;
                return;
            }

            if (legacySnakeSkin != null && legacySnakeSkin.IsConfigured) return;

            legacySnakeSkin = Resources.Load<SnakeSpriteSkin>(DefaultSnakeSkinResource);
            if (legacySnakeSkin == null)
            {
                Debug.LogError($"Snake skin resource was not found at {DefaultSnakeSkinResource}.", this);
            }
            else if (!legacySnakeSkin.IsConfigured)
            {
                Debug.LogError(
                    $"Snake skin {legacySnakeSkin.name} is incomplete (phases: {legacySnakeSkin.PhaseCount}).",
                    legacySnakeSkin);
            }
        }

        private void EnsureBlockSkin()
        {
            var themes = ThemeCatalog.Load();
            var selectedTheme = LocalProfileStore.SelectedBlockTheme;
            var theme = themes != null ? themes.FindBlock(selectedTheme) : null;
            if (theme != null && theme.IsAvailable && LocalProfileStore.IsBlockThemeUnlocked(selectedTheme))
            {
                autotileBlockSkin = theme.skin;
            }

            if (autotileBlockSkin == null || !autotileBlockSkin.IsConfigured)
            {
                autotileBlockSkin = Resources.Load<AutotileBlockSkin>(DefaultAutotileBlockSkinResource);
            }

            if (autotileBlockSkin != null && autotileBlockSkin.IsConfigured) return;

            if (legacyBlockSkin != null && legacyBlockSkin.IsConfigured) return;

            legacyBlockSkin = Resources.Load<BlockSpriteSkin>(DefaultBlockSkinResource);
            if (legacyBlockSkin == null)
            {
                Debug.LogError($"Block skin resource was not found at {DefaultBlockSkinResource}.", this);
            }
            else if (!legacyBlockSkin.IsConfigured)
            {
                Debug.LogError($"Block skin {legacyBlockSkin.name} is incomplete.", legacyBlockSkin);
            }
        }

        private void Start()
        {
            LoadSelectedLevel();
            AudioManager.RescanWidgets();
        }

        private void WatchAdFromWin()
        {
            if (winAdInProgress) return;
            winAdInProgress = rewardedAd.TryShow(committed =>
            {
                winAdInProgress = false;
                winRewardText.text = committed
                    ? $"+{RewardLedger.RewardCoins} BONUS COINS!"
                    : "NO REWARD THIS TIME";
            });
        }

        private void RefreshWinAdButton()
        {
            var remaining = RewardLedger.RemainingGrants(RewardLedger.NowUnix);
            winWatchAdButton.gameObject.SetActive(remaining > 0);
            winWatchAdLabel.text = winAdInProgress ? "WATCHING..." : $"WATCH AD  +{RewardLedger.RewardCoins}";
            winWatchAdButton.interactable = !winAdInProgress && rewardedAd.IsReady;
        }

        private void Update()
        {
            if (winOverlay.activeSelf) RefreshWinAdButton();
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                HandleBack();
                return;
            }

            if (session == null || IsModalOpen()) return;

            if (Input.GetKeyDown(KeyCode.Z))
            {
                Undo();
                return;
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                Restart();
                return;
            }

            if (TryReadKeyboardDirection(out var direction)) TryMove(direction);
            ReadSwipe();
        }

        private void LoadSelectedLevel()
        {
            CancelActiveAnimation();
            cellRects.Clear();
            var catalog = LegacyLevelCatalogLoader.Load();
            var mode = LevelProgressStore.SelectedMode;
            var number = LevelProgressStore.SelectedLevel;
            levelRecord = catalog.Find(mode, number);
            if (levelRecord == null)
            {
                feedbackText.text = $"Missing {mode} level {number}.";
                return;
            }

            session = new PuzzleGameSession(LegacyLevelAdapter.ToDefinition(levelRecord), new PuzzleRules());
            acceptedMoves.Clear();
            completionCommitted = false;
            pauseOverlay.SetActive(false);
            pauseSettingsOverlay.SetActive(false);
            confirmHomeOverlay.SetActive(false);
            winOverlay.SetActive(false);
            loseOverlay.SetActive(false);
            levelText.text = $"{mode.ToUpperInvariant()}  {number}";
            RefreshBoard(IsChallengeMode()
                ? "Reach the exit. Apples are optional in Challenge."
                : "Eat every apple, then enter the exit.");
        }

        private void BuildBoard(PuzzleState state)
        {
            for (var index = boardGrid.childCount - 1; index >= 0; index--)
            {
                // Destroy is deferred until end-of-frame. Disable first so GridLayoutGroup does
                // not include stale cells when this same frame builds the replacement board.
                var child = boardGrid.GetChild(index).gameObject;
                child.SetActive(false);
                Destroy(child);
            }

            ClearAppleViews();
            cellImages.Clear();
            cellLabels.Clear();
            cellRects.Clear();

            for (var index = snakeSegments.Count - 1; index >= 0; index--)
            {
                if (snakeSegments[index] != null)
                {
                    Destroy(snakeSegments[index]);
                }
            }
            snakeSegments.Clear();

            for (var index = snakeOverlay.childCount - 1; index >= 0; index--)
            {
                var child = snakeOverlay.GetChild(index).gameObject;
                child.SetActive(false);
                Destroy(child);
            }
            var minX = state.Size.Width;
            var minY = state.Size.Height;
            var maxX = 0;
            var maxY = 0;
            for (var y = 0; y < state.Size.Height; y++)
            {
                for (var x = 0; x < state.Size.Width; x++)
                {
                    var position = new GridPos(x, y);
                    if (state.CellAt(position) == '0') continue;
                    minX = Mathf.Min(minX, x);
                    maxX = Mathf.Max(maxX, x);
                    minY = Mathf.Min(minY, y);
                    maxY = Mathf.Max(maxY, y);
                }
            }

            foreach (var segment in state.Snake)
            {
                minX = Mathf.Min(minX, segment.X);
                maxX = Mathf.Max(maxX, segment.X);
                minY = Mathf.Min(minY, segment.Y);
                maxY = Mathf.Max(maxY, segment.Y);
            }

            foreach (var movable in state.Movables)
            {
                minX = Mathf.Min(minX, movable.Key.X);
                maxX = Mathf.Max(maxX, movable.Key.X);
                minY = Mathf.Min(minY, movable.Key.Y);
                maxY = Mathf.Max(maxY, movable.Key.Y);
            }

            foreach (var platform in state.SlidePlatforms)
            {
                minX = Mathf.Min(minX, Mathf.Min(platform.First.X, platform.Second.X));
                maxX = Mathf.Max(maxX, Mathf.Max(platform.First.X, platform.Second.X));
                minY = Mathf.Min(minY, Mathf.Min(platform.First.Y, platform.Second.Y));
                maxY = Mathf.Max(maxY, Mathf.Max(platform.First.Y, platform.Second.Y));
            }

            minX = Mathf.Max(0, minX - 1);
            maxX = Mathf.Min(state.Size.Width - 1, maxX + 1);
            minY = Mathf.Max(0, minY - 1);
            maxY = Mathf.Min(state.Size.Height - 1, maxY + 1);
            var columns = Mathf.Max(1, maxX - minX + 1);
            var rows = Mathf.Max(1, maxY - minY + 1);
            var layout = boardGrid.GetComponent<GridLayoutGroup>();
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = columns;
            layout.startCorner = GridLayoutGroup.Corner.LowerLeft;
            // V1 wall sprites are modular tiles whose edges meet exactly at the grid boundary.
            layout.spacing = Vector2.zero;
            var availableWidth = boardGrid.rect.width > 0f ? boardGrid.rect.width : 820f;
            var availableHeight = boardGrid.rect.height > 0f ? boardGrid.rect.height : 640f;
            var cellSize = Mathf.Min(
                availableWidth / columns,
                availableHeight / rows);
            cellSize = Mathf.Max(8f, cellSize);
            layout.cellSize = new Vector2(cellSize, cellSize);
            layout.childAlignment = TextAnchor.MiddleCenter;

            for (var y = minY; y <= maxY; y++)
            {
                for (var x = minX; x <= maxX; x++)
                {
                    var position = new GridPos(x, y);
                    var cellObject = new GameObject($"Cell_{x}_{y}", typeof(RectTransform), typeof(Image));
                    cellObject.transform.SetParent(boardGrid, false);
                    var image = cellObject.GetComponent<Image>();
                    image.raycastTarget = false;
                    var labelObject = new GameObject("Label", typeof(RectTransform), typeof(Text));
                    labelObject.transform.SetParent(cellObject.transform, false);
                    var labelRect = (RectTransform)labelObject.transform;
                    labelRect.anchorMin = Vector2.zero;
                    labelRect.anchorMax = Vector2.one;
                    labelRect.offsetMin = Vector2.zero;
                    labelRect.offsetMax = Vector2.zero;
                    var label = labelObject.GetComponent<Text>();
                    label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    label.fontSize = Mathf.Clamp(Mathf.RoundToInt(cellSize * 0.22f), 9, 20);
                    label.fontStyle = FontStyle.Bold;
                    label.alignment = TextAnchor.MiddleCenter;
                    label.color = Color.white;
                    label.raycastTarget = false;
                    cellImages[position] = image;
                    cellLabels[position] = label;
                    cellRects[position] = (RectTransform)cellObject.transform;
                }
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(boardGrid);
        }

        private void RefreshBoard(string feedback)
        {
            var state = session.CurrentState;
            if (cellRects.Count == 0 || boardGrid.childCount == 0)
            {
                BuildBoard(state);
            }
            foreach (var entry in cellImages)
            {
                var pos = entry.Key;
                if (state.Movables.ContainsKey(pos) || IsSlidePlatformPosition(state, pos))
                {
                    entry.Value.sprite = null;
                    entry.Value.color = FloorColor;
                    entry.Value.rectTransform.localEulerAngles = Vector3.zero;
                    cellLabels[pos].text = string.Empty;
                    continue;
                }

                PaintStaticCell(state, pos);
            }

            SyncAppleViews(state);

            foreach (var platform in state.SlidePlatforms)
            {
                SetCellSprite(platform.Current, itemSkin != null ? itemSkin.SlidePlatform : null, Color.white, "BAR");
            }

            foreach (var movable in state.Movables)
            {
                var sprite = movable.Value == MovableKind.Bomb
                    ? (itemSkin != null ? itemSkin.Bomb : null)
                    : (itemSkin != null ? itemSkin.Rock : null);
                var fallbackColor = movable.Value == MovableKind.Bomb ? LaserColor : RockColor;
                var fallbackLabel = movable.Value == MovableKind.Bomb ? "BOMB" : "ROCK";
                SetCellSprite(movable.Key, sprite, Color.white, fallbackLabel);
            }

            PaintSnake(state);

            appleIconText.text = IsChallengeMode() ? "◇" : "●";
            appleText.text = IsChallengeMode() ? "EXIT" : state.RemainingApples.ToString();
            if (exitStatus != null) exitStatus.Apply(state.RemainingApples, IsChallengeMode());
            undoButton.interactable = session.CanUndo;
            loseUndoButton.interactable = session.CanUndo;
            feedbackText.text = feedback;
        }

        // Apples are drawn in their own overlay (above walls, below the snake) because wall sprites spill into
        // the neighbouring cells and would otherwise cover them.
        private void EnsureItemOverlay()
        {
            if (itemOverlay != null) return;
            var node = new GameObject("ItemOverlay", typeof(RectTransform));
            itemOverlay = (RectTransform)node.transform;
            itemOverlay.SetParent(boardGrid.parent, false);
            itemOverlay.anchorMin = snakeOverlay.anchorMin;
            itemOverlay.anchorMax = snakeOverlay.anchorMax;
            itemOverlay.pivot = snakeOverlay.pivot;
            itemOverlay.anchoredPosition = snakeOverlay.anchoredPosition;
            itemOverlay.sizeDelta = snakeOverlay.sizeDelta;
            itemOverlay.SetSiblingIndex(boardGrid.GetSiblingIndex() + 1);

            // Teleporter rims are drawn in front of the snake so the body looks like it passes through the gate.
            var front = new GameObject("PortalFrontOverlay", typeof(RectTransform));
            portalFrontOverlay = (RectTransform)front.transform;
            portalFrontOverlay.SetParent(snakeOverlay.parent, false);
            portalFrontOverlay.anchorMin = snakeOverlay.anchorMin;
            portalFrontOverlay.anchorMax = snakeOverlay.anchorMax;
            portalFrontOverlay.pivot = snakeOverlay.pivot;
            portalFrontOverlay.anchoredPosition = snakeOverlay.anchoredPosition;
            portalFrontOverlay.sizeDelta = snakeOverlay.sizeDelta;
            portalFrontOverlay.SetSiblingIndex(snakeOverlay.GetSiblingIndex() + 1);
        }

        private void PaintApple(PuzzleState state, GridPos position, bool vip)
        {
            var sprite = itemSkin != null ? (vip ? itemSkin.AppleVip : itemSkin.AppleNormal) : null;
            if (sprite == null || itemOverlay == null)
            {
                SetCellSprite(position, sprite, Color.white, vip ? "VIP" : "APPLE");
                return;
            }

            SetCell(position, Color.clear, string.Empty);
            if (!cellRects.TryGetValue(position, out var cellRect)) return;

            if (!appleViews.TryGetValue(position, out var view) || view == null)
            {
                view = BoardAppleView.Create(itemOverlay, position.X * 31 + position.Y * 17, sprite, vip, itemSkin);
                appleViews[position] = view;
                view.PlayAppear();
            }

            var restsOnWall = IsWallAt(state, new GridPos(position.X, position.Y - 1));
            var wallOnRight = IsWallAt(state, new GridPos(position.X + 1, position.Y));
            view.Place(itemOverlay.InverseTransformPoint(cellRect.position), GetCellSize(), restsOnWall, wallOnRight);
        }

        // An apple that is no longer in the state was eaten: play its pop effect instead of vanishing.
        private void SyncAppleViews(PuzzleState state)
        {
            List<GridPos> eaten = null;
            foreach (var pair in appleViews)
            {
                var cell = state.CellAt(pair.Key);
                if (cell == '2' || cell == '8') continue;
                if (eaten == null) eaten = new List<GridPos>();
                eaten.Add(pair.Key);
            }

            if (eaten == null) return;
            foreach (var position in eaten)
            {
                if (appleViews[position] != null) appleViews[position].PlayEat();
                appleViews.Remove(position);
            }
        }

        private void ClearAppleViews()
        {
            foreach (var view in appleViews.Values)
            {
                if (view != null) Destroy(view.gameObject);
            }

            appleViews.Clear();

            foreach (var visual in itemVisuals.Values)
            {
                if (visual != null) Destroy(visual.gameObject);
            }

            itemVisuals.Clear();

            foreach (var visual in portalFronts.Values)
            {
                if (visual != null) Destroy(visual.gameObject);
            }

            portalFronts.Clear();
            portalSides.Clear();
        }

        private void PaintStaticCell(PuzzleState state, GridPos position)
        {
            var value = state.CellAt(position);
            if (IsWallCell(value))
            {
                if (autotileBlockSkin != null && autotileBlockSkin.IsConfigured)
                {
                    PaintAutotileWall(state, position);
                }
                else
                {
                    PaintLegacyWall(state, position);
                }
            }
            else if (value == '2')
            {
                PaintApple(state, position, false);
            }
            else if (value == '8')
            {
                PaintApple(state, position, true);
            }
            else if (value == 'X')
            {
                var available = IsExitAvailable(state);
                var tint = available ? Color.white : new Color(0.5f, 0.5f, 0.55f, 0.9f);
                SetCellSprite(position, itemSkin != null ? itemSkin.ExitPortal : null, tint, available ? "EXIT" : "LOCK",
                    0f, 1f, ItemLift);
            }
            else if ("ghij".IndexOf(value) >= 0)
            {
                var rot = value == 'g' ? 0f : value == 'h' ? 270f : value == 'i' ? 180f : 90f;
                SetCellSprite(position, itemSkin != null ? itemSkin.Spike : null, Color.white, "SPIKE", rot);
            }
            else if (value == '6')
            {
                SetCellSprite(position, itemSkin != null ? itemSkin.ToxicAcid : null, Color.white, "TOX");
            }
            else if (value == 'A')
            {
                SetCellSprite(position, itemSkin != null ? itemSkin.SpiderWeb : null, Color.white, "WEB");
            }
            else if ("cdefCDEF".IndexOf(value) >= 0)
            {
                PaintTeleport(state, position, value);
            }
            else if ("LMNOQRST".IndexOf(value) >= 0)
            {
                var isPressed = state.IsButtonPressed(position);
                var btnSprite = itemSkin != null ? (isPressed ? itemSkin.ButtonPressed : itemSkin.ButtonReleased) : null;
                SetCellSprite(position, btnSprite, Color.white, "BUTTON");
            }
            else if ("lmno".IndexOf(value) >= 0)
            {
                var isClosed = state.IsDoorClosed(position);
                if (isClosed)
                {
                    var bar = (value == 'l' || value == 'm')
                        ? (itemSkin != null ? itemSkin.DoorBarBlue : null)
                        : (itemSkin != null ? itemSkin.DoorBarGreen : null);
                    if (bar == null && itemSkin != null) bar = itemSkin.DoorLock;
                    SetCellSprite(position, bar, Color.white, "DOOR");
                }
                else
                {
                    SetCell(position, FloorColor, string.Empty);
                }
            }
            else if ("qrst".IndexOf(value) >= 0)
            {
                SetCellSprite(position, itemSkin != null ? itemSkin.LaserGun : null, Color.white, "LASER");
            }
            else if (value == 'k')
            {
                SetCellSprite(position, itemSkin != null ? itemSkin.BombRemote : null, Color.white, "REMOTE");
            }
            else
            {
                SetCell(position, FloorColor, string.Empty);
            }
        }

        private void PaintAutotileWall(PuzzleState state, GridPos position)
        {
            var up = IsWallAt(state, new GridPos(position.X, position.Y + 1));
            var right = IsWallAt(state, new GridPos(position.X + 1, position.Y));
            var down = IsWallAt(state, new GridPos(position.X, position.Y - 1));
            var left = IsWallAt(state, new GridPos(position.X - 1, position.Y));

            var mask = 0;
            if (up) mask |= 1;
            if (right) mask |= 2;
            if (down) mask |= 4;
            if (left) mask |= 8;

            var sprite = autotileBlockSkin.GetSprite(mask, position);
            if (sprite == null)
            {
                PaintLegacyWall(state, position);
                return;
            }

            var baseImage = cellImages[position];
            baseImage.sprite = null;
            baseImage.color = Color.clear;
            cellLabels[position].text = string.Empty;

            var cellRect = cellRects[position];
            var visualTransform = cellRect.Find("BlockVisual");
            GameObject visualObject;
            if (visualTransform != null)
            {
                visualObject = visualTransform.gameObject;
            }
            else
            {
                visualObject = new GameObject("BlockVisual", typeof(RectTransform), typeof(Image));
                visualObject.transform.SetParent(cellRect, false);
            }

            var visualRect = (RectTransform)visualObject.transform;
            visualRect.anchorMin = visualRect.anchorMax = new Vector2(0.5f, 0.5f);

            var ppu = sprite.pixelsPerUnit > 0f ? sprite.pixelsPerUnit : 256f;
            var pivot = new Vector2(sprite.pivot.x / sprite.rect.width, sprite.pivot.y / sprite.rect.height);
            visualRect.pivot = pivot;
            visualRect.anchoredPosition = Vector2.zero;

            var cellSize = Mathf.Min(cellRect.rect.width, cellRect.rect.height);
            if (cellSize <= 0f)
            {
                var layout = boardGrid.GetComponent<GridLayoutGroup>();
                if (layout != null) cellSize = layout.cellSize.x;
                if (cellSize <= 0f) cellSize = 64f;
            }

            var unitSize = sprite.rect.size / ppu;
            visualRect.sizeDelta = unitSize * cellSize;

            var visualImage = visualObject.GetComponent<Image>();
            visualImage.sprite = sprite;
            visualImage.color = Color.white;
            visualImage.type = Image.Type.Simple;
            visualImage.preserveAspect = false;
            visualImage.raycastTarget = false;
        }

        private void PaintLegacyWall(PuzzleState state, GridPos position)
        {
            if (legacyBlockSkin == null || !legacyBlockSkin.IsConfigured)
            {
                SetCell(position, WallColor, string.Empty);
                return;
            }

            var left = IsWallAt(state, new GridPos(position.X - 1, position.Y));
            var right = IsWallAt(state, new GridPos(position.X + 1, position.Y));
            var top = IsWallAt(state, new GridPos(position.X, position.Y + 1));
            var bottom = IsWallAt(state, new GridPos(position.X, position.Y - 1));

            var image = cellImages[position];
            image.sprite = legacyBlockSkin.Body(left, right, top, bottom, position);
            image.color = Color.white;
            image.type = Image.Type.Simple;
            image.preserveAspect = false;
            cellLabels[position].text = string.Empty;

            // These are the five child layers and local positions used by V1's BoxObject.
            if (!top)
            {
                var topSprite = !left
                    ? legacyBlockSkin.TopLeft
                    : !right ? legacyBlockSkin.TopRight : legacyBlockSkin.TopMid;
                AddBlockLayer(position, "TopEdge", topSprite, new Vector2(-0.02f, 0.463f));
            }

            if (!left)
            {
                var leftSprite = !top
                    ? legacyBlockSkin.LeftTop
                    : !bottom ? legacyBlockSkin.LeftBot : legacyBlockSkin.LeftMid;
                AddBlockLayer(position, "LeftEdge", leftSprite, new Vector2(-0.456f, -0.011f));
            }

            if (!top && !right)
            {
                AddBlockLayer(position, "TopRightCorner", legacyBlockSkin.TopRightCorner,
                    new Vector2(0.5145f, 0.365f));
            }

            if (!left && !bottom)
            {
                AddBlockLayer(position, "BottomLeftCorner", legacyBlockSkin.BotLeftCorner,
                    new Vector2(-0.4099f, -0.6344f));
            }
        }

        private void AddBlockLayer(GridPos position, string layerName, Sprite sprite, Vector2 localPosition)
        {
            if (sprite == null) return;

            var cellRect = cellRects[position];
            var layerObject = new GameObject(layerName, typeof(RectTransform), typeof(Image));
            layerObject.transform.SetParent(cellRect, false);
            var layerRect = (RectTransform)layerObject.transform;
            layerRect.anchorMin = layerRect.anchorMax = new Vector2(0.5f, 0.5f);
            layerRect.pivot = new Vector2(0.5f, 0.5f);

            var unitSize = sprite.rect.size / sprite.pixelsPerUnit;
            var pivot = new Vector2(sprite.pivot.x / sprite.rect.width, sprite.pivot.y / sprite.rect.height);
            var pivotCorrection = Vector2.Scale(new Vector2(0.5f - pivot.x, 0.5f - pivot.y), unitSize);
            var cellSize = Mathf.Min(cellRect.rect.width, cellRect.rect.height);
            layerRect.sizeDelta = unitSize * cellSize;
            layerRect.anchoredPosition = (localPosition + pivotCorrection) * cellSize;

            var layerImage = layerObject.GetComponent<Image>();
            layerImage.sprite = sprite;
            layerImage.color = Color.white;
            layerImage.type = Image.Type.Simple;
            layerImage.preserveAspect = true;
            layerImage.raycastTarget = false;
        }

        private static bool IsWallAt(PuzzleState state, GridPos position)
        {
            return position.X >= 0 && position.X < state.Size.Width &&
                   position.Y >= 0 && position.Y < state.Size.Height &&
                   IsWallCell(state.CellAt(position));
        }

        private static bool IsWallCell(char value)
        {
            return value == '1' || value == '7' || value == 'p' || value == 'B';
        }

        private void TryMove(Direction direction)
        {
            if (session == null || session.CurrentState.Status != SessionStatus.Playing) return;

            if (isAnimating)
            {
                queuedDirection = direction;
                return;
            }

            var startState = session.CurrentState.Clone();
            var result = session.Move(direction);
            if (!result.IsAccepted)
            {
                StartCoroutine(BumpHeadRoutine(direction, result.RejectionReason));
                return;
            }

            acceptedMoves.Add(direction);
            activeMoveRoutine = StartCoroutine(AnimateMoveRoutine(startState, result, direction));
        }

        private IEnumerator BumpHeadRoutine(Direction direction, MoveRejectionReason reason)
        {
            isAnimating = true;
            AudioManager.Play(SfxCue.Blocked);
            feedbackText.text = $"Blocked: {reason}";

            Transform headTransform = null;
            if (snakeSegments.Count > 0 && snakeSegments[snakeSegments.Count - 1] != null)
            {
                headTransform = snakeSegments[snakeSegments.Count - 1].transform;
            }
            else if (snakeOverlay.childCount > 0)
            {
                headTransform = snakeOverlay.GetChild(snakeOverlay.childCount - 1);
            }

            if (headTransform != null)
            {
                var spine = headTransform.GetComponentInChildren<SnakeSpineHead>();
                if (spine != null)
                {
                    spine.PlayHitWall();
                }

                var initialPos = headTransform.localPosition;
                var dirOffset = direction.ToOffset();
                var bumpOffset = new Vector3(dirOffset.X, dirOffset.Y, 0f) * (GetCellSize() * 0.16f);

                const float bumpDuration = 0.10f;
                var elapsed = 0f;
                while (elapsed < bumpDuration)
                {
                    elapsed += Time.deltaTime;
                    var t = Mathf.Clamp01(elapsed / bumpDuration);
                    var pulse = Mathf.Sin(t * Mathf.PI);
                    headTransform.localPosition = initialPos + bumpOffset * pulse;
                    yield return null;
                }

                headTransform.localPosition = initialPos;
            }

            isAnimating = false;
        }

        private IEnumerator AnimateMoveRoutine(PuzzleState startState, PuzzleTurnResult result, Direction direction)
        {
            isAnimating = true;

            var slideSnake = result.PreGravitySnake ?? result.State.Snake;
            var startSnake = startState.Snake;

            var startPositions = new Vector3[slideSnake.Count];
            var endPositions = new Vector3[slideSnake.Count];

            for (var i = 0; i < slideSnake.Count; i++)
            {
                if (i < startSnake.Count)
                {
                    startPositions[i] = GetCellLocalPosition(startSnake[i]);
                }
                else
                {
                    startPositions[i] = GetCellLocalPosition(startSnake[startSnake.Count - 1]);
                }

                endPositions[i] = GetCellLocalPosition(slideSnake[i]);
            }

            AudioManager.Play(SfxCue.Move);
            for (var i = 0; i < slideSnake.Count && i < startSnake.Count; i++)
            {
                if (startSnake[i].ManhattanDistance(slideSnake[i]) > 1)
                {
                    AudioManager.Play(SfxCue.Teleport);
                    break;
                }
            }

            // Phase 1: Forward slide (0.12s)
            const float slideDuration = 0.12f;
            var elapsed = 0f;
            var currentPositions = new Vector3[slideSnake.Count];

            while (elapsed < slideDuration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / slideDuration);
                var easeT = Mathf.SmoothStep(0f, 1f, t);

                for (var i = 0; i < slideSnake.Count; i++)
                {
                    // A segment that went through a teleporter must not fly across the board: snap to the exit.
                    var teleported = i < startSnake.Count && startSnake[i].ManhattanDistance(slideSnake[i]) > 1;
                    currentPositions[i] = teleported
                        ? Vector3.Lerp(PortalMouthPosition(startState, slideSnake[i], endPositions[i]), endPositions[i], easeT)
                        : Vector3.Lerp(startPositions[i], endPositions[i], easeT);
                }

                RenderSnakeInterpolated(slideSnake, currentPositions);
                yield return null;
            }

            // Chomp feedback on apple eaten
            if (result.AteApple && slideSnake.Count > 0)
            {
                GetActiveSpineHead()?.PlayEat();
                AudioManager.Play(SfxCue.AppleEat);
                SetCell(slideSnake[0], FloorColor, string.Empty);
                var punchElapsed = 0f;
                const float punchDuration = 0.08f;
                while (punchElapsed < punchDuration)
                {
                    punchElapsed += Time.deltaTime;
                    var pt = Mathf.Clamp01(punchElapsed / punchDuration);
                    var scalePunch = Mathf.Lerp(1.25f, 1f, pt);
                    RenderSnakeInterpolated(slideSnake, endPositions, headScaleMultiplier: scalePunch);
                    yield return null;
                }
            }

            // Phase 2: Gravity / Falling
            var finalSnake = result.State.Snake;
            var fell = slideSnake.Count > 0 && finalSnake.Count > 0 && slideSnake[0] != finalSnake[0];
            var fellOutside = result.State.LossReason == PuzzleLossReason.FellOutside;

            if (fell || fellOutside)
            {
                var fallStartPositions = new Vector3[slideSnake.Count];
                var fallEndPositions = new Vector3[slideSnake.Count];

                for (var i = 0; i < slideSnake.Count; i++)
                {
                    fallStartPositions[i] = endPositions[i];
                    if (fellOutside)
                    {
                        fallEndPositions[i] = fallStartPositions[i] + new Vector3(0f, -GetCellSize() * 6f, 0f);
                    }
                    else if (i < finalSnake.Count)
                    {
                        fallEndPositions[i] = GetCellLocalPosition(finalSnake[i]);
                    }
                    else
                    {
                        fallEndPositions[i] = fallStartPositions[i];
                    }
                }

                AudioManager.Play(SfxCue.Fall);
                var dropCells = fellOutside ? 6 : Mathf.Abs(slideSnake[0].Y - finalSnake[0].Y);
                var fallDuration = Mathf.Clamp(dropCells * 0.065f, 0.08f, 0.28f);
                var fallElapsed = 0f;
                var fallingPositions = new Vector3[slideSnake.Count];

                while (fallElapsed < fallDuration)
                {
                    fallElapsed += Time.deltaTime;
                    var t = Mathf.Clamp01(fallElapsed / fallDuration);
                    var easeFall = t * t;

                    for (var i = 0; i < slideSnake.Count; i++)
                    {
                        fallingPositions[i] = Vector3.Lerp(fallStartPositions[i], fallEndPositions[i], easeFall);
                    }

                    RenderSnakeInterpolated(fellOutside ? slideSnake : (IReadOnlyList<GridPos>)finalSnake, fallingPositions);
                    yield return null;
                }

                // Landing squash bounce (if landed inside board)
                if (!fellOutside)
                {
                    AudioManager.Play(SfxCue.Land);
                    var bounceElapsed = 0f;
                    const float bounceDuration = 0.09f;
                    while (bounceElapsed < bounceDuration)
                    {
                        bounceElapsed += Time.deltaTime;
                        var bt = Mathf.Clamp01(bounceElapsed / bounceDuration);
                        var squashSin = Mathf.Sin(bt * Mathf.PI);
                        var squashScale = new Vector3(1f + 0.14f * squashSin, 1f - 0.14f * squashSin, 1f);
                        RenderSnakeInterpolated(finalSnake, fallEndPositions, bodySquash: squashScale);
                        yield return null;
                    }
                }
            }

            // Phase 3: Finalize board state and UI
            if (result.State.Status == SessionStatus.Won)
            {
                GetActiveSpineHead()?.PlayWin();
                var reward = CommitCompletion();
                AudioManager.Play(SfxCue.Win);
                AudioManager.Play(SfxCue.WinFanfare);
                AudioManager.Play(SfxCue.Firework);
                if (reward > 0) AudioManager.Play(SfxCue.CoinReward);
                RefreshBoard(reward > 0 ? $"Level complete! +{reward} coins" : "Level complete!");
                UiLookup.Find<Text>(winOverlay.transform, "WinMoveText").text =
                    $"Solved in {result.State.TurnIndex} moves";
                winRewardText.text = reward > 0 ? $"FIRST CLEAR  •  +{reward} COINS" : "LEVEL REPLAYED";
                winOverlay.SetActive(true);
            }
            else if (result.State.Status == SessionStatus.Lost)
            {
                GetActiveSpineHead()?.PlayDie();
                PlayLossSound(result.State.LossReason);
                RefreshBoard($"Try again: {result.State.LossReason}");
                loseSubtitleText.text = LossMessage(result.State.LossReason);
                loseOverlay.SetActive(true);
            }
            else
            {
                RefreshBoard(IsExitAvailable(result.State) ? "The exit is open!" : "Good move.");
            }

            isAnimating = false;
            activeMoveRoutine = null;

            if (queuedDirection.HasValue)
            {
                var nextDir = queuedDirection.Value;
                queuedDirection = null;
                TryMove(nextDir);
            }
        }

        private static void PlayLossSound(PuzzleLossReason reason)
        {
            switch (reason)
            {
                case PuzzleLossReason.Spike: AudioManager.Play(SfxCue.Spike); break;
                case PuzzleLossReason.Toxic: AudioManager.Play(SfxCue.Toxic); break;
                case PuzzleLossReason.Explosion: AudioManager.Play(SfxCue.Explosion); break;
                case PuzzleLossReason.Laser:
                case PuzzleLossReason.Crushed: AudioManager.Play(SfxCue.Hurt); break;
                case PuzzleLossReason.FellOutside: AudioManager.Play(SfxCue.Scream); break;
            }
            AudioManager.Play(SfxCue.Lose);
        }

        private void CancelActiveAnimation()
        {
            if (activeMoveRoutine != null)
            {
                StopCoroutine(activeMoveRoutine);
                activeMoveRoutine = null;
            }
            isAnimating = false;
            queuedDirection = null;
        }

        private int CommitCompletion()
        {
            if (completionCommitted) return 0;
            completionCommitted = true;
            var reward = LevelProgressStore.Complete(levelRecord.mode, levelRecord.number, session.CurrentState.CollectedVipApples);
            var total = LegacyLevelCatalogLoader.Load().Count(levelRecord.mode);
            UiLookup.Find<Button>(winOverlay.transform, "WinNextButton").GetComponentInChildren<Text>().text =
                levelRecord.number < total ? "NEXT LEVEL" : "ALL LEVELS";
            return reward;
        }

        private void NextLevel()
        {
            var total = LegacyLevelCatalogLoader.Load().Count(levelRecord.mode);
            if (levelRecord.number >= total)
            {
                navigator.Load(ScreenNames.LevelSelect);
                return;
            }

            LoadSelectedLevel();
        }

        private void Undo()
        {
            CancelActiveAnimation();
            if (session != null && session.Undo())
            {
                if (acceptedMoves.Count > 0) acceptedMoves.RemoveAt(acceptedMoves.Count - 1);
                AudioManager.Play(SfxCue.Undo);
                loseOverlay.SetActive(false);
                winOverlay.SetActive(false);
                RefreshBoard("Move undone.");
            }
            else if (session != null)
            {
                RefreshBoard("Nothing to undo.");
            }
        }

        private void Restart()
        {
            CancelActiveAnimation();
            if (session == null) return;
            session.Restart();
            cellRects.Clear();
            acceptedMoves.Clear();
            completionCommitted = false;
            pauseOverlay.SetActive(false);
            pauseSettingsOverlay.SetActive(false);
            confirmHomeOverlay.SetActive(false);
            winOverlay.SetActive(false);
            loseOverlay.SetActive(false);
            RefreshBoard("Level restarted.");
        }

        private void ShowPause() => pauseOverlay.SetActive(true);
        private void HidePause() => pauseOverlay.SetActive(false);
        private void ShowPauseSettings() => pauseSettingsOverlay.SetActive(true);
        private void HidePauseSettings() => pauseSettingsOverlay.SetActive(false);
        private void ShowHomeConfirmation() => confirmHomeOverlay.SetActive(true);
        private void HideHomeConfirmation() => confirmHomeOverlay.SetActive(false);
        private void RestartFromPause() { HidePause(); Restart(); }
        private void UndoFromLose() { loseOverlay.SetActive(false); Undo(); }
        private void RestartFromLose() { loseOverlay.SetActive(false); Restart(); }
        public bool IsGameplayOverlayOpen => IsModalOpen();

        private bool IsModalOpen() => pauseOverlay.activeSelf || pauseSettingsOverlay.activeSelf ||
                                      confirmHomeOverlay.activeSelf || winOverlay.activeSelf || loseOverlay.activeSelf;

        private void HandleBack()
        {
            if (confirmHomeOverlay.activeSelf) { HideHomeConfirmation(); return; }
            if (pauseSettingsOverlay.activeSelf) { HidePauseSettings(); return; }
            if (pauseOverlay.activeSelf) { HidePause(); return; }
            if (winOverlay.activeSelf || loseOverlay.activeSelf) return;
            ShowPause();
        }

        private void ShowHint()
        {
            if (session == null || session.CurrentState.Status != SessionStatus.Playing) return;
            var solution = levelRecord?.solution ?? string.Empty;
            if (string.IsNullOrEmpty(solution))
            {
                RefreshBoard("Hint unavailable: this level has no verified solution yet.");
                return;
            }

            for (var index = 0; index < acceptedMoves.Count; index++)
            {
                if (index >= solution.Length || ToSolutionSymbol(acceptedMoves[index]) !=
                    char.ToUpperInvariant(solution[index]))
                {
                    RefreshBoard("Hint unavailable here. Undo or restart to return to the solution path.");
                    return;
                }
            }

            if (acceptedMoves.Count >= solution.Length || !TryParseSolutionDirection(solution[acceptedMoves.Count], out var direction))
            {
                RefreshBoard("No next hint is available from this position.");
                return;
            }

            RefreshBoard($"Hint: move {direction.ToString().ToUpperInvariant()}.");
        }

        private static char ToSolutionSymbol(Direction direction)
        {
            switch (direction)
            {
                case Direction.Up: return 'U';
                case Direction.Down: return 'D';
                case Direction.Left: return 'L';
                case Direction.Right: return 'R';
                default: return '?';
            }
        }

        private static bool TryParseSolutionDirection(char symbol, out Direction direction)
        {
            switch (char.ToUpperInvariant(symbol))
            {
                case 'U': direction = Direction.Up; return true;
                case 'D': direction = Direction.Down; return true;
                case 'L': direction = Direction.Left; return true;
                case 'R': direction = Direction.Right; return true;
                default: direction = default; return false;
            }
        }

        private bool IsChallengeMode()
        {
            return levelRecord != null &&
                   string.Equals(levelRecord.mode, LevelProgressStore.ChallengeMode, System.StringComparison.OrdinalIgnoreCase);
        }

        private bool IsExitAvailable(PuzzleState state) => IsChallengeMode() || state.IsExitOpen;

        private static string LossMessage(PuzzleLossReason reason)
        {
            switch (reason)
            {
                case PuzzleLossReason.Spike: return "A worm segment touched a spike.";
                case PuzzleLossReason.Toxic: return "A worm segment touched toxic ground.";
                case PuzzleLossReason.Laser: return "The worm crossed an active laser.";
                case PuzzleLossReason.Explosion: return "The worm was caught in an explosion.";
                case PuzzleLossReason.FellOutside: return "The worm fell outside the board.";
                case PuzzleLossReason.Crushed: return "The worm was crushed by a falling object.";
                default: return "The worm needs another route.";
            }
        }

        private void SetCell(GridPos position, Color color, string label)
        {
            if (!cellImages.TryGetValue(position, out var image)) return;
            image.sprite = null;
            image.color = color;
            image.rectTransform.localEulerAngles = Vector3.zero;
            cellLabels[position].text = label;
            RemoveItemVisual(position);
        }

        private void RemoveItemVisual(GridPos position)
        {
            if (portalFronts.TryGetValue(position, out var front))
            {
                portalFronts.Remove(position);
                if (front != null) Destroy(front.gameObject);
            }

            if (!itemVisuals.TryGetValue(position, out var visual)) return;
            itemVisuals.Remove(position);
            if (visual != null)
            {
                visual.gameObject.SetActive(false);
                Destroy(visual.gameObject);
            }
        }

        // Item sprites (exit, hazards, rocks, doors...) are drawn in the item overlay, above wall tiles, so the
        // sprites that wall tiles spill into neighbouring cells can never cover them. `lift` raises the item
        // (fraction of a cell) so it lines up with the raised top faces of the blocks.
        private void SetCellSprite(GridPos position, Sprite sprite, Color tint, string fallbackLabel, float rotation = 0f, float scale = 1f, float lift = 0f)
        {
            if (!cellImages.TryGetValue(position, out var baseImage)) return;
            if (sprite == null)
            {
                baseImage.sprite = null;
                baseImage.color = tint;
                baseImage.rectTransform.localEulerAngles = Vector3.zero;
                cellLabels[position].text = fallbackLabel ?? string.Empty;
                RemoveItemVisual(position);
                return;
            }

            baseImage.sprite = null;
            baseImage.color = Color.clear;
            cellLabels[position].text = string.Empty;

            var cellRect = cellRects[position];
            var parent = itemOverlay != null ? (Transform)itemOverlay : cellRect;
            if (!itemVisuals.TryGetValue(position, out var visualRect) || visualRect == null)
            {
                var visualObject = new GameObject("ObjectVisual", typeof(RectTransform), typeof(Image));
                visualObject.transform.SetParent(parent, false);
                visualRect = (RectTransform)visualObject.transform;
                itemVisuals[position] = visualRect;
            }

            visualRect.gameObject.SetActive(true);
            visualRect.anchorMin = visualRect.anchorMax = new Vector2(0.5f, 0.5f);
            visualRect.pivot = new Vector2(0.5f, 0.5f);
            visualRect.localEulerAngles = new Vector3(0f, 0f, rotation);

            var cellSize = Mathf.Min(cellRect.rect.width, cellRect.rect.height);
            if (cellSize <= 0f)
            {
                var layout = boardGrid.GetComponent<GridLayoutGroup>();
                if (layout != null) cellSize = layout.cellSize.x;
                if (cellSize <= 0f) cellSize = 64f;
            }

            var center = itemOverlay != null
                ? (Vector2)itemOverlay.InverseTransformPoint(cellRect.position)
                : Vector2.zero;
            visualRect.anchoredPosition = center + new Vector2(0f, cellSize * lift);
            visualRect.sizeDelta = new Vector2(cellSize * scale, cellSize * scale);

            var visualImage = visualRect.GetComponent<Image>();
            visualImage.sprite = sprite;
            visualImage.color = tint;
            visualImage.type = Image.Type.Simple;
            visualImage.preserveAspect = true;
            visualImage.raycastTarget = false;
        }

        // V1 builds a teleporter from layers: a glowing disc behind the snake and a metal rim in front of it, offset
        // towards the side where the hidden pocket (a wall cell) is. Lower case pairs are blue/green, upper case purple.
        private void PaintTeleport(PuzzleState state, GridPos position, char value)
        {
            var green = "dfDF".IndexOf(value) >= 0;
            var gate = itemSkin != null ? (green ? itemSkin.TeleportOrange : itemSkin.TeleportBlue) : null;
            Sprite core = null;
            if (itemSkin != null)
            {
                core = char.IsUpper(value)
                    ? itemSkin.TeleportCorePurple
                    : green ? itemSkin.TeleportCoreOrange : itemSkin.TeleportCoreBlue;
            }

            if (gate == null || core == null || itemOverlay == null || portalFrontOverlay == null ||
                !cellRects.ContainsKey(position))
            {
                SetCellSprite(position, gate, Color.white, "PORTAL");
                return;
            }

            var pocketRight = IsPortalWall(state, position + new GridPos(1, 0)) &&
                              !IsPortalWall(state, position + new GridPos(-1, 0));
            var side = pocketRight ? 1f : -1f;
            portalSides[position] = side;
            var coreX = side * 0.25f;
            var gateX = coreX - side * 0.148f;

            if (cellImages.TryGetValue(position, out var baseImage))
            {
                baseImage.sprite = null;
                baseImage.color = Color.clear;
                cellLabels[position].text = string.Empty;
            }

            PlacePortalPiece(itemVisuals, itemOverlay, position, core, new Vector2(0.342f, 0.956f), coreX, pocketRight);
            PlacePortalPiece(portalFronts, portalFrontOverlay, position, gate, new Vector2(0.406f, 1.028f), gateX, pocketRight);
        }

        private void PlacePortalPiece(
            Dictionary<GridPos, RectTransform> store,
            RectTransform overlay,
            GridPos position,
            Sprite sprite,
            Vector2 sizeInCells,
            float offsetInCells,
            bool mirror)
        {
            var cellRect = cellRects[position];
            var cellSize = Mathf.Min(cellRect.rect.width, cellRect.rect.height);
            if (cellSize <= 0f) cellSize = GetCellSize();

            if (!store.TryGetValue(position, out var rect) || rect == null)
            {
                var piece = new GameObject("PortalPiece", typeof(RectTransform), typeof(Image));
                piece.transform.SetParent(overlay, false);
                rect = (RectTransform)piece.transform;
                store[position] = rect;
            }

            rect.gameObject.SetActive(true);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            var center = (Vector2)overlay.InverseTransformPoint(cellRect.position);
            rect.anchoredPosition = center + new Vector2(offsetInCells * cellSize, 0f);
            rect.sizeDelta = new Vector2(sizeInCells.x * cellSize, sizeInCells.y * cellSize);
            rect.localScale = new Vector3(mirror ? -1f : 1f, 1f, 1f);
            rect.localEulerAngles = Vector3.zero;

            var image = rect.GetComponent<Image>();
            image.sprite = sprite;
            image.color = Color.white;
            image.type = Image.Type.Simple;
            image.preserveAspect = false;
            image.raycastTarget = false;
        }

        private static bool IsPortalWall(PuzzleState state, GridPos position)
        {
            var value = state.CellAt(position);
            return value == '1' || value == '7' || value == 'p' || value == 'B';
        }

        public bool HasItemSkin => itemSkin != null && itemSkin.IsConfigured;

        public void ConfigureItemSkin(GameplayItemSkin skin)
        {
            itemSkin = skin;
        }

        public bool HasLegacySnakeDemoSprites => legacySnakeSkin != null && legacySnakeSkin.IsConfigured;

        public void ConfigureLegacySnakeSkin(SnakeSpriteSkin skin)
        {
            legacySnakeSkin = skin;
        }

        public bool HasAutotileBlockSkin => autotileBlockSkin != null && autotileBlockSkin.IsConfigured;

        public void ConfigureAutotileBlockSkin(AutotileBlockSkin skin)
        {
            autotileBlockSkin = skin;
        }

        private Vector3 GetCellLocalPosition(GridPos position)
        {
            if (cellRects.TryGetValue(position, out var cellRect))
            {
                return cellRect.localPosition;
            }

            var cellSize = GetCellSize();
            foreach (var kvp in cellRects)
            {
                var refPos = kvp.Key;
                var refLocal = kvp.Value.localPosition;
                return refLocal + new Vector3((position.X - refPos.X) * cellSize, (position.Y - refPos.Y) * cellSize, 0f);
            }

            return Vector3.zero;
        }

        private float GetCellSize()
        {
            foreach (var rect in cellRects.Values)
            {
                if (rect != null && rect.rect.width > 0f) return rect.rect.width;
            }

            var layout = boardGrid != null ? boardGrid.GetComponent<GridLayoutGroup>() : null;
            if (layout != null && layout.cellSize.x > 0f) return layout.cellSize.x;

            return 64f;
        }

        private void RenderSnakeInterpolated(
            IReadOnlyList<GridPos> snake,
            Vector3[] customPositions = null,
            float headScaleMultiplier = 1f,
            Vector3? bodySquash = null)
        {
            if (snake == null || snake.Count == 0) return;

            if (!HasLegacySnakeDemoSprites)
            {
                for (var index = snake.Count - 1; index >= 0; index--)
                {
                    var isHead = index == 0;
                    var isTail = index == snake.Count - 1;
                    SetCell(snake[index], isHead ? HeadColor : isTail ? TailColor : BodyColor,
                        isHead ? "HEAD" : isTail ? "TAIL" : string.Empty);
                }

                return;
            }

            var cellSize = GetCellSize();
            var baseCellSize = new Vector2(cellSize, cellSize);

            snakeSegments.RemoveAll(go => go == null);

            while (snakeSegments.Count < snake.Count)
            {
                var newObj = new GameObject($"Snake_Segment_{snakeSegments.Count:00}", typeof(RectTransform), typeof(Image));
                newObj.transform.SetParent(snakeOverlay, false);
                var img = newObj.GetComponent<Image>();
                img.raycastTarget = false;
                img.color = Color.white;
                snakeSegments.Add(newObj);
            }

            for (var i = 0; i < snakeSegments.Count; i++)
            {
                snakeSegments[i].SetActive(i < snake.Count);
            }

            for (var index = snake.Count - 1; index >= 0; index--)
            {
                var position = snake[index];
                if (IsInsidePortalPocket(position))
                {
                    // The body is inside the teleporter's hidden pocket: it is not drawn until it comes out.
                    snakeSegments[snake.Count - 1 - index].SetActive(false);
                    continue;
                }

                var localPos = customPositions != null && index < customPositions.Length
                    ? customPositions[index]
                    : GetCellLocalPosition(position);

                Sprite sprite;
                float rotation;
                var scale = Vector3.one;
                var size = baseCellSize;

                if (index == 0)
                {
                    sprite = legacySnakeSkin.Head;
                    // The whole snake is one rigid ribbon: crawling back leaves the back down-side-up, so the
                    // head rotates (upside down when it heads west) instead of being mirrored.
                    rotation = RotationForDirection(position, ResolveVisualNeighbor(position, snake[Mathf.Min(1, snake.Count - 1)]), true);

                    size = new Vector2(
                        size.x * legacySnakeSkin.HeadWidthInCells,
                        size.y * legacySnakeSkin.HeadHeightInCells);

                    scale *= headScaleMultiplier;
                }
                else if (index == snake.Count - 1)
                {
                    sprite = legacySnakeSkin.TailAt(PhaseForSegment(index));
                    rotation = RotationForDirection(position, ResolveVisualNeighbor(position, snake[index - 1]), false);
                }
                else
                {
                    var previous = ResolveVisualNeighbor(position, snake[index - 1]);
                    var next = ResolveVisualNeighbor(position, snake[index + 1]);
                    if (previous.X == next.X || previous.Y == next.Y)
                    {
                        sprite = legacySnakeSkin.BodyAt(PhaseForSegment(index));
                        // Straight runs rotate with the travel direction (tail -> head); the belly is always
                        // on the right-hand side of that direction, so there is never a twist at a bend.
                        rotation = RotationForDirection(position, previous, false);
                    }
                    else
                    {
                        // Belly inside the bend uses the base corner art, belly outside uses the mirrored family.
                        var bellyInside = IsBellyInsideBend(position, previous, next);
                        sprite = bellyInside
                            ? legacySnakeSkin.CornerAt(PhaseForSegment(index))
                            : legacySnakeSkin.CornerOutsideAt(PhaseForSegment(index));
                        rotation = CornerRotation(position, previous, next);
                    }
                }

                if (bodySquash.HasValue)
                {
                    scale = Vector3.Scale(scale, bodySquash.Value);
                }

                var childIndex = snake.Count - 1 - index;
                var segmentObj = snakeSegments[childIndex];
                segmentObj.transform.SetAsLastSibling();

                var rect = (RectTransform)segmentObj.transform;
                rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
                rect.localPosition = localPos;
                rect.sizeDelta = size;
                rect.localRotation = Quaternion.Euler(0f, 0f, rotation);
                rect.localScale = scale;

                var image = segmentObj.GetComponent<Image>();
                var spineChild = segmentObj.transform.Find("SpineHead");
                var neckChild = segmentObj.transform.Find("SpineNeck");

                if (index == 0 && legacySnakeSkin != null && legacySnakeSkin.UseSpineHead)
                {
                    // 1. Neck Underlay setup (ADR-007): bridges round Spine head to flat body segment
                    var neckSprite = legacySnakeSkin.Neck;
                    if (neckSprite != null)
                    {
                        if (neckChild == null)
                        {
                            var neckGo = new GameObject("SpineNeck", typeof(RectTransform), typeof(Image));
                            neckGo.transform.SetParent(segmentObj.transform, false);
                            neckChild = neckGo.transform;
                            var neckImg = neckGo.GetComponent<Image>();
                            neckImg.raycastTarget = false;
                        }

                        neckChild.SetSiblingIndex(0);
                        neckChild.gameObject.SetActive(true);

                        var neckImgComp = neckChild.GetComponent<Image>();
                        if (neckImgComp != null)
                        {
                            neckImgComp.sprite = neckSprite;
                            neckImgComp.color = Color.white;
                            neckImgComp.preserveAspect = false;
                            neckImgComp.raycastTarget = false;
                        }

                        var neckRect = (RectTransform)neckChild;
                        neckRect.anchorMin = neckRect.anchorMax = neckRect.pivot = new Vector2(0f, 0.5f);
                        neckRect.localPosition = new Vector3(-size.x * 0.5f, 0f, 0f);
                        var neckAspect = neckSprite.rect.height > 0f ? (neckSprite.rect.width / neckSprite.rect.height) : (146f / 256f);
                        neckRect.sizeDelta = new Vector2(size.x * neckAspect, size.y);
                        neckRect.localScale = Vector3.one;
                        neckRect.localRotation = Quaternion.identity;
                    }
                    else if (neckChild != null)
                    {
                        neckChild.gameObject.SetActive(false);
                    }

                    // 2. Spine Head setup
                    if (spineChild == null)
                    {
                        var spineGo = new GameObject("SpineHead", typeof(RectTransform));
                        spineGo.transform.SetParent(segmentObj.transform, false);
                        spineChild = spineGo.transform;
                    }

                    spineChild.SetSiblingIndex(1);

                    var spineRect = (RectTransform)spineChild;
                    spineRect.anchorMin = spineRect.anchorMax = spineRect.pivot = new Vector2(0.5f, 0.5f);
                    // The V1 rig origin is at the base of the head. Align it with the cell's
                    // lower edge, matching the full-height neck instead of raising it by 0.05 cells.
                    spineRect.localPosition = new Vector3(-baseCellSize.x * 0.0652f, -size.y * 0.5f, 0f);
                    spineRect.sizeDelta = size;
                    spineRect.localScale = new Vector3(size.x / 480f, size.y / 480f, 1f);

                    var spineHead = spineChild.GetComponent<SnakeSpineHead>();
                    if (spineHead == null)
                    {
                        spineHead = spineChild.gameObject.AddComponent<SnakeSpineHead>();
                    }

                    if (!spineHead.IsInitialized || spineHead.CurrentSkinName != legacySnakeSkin.SpineSkinName)
                    {
                        spineHead.Initialize(legacySnakeSkin.SkeletonDataAsset, legacySnakeSkin.SpineSkinName);
                    }

                    if (spineHead.IsInitialized)
                    {
                        spineChild.gameObject.SetActive(true);
                        if (image != null) image.enabled = false;

                        spineHead.UpdateAppleProximity(
                            session != null && IsAdjacentToApple(session.CurrentState, position));
                    }
                    else
                    {
                        // Fallback to Sprite head if Spine is not ready or failed to initialize
                        if (neckChild != null) neckChild.gameObject.SetActive(false);
                        spineChild.gameObject.SetActive(false);
                        if (image != null)
                        {
                            image.enabled = true;
                            image.sprite = sprite;
                            image.color = Color.white;
                            image.preserveAspect = false;
                            image.raycastTarget = false;
                        }
                    }
                }
                else
                {
                    if (neckChild != null)
                    {
                        neckChild.gameObject.SetActive(false);
                    }

                    if (spineChild != null)
                    {
                        spineChild.gameObject.SetActive(false);
                    }

                    if (image != null)
                    {
                        image.enabled = true;
                        image.sprite = sprite;
                        image.color = Color.white;
                        image.preserveAspect = false;
                        image.raycastTarget = false;
                    }
                }

                ApplyPortalClip(segmentObj, image, spineChild, localPos, size, rotation);

                if (cellLabels.TryGetValue(position, out var label))
                {
                    label.text = string.Empty;
                }
            }
        }

        private SnakeSpineHead GetActiveSpineHead()
        {
            if (session == null || session.CurrentState == null) return null;
            var snake = session.CurrentState.Snake;
            if (snake == null || snake.Count == 0) return null;
            var headIndex = snake.Count - 1;
            if (headIndex >= 0 && headIndex < snakeSegments.Count && snakeSegments[headIndex] != null)
            {
                return snakeSegments[headIndex].GetComponentInChildren<SnakeSpineHead>();
            }
            return null;
        }

        private static bool IsSlidePlatformPosition(PuzzleState state, GridPos position)
        {
            if (state == null || state.SlidePlatforms == null) return false;
            for (var i = 0; i < state.SlidePlatforms.Count; i++)
            {
                if (state.SlidePlatforms[i].Current == position) return true;
            }
            return false;
        }

        private static bool IsAdjacentToApple(PuzzleState state, GridPos headPos)
        {
            if (state == null) return false;
            var dirs = new[] { new GridPos(1, 0), new GridPos(-1, 0), new GridPos(0, 1), new GridPos(0, -1) };
            for (var i = 0; i < dirs.Length; i++)
            {
                var neighbor = new GridPos(headPos.X + dirs[i].X, headPos.Y + dirs[i].Y);
                if (state.Size.Contains(neighbor))
                {
                    var val = state.CellAt(neighbor);
                    if (val == '2' || val == '8') return true;
                }
            }
            return false;
        }

        private void PaintSnake(PuzzleState state)
        {
            RenderSnakeInterpolated(state.Snake, null);
        }

        private static int PhaseForSegment(int segmentIndex)
        {
            // Head has its own artwork; the first body node starts at phase zero.
            return Mathf.Max(0, segmentIndex - 1);
        }

        private static float RotationForDirection(GridPos position, GridPos neighbor, bool faceAwayFromNeighbor)
        {
            var x = faceAwayFromNeighbor ? position.X - neighbor.X : neighbor.X - position.X;
            var y = faceAwayFromNeighbor ? position.Y - neighbor.Y : neighbor.Y - position.Y;
            if (x > 0) return 0f;
            if (y > 0) return 90f;
            if (x < 0) return 180f;
            return -90f;
        }

        // The belly stays on the right-hand side of the tail -> head travel, so it is inside the bend on right turns.
        private static bool IsBellyInsideBend(GridPos position, GridPos previous, GridPos next)
        {
            // Travel goes tail -> head: in = next -> position, out = position -> previous.
            var inX = position.X - next.X;
            var inY = position.Y - next.Y;
            var outX = previous.X - position.X;
            var outY = previous.Y - position.Y;
            var cross = inX * outY - inY * outX; // > 0 : left turn, < 0 : right turn
            return cross < 0;
        }

        // The cell of the teleporter a segment is coming out of: it slides from the gate plane to its exit cell.
        private Vector3 PortalMouthPosition(PuzzleState state, GridPos exitCell, Vector3 fallback)
        {
            var offsets = new[] { new GridPos(1, 0), new GridPos(-1, 0), new GridPos(0, 1), new GridPos(0, -1) };
            for (var index = 0; index < offsets.Length; index++)
            {
                var candidate = exitCell + offsets[index];
                if ("cdefCDEF".IndexOf(state.CellAt(candidate)) < 0) continue;
                var mouth = GetCellLocalPosition(candidate);
                // Start fully behind the gate plane (plane = 0.25 cell towards the pocket, plus half a segment), so the
                // segment is revealed progressively as it slides out of the disc.
                if (portalSides.TryGetValue(candidate, out var side)) mouth.x += side * 0.75f * GetCellSize();
                return mouth;
            }

            return fallback;
        }

        // Clips a segment at the plane of every nearby teleporter, like V1's portal mask: whatever is beyond the plane
        // (towards the hidden pocket) is not drawn, so the body looks like it passes through the gate disc.
        private void ApplyPortalClip(
            GameObject segmentObj,
            Image image,
            Transform spineChild,
            Vector3 localPos,
            Vector2 size,
            float rotation)
        {
            var mask = segmentObj.GetComponent<RectMask2D>();
            if (image != null && image.type != Image.Type.Simple) image.type = Image.Type.Simple;
            if (mask != null) mask.enabled = false;
            if (portalSides.Count == 0 || size.x <= 0f) return;

            var left = localPos.x - size.x * 0.5f;
            var right = localPos.x + size.x * 0.5f;
            var hiddenLeft = 0f;
            var hiddenRight = 0f;
            foreach (var pair in portalSides)
            {
                var center = GetCellLocalPosition(pair.Key);
                if (Mathf.Abs(center.y - localPos.y) > size.y * 0.45f) continue;
                var plane = center.x + pair.Value * 0.25f * size.x;
                if (Mathf.Abs(localPos.x - plane) > size.x * 1.5f) continue;
                if (pair.Value < 0f)
                {
                    hiddenLeft = Mathf.Max(hiddenLeft, Mathf.Clamp(plane - left, 0f, size.x));
                }
                else
                {
                    hiddenRight = Mathf.Max(hiddenRight, Mathf.Clamp(right - plane, 0f, size.x));
                }
            }

            var hidden = hiddenLeft + hiddenRight;
            if (hidden <= 0.5f) return;
            if (hidden >= size.x - 0.5f)
            {
                segmentObj.SetActive(false);
                return;
            }

            var horizontal = Mathf.Approximately(Mathf.Abs(Mathf.Sin(rotation * Mathf.Deg2Rad)), 0f);
            if (!horizontal)
            {
                // A vertical segment next to a gate: keep it simple and hide it once its centre passes the plane.
                if (hidden > size.x * 0.5f) segmentObj.SetActive(false);
                return;
            }

            var flipped = Mathf.Cos(rotation * Mathf.Deg2Rad) < 0f; // 180 degrees: local left is world right
            var cutLocalLeft = flipped ? hiddenRight : hiddenLeft;
            var cutLocalRight = flipped ? hiddenLeft : hiddenRight;
            var spineActive = spineChild != null && spineChild.gameObject.activeSelf;
            if (spineActive)
            {
                if (mask == null) mask = segmentObj.AddComponent<RectMask2D>();
                var open = -size.x * 3f;
                mask.padding = new Vector4(
                    cutLocalLeft > 0f ? cutLocalLeft : open,
                    open,
                    cutLocalRight > 0f ? cutLocalRight : open,
                    open);
                mask.enabled = true;
                return;
            }

            if (image == null || !image.enabled) return;
            image.type = Image.Type.Filled;
            image.fillMethod = Image.FillMethod.Horizontal;
            image.fillOrigin = cutLocalLeft > 0f ? (int)Image.OriginHorizontal.Right : (int)Image.OriginHorizontal.Left;
            image.fillAmount = Mathf.Clamp01(1f - hidden / size.x);
        }

        private bool IsInsidePortalPocket(GridPos position)
        {
            var state = session != null ? session.CurrentState : null;
            if (state == null) return false;
            var value = state.CellAt(position);
            return value == '1' || value == '7' || value == 'p' || value == 'B';
        }

        // A neighbouring body segment can be on the far side of a teleporter. Its real cell is not adjacent, so
        // orient this segment as if the body continued straight out of the portal tile next to it.
        private GridPos ResolveVisualNeighbor(GridPos position, GridPos neighbor)
        {
            if (position.ManhattanDistance(neighbor) == 1) return neighbor;
            var state = session != null ? session.CurrentState : null;
            if (state == null) return neighbor;
            var offsets = new[] { new GridPos(1, 0), new GridPos(-1, 0), new GridPos(0, 1), new GridPos(0, -1) };
            for (var index = 0; index < offsets.Length; index++)
            {
                var candidate = position + offsets[index];
                if ("cdefCDEF".IndexOf(state.CellAt(candidate)) >= 0) return candidate;
            }

            return neighbor;
        }

        private static float CornerRotation(GridPos position, GridPos previous, GridPos next)
        {
            var previousX = previous.X - position.X;
            var previousY = previous.Y - position.Y;
            var nextX = next.X - position.X;
            var nextY = next.Y - position.Y;
            if (HasDirections(previousX, previousY, nextX, nextY, 1, 0, 0, -1)) return 0f;
            if (HasDirections(previousX, previousY, nextX, nextY, 0, 1, 1, 0)) return 90f;
            if (HasDirections(previousX, previousY, nextX, nextY, -1, 0, 0, 1)) return 180f;
            return -90f;
        }

        private static bool HasDirections(
            int ax, int ay, int bx, int by,
            int expectedAx, int expectedAy, int expectedBx, int expectedBy)
        {
            return ax == expectedAx && ay == expectedAy && bx == expectedBx && by == expectedBy ||
                   ax == expectedBx && ay == expectedBy && bx == expectedAx && by == expectedAy;
        }

        private void BindButton(string objectName, UnityEngine.Events.UnityAction action)
        {
            UiLookup.Find<Button>(transform, objectName).onClick.AddListener(action);
        }

        private void BindQuickSetting(
            string objectName,
            bool initialValue,
            UnityEngine.Events.UnityAction<bool> onChanged)
        {
            var toggle = UiLookup.Find<Toggle>(transform, objectName);
            toggle.SetIsOnWithoutNotify(initialValue);
            toggle.onValueChanged.AddListener(onChanged);
        }

        private void ReadSwipe()
        {
            if (Input.GetMouseButtonDown(0) && (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject()))
            {
                pointerDown = Input.mousePosition;
                pointerTracking = true;
            }

            if (!pointerTracking || !Input.GetMouseButtonUp(0)) return;
            pointerTracking = false;
            var delta = (Vector2)Input.mousePosition - pointerDown;
            if (delta.magnitude < 55f) return;
            TryMove(Mathf.Abs(delta.x) > Mathf.Abs(delta.y)
                ? delta.x > 0 ? Direction.Right : Direction.Left
                : delta.y > 0 ? Direction.Up : Direction.Down);
        }

        private static bool TryReadKeyboardDirection(out Direction direction)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) { direction = Direction.Up; return true; }
            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) { direction = Direction.Down; return true; }
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) { direction = Direction.Left; return true; }
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) { direction = Direction.Right; return true; }
            direction = default;
            return false;
        }
    }
}
