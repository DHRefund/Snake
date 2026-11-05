using System;
using System.IO;
using System.Linq;
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
    public static class UiSceneValidator
    {
        private static readonly SceneExpectation[] Expectations =
        {
            new SceneExpectation("Assets/Game/Scenes/Boot.unity", typeof(BootScreenController), "RetryButton", "ProgressFill"),
            new SceneExpectation("Assets/Game/Scenes/Home.unity", typeof(HomeScreenController), "PlayButton", "LevelsButton", "SettingsButton"),
            new SceneExpectation("Assets/Game/Scenes/LevelSelect.unity", typeof(LevelSelectScreenController), "NormalTab", "HardTab", "ChallengeTab", "Level_020"),
            new SceneExpectation("Assets/Game/Scenes/Gameplay.unity", typeof(GameplayScreenController), "BoardGrid", "HintButton", "PauseSettingsOverlay", "ConfirmHomeOverlay", "WinRewardText"),
            new SceneExpectation("Assets/Game/Scenes/Settings.unity", typeof(SettingsScreenController), "MusicToggle", "ReducedMotionToggle"),
            new SceneExpectation("Assets/Game/Scenes/Cosmetics.unity", typeof(CosmeticsScreenController), "Skin_00", "Skin_11")
        };

        [InitializeOnLoadMethod]
        private static void QueueValidation()
        {
            EditorApplication.delayCall += ValidateWhenReady;
        }

        [MenuItem("Snake Apple V2/Validate UI Scenes")]
        public static void ValidateFromMenu()
        {
            ValidateAll();
        }

        private static void ValidateWhenReady()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.delayCall += ValidateWhenReady;
                return;
            }

            var versionPath = Path.Combine(UnityEngine.Application.dataPath, "Game/UI_GENERATED_VERSION.txt");
            if (!File.Exists(versionPath) || File.ReadAllText(versionPath).Trim() != "6") return;

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(Expectations[0].Path) != null)
            {
                ValidateAll();
            }
        }

        private static void ValidateAll()
        {
            // Menu callbacks and delayed calls can cross a Play Mode transition after
            // ValidateWhenReady has checked the state. Never use EditorSceneManager then.
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isPlaying)
            {
                return;
            }

            Require(PlayerSettings.defaultInterfaceOrientation == UIOrientation.Portrait,
                "PlayerSettings", "portrait orientation");

            var expectedBuildPaths = Expectations.Select(expectation => expectation.Path).ToArray();
            var actualBuildPaths = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();
            if (!actualBuildPaths.SequenceEqual(expectedBuildPaths))
            {
                Debug.LogError("[Snake Apple V2] Build Settings scene order is invalid. Boot must be scene 0.");
                return;
            }

            var originalScene = SceneManager.GetActiveScene();
            try
            {
                foreach (var expectation in Expectations)
                {
                    ValidateScene(expectation);
                }
            }
            finally
            {
                if (originalScene.IsValid() && originalScene.isLoaded)
                {
                    SceneManager.SetActiveScene(originalScene);
                }
            }

            Debug.Log("[Snake Apple V2] UI scene validation passed: 6 scenes, Boot entry, no missing scripts.");

            if (!originalScene.IsValid() ||
                string.IsNullOrEmpty(originalScene.path) ||
                originalScene.path.EndsWith("/Prototype.unity", StringComparison.Ordinal))
            {
                EditorSceneManager.OpenScene(Expectations[0].Path, OpenSceneMode.Single);
            }
        }

        private static void ValidateScene(SceneExpectation expectation)
        {
            var scene = SceneManager.GetSceneByPath(expectation.Path);
            var wasAlreadyLoaded = scene.IsValid() && scene.isLoaded;
            if (!wasAlreadyLoaded)
            {
                scene = EditorSceneManager.OpenScene(expectation.Path, OpenSceneMode.Additive);
            }

            try
            {
                var roots = scene.GetRootGameObjects();
                Require(roots.SelectMany(root => root.GetComponentsInChildren<Canvas>(true)).Any(), expectation.Path, "Canvas");
                Require(roots.SelectMany(root => root.GetComponentsInChildren<EventSystem>(true)).Any(), expectation.Path, "EventSystem");
                var scalers = roots.SelectMany(root => root.GetComponentsInChildren<CanvasScaler>(true)).ToArray();
                Require(scalers.Any(), expectation.Path, "CanvasScaler");
                Require(scalers.All(scaler => scaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize &&
                                              scaler.referenceResolution == new Vector2(1080, 1920) &&
                                              Mathf.Approximately(scaler.matchWidthOrHeight, 0.45f)),
                    expectation.Path, "1080x1920 ScaleWithScreenSize");
                Require(roots.SelectMany(root => root.GetComponentsInChildren<SafeAreaPanel>(true)).Any(),
                    expectation.Path, "SafeAreaPanel");
                EnsureCamera(scene, roots, expectation.Path);
                Require(
                    roots.SelectMany(root => root.GetComponentsInChildren(expectation.ControllerType, true)).Any(),
                    expectation.Path,
                    expectation.ControllerType.Name);
                var objectNames = roots.SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                    .Select(item => item.name)
                    .ToHashSet();
                foreach (var objectName in expectation.RequiredObjectNames)
                {
                    Require(objectNames.Contains(objectName), expectation.Path, objectName);
                }

                var missingScripts = roots.Sum(CountMissingScriptsRecursively);
                if (missingScripts != 0)
                {
                    throw new InvalidOperationException($"{expectation.Path} contains {missingScripts} missing scripts.");
                }
            }
            finally
            {
                if (!wasAlreadyLoaded)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        private static void EnsureCamera(Scene scene, GameObject[] roots, string path)
        {
            if (roots.SelectMany(root => root.GetComponentsInChildren<Camera>(true)).Any())
            {
                return;
            }

            var cameraObject = new GameObject("UICamera", typeof(Camera));
            SceneManager.MoveGameObjectToScene(cameraObject, scene);
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color32(12, 65, 69, 255);
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.cullingMask = 0;
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.LogWarning($"[Snake Apple V2] Repaired missing UICamera in {path}.");
        }

        private static void Require(bool condition, string path, string requirement)
        {
            if (!condition)
            {
                throw new InvalidOperationException($"{path} is missing {requirement}.");
            }
        }

        private static int CountMissingScriptsRecursively(GameObject root)
        {
            var count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(root);
            for (var index = 0; index < root.transform.childCount; index++)
            {
                count += CountMissingScriptsRecursively(root.transform.GetChild(index).gameObject);
            }

            return count;
        }

        private sealed class SceneExpectation
        {
            public SceneExpectation(string path, Type controllerType, params string[] requiredObjectNames)
            {
                Path = path;
                ControllerType = controllerType;
                RequiredObjectNames = requiredObjectNames ?? Array.Empty<string>();
            }

            public string Path { get; }
            public Type ControllerType { get; }
            public string[] RequiredObjectNames { get; }
        }
    }
}
