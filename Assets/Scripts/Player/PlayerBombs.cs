using System.Collections.Generic;
using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// The panic button: wipes every enemy bullet off the screen and blasts all visible enemies.
    /// </summary>
    public sealed class PlayerBombs : MonoBehaviour
    {
        static readonly List<Enemy> targets = new List<Enemy>(128);

        [SerializeField] int startingBombs = 2;
        [SerializeField] int maxBombs = 5;
        [SerializeField] float damage = 160f;
        [SerializeField] float cooldown = 0.8f;
        [Tooltip("Seconds of invulnerability granted when a bomb goes off.")]
        [SerializeField] float invulnerability = 1.2f;
        [SerializeField] GameObject blastEffect;

        public int Count { get; private set; }
        public int Max => maxBombs;
        public bool IsFull => Count >= maxBombs;

        PlayerDamageReceiver damageReceiver;
        float cooldownLeft;

        void Awake()
        {
            Count = Mathf.Clamp(startingBombs, 0, maxBombs);
            damageReceiver = GetComponent<PlayerDamageReceiver>();
        }

        public void Add(int amount)
        {
            if (amount > 0) Count = Mathf.Min(maxBombs, Count + amount);
        }

        void Update()
        {
            if (cooldownLeft > 0f) cooldownLeft -= Time.deltaTime;
            if (!GameManager.Playing) return;

            GameInput input = GameInput.Instance;
            if (input != null && input.BombPressed && Count > 0 && cooldownLeft <= 0f) Detonate();
        }

        void Detonate()
        {
            Count--;
            cooldownLeft = cooldown;

            Vector2 center = transform.position;

            Projectile.DespawnAll(Team.Enemy, true);

            // Copy first: enemies remove themselves from the live list as they die.
            targets.Clear();
            targets.AddRange(Enemy.Active);
            foreach (Enemy enemy in targets)
            {
                if (enemy == null || !enemy.IsTargetable) continue;
                Vector2 away = enemy.Position - center;
                enemy.TakeDamage(new DamageInfo(damage, enemy.Position, away.sqrMagnitude > 0f ? away.normalized : Vector2.up, Team.Player));
            }
            targets.Clear();

            if (blastEffect != null) PoolManager.Spawn(blastEffect, center, Quaternion.identity);
            if (damageReceiver != null) damageReceiver.GrantInvulnerability(invulnerability);

            AudioManager.Play(SfxId.Bomb);
            CameraShake.Shake(0.9f);
            GameEvents.RaiseScreenFlash(Color.white, 0.75f);
        }
    }
}
