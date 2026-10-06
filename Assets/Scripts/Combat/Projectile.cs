using System.Collections.Generic;
using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// A bullet, missile or plasma ball. Used by both sides - the <see cref="Team"/> passed to
    /// <see cref="Launch"/> decides what it can hit.
    /// It has no collider of its own: every frame it sweeps a circle along its path, so even very
    /// fast shots can't tunnel through a target, and hundreds of them stay cheap.
    /// </summary>
    public sealed class Projectile : MonoBehaviour, IPoolable
    {
        const float OffscreenMargin = 1.5f;

        static readonly List<Projectile> playerShots = new List<Projectile>(256);
        static readonly List<Projectile> enemyShots = new List<Projectile>(512);
        static readonly RaycastHit2D[] hitBuffer = new RaycastHit2D[16];
        static readonly Collider2D[] splashBuffer = new Collider2D[64];

        [Header("Shape")]
        [SerializeField] float radius = 0.12f;
        [Tooltip("Rotate the sprite to point where it is flying.")]
        [SerializeField] bool alignToVelocity = true;

        [Header("Behaviour")]
        [Tooltip("How many extra targets the shot passes through before stopping.")]
        [SerializeField] int pierceCount;
        [Tooltip("Bigger than 0 makes the shot explode, hurting everything in this radius.")]
        [SerializeField] float explosionRadius;
        [Tooltip("Share of the damage dealt to targets caught in the explosion.")]
        [SerializeField, Range(0f, 1f)] float splashDamage = 0.6f;

        [Header("Effects")]
        [SerializeField] GameObject hitEffect;
        [SerializeField] SfxId hitSfx = SfxId.None;
        [SerializeField] float hitShake;

        public Team Team { get; private set; }
        public float Damage { get; private set; }

        public Vector2 Velocity
        {
            get => velocity;
            set
            {
                velocity = value;
                if (alignToVelocity && velocity.sqrMagnitude > 0.0001f) transform.rotation = GameMath.FaceUp(velocity);
            }
        }

        // Targets this shot has already damaged, so a piercing shot or an explosion never hits the
        // same target twice (big ships have several colliders).
        readonly List<IDamageable> alreadyHit = new List<IDamageable>(4);
        TrailRenderer[] trails;
        ContactFilter2D filter;
        Vector2 velocity;
        float lifeLeft;
        int piercesLeft;
        int listIndex = -1;
        bool live;

        // ---------------------------------------------------------------- static registry

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            playerShots.Clear();
            enemyShots.Clear();
        }

        static List<Projectile> ListFor(Team team) => team == Team.Player ? playerShots : enemyShots;

        public static int CountFor(Team team) => ListFor(team).Count;

        /// <summary>Removes every live shot belonging to a team (bombs wipe enemy bullets with this).</summary>
        public static void DespawnAll(Team team, bool withEffect)
        {
            List<Projectile> list = ListFor(team);
            for (int i = list.Count - 1; i >= 0; i--)
            {
                Projectile shot = list[i];
                if (withEffect) shot.SpawnHitEffect(shot.transform.position);
                shot.Despawn();
            }
        }

        // ---------------------------------------------------------------- lifecycle

        void Awake() => trails = GetComponentsInChildren<TrailRenderer>(true);

        public void OnSpawned()
        {
            alreadyHit.Clear();
            for (int i = 0; i < trails.Length; i++) trails[i].Clear();
        }

        public void OnDespawned()
        {
            live = false;
            Unregister();
        }

        void OnDestroy() => Unregister();

        /// <summary>Sends the shot on its way. Call right after spawning it.</summary>
        public void Launch(Team team, Vector2 direction, float speed, float damage, float lifetime)
        {
            Unregister();

            Team = team;
            Damage = damage;
            lifeLeft = lifetime;
            piercesLeft = pierceCount;

            filter = new ContactFilter2D { useTriggers = true };
            filter.SetLayerMask(GameLayers.HitMaskFor(team));

            Velocity = direction.sqrMagnitude > 0.0001f ? direction.normalized * speed : Vector2.up * speed;

            live = true;
            Register();
        }

        void Update()
        {
            if (!live) return;

            float dt = Time.deltaTime;
            if (dt <= 0f) return;

            Vector2 from = transform.position;
            Vector2 step = velocity * dt;
            float distance = step.magnitude;

            if (distance > 0f)
            {
                int hits = Physics2D.CircleCast(from, radius, step / distance, filter, hitBuffer, distance);
                for (int i = 0; i < hits && live; i++)
                {
                    RaycastHit2D hit = hitBuffer[i];
                    if (hit.collider == null) continue;
                    // A cast that starts inside a collider reports a zero point; use our own position then.
                    Vector2 point = hit.distance > 0f ? hit.point : from;
                    HandleHit(hit.collider, point);
                }
                if (!live) return;
            }

            Vector2 to = from + step;
            transform.position = to;

            lifeLeft -= dt;
            if (lifeLeft <= 0f || !PlayArea.Contains(to, OffscreenMargin)) Despawn();
        }

        // ---------------------------------------------------------------- hits

        void HandleHit(Collider2D other, Vector2 point)
        {
            var target = other.GetComponentInParent<IDamageable>();
            if (target == null || !target.IsAlive || alreadyHit.Contains(target)) return;

            Vector2 direction = velocity.sqrMagnitude > 0f ? velocity.normalized : Vector2.up;
            if (!target.TakeDamage(new DamageInfo(Damage, point, direction, Team))) return;

            alreadyHit.Add(target);

            if (explosionRadius > 0f) Explode(point);
            SpawnHitEffect(point);
            if (hitShake > 0f) CameraShake.Shake(hitShake);
            AudioManager.Play(hitSfx);

            if (piercesLeft > 0)
            {
                piercesLeft--;
                return;
            }
            Despawn();
        }

        void Explode(Vector2 center)
        {
            int count = Physics2D.OverlapCircle(center, explosionRadius, filter, splashBuffer);
            float damage = Damage * splashDamage;

            for (int i = 0; i < count; i++)
            {
                Collider2D other = splashBuffer[i];
                if (other == null) continue;

                var target = other.GetComponentInParent<IDamageable>();
                if (target == null || !target.IsAlive || alreadyHit.Contains(target)) continue;
                alreadyHit.Add(target);

                Vector2 position = other.transform.position;
                Vector2 away = position - center;
                target.TakeDamage(new DamageInfo(damage, position, away.sqrMagnitude > 0f ? away.normalized : Vector2.up, Team));
            }
        }

        void SpawnHitEffect(Vector2 point)
        {
            if (hitEffect != null) PoolManager.Spawn(hitEffect, point, Quaternion.identity);
        }

        void Despawn()
        {
            live = false;
            PoolManager.Despawn(gameObject);
        }

        // ---------------------------------------------------------------- registry helpers

        void Register()
        {
            List<Projectile> list = ListFor(Team);
            listIndex = list.Count;
            list.Add(this);
        }

        void Unregister()
        {
            if (listIndex < 0) return;

            // Swap-remove: O(1) instead of shuffling the whole list down.
            List<Projectile> list = ListFor(Team);
            int last = list.Count - 1;
            if (listIndex <= last)
            {
                Projectile moved = list[last];
                list[listIndex] = moved;
                moved.listIndex = listIndex;
                list.RemoveAt(last);
            }
            listIndex = -1;
        }
    }
}
