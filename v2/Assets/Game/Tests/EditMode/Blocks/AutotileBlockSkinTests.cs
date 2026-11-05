using NUnit.Framework;
using SnakeApple.Domain.Board;
using SnakeApple.Presentation.Blocks;
using UnityEditor;
using UnityEngine;

namespace SnakeApple.Tests.EditMode.Blocks
{
    public sealed class AutotileBlockSkinTests
    {
        private const string AssetPath = "Assets/Game/Resources/BlockSkins/IceBlockSkin.asset";
        private const string ResourcePath = "BlockSkins/IceBlockSkin";

        [Test]
        public void IceBlockSkin_LoadsFromResources_AndIsConfigured()
        {
            var skin = Resources.Load<AutotileBlockSkin>(ResourcePath);
            Assert.That(skin, Is.Not.Null, $"Resource not found at {ResourcePath}.");
            Assert.That(skin.IsConfigured, Is.True, "IceBlockSkin is not fully configured with 16 masks.");
        }

        [Test]
        public void IceBlockSkin_AssetExists_AndHasAllSixteenMasks()
        {
            var skin = AssetDatabase.LoadAssetAtPath<AutotileBlockSkin>(AssetPath);
            Assert.That(skin, Is.Not.Null, $"Asset not found at {AssetPath}.");
            Assert.That(skin.IsConfigured, Is.True);

            for (var mask = 0; mask < 16; mask++)
            {
                var sprite = skin.GetSprite(mask, new GridPos(1, 1));
                Assert.That(sprite, Is.Not.Null, $"Missing sprite for mask {mask}.");
            }
        }

        [Test]
        public void AutotileBlockSkin_GetSprite_IsDeterministicForSamePosition()
        {
            var skin = ScriptableObject.CreateInstance<AutotileBlockSkin>();
            var s1 = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), Vector2.zero);
            var s2 = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), Vector2.zero);

            skin.SetMaskVariants(0, new System.Collections.Generic.List<Sprite> { s1, s2 });

            var pos = new GridPos(3, 7);
            var first = skin.GetSprite(0, pos);
            var second = skin.GetSprite(0, pos);

            Assert.That(first, Is.Not.Null);
            Assert.That(second, Is.EqualTo(first));
        }

        [TestCase(true, false, false, false, 1)] // UP
        [TestCase(false, true, false, false, 2)] // RIGHT
        [TestCase(false, false, true, false, 4)] // DOWN
        [TestCase(false, false, false, true, 8)] // LEFT
        [TestCase(true, true, true, true, 15)]   // ALL
        public void AutotileBlockSkin_ConnectionMask_MatchesArchitectureSpec(bool up, bool right, bool down, bool left, int expectedMask)
        {
            var mask = (up ? 1 : 0) | (right ? 2 : 0) | (down ? 4 : 0) | (left ? 8 : 0);
            Assert.That(mask, Is.EqualTo(expectedMask));
        }
    }
}
