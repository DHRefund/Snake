using UnityEngine;
using UnityEngine.UI;

namespace SnakeApple.Presentation.UI
{
    public enum SkinCellState
    {
        Locked,
        Owned,
        Selected
    }

    // One skin tile in the Cosmetics grid: preview, name, state frame and a price/use button face.
    public sealed class SkinCellView : MonoBehaviour
    {
        [SerializeField] private Image preview;
        [SerializeField] private Text nameText;
        [SerializeField] private Image actionFace;
        [SerializeField] private Text actionText;
        [SerializeField] private Image coinIcon;
        [SerializeField] private GameObject selectedFrame;
        [SerializeField] private GameObject selectedBadge;
        [SerializeField] private Sprite freeFace;
        [SerializeField] private Sprite priceFace;

        public void Configure(Image previewImage, Text name, Image face, Text action, Image coin,
            GameObject frame, GameObject badge, Sprite free, Sprite price)
        {
            preview = previewImage;
            nameText = name;
            actionFace = face;
            actionText = action;
            coinIcon = coin;
            selectedFrame = frame;
            selectedBadge = badge;
            freeFace = free;
            priceFace = price;
        }

        public void Apply(string displayName, Sprite previewSprite, SkinCellState state, int price)
        {
            if (nameText != null) nameText.text = displayName;
            if (preview != null)
            {
                preview.sprite = previewSprite;
                preview.enabled = previewSprite != null;
                // Locked skins are shown dimmed so owned ones stand out.
                preview.color = state == SkinCellState.Locked ? new Color(0.62f, 0.62f, 0.62f, 1f) : Color.white;
            }

            if (selectedFrame != null) selectedFrame.SetActive(state == SkinCellState.Selected);
            if (selectedBadge != null) selectedBadge.SetActive(state == SkinCellState.Selected);

            var locked = state == SkinCellState.Locked;
            if (actionFace != null) actionFace.sprite = locked ? priceFace : freeFace;
            if (coinIcon != null) coinIcon.gameObject.SetActive(locked);
            if (actionText != null)
            {
                actionText.text = state == SkinCellState.Selected ? "IN USE"
                    : state == SkinCellState.Owned ? "USE"
                    : price.ToString();
                actionText.rectTransform.anchoredPosition = new Vector2(locked ? 18f : 0f, 4f);
            }
        }
    }
}
