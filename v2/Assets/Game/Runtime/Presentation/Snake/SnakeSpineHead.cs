using Spine;
using Spine.Unity;
using UnityEngine;

namespace SnakeApple.Presentation.Snake
{
    /// <summary>
    /// Presentation component managing the Spine 2D expressive head rig (ADR-007).
    /// Handles FSM animation states: idle, open_mouth, eating, hit_wall, die, win.
    /// Supports dynamic skin switching (skin_cat, skin_dog, skin_tiger, etc.).
    /// </summary>
    public sealed class SnakeSpineHead : MonoBehaviour
    {
        [Header("Spine Components")]
        [SerializeField] private SkeletonGraphic skeletonGraphic;

        [Header("Skin Settings")]
        [SerializeField] private string currentSkinName = "skin_cat";

        [Header("Animation Names")]
        [SerializeField] private string idleAnim = "idle_2";
        [SerializeField] private string openMouthAnim = "open_mouth";
        [SerializeField] private string eatAnim = "eating";
        [SerializeField] private string hitWallAnim = "hit_wall";
        [SerializeField] private string dieAnim = "die";
        [SerializeField] private string winAnim = "win";

        private Skeleton skeleton;
        private Spine.AnimationState animState;
        private bool isInitialized;

        public bool IsInitialized => isInitialized;
        public string CurrentSkinName => currentSkinName;
        public SkeletonGraphic SkeletonGraphic => skeletonGraphic;

        private static SkeletonDataAsset cachedSkeletonData;

        public static SkeletonDataAsset GetOrCreateSkeletonData()
        {
            if (cachedSkeletonData != null) return cachedSkeletonData;

            var jsonText = Resources.Load<TextAsset>("Spine/skeleton-data");
            var atlasText = Resources.Load<TextAsset>("Spine/skeleton.atlas");
            var mat1 = Resources.Load<Material>("Spine/skeleton_skeleton");
            var mat2 = Resources.Load<Material>("Spine/skeleton_skeleton_2");

            var tex1 = Resources.Load<Texture2D>("Spine/skeleton");
            var tex2 = Resources.Load<Texture2D>("Spine/skeleton_2");

            if (mat1 == null)
            {
                var s = Shader.Find("Spine/SkeletonGraphic") ?? Shader.Find("Spine/Skeleton");
                if (s != null) mat1 = new Material(s);
            }
            if (mat1 != null)
            {
                if (tex1 != null && mat1.mainTexture == null) mat1.mainTexture = tex1;
                if (mat1.mainTexture != null) mat1.mainTexture.name = "skeleton";
                mat1.name = "skeleton";
            }

            if (mat2 == null && tex2 != null)
            {
                var s = Shader.Find("Spine/SkeletonGraphic") ?? Shader.Find("Spine/Skeleton");
                if (s != null) mat2 = new Material(s);
            }
            if (mat2 != null)
            {
                if (tex2 != null && mat2.mainTexture == null) mat2.mainTexture = tex2;
                if (mat2.mainTexture != null) mat2.mainTexture.name = "skeleton_2";
                mat2.name = "skeleton_2";
            }

            if (jsonText != null && atlasText != null && mat1 != null)
            {
                var materials = mat2 != null ? new[] { mat1, mat2 } : new[] { mat1 };
                var atlasAsset = SpineAtlasAsset.CreateRuntimeInstance(atlasText, materials, true);
                cachedSkeletonData = SkeletonDataAsset.CreateRuntimeInstance(jsonText, atlasAsset, true, 0.01f);
                Debug.Log($"[SnakeSpineHead] Created runtime SkeletonDataAsset successfully (atlas pages: {materials.Length}).");
            }
            else
            {
                Debug.LogWarning($"[SnakeSpineHead] Spine resource missing: json={jsonText != null}, atlas={atlasText != null}, mat1={mat1 != null}");
            }
            return cachedSkeletonData;
        }

