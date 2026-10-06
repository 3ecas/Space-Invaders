namespace SpaceShooter
{
    /// <summary>Anything a projectile, explosion or collision can hurt.</summary>
    public interface IDamageable
    {
        bool IsAlive { get; }

        /// <summary>
        /// Applies a hit. Returns false when the target ignored it completely (for example the
        /// player mid-dash), in which case a bullet should keep flying instead of being used up.
        /// </summary>
        bool TakeDamage(DamageInfo damage);
    }
}
