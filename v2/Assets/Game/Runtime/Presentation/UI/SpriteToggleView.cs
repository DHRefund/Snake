using UnityEngine;
using UnityEngine.UI;

namespace SnakeApple.Presentation.UI
{
    // Draws a Toggle with sprites: a square background (on/off) plus an optional icon or ON/OFF text.
    [RequireComponent(typeof(Toggle))]
    public sealed class SpriteToggleView : MonoBehaviour
    {
        [SerializeField] private Image background;
        [SerializeField] private Sprite onSprite;
        [SerializeField] private Sprite offSprite;
        [SerializeField] private Image icon;
        [SerializeField] private Sprite iconOn;
        [SerializeField] private Sprite iconOff;
        [SerializeField] private Text stateLabel;

        private Toggle toggle;

        public void Configure(Image backgroundImage, Sprite on, Sprite off,
            Image iconImage, Sprite iconOnSprite, Sprite iconOffSprite, Text label)
        {
            background = backgroundImage;
            onSprite = on;
            offSprite = off;
            icon = iconImage;
            iconOn = iconOnSprite;
            iconOff = iconOffSprite;
            stateLabel = label;
        }

        private void Awake()
        {
            toggle = GetComponent<Toggle>();
            toggle.onValueChanged.AddListener(_ => Refresh());
        }

        private void OnEnable()
        {
            if (toggle == null) toggle = GetComponent<Toggle>();
            Refresh();
        }

        // The screen sets the saved value without notifying, so also refresh on the first frame.
        private void Start() => Refresh();

        private void Refresh()
        {
            var isOn = toggle.isOn;
            if (background != null)
            {
                var sprite = isOn ? onSprite : offSprite;
                if (sprite != null) background.sprite = sprite;
            }

            if (icon != null)
            {
                var sprite = isOn ? iconOn : iconOff;
                icon.enabled = sprite != null;
                if (sprite != null) icon.sprite = sprite;
            }

            if (stateLabel != null) stateLabel.text = isOn ? "ON" : "OFF";
        }
    }
}
