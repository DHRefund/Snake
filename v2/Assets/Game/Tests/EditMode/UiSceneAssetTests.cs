using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SnakeApple.Presentation.Screens;
using SnakeApple.Presentation.Snake;
using SnakeApple.Presentation.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SnakeApple.Tests.EditMode
{
    public sealed class UiSceneAssetTests
    {
        private static readonly string[] RequiredScenes =
        {
            "Assets/Game/Scenes/Boot.unity",
            "Assets/Game/Scenes/Home.unity",
            "Assets/Game/Scenes/LevelSelect.unity",
            "Assets/Game/Scenes/Gameplay.unity",
            "Assets/Game/Scenes/Settings.unity",
            "Assets/Game/Scenes/Cosmetics.unity"
        };

        [Test]
        public void BuildSettings_StartsAtBootAndContainsAllScreens()
        {
            var enabledScenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            Assert.That(enabledScenes, Is.EqualTo(RequiredScenes));
            Assert.That(PlayerSettings.defaultInterfaceOrientation, Is.EqualTo(UIOrientation.Portrait));
        }

        [TestCaseSource(nameof(SceneControllerCases))]
        public void Scene_HasCanvasEventSystemControllerAndNoMissingScripts(string path, System.Type controllerType)
        {
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            try
            {
                var roots = scene.GetRootGameObjects();
                Assert.That(roots.SelectMany(root => root.GetComponentsInChildren<Canvas>(true)), Is.Not.Empty);
                Assert.That(roots.SelectMany(root => root.GetComponentsInChildren<EventSystem>(true)), Is.Not.Empty);
                Assert.That(roots.SelectMany(root => root.GetComponentsInChildren<SafeAreaPanel>(true)), Is.Not.Empty);
                var scalers = roots.SelectMany(root => root.GetComponentsInChildren<CanvasScaler>(true)).ToArray();
                Assert.That(scalers, Is.Not.Empty);
                Assert.That(scalers.All(scaler => scaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize &&
                                                  scaler.referenceResolution == new Vector2(1080, 1920) &&
                                                  Mathf.Approximately(scaler.matchWidthOrHeight, 0.45f)), Is.True);
                Assert.That(
                    roots.SelectMany(root => root.GetComponentsInChildren(controllerType, true)),
                    Is.Not.Empty,
                    $"{path} is missing {controllerType.Name}.");

                var missingScripts = roots.Sum(CountMissingScriptsRecursively);
                Assert.That(missingScripts, Is.Zero, $"{path} contains missing scripts.");
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void GameplayScene_ContainsCompletePortraitHudAndModalFlow()
        {
            var scene = EditorSceneManager.OpenScene(RequiredScenes[3], OpenSceneMode.Additive);
            try
            {
                var names = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                    .Select(item => item.name)
                    .ToHashSet();
                foreach (var required in new[]
                         {
                             "BoardGrid", "UndoButton", "RestartButton", "HintButton", "PauseOverlay",
                             "PauseSettingsOverlay", "ConfirmHomeOverlay", "WinOverlay", "LoseOverlay",
                             "WinRewardText", "SnakeOverlay"
                         })
                {
                    Assert.That(names.Contains(required), Is.True, $"Gameplay scene is missing {required}.");
                }

                var controller = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<GameplayScreenController>(true))
                    .Single();
                Assert.That(controller.HasLegacySnakeDemoSprites, Is.True,
                    "Gameplay scene is missing one or more V1 legacy snake demo sprites.");
                Assert.That(controller.HasAutotileBlockSkin, Is.True,
                    "Gameplay scene is missing the production autotile block skin.");
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void LegacyV1SnakeSkin_HasMatchingEightPhaseGeometry()
        {
            const string path =
                "Assets/Game/Resources/SnakeSkins/LegacyV1SnakeSkin.asset";
            var skin = AssetDatabase.LoadAssetAtPath<SnakeSpriteSkin>(path);

            Assert.That(skin, Is.Not.Null);
            Assert.That(skin.IsConfigured, Is.True);
            Assert.That(skin.PhaseCount, Is.EqualTo(8));
            Assert.That(skin.HeadWidthInCells, Is.EqualTo(1.71f).Within(0.001f));
            for (var phase = 0; phase < skin.PhaseCount; phase++)
            {
                Assert.That(skin.BodyAt(phase), Is.Not.Null, $"Missing body phase {phase}.");
                Assert.That(skin.CornerAt(phase), Is.Not.Null, $"Missing corner phase {phase}.");
                Assert.That(skin.TailAt(phase), Is.Not.Null, $"Missing tail phase {phase}.");
            }
        }

        [Test]
        public void CatSnakeSkin_IsConfiguredProperly()
        {
            const string path = "Assets/Game/Resources/SnakeSkins/CatSnakeSkin.asset";
            var skin = AssetDatabase.LoadAssetAtPath<SnakeSpriteSkin>(path);

            Assert.That(skin, Is.Not.Null);
            Assert.That(skin.IsConfigured, Is.True);
            Assert.That(skin.PhaseCount, Is.EqualTo(1));
            Assert.That(skin.HeadWidthInCells, Is.EqualTo(1f).Within(0.001f));
            Assert.That(skin.HeadHeightInCells, Is.EqualTo(1f).Within(0.001f));
            Assert.That(skin.Head, Is.Not.Null);
            Assert.That(skin.BodyAt(0), Is.Not.Null);
            Assert.That(skin.CornerAt(0), Is.Not.Null);
            Assert.That(skin.TailAt(0), Is.Not.Null);
        }

        private static IEnumerable<TestCaseData> SceneControllerCases()
        {
            yield return new TestCaseData(RequiredScenes[0], typeof(BootScreenController));
            yield return new TestCaseData(RequiredScenes[1], typeof(HomeScreenController));
            yield return new TestCaseData(RequiredScenes[2], typeof(LevelSelectScreenController));
            yield return new TestCaseData(RequiredScenes[3], typeof(GameplayScreenController));
            yield return new TestCaseData(RequiredScenes[4], typeof(SettingsScreenController));
            yield return new TestCaseData(RequiredScenes[5], typeof(CosmeticsScreenController));
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
    }
}
