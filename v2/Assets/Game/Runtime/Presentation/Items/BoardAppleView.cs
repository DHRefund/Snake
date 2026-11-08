using SnakeApple.Infrastructure.Save;
using UnityEngine;
using UnityEngine.UI;

namespace SnakeApple.Presentation.Items
{
    // One apple on the board. It lives in the item overlay (above walls, below the snake) so wall tiles that
    // spill into the neighbouring cell never cover it. Handles placement, idle motion, spawn and eat effects.
    // Apples resting on a wall get a contact shadow and stay still; floating apples bob and cast no shadow.
    public sealed class BoardAppleView : MonoBehaviour
    {
        private enum Phase
        {
            Idle,
            Appearing,
            Eating
        }

        // Layout constants, all in cell-size units measured from the cell centre.
        private const float BodyFrame = 0.92f;          // sprite frame size (art fills ~94% of the frame)
        private const float GroundedBase = -0.34f;      // frame bottom: on the 25% top face of the wall below, lifted like other items
        private const float FloatingBase = -0.40f;      // frame centred in the cell, lifted slightly
        private const float RightBlockedShift = -0.06f; // keep clear of the left face of a wall on the right
        private const float BobAmplitude = 0.03f;
        private const float AppearDuration = 0.28f;
        private const float EatDuration = 0.30f;
        private const int ParticleCount = 6;

        private static readonly Color NormalBurst = new Color(1f, 0.45f, 0.35f, 1f);
        private static readonly Color VipBurst = new Color(1f, 0.86f, 0.32f, 1f);

        private RectTransform root;
        private RectTransform body;
        private Image bodyImage;
        private RectTransform shadow;
        private Image shadowImage;
        private RectTransform glow;
        private Image glowImage;
        private RectTransform[] sparkles;
        private Image[] sparkleImages;
        private RectTransform[] particles;
        private Image[] particleImages;
        private Sprite sparkleSprite;
        private bool vip;
        private bool grounded;
        private bool reducedMotion;
        private float cell = 64f;
        private float phaseOffset;
        private float phaseTime;
        private Phase phase = Phase.Idle;

        public static BoardAppleView Create(Transform parent, int seed, Sprite apple, bool isVip, GameplayItemSkin skin)
        {
            var node = new GameObject(isVip ? "VipApple" : "Apple", typeof(RectTransform));
            node.transform.SetParent(parent, false);
            var view = node.AddComponent<BoardAppleView>();
            view.Build(seed, apple, isVip, skin);
            return view;
        }

        private void Build(int seed, Sprite apple, bool isVip, GameplayItemSkin skin)
        {
            vip = isVip;
            phaseOffset = (seed & 255) * 0.0245f;
            reducedMotion = LocalProfileStore.ReducedMotion;
            sparkleSprite = skin != null ? skin.Sparkle : null;

            root = (RectTransform)transform;
            root.anchorMin = root.anchorMax = root.pivot = new Vector2(0.5f, 0.5f);

            shadow = MakeImage("Shadow", skin != null ? skin.AppleShadow : null, new Vector2(0.5f, 0.5f), out shadowImage);
            glow = MakeImage("Glow", skin != null ? skin.Glow : null, new Vector2(0.5f, 0.5f), out glowImage);
            glowImage.color = new Color(1f, 0.82f, 0.3f, 0f);
            glow.gameObject.SetActive(vip && skin != null && skin.Glow != null);
            body = MakeImage("Body", apple, new Vector2(0.5f, 0f), out bodyImage);
            bodyImage.preserveAspect = true;

            if (vip && sparkleSprite != null)
            {
                sparkles = new RectTransform[3];
                sparkleImages = new Image[3];
                for (var i = 0; i < 3; i++)
                {
                    sparkles[i] = MakeImage("Sparkle" + i, sparkleSprite, new Vector2(0.5f, 0.5f), out sparkleImages[i]);
                    sparkleImages[i].color = new Color(1f, 0.97f, 0.8f, 0f);
                }
            }
        }

        private RectTransform MakeImage(string objectName, Sprite sprite, Vector2 pivot, out Image image)
        {
            var node = new GameObject(objectName, typeof(RectTransform), typeof(Image));
            node.transform.SetParent(transform, false);
            image = node.GetComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = false;
            image.enabled = sprite != null;
            var rect = (RectTransform)node.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = pivot;
            return rect;
        }

        // localCenter is the cell centre in the item overlay's local space.
        public void Place(Vector3 localCenter, float cellSize, bool restsOnWall, bool wallOnRight)
        {
            cell = Mathf.Max(8f, cellSize);
            grounded = restsOnWall;
            root.localPosition = localCenter + new Vector3(wallOnRight ? RightBlockedShift * cell : 0f, 0f, 0f);
            root.sizeDelta = new Vector2(cell, cell);

            body.sizeDelta = new Vector2(cell * BodyFrame, cell * BodyFrame);
            var baseY = (grounded ? GroundedBase : FloatingBase) * cell;
            body.anchoredPosition = new Vector2(0f, baseY);

            shadow.sizeDelta = new Vector2(cell * 0.66f, cell * 0.17f);
            shadow.anchoredPosition = new Vector2(0f, baseY + cell * 0.035f);
            shadow.gameObject.SetActive(grounded);

            glow.sizeDelta = new Vector2(cell * 1.15f, cell * 1.15f);
            glow.anchoredPosition = new Vector2(0f, baseY + cell * 0.45f);

            if (sparkles == null) return;
            PlaceSparkle(0, new Vector2(-0.30f, 0.30f), 0.22f, baseY);
            PlaceSparkle(1, new Vector2(0.32f, 0.16f), 0.17f, baseY);
            PlaceSparkle(2, new Vector2(0.04f, 0.46f), 0.13f, baseY);
        }

