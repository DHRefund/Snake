using UnityEngine;
using UnityEngine.UI;

namespace SnakeApple.Presentation.UI
{
    // HUD exit-portal icon: dim while apples remain (with a count badge), bright once the exit is open.
    public sealed class ExitStatusView : MonoBehaviour
    {
        private static readonly Color ClosedTint = new Color(0.5f, 0.5f, 0.55f, 1f);

        [SerializeField] private Image frame;
        [SerializeField] private Image portal;
        [SerializeField] private GameObject badge;

        public void Configure(Image frameImage, Image portalImage, GameObject badgeObject)
        {
            frame = frameImage;
            portal = portalImage;
            badge = badgeObject;
        }

        public void Apply(int remainingApples, bool exitAlwaysOpen)
        {
            var open = exitAlwaysOpen || remainingApples <= 0;
            var tint = open ? Color.white : ClosedTint;
            if (frame != null) frame.color = tint;
            if (portal != null) portal.color = tint;
            if (badge != null) badge.SetActive(!open);
        }
    }
}
