using UnityEngine;

namespace SnakeApple.Presentation.Items
{
    [CreateAssetMenu(fileName = "GameplayItemSkin", menuName = "Snake & Apple/Gameplay Item Skin")]
    public sealed class GameplayItemSkin : ScriptableObject
    {
        [Header("Food & Collectibles")]
        [SerializeField] private Sprite appleNormal;
        [SerializeField] private Sprite appleVip;

        [Header("Exit & Portals")]
        [SerializeField] private Sprite exitPortal;
        [SerializeField] private Sprite exitPortalFrame;
        [SerializeField] private Sprite teleportBlue;
        [SerializeField] private Sprite teleportOrange;

        [Header("Hazards")]
        [SerializeField] private Sprite spike;
        [SerializeField] private Sprite toxicAcid;
        [SerializeField] private Sprite spiderWeb;

        [Header("Movables & Bombs")]
        [SerializeField] private Sprite rock;
        [SerializeField] private Sprite bomb;
        [SerializeField] private Sprite bombRemote;

        [Header("Mechanics & Triggers")]
        [SerializeField] private Sprite slidePlatform;
        [SerializeField] private Sprite buttonReleased;
        [SerializeField] private Sprite buttonPressed;
        [SerializeField] private Sprite doorBarBlue;
        [SerializeField] private Sprite doorBarGreen;
        [SerializeField] private Sprite doorLock;
        [SerializeField] private Sprite laserGun;
        [SerializeField] private Sprite laserBeam;

        public Sprite AppleNormal => appleNormal;
        public Sprite AppleVip => appleVip;
        public Sprite ExitPortal => exitPortal;
        public Sprite ExitPortalFrame => exitPortalFrame;
        public Sprite TeleportBlue => teleportBlue;
        public Sprite TeleportOrange => teleportOrange;
        public Sprite Spike => spike;
        public Sprite ToxicAcid => toxicAcid;
        public Sprite SpiderWeb => spiderWeb;
        public Sprite Rock => rock;
        public Sprite Bomb => bomb;
        public Sprite BombRemote => bombRemote;
        public Sprite SlidePlatform => slidePlatform;
        public Sprite ButtonReleased => buttonReleased;
        public Sprite ButtonPressed => buttonPressed;
        public Sprite DoorBarBlue => doorBarBlue;
        public Sprite DoorBarGreen => doorBarGreen;
        public Sprite DoorLock => doorLock;
        public Sprite LaserGun => laserGun;
        public Sprite LaserBeam => laserBeam;

        public bool IsConfigured => appleNormal != null && exitPortal != null && spike != null;
    }
}
