using System.Collections.Generic;
using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// The core of every enemy: health, taking damage, ramming the player and dying.
    /// How it moves and shoots comes from the <see cref="EnemyBehaviour"/> components next to it,
    /// so new enemy types are made by mixing behaviours rather than writing a new class.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public sealed class Enemy : MonoBehaviour, IDamageable, IPoolable
    {
        // Enemies that never make it onto the screen are cleaned up after this long.
        const float NeverArrivedTimeout = 20f;
        const float OnScreenMargin = 0.4f;

        static readonly List<Enemy> active = new List<Enemy>(256);

        /// <summary>Every enemy currently alive.</summary>
        public static IReadOnlyList<Enemy> Active => active;

        /// <summary>Living enemies that still have to be dealt with before the wave is over.</summary>
        public static int WaveBlockers { get; private set; }

        [SerializeField] HitFlash hitFlash;
        [Tooltip("How far past the screen edge it may wander before being removed.")]
        [SerializeField] float despawnMargin = 3f;

        public EnemyData Data { get; private set; }
        public Health Health { get; private set; }
        public EnemySpawnInfo SpawnInfo { get; private set; }
        public DifficultyScale Difficulty { get; private set; } = DifficultyScale.Normal;

        public float MoveSpeed => Data.moveSpeed * Difficulty.Speed;
        public float BulletSpeedScale => Difficulty.BulletSpeed;
        public float DamageScale => Difficulty.Damage;
        public Vector2 Position => transform.position;

        public bool IsAlive => alive;
        public bool IsOnScreen { get; private set; }
        public bool HasEnteredScreen { get; private set; }

        /// <summary>Alive and visible - a valid target for homing missiles and bombs.</summary>
        public bool IsTargetable => alive && IsOnScreen;

        /// <summary>Raised when the player destroys this enemy (not when it simply flies away).</summary>
        public event System.Action<Enemy> Killed;

        EnemyBehaviour[] behaviours;
        bool alive;
        bool blocksWave;
        float age;
        int listIndex = -1;

        // ---------------------------------------------------------------- static helpers

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            active.Clear();
            WaveBlockers = 0;
        }

        /// <summary>Creates an enemy of the given type. This is the only way enemies should be spawned.</summary>
        public static Enemy Spawn(EnemyData data, Vector2 position, EnemySpawnInfo info, DifficultyScale difficulty)
        {
            if (data == null || data.prefab == null) return null;

            Enemy enemy = PoolManager.Spawn(data.prefab, position, Quaternion.identity);
            if (enemy != null) enemy.Init(data, info, difficulty);
            return enemy;
        }

        public static Enemy FindNearest(Vector2 from, float maxDistance)
        {
            Enemy best = null;
            float bestSqr = maxDistance * maxDistance;

            for (int i = 0; i < active.Count; i++)
            {
                Enemy enemy = active[i];
                if (!enemy.IsTargetable) continue;

                float sqr = (enemy.Position - from).sqrMagnitude;
                if (sqr >= bestSqr) continue;
                bestSqr = sqr;
                best = enemy;
            }
            return best;
        }

        // ---------------------------------------------------------------- lifecycle

        void Awake()
        {
            Health = GetComponent<Health>();
            Health.Died += OnHealthDepleted;
            behaviours = GetComponents<EnemyBehaviour>();
        }

        void Init(EnemyData data, EnemySpawnInfo info, DifficultyScale difficulty)
        {
            Data = data;
            SpawnInfo = info;
            Difficulty = difficulty;

            Health.SetMax(data.maxHealth * difficulty.Health, true);

            age = 0f;
            alive = true;
            HasEnteredScreen = false;
            IsOnScreen = PlayArea.Contains(transform.position, OnScreenMargin);

            Register();
            blocksWave = data.blocksWaveClear;
            if (blocksWave) WaveBlockers++;

            for (int i = 0; i < behaviours.Length; i++) behaviours[i].Begin(this);
        }

        public void OnSpawned()
        {
            if (hitFlash != null) hitFlash.ResetFlash();
        }

        public void OnDespawned() => Cleanup();

        void OnDestroy() => Cleanup();

        void Cleanup()
        {
            bool wasRegistered = listIndex >= 0;
            alive = false;
            Unregister();

            if (blocksWave)
            {
                blocksWave = false;
                WaveBlockers = Mathf.Max(0, WaveBlockers - 1);
            }

            if (!wasRegistered || behaviours == null) return;
            for (int i = 0; i < behaviours.Length; i++) behaviours[i].End();
        }

        void Update()
        {
            if (!alive) return;

            float dt = Time.deltaTime;
            age += dt;

            for (int i = 0; i < behaviours.Length; i++)
            {
                behaviours[i].Tick(dt);
                if (!alive) return;
            }

            Vector2 position = transform.position;
            IsOnScreen = PlayArea.Contains(position, OnScreenMargin);

            if (IsOnScreen)
            {
                HasEnteredScreen = true;
            }
            else if (HasEnteredScreen ? !PlayArea.Contains(position, despawnMargin) : age > NeverArrivedTimeout)
            {
                Leave();
            }
        }

        /// <summary>Removes the enemy without a kill: no score, no explosion, no drops.</summary>
        public void Leave()
        {
            if (!alive) return;
            alive = false;
            PoolManager.Despawn(gameObject);
        }

        // ---------------------------------------------------------------- damage

        public bool TakeDamage(DamageInfo damage)
        {
            // Can't be shot before it is actually visible.
            if (!alive || !IsOnScreen) return false;

            Health.Damage(damage.Amount);

            if (alive)
            {
                if (hitFlash != null) hitFlash.Flash();
                AudioManager.Play(SfxId.Hit, 0.6f);
            }
            return true;
        }

        void OnHealthDepleted()
        {
            if (!alive) return;
            alive = false;

            Vector2 position = transform.position;
            if (Data.deathEffect != null) PoolManager.Spawn(Data.deathEffect, position, Quaternion.identity);
            AudioManager.Play(Data.deathSfx);
            if (Data.deathShake > 0f) CameraShake.Shake(Data.deathShake);

            Killed?.Invoke(this);
            GameEvents.RaiseEnemyKilled(this);

            PoolManager.Despawn(gameObject);
        }

        void OnTriggerStay2D(Collider2D other)
        {
            if (!alive || Data.contactDamage <= 0f) return;
            if (other.gameObject.layer != GameLayers.Player) return;

            Player player = Player.Instance;
            if (player == null || !player.IsAlive || player.Damage == null) return;

            var hit = new DamageInfo(Data.contactDamage * DamageScale, transform.position, Vector2.down, Team.Enemy);
            if (player.Damage.TakeDamage(hit) && Data.dieOnContact) Health.Kill();
        }

        // ---------------------------------------------------------------- registry

        void Register()
        {
            if (listIndex >= 0) return;
            listIndex = active.Count;
            active.Add(this);
        }

        void Unregister()
        {
            if (listIndex < 0) return;

            int last = active.Count - 1;
            if (listIndex <= last)
            {
                Enemy moved = active[last];
                active[listIndex] = moved;
                moved.listIndex = listIndex;
                active.RemoveAt(last);
            }
            listIndex = -1;
        }
    }
}
