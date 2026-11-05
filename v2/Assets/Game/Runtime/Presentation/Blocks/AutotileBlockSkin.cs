using System;
using System.Collections.Generic;
using SnakeApple.Domain.Board;
using UnityEngine;

namespace SnakeApple.Presentation.Blocks
{
    [CreateAssetMenu(fileName = "AutotileBlockSkin", menuName = "Snake & Apple/Autotile Block Skin")]
    public sealed class AutotileBlockSkin : ScriptableObject
    {
        [Serializable]
        public sealed class MaskVariants
        {
            [SerializeField] private List<Sprite> variants = new List<Sprite>();

            public List<Sprite> Variants => variants;

            public Sprite PickStable(GridPos position)
            {
                if (variants == null || variants.Count == 0) return null;
                if (variants.Count == 1) return variants[0];
                var hash = unchecked(position.X * 73856093 ^ position.Y * 19349663);
                var index = (hash & int.MaxValue) % variants.Count;
                return variants[index];
            }
        }

        [SerializeField] private MaskVariants[] masks = new MaskVariants[16];

        public bool IsConfigured
        {
            get
            {
                if (masks == null || masks.Length < 16) return false;
                for (var i = 0; i < 16; i++)
                {
                    if (masks[i] == null || masks[i].Variants == null || masks[i].Variants.Count == 0)
                        return false;
                }
                return true;
            }
        }

        public Sprite GetSprite(int mask, GridPos position)
        {
            if (masks == null || mask < 0 || mask >= masks.Length || masks[mask] == null)
                return null;
            return masks[mask].PickStable(position);
        }

        public void SetMaskVariants(int mask, List<Sprite> spriteList)
        {
            if (masks == null || masks.Length < 16)
                masks = new MaskVariants[16];

            if (mask >= 0 && mask < 16)
            {
                masks[mask] = new MaskVariants();
                masks[mask].Variants.AddRange(spriteList);
            }
        }
    }
}
