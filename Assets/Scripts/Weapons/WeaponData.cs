using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// One weapon, defined entirely by numbers. To add a weapon: create a new asset
    /// (Create > Space Shooter > Weapon), fill it in and add it to the player's loadout.
    /// </summary>
    [CreateAssetMenu(menuName = "Space Shooter/Weapon", fileName = "Weapon")]
    public sealed class WeaponData : ScriptableObject
    {
        [Header("Identity")]
        public string displayName = "Weapon";
        public Sprite icon;
        public Color color = Color.white;

        [Header("Shot")]
        public Projectile projectilePrefab;
        [Tooltip("Shots per second.")]
        public float fireRate = 6f;
        public float damage = 10f;
        public float projectileSpeed = 28f;
        public float projectileLifetime = 2f;

        [Header("Pattern")]
        [Tooltip("Projectiles launched per shot.")]
        public int projectilesPerShot = 1;
        [Tooltip("Degrees between neighbouring projectiles (fans the shot out).")]
        public float angleStep;
        [Tooltip("World units between neighbouring projectiles (places them side by side).")]
        public float lateralSpacing;
        [Tooltip("Random wobble in degrees added to every projectile.")]
        public float randomSpread;

        [Header("Ammo")]
        public bool infiniteAmmo;
        public int ammoPerPickup = 60;
        public int maxAmmo = 200;

        [Header("Power level scaling")]
        [Tooltip("Extra projectiles gained per power level. 0.5 means +1 every second level.")]
        public float projectilesPerLevel = 0.5f;
        [Tooltip("Extra damage per power level, as a fraction (0.15 = +15%).")]
        public float damagePerLevel = 0.15f;
        [Tooltip("Extra fire rate per power level, as a fraction.")]
        public float fireRatePerLevel = 0.05f;

        [Header("Feel")]
        public SfxId fireSfx = SfxId.Blaster;
        [Range(0f, 1f)] public float fireShake;

        public int GetProjectileCount(int level)
        {
            return Mathf.Max(1, projectilesPerShot + Mathf.FloorToInt((level - 1) * projectilesPerLevel + 0.001f));
        }

        public float GetDamage(int level) => damage * (1f + damagePerLevel * (level - 1));

        public float GetFireRate(int level) => fireRate * (1f + fireRatePerLevel * (level - 1));
    }
}
