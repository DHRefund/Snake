using System.Collections.Generic;
using SnakeApple.Domain.Board;
using UnityEngine;

namespace SnakeApple.Presentation.Blocks
{
    [CreateAssetMenu(fileName = "BlockSpriteSkin", menuName = "Snake Apple/Block Sprite Skin")]
    public sealed class BlockSpriteSkin : ScriptableObject
    {
        public int id;
        public int adRequired;
        public Sprite shopIcon;
        public Sprite previewShop;

        [Header("Legacy V1 edge layers")]
        public Sprite LeftTop;
        public Sprite LeftMid;
        public Sprite LeftBot;
        public Sprite TopLeft;
        public Sprite TopMid;
        public Sprite TopRight;
        public Sprite BotLeftCorner;
        public Sprite TopRightCorner;

        [Header("Legacy V1 body topology (1-16, three visual variants each)")]
        public List<Sprite> listBody1;
        public List<Sprite> listBody2;
        public List<Sprite> listBody3;
        public List<Sprite> listBody4;
        public List<Sprite> listBody5;
        public List<Sprite> listBody6;
        public List<Sprite> listBody7;
        public List<Sprite> listBody8;
        public List<Sprite> listBody9;
        public List<Sprite> listBody10;
        public List<Sprite> listBody11;
        public List<Sprite> listBody12;
        public List<Sprite> listBody13;
        public List<Sprite> listBody14;
        public List<Sprite> listBody15;
        public List<Sprite> listBody16;

        public bool IsConfigured => listBody1 != null && listBody1.Count > 0 &&
                                    listBody16 != null && listBody16.Count > 0;

        public Sprite Body(bool left, bool right, bool top, bool bottom, GridPos position)
        {
            // This is the truth-table ordering used by V1: list 1 means all four
            // neighbours exist; each missing neighbour adds its bit to the index.
            var topology = 1;
            if (!left) topology += 1;
            if (!right) topology += 2;
            if (!top) topology += 4;
            if (!bottom) topology += 8;

            var variants = BodyList(topology);
            if (variants == null || variants.Count == 0) return null;

            // Stable variation: moving the snake must never reshuffle wall artwork.
            var hash = unchecked(position.X * 73856093 ^ position.Y * 19349663);
            var variant = (hash & int.MaxValue) % variants.Count;
            return variants[variant];
        }

        private List<Sprite> BodyList(int topology)
        {
            switch (topology)
            {
                case 1: return listBody1;
                case 2: return listBody2;
                case 3: return listBody3;
                case 4: return listBody4;
                case 5: return listBody5;
                case 6: return listBody6;
                case 7: return listBody7;
                case 8: return listBody8;
                case 9: return listBody9;
                case 10: return listBody10;
                case 11: return listBody11;
                case 12: return listBody12;
                case 13: return listBody13;
                case 14: return listBody14;
                case 15: return listBody15;
                default: return listBody16;
            }
        }
    }
}
