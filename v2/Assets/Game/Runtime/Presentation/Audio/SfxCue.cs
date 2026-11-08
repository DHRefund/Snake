namespace SnakeApple.Presentation.Audio
{
    /// <summary>Semantic sound cues. Values are serialized in SoundBank.asset: append only, never reorder.</summary>
    public enum SfxCue
    {
        UiClick = 0,
        UiBack = 1,
        Move = 2,
        Blocked = 3,
        AppleEat = 4,
        Fall = 5,
        Land = 6,
        Teleport = 7,
        Win = 8,
        WinFanfare = 9,
        Firework = 10,
        Lose = 11,
        Scream = 12,
        Hurt = 13,
        Spike = 14,
        Toxic = 15,
        Water = 16,
        Explosion = 17,
        CoinReward = 18,
        Purchase = 19,
        Unlock = 20,
        Undo = 21,
        Select = 22
    }
}
