using NUnit.Framework;
using SnakeApple.Presentation.Items;
using UnityEditor;
using UnityEngine;

namespace SnakeApple.Tests.EditMode
{
    public sealed class GameplayItemSkinTests
    {
        private const string AssetPath = "Assets/Game/Resources/ItemSkins/DefaultItemSkin.asset";
        private const string ResourcePath = "ItemSkins/DefaultItemSkin";

        [Test]
        public void DefaultItemSkin_LoadsFromResources_AndIsConfigured()
        {
            var skin = Resources.Load<GameplayItemSkin>(ResourcePath);
            Assert.That(skin, Is.Not.Null, $"Resource not found at {ResourcePath}.");
            Assert.That(skin.IsConfigured, Is.True, "DefaultItemSkin is not configured with essential sprites.");
        }

        [Test]
        public void DefaultItemSkin_AssetExists_AndHasPrimarySprites()
        {
            var skin = AssetDatabase.LoadAssetAtPath<GameplayItemSkin>(AssetPath);
            Assert.That(skin, Is.Not.Null, $"Asset not found at {AssetPath}.");
            Assert.That(skin.IsConfigured, Is.True);

            Assert.That(skin.AppleNormal, Is.Not.Null, "AppleNormal is missing.");
            Assert.That(skin.ExitPortal, Is.Not.Null, "ExitPortal is missing.");
            Assert.That(skin.Spike, Is.Not.Null, "Spike is missing.");
            Assert.That(skin.Rock, Is.Not.Null, "Rock is missing.");
            Assert.That(skin.Bomb, Is.Not.Null, "Bomb is missing.");
            Assert.That(skin.ToxicAcid, Is.Not.Null, "ToxicAcid is missing.");
            Assert.That(skin.SpiderWeb, Is.Not.Null, "SpiderWeb is missing.");
        }
    }
}
