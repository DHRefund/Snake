using UnityEngine;
using UnityEngine.UI;

namespace SnakeApple.Presentation.UI
{
    public enum LevelSlotState
    {
        Locked,
        Available,
        Completed
    }

    // Visual state of one cell in the Level Select grid. The scene assigns the sprites.
    public sealed class LevelSlotView : MonoBehaviour
    {
        [SerializeField] private Image background;
        [SerializeField] private Text label;
        [SerializeField] private Sprite availableSprite;
        [SerializeField] private Sprite completedSprite;
        [SerializeField] private Sprite lockedSprite;

        public void Configure(Image backgroundImage, Text labelText, Sprite available, Sprite completed, Sprite locked)
        {
            background = backgroundImage;
            label = labelText;
            availableSprite = available;
            completedSprite = completed;
            lockedSprite = locked;
        }

        public void Apply(LevelSlotState state, int level)
        {
            switch (state)
            {
                case LevelSlotState.Locked:
                    SetVisual(lockedSprite, Color.white, string.Empty);
                    break;
                case LevelSlotState.Completed:
                    SetVisual(completedSprite, new Color(0.72f, 1f, 0.72f, 1f),
                        $"{level}\n<size=26>DONE</size>");
                    break;
                default:
                    SetVisual(availableSprite, Color.white, level.ToString());
                    break;
            }
        }

        private void SetVisual(Sprite sprite, Color tint, string text)
        {
            if (background != null)
            {
                if (sprite != null) background.sprite = sprite;
                background.color = tint;
            }

            if (label != null) label.text = text;
        }
    }
}
