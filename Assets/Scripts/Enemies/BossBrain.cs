using System.Collections;
using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// Runs the boss fight: cycles through attack patterns and gets nastier as its health drops
    /// (phase 0 above two-thirds health, phase 1 above one-third, phase 2 below that).
    /// Each attack is its own coroutine, so adding a new one is just another method.
    /// </summary>
    [RequireComponent(typeof(Enemy))]
    public sealed class BossBrain : EnemyBehaviour
    {
        [SerializeField] BossMovement movement;

        [Header("Weapons")]
        [SerializeField] Projectile boltPrefab;
        [SerializeField] Projectile orbPrefab;
        [SerializeField] Projectile heavyPrefab;
        [SerializeField] Transform[] sideMuzzles;
        [SerializeField] Transform coreMuzzle;
        [SerializeField] float bulletSpeed = 7.5f;
        [SerializeField] float damage = 12f;
        [SerializeField] float bulletLifetime = 8f;

        [Header("Reinforcements")]
        [SerializeField] EnemyData minionData;

        Coroutine routine;
        float spin;

        int Phase
        {
            get
            {
                float health = Owner.Health.Normalized;
                return health > 0.66f ? 0 : health > 0.33f ? 1 : 2;
            }
        }

        float Speed => bulletSpeed * Owner.BulletSpeedScale;
        float Damage => damage * Owner.DamageScale;
        Vector2 Core => coreMuzzle != null ? (Vector2)coreMuzzle.position : Owner.Position;

        void Awake() => GetComponent<Enemy>().Killed += OnKilled;

        protected override void OnBegin()
        {
            spin = 0f;
            routine = StartCoroutine(Fight());
            GameEvents.RaiseBossSpawned(Owner);
        }

        public override void End()
        {
            if (routine == null) return;
            StopCoroutine(routine);
            routine = null;
        }

        IEnumerator Fight()
        {
            while (movement != null && !movement.HasArrived) yield return null;
            yield return new WaitForSeconds(0.6f);

            int step = 0;
            while (true)
            {
                switch (step % 4)
                {
                    case 0: yield return AimedVolleys(); break;
                    case 1: yield return RingBursts(); break;
                    case 2: yield return SpiralStorm(); break;
                    default: yield return Reinforcements(); break;
                }
                step++;
                yield return new WaitForSeconds(Mathf.Lerp(1.5f, 0.7f, Phase * 0.5f));
            }
        }

        // ---------------------------------------------------------------- attacks

        /// <summary>Fans of fast bolts from the wing cannons, aimed at the player.</summary>
        IEnumerator AimedVolleys()
        {
            int volleys = 4 + Phase * 2;
            for (int v = 0; v < volleys; v++)
            {
                foreach (Transform muzzle in sideMuzzles)
                {
                    if (muzzle == null) continue;
                    Vector2 origin = muzzle.position;
                    BulletPatterns.Fan(boltPrefab, Team.Enemy, origin, Player.Position - origin,
                        3 + Phase, 9f, 0f, Speed * 1.25f, Damage, bulletLifetime);
                }
                AudioManager.Play(SfxId.EnemyShoot);
                yield return new WaitForSeconds(Mathf.Lerp(0.55f, 0.35f, Phase * 0.5f));
            }
        }

        /// <summary>Expanding rings of orbs, each offset so the gaps don't line up.</summary>
        IEnumerator RingBursts()
        {
            int rings = 3 + Phase;
            for (int r = 0; r < rings; r++)
            {
                int count = 16 + Phase * 6;
                BulletPatterns.Ring(orbPrefab, Team.Enemy, Core, count, r * (180f / count), Speed * 0.8f, Damage, bulletLifetime);
                AudioManager.Play(SfxId.EnemyShoot);
                yield return new WaitForSeconds(0.7f - Phase * 0.1f);
            }
        }

        /// <summary>A rotating spray: several arms of bullets spinning out of the core.</summary>
        IEnumerator SpiralStorm()
        {
            const float interval = 0.09f;
            float duration = 3f + Phase;
            int arms = 3 + Phase;

            for (float t = 0f; t < duration; t += interval)
            {
                BulletPatterns.Ring(orbPrefab, Team.Enemy, Core, arms, spin, Speed, Damage, bulletLifetime);
                spin += 11f;
                AudioManager.Play(SfxId.EnemyShoot, 0.5f);
                yield return new WaitForSeconds(interval);
            }
        }

        /// <summary>Launches escorts, then lobs slow heavy shots while the player deals with them.</summary>
        IEnumerator Reinforcements()
        {
            if (minionData != null)
            {
                int minions = 3 + Phase * 2;
                for (int i = 0; i < minions; i++)
                {
                    EnemySpawnInfo info = EnemySpawnInfo.Default;
                    info.direction = GameMath.Rotate(Vector2.down, Random.Range(-50f, 50f));
                    Enemy.Spawn(minionData, Core + Random.insideUnitCircle * 1.2f, info, Owner.Difficulty);
                    yield return new WaitForSeconds(0.25f);
                }
            }

            int shots = 3 + Phase;
            for (int i = 0; i < shots; i++)
            {
                Vector2 origin = Core;
                BulletPatterns.Fan(heavyPrefab, Team.Enemy, origin, Player.Position - origin,
                    1 + Phase * 2, 16f, 0f, Speed * 0.7f, Damage * 1.5f, bulletLifetime);
                AudioManager.Play(SfxId.EnemyShoot);
                yield return new WaitForSeconds(0.6f);
            }
        }

        // ---------------------------------------------------------------- death

        void OnKilled(Enemy boss)
        {
            Projectile.DespawnAll(Team.Enemy, true);
            GameEvents.RaiseScreenFlash(Color.white, 0.8f);
            if (GameManager.Instance != null) GameManager.Instance.HitStop(0.25f);
            GameEvents.RaiseBossDefeated(boss);
        }
    }
}
