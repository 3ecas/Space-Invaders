using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// Decides what happens when something hits the ship. Hits are checked in this order:
    /// dash (dodged) -> shield (blocked) -> brief invulnerability after a hit -> armor -> hull.
    /// </summary>
    public sealed class PlayerDamageReceiver : MonoBehaviour, IDamageable
    {
        [Tooltip("Seconds of invulnerability after taking a hit.")]
        [SerializeField] float invulnerabilityAfterHit = 0.5f;
        [Tooltip("Sprites that blink while the ship is invulnerable.")]
        [SerializeField] SpriteRenderer[] blinkRenderers;
        [SerializeField] GameObject hitEffect;
        [SerializeField] float hitShake = 0.4f;
        [SerializeField] float hitStop = 0.06f;

        Health health;
        Armor armor;
        Shield shield;
        PlayerController controller;
        float invulnerableTime;
        bool dimmed;

        public bool IsAlive => health != null && health.IsAlive;
        public bool IsInvulnerable => invulnerableTime > 0f;

        void Awake()
        {
            health = GetComponent<Health>();
            armor = GetComponent<Armor>();
            shield = GetComponent<Shield>();
            controller = GetComponent<PlayerController>();
        }

        public bool TakeDamage(DamageInfo damage)
        {
            if (!IsAlive || !GameManager.Playing) return false;

            // Dashing: the shot passes straight through.
            if (controller != null && controller.IsDashing) return false;

            if (shield != null && shield.IsActive)
            {
                shield.RegisterBlockedHit();
                return true;
            }

            // Still flashing from the last hit: the shot is used up but does nothing.
            if (IsInvulnerable) return true;

            float toHull = armor != null ? armor.Absorb(damage.Amount) : damage.Amount;
            invulnerableTime = invulnerabilityAfterHit;

            if (hitEffect != null) PoolManager.Spawn(hitEffect, damage.Point, Quaternion.identity);
            AudioManager.Play(SfxId.PlayerHurt);
            CameraShake.Shake(hitShake);
            GameEvents.RaisePlayerDamaged(damage.Amount);

            health.Damage(toHull);

            if (health.IsAlive && GameManager.Instance != null) GameManager.Instance.HitStop(hitStop);
            return true;
        }

        /// <summary>Makes the ship untouchable for a while (bombs use this).</summary>
        public void GrantInvulnerability(float seconds)
        {
            invulnerableTime = Mathf.Max(invulnerableTime, seconds);
        }

        void Update()
        {
            if (invulnerableTime > 0f) invulnerableTime -= Time.deltaTime;

            bool dim = invulnerableTime > 0f && Mathf.Repeat(Time.time * 14f, 1f) < 0.5f;
            if (dim == dimmed) return;
            dimmed = dim;
            SetAlpha(dim ? 0.3f : 1f);
        }

        void OnDisable()
        {
            dimmed = false;
            SetAlpha(1f);
        }

        void SetAlpha(float alpha)
        {
            if (blinkRenderers == null) return;
            foreach (SpriteRenderer sprite in blinkRenderers)
            {
                if (sprite == null) continue;
                Color tint = sprite.color;
                tint.a = alpha;
                sprite.color = tint;
            }
        }
    }
}