        private void PlaceSparkle(int index, Vector2 offset, float size, float baseY)
        {
            sparkles[index].sizeDelta = new Vector2(cell * size, cell * size);
            // offset is relative to the apple centre, which sits 0.46 cell above the frame bottom.
            sparkles[index].anchoredPosition = new Vector2(offset.x * cell, baseY + (0.46f + offset.y) * cell);
        }

        public void PlayAppear()
        {
            if (reducedMotion) return;
            phase = Phase.Appearing;
            phaseTime = 0f;
        }

        // Pops the apple and detaches nothing: the view destroys itself when the effect ends.
        public void PlayEat()
        {
            if (phase == Phase.Eating) return;
            if (reducedMotion)
            {
                Destroy(gameObject);
                return;
            }

            phase = Phase.Eating;
            phaseTime = 0f;
            if (sparkleSprite == null) return;

            var count = vip ? ParticleCount + 2 : ParticleCount;
            particles = new RectTransform[count];
            particleImages = new Image[count];
            for (var i = 0; i < count; i++)
            {
                particles[i] = MakeImage("Particle" + i, sparkleSprite, new Vector2(0.5f, 0.5f), out particleImages[i]);
                particleImages[i].color = vip ? VipBurst : NormalBurst;
                particles[i].sizeDelta = new Vector2(cell * 0.2f, cell * 0.2f);
                particles[i].anchoredPosition = body.anchoredPosition + new Vector2(0f, cell * 0.42f);
            }
        }

        private void Update()
        {
            var t = Time.unscaledTime + phaseOffset;
            var stateScale = 1f;
            var stateAlpha = 1f;

            if (phase == Phase.Appearing)
            {
                phaseTime += Time.unscaledDeltaTime;
                var k = Mathf.Clamp01(phaseTime / AppearDuration);
                stateScale = EaseOutBack(k);
                stateAlpha = Mathf.Clamp01(k * 3f);
                if (k >= 1f) phase = Phase.Idle;
            }
            else if (phase == Phase.Eating)
            {
                phaseTime += Time.unscaledDeltaTime;
                var k = Mathf.Clamp01(phaseTime / EatDuration);
                stateScale = k < 0.3f ? 1f + 0.25f * (k / 0.3f) : 1.25f * (1f - (k - 0.3f) / 0.7f);
                stateAlpha = k < 0.5f ? 1f : 1f - (k - 0.5f) / 0.5f;
                UpdateParticles(k);
                if (k >= 1f)
                {
                    Destroy(gameObject);
                    return;
                }
            }

            var bob = 0f;
            var squash = 0f;
            if (!reducedMotion && phase != Phase.Eating)
            {
                var wave = Mathf.Sin(t * 2.4f);
                bob = grounded ? 0f : wave * BobAmplitude * cell;
                squash = (grounded ? 0.012f : 0.02f) * wave;
            }

            var baseY = (grounded ? GroundedBase : FloatingBase) * cell;
            body.anchoredPosition = new Vector2(0f, baseY + bob);
            body.localScale = new Vector3((1f - squash) * stateScale, (1f + squash) * stateScale, 1f);
            bodyImage.color = new Color(1f, 1f, 1f, stateAlpha);

            if (grounded)
            {
                shadow.localScale = new Vector3(stateScale * (1f + squash), stateScale, 1f);
                shadowImage.color = new Color(1f, 1f, 1f, stateAlpha);
            }

            if (vip) UpdateVip(t, stateScale, stateAlpha);
        }

        private void UpdateVip(float t, float stateScale, float stateAlpha)
        {
            if (glow.gameObject.activeSelf)
            {
                var pulse = reducedMotion ? 0f : Mathf.Sin(t * 2f);
                glow.localScale = Vector3.one * (stateScale * (1f + 0.06f * pulse));
                glowImage.color = new Color(1f, 0.82f, 0.3f, (0.30f + 0.12f * pulse) * stateAlpha);
            }

            if (sparkles == null) return;
            for (var i = 0; i < sparkles.Length; i++)
            {
                var twinkle = reducedMotion ? 0.55f : Mathf.Max(0f, Mathf.Sin(t * 2.6f + i * 2.1f));
                sparkles[i].localScale = Vector3.one * (twinkle * stateScale);
                sparkles[i].localEulerAngles = new Vector3(0f, 0f, reducedMotion ? 0f : t * 25f);
                sparkleImages[i].color = new Color(1f, 0.97f, 0.8f, twinkle * stateAlpha);
            }
        }

        private void UpdateParticles(float k)
        {
            if (particles == null) return;
            var origin = body.anchoredPosition + new Vector2(0f, cell * 0.42f);
            for (var i = 0; i < particles.Length; i++)
            {
                var angle = (i / (float)particles.Length) * Mathf.PI * 2f + 0.4f;
                var distance = cell * 0.62f * (1f - Mathf.Pow(1f - k, 2f));
                particles[i].anchoredPosition = origin + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
                particles[i].localScale = Vector3.one * (1f - k * 0.6f);
                particles[i].localEulerAngles = new Vector3(0f, 0f, k * 180f);
                var color = particleImages[i].color;
                color.a = 1f - k;
                particleImages[i].color = color;
            }
        }

        private static float EaseOutBack(float k)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            var x = k - 1f;
            return 1f + c3 * x * x * x + c1 * x * x;
        }
    }
}
