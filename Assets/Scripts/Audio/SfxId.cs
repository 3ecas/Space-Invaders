namespace SpaceShooter
{
    /// <summary>
    /// Every sound effect the game can play. The numbers are fixed so that adding a new entry
    /// never changes which sound existing prefabs and assets point to.
    /// </summary>
    public enum SfxId
    {
        None = 0,

        // Player weapons
        Blaster = 1,
        Spread = 2,
        Vulcan = 3,
        Rail = 4,
        Missile = 5,
        Plasma = 6,

        // Combat
        EnemyShoot = 10,
        Hit = 11,
        ExplosionSmall = 12,
        ExplosionMedium = 13,
        ExplosionLarge = 14,

        // Player
        PlayerHurt = 20,
        PlayerDeath = 21,
        ShieldUp = 22,
        ShieldBlock = 23,
        ShieldDown = 24,
        Dash = 25,
        Bomb = 26,
        WeaponSwitch = 27,

        // Pickups
        Pickup = 30,
        PowerUp = 31,
        Heal = 32,

        // Game flow
        WaveStart = 40,
        BossWarning = 41,
        GameOver = 42,
        Confirm = 43
    }
}