        public void Initialize(SkeletonDataAsset skeletonDataAsset = null, string skinName = null)
        {
            try
            {
                if (skeletonGraphic == null)
                {
                    skeletonGraphic = GetComponent<SkeletonGraphic>();
                }

                if (skeletonGraphic == null)
                {
                    skeletonGraphic = gameObject.AddComponent<SkeletonGraphic>();
                }

                if (!string.IsNullOrEmpty(skinName))
                {
                    currentSkinName = skinName;
                }

                SkeletonDataAsset validAsset = null;
                try
                {
                    if (skeletonDataAsset != null && skeletonDataAsset.GetSkeletonData(true) != null)
                    {
                        validAsset = skeletonDataAsset;
                    }
                }
                catch
                {
                    validAsset = null;
                }

                if (validAsset == null)
                {
                    validAsset = GetOrCreateSkeletonData();
                }

                skeletonGraphic.skeletonDataAsset = validAsset;

                if (skeletonGraphic.skeletonDataAsset != null)
                {
                    skeletonGraphic.allowMultipleCanvasRenderers = true;
                    if (skeletonGraphic.material == skeletonGraphic.defaultMaterial)
                    {
                        var defaultMat = Resources.Load<Material>("Spine/SkeletonGraphicDefault");
                        if (defaultMat != null)
                        {
                            skeletonGraphic.material = new Material(defaultMat);
                        }
                        else
                        {
                            var shader = Shader.Find("Spine/SkeletonGraphic");
                            if (shader != null)
                            {
                                skeletonGraphic.material = new Material(shader);
                            }
                        }
                    }

                    skeletonGraphic.initialSkinName = currentSkinName;
                    skeletonGraphic.startingAnimation = idleAnim;
                    skeletonGraphic.startingLoop = true;
                    skeletonGraphic.Initialize(true);

                    skeleton = skeletonGraphic.Skeleton;
                    animState = skeletonGraphic.AnimationState;

                    if (skeleton != null && !string.IsNullOrEmpty(currentSkinName))
                    {
                        SetSkin(currentSkinName);
                    }

                    isInitialized = (skeleton != null);
                    if (isInitialized)
                    {
                        PlayIdle();
                        Debug.Log($"[SnakeSpineHead] Initialized successfully with skin '{currentSkinName}'.");
                    }
                    else
                    {
                        Debug.LogWarning($"[SnakeSpineHead] skeletonGraphic.Skeleton is null after Initialize!");
                    }
                }
                else
                {
                    Debug.LogWarning("[SnakeSpineHead] skeletonGraphic.skeletonDataAsset is null!");
                    isInitialized = false;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[SnakeSpineHead] Initialize failed with exception: {ex}");
                isInitialized = false;
            }
        }

        public void SetSkin(string skinName)
        {
            if (string.IsNullOrEmpty(skinName)) return;
            currentSkinName = skinName;

            if (skeleton == null && skeletonGraphic != null)
            {
                skeleton = skeletonGraphic.Skeleton;
            }

            if (skeleton != null)
            {
                skeleton.SetSkin(currentSkinName);
                skeleton.SetSlotsToSetupPose();
                if (skeletonGraphic != null)
                {
                    skeletonGraphic.LateUpdate();
                }
            }
        }

        public void PlayIdle(bool loop = true)
        {
            PlayAnimation(idleAnim, loop);
        }

        public void PlayOpenMouth(bool loop = false)
        {
            PlayAnimation(openMouthAnim, loop);
        }

        public void UpdateAppleProximity(bool isAdjacent)
        {
            if (!isInitialized || animState == null) return;

            var currentName = animState.GetCurrent(0)?.Animation.Name;
            // Proximity controls the resting expression. Let eating, hit, and terminal animations finish.
            if (currentName != idleAnim && currentName != openMouthAnim) return;

            var nextName = isAdjacent ? openMouthAnim : idleAnim;
            if (currentName == nextName) return;

            skeleton.SetSlotsToSetupPose();
            PlayAnimation(nextName, !isAdjacent);
        }

        public void PlayEat()
        {
            PlayAnimation(eatAnim, false);
            AddAnimation(idleAnim, true, 0f);
        }

        public void PlayHitWall()
        {
            PlayAnimation(hitWallAnim, false);
            AddAnimation(idleAnim, true, 0f);
        }

        public void PlayDie()
        {
            PlayAnimation(dieAnim, false);
        }

        public void PlayWin()
        {
            PlayAnimation(winAnim, true);
        }

        public void PlayAnimation(string animName, bool loop)
        {
            EnsureInitialized();
            if (animState != null && !string.IsNullOrEmpty(animName))
            {
                animState.SetAnimation(0, animName, loop);
            }
        }

        public void AddAnimation(string animName, bool loop, float delay)
        {
            EnsureInitialized();
            if (animState != null && !string.IsNullOrEmpty(animName))
            {
                animState.AddAnimation(0, animName, loop, delay);
            }
        }

        private void EnsureInitialized()
        {
            if (!isInitialized && skeletonGraphic != null && skeletonGraphic.skeletonDataAsset != null)
            {
                Initialize();
            }
        }
    }
}
