using System;
using UnityEngine;

namespace SnakeApple.Presentation.Snake
{
    [CreateAssetMenu(menuName = "Snake & Apple/Snake Sprite Skin", fileName = "SnakeSpriteSkin")]
    public sealed class SnakeSpriteSkin : ScriptableObject
    {
        [SerializeField] private Sprite head;
        [SerializeField, Min(1f)] private float headWidthInCells = 1f;
        [SerializeField, Min(0.1f)] private float headHeightInCells = 1f;
        [SerializeField] private Sprite[] bodyPhases = Array.Empty<Sprite>();
        [SerializeField] private Sprite[] cornerPhases = Array.Empty<Sprite>();
        [SerializeField] private Sprite[] tailPhases = Array.Empty<Sprite>();

        [Header("Spine Head Presentation (ADR-007)")]
        [SerializeField] private bool useSpineHead;
        [SerializeField] private string spineSkinName = "skin_cat";
        [SerializeField] private Spine.Unity.SkeletonDataAsset skeletonDataAsset;

        public Sprite Head => head;
        public float HeadWidthInCells => headWidthInCells;
        public float HeadHeightInCells => headHeightInCells;
        public int PhaseCount => bodyPhases?.Length ?? 0;

        public bool UseSpineHead => useSpineHead;
        public string SpineSkinName => spineSkinName;
        public Spine.Unity.SkeletonDataAsset SkeletonDataAsset => skeletonDataAsset;

        public bool IsConfigured => (head != null || UseSpineHead) &&
                                    HasSprites(bodyPhases) &&
                                    HasSprites(cornerPhases) &&
                                    HasSprites(tailPhases) &&
                                    bodyPhases.Length == cornerPhases.Length &&
                                    bodyPhases.Length == tailPhases.Length;

        public Sprite BodyAt(int phase) => AtPhase(bodyPhases, phase);
        public Sprite CornerAt(int phase) => AtPhase(cornerPhases, phase);
        public Sprite TailAt(int phase) => AtPhase(tailPhases, phase);

        private static Sprite AtPhase(Sprite[] sprites, int phase)
        {
            if (sprites == null || sprites.Length == 0) return null;
            var index = ((phase % sprites.Length) + sprites.Length) % sprites.Length;
            return sprites[index];
        }

        private static bool HasSprites(Sprite[] sprites)
        {
            if (sprites == null || sprites.Length == 0) return false;
            for (var index = 0; index < sprites.Length; index++)
            {
                if (sprites[index] == null) return false;
            }

            return true;
        }
    }
}
