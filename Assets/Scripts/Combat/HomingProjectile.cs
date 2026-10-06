using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// Add next to a <see cref="Projectile"/> to make it steer towards a target: the nearest enemy
    /// for player shots, the player for enemy shots.
    /// </summary>
    [RequireComponent(typeof(Projectile))]
    public sealed class HomingProjectile : MonoBehaviour, IPoolable
    {
        [Tooltip("Degrees per second the shot can turn.")]
        [SerializeField] float turnRate = 260f;
        [Tooltip("Seconds it flies straight before it starts tracking.")]
        [SerializeField] float trackingDelay = 0.12f;
        [SerializeField] float searchRadius = 16f;
        [SerializeField] float acceleration = 26f;
        [SerializeField] float maxSpeed = 24f;

        Projectile projectile;
        Enemy target;
        float age;
        float searchTimer;

        void Awake() => projectile = GetComponent<Projectile>();

        public void OnSpawned()
        {
            target = null;
            age = 0f;
            searchTimer = 0f;
        }

        public void OnDespawned() => target = null;

        void Update()
        {
            float dt = Time.deltaTime;
            if (dt <= 0f) return;
            age += dt;

            Vector2 velocity = projectile.Velocity;
            float speed = velocity.magnitude;
            if (speed < 0.01f) return;

            Vector2 heading = velocity / speed;
            speed = Mathf.MoveTowards(speed, maxSpeed, acceleration * dt);

            if (age >= trackingDelay && TryGetTargetPosition(dt, out Vector2 targetPosition))
            {
                Vector2 toTarget = targetPosition - (Vector2)transform.position;
                float angle = Vector2.SignedAngle(heading, toTarget);
                float maxTurn = turnRate * dt;
                heading = GameMath.Rotate(heading, Mathf.Clamp(angle, -maxTurn, maxTurn));
            }

            projectile.Velocity = heading * speed;
        }

        bool TryGetTargetPosition(float dt, out Vector2 position)
        {
            if (projectile.Team == Team.Enemy)
            {
                position = Player.Position;
                return Player.Exists;
            }

            if (target == null || !target.IsTargetable)
            {
                target = null;
                searchTimer -= dt;
                if (searchTimer <= 0f)
                {
                    searchTimer = 0.15f;
                    target = Enemy.FindNearest(transform.position, searchRadius);
                }
            }

            position = target != null ? target.Position : Vector2.zero;
            return target != null;
        }
    }
}
