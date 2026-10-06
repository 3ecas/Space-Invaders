using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// A collectible floating in space. It pops out of a destroyed enemy, drifts down with the
    /// scrolling world, and gets sucked in when the ship comes close.
    /// </summary>
    public sealed class Pickup : MonoBehaviour, IPoolable
    {
        public static int ActiveCount { get; private set; }

        [SerializeField] SpriteRenderer icon;
        [SerializeField] SpriteRenderer glow;

        [Header("Movement")]
        [Tooltip("Drift speed compared to the background scroll speed.")]
        [SerializeField] float driftFactor = 0.8f;
        [SerializeField] float popSpeed = 4f;

        [Header("Collection")]
        [SerializeField] float magnetRadius = 3.5f;
        [SerializeField] float magnetSpeed = 18f;
        [SerializeField] float collectRadius = 0.9f;

        PickupData data;
        Vector2 velocity;
        float age;
        bool counted;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => ActiveCount = 0;

        public void Init(PickupData pickup)
        {
            data = pickup;
            if (icon != null) icon.sprite = pickup.sprite;
            if (glow != null)
            {
                Color tint = pickup.color;
                tint.a = 0.45f;
                glow.color = tint;
            }
            // Pop outwards a little, so several drops from one enemy don't overlap.
            velocity = Random.insideUnitCircle * popSpeed + Vector2.up * (popSpeed * 0.5f);
        }

        public void OnSpawned()
        {
            age = 0f;
            if (!counted)
            {
                counted = true;
                ActiveCount++;
            }
        }

        public void OnDespawned() => Uncount();

        void OnDestroy() => Uncount();

        void Uncount()
        {
            if (!counted) return;
            counted = false;
            ActiveCount = Mathf.Max(0, ActiveCount - 1);
        }

        void Update()
        {
            if (data == null) return;

            float dt = Time.deltaTime;
            age += dt;
            Vector2 position = transform.position;
            bool pulled = false;

            if (Player.Exists)
            {
                Vector2 toPlayer = Player.Position - position;
                float distance = toPlayer.magnitude;

                if (distance <= collectRadius)
                {
                    Collect();
                    return;
                }

                if (distance <= magnetRadius)
                {
                    velocity = Vector2.MoveTowards(velocity, toPlayer / distance * magnetSpeed, 70f * dt);
                    pulled = true;
                }
            }

            if (!pulled)
            {
                Vector2 drift = Vector2.down * (WorldScroll.Speed * driftFactor);
                velocity = Vector2.MoveTowards(velocity, drift, 8f * dt);
            }

            position += velocity * dt;
            position.x = Mathf.Clamp(position.x, PlayArea.Left + 0.6f, PlayArea.Right - 0.6f);
            transform.position = position;

            if (icon != null)
            {
                float pulse = 1f + Mathf.Sin(age * 6f) * 0.08f;
                icon.transform.localScale = new Vector3(pulse, pulse, 1f);
            }

            if (position.y < PlayArea.Bottom - 1.5f) PoolManager.Despawn(gameObject);
        }

        void Collect()
        {
            Player player = Player.Instance;
            PickupEffects.Apply(data, player);

            AudioManager.Play(data.sfx);
            GameEvents.RaiseFloatingText(transform.position, data.label, data.color);
            GameEvents.RaisePickupCollected(data);

            PoolManager.Despawn(gameObject);
        }
    }
}
