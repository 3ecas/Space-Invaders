using UnityEngine;

namespace SpaceShooter
{
    public enum FirePattern
    {
        /// <summary>Straight at the player.</summary>
        Aimed = 0,
        /// <summary>Wherever the enemy is facing (straight down unless it rotates).</summary>
        Forward = 1,
        /// <summary>Evenly in all directions. A non-zero spin step turns it into a spiral.</summary>
        Ring = 2
    }

    /// <summary>Makes an enemy shoot. Add more than one for enemies with several attacks.</summary>
    public sealed class EnemyShooter : EnemyBehaviour
    {
        // Hard ceiling so a busy screen never melts into an unreadable (and slow) wall of bullets.
        const int MaxEnemyBullets = 500;

        [SerializeField] Projectile bulletPrefab;
        [Tooltip("Where bullets appear. Leave empty to use the enemy's centre.")]
        [SerializeField] Transform muzzle;
        [SerializeField] FirePattern pattern = FirePattern.Aimed;

        [Header("Timing")]
        [Tooltip("Seconds between volleys.")]
        [SerializeField] float interval = 2.5f;
        [Tooltip("Random time added to or taken from each interval.")]
        [SerializeField] float intervalJitter = 0.5f;
        [SerializeField] float firstShotDelay = 1f;
        [Tooltip("Shots per volley.")]
        [SerializeField] int burstCount = 1;
        [SerializeField] float burstSpacing = 0.15f;

        [Header("Bullets")]
        [Tooltip("Bullets per shot.")]
        [SerializeField] int bulletsPerShot = 1;
        [Tooltip("Degrees between neighbouring bullets (Aimed / Forward).")]
        [SerializeField] float angleStep = 12f;
        [Tooltip("Degrees the ring turns after every shot (Ring).")]
        [SerializeField] float spinStep;
        [Tooltip("Random aiming error in degrees.")]
        [SerializeField] float aimError;
        [SerializeField] float bulletSpeed = 7f;
        [SerializeField] float damage = 10f;
        [SerializeField] float bulletLifetime = 7f;
        [SerializeField] SfxId sfx = SfxId.EnemyShoot;

        float timer;
        float burstTimer;
        float spin;
        int burstLeft;

        protected override void OnBegin()
        {
            timer = firstShotDelay + Random.Range(0f, intervalJitter);
            burstLeft = 0;
            burstTimer = 0f;
            spin = Random.Range(0f, 360f);
        }

        public override void Tick(float deltaTime)
        {
            if (bulletPrefab == null || !Owner.IsOnScreen) return;

            if (burstLeft > 0)
            {
                burstTimer -= deltaTime;
                if (burstTimer > 0f) return;
                Shoot();
                burstLeft--;
                burstTimer = burstSpacing;
                return;
            }

            timer -= deltaTime;
            if (timer > 0f) return;
            timer = Mathf.Max(0.2f, interval + Random.Range(-intervalJitter, intervalJitter));

            if (!Player.Exists || Projectile.CountFor(Team.Enemy) >= MaxEnemyBullets) return;

            burstLeft = Mathf.Max(1, burstCount);
            burstTimer = 0f;
        }

        void Shoot()
        {
            Vector2 origin = muzzle != null ? (Vector2)muzzle.position : Owner.Position;
            float speed = bulletSpeed * Owner.BulletSpeedScale;
            float hit = damage * Owner.DamageScale;

            switch (pattern)
            {
                case FirePattern.Aimed:
                {
                    Vector2 direction = Player.Position - origin;
                    if (aimError > 0f) direction = GameMath.Rotate(direction, Random.Range(-aimError, aimError));
                    BulletPatterns.Fan(bulletPrefab, Team.Enemy, origin, direction, bulletsPerShot, angleStep, 0f, speed, hit, bulletLifetime);
                    break;
                }
                case FirePattern.Forward:
                {
                    // Enemy sprites are drawn facing down, so "forward" is the opposite of transform.up.
                    Vector2 direction = -(Vector2)transform.up;
                    BulletPatterns.Fan(bulletPrefab, Team.Enemy, origin, direction, bulletsPerShot, angleStep, 0f, speed, hit, bulletLifetime);
                    break;
                }
                case FirePattern.Ring:
                {
                    BulletPatterns.Ring(bulletPrefab, Team.Enemy, origin, bulletsPerShot, spin, speed, hit, bulletLifetime);
                    spin += spinStep;
                    break;
                }
            }

            AudioManager.Play(sfx, 0.7f);
        }
    }
}
