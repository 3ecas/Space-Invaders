using UnityEngine;

namespace SpaceShooter
{
    /// <summary>A pool of hit points. Used by the player and by every enemy.</summary>
    public sealed class Health : MonoBehaviour
    {
        [SerializeField] float maxHealth = 100f;

        public float Max => maxHealth;
        public float Current { get; private set; }
        public float Normalized => maxHealth > 0f ? Mathf.Clamp01(Current / maxHealth) : 0f;
        public bool IsAlive => Current > 0f;
        public bool IsFull => Current >= maxHealth;

        /// <summary>Raised with the amount of health actually lost.</summary>
        public event System.Action<float> Damaged;
        /// <summary>Raised with the amount of health actually restored.</summary>
        public event System.Action<float> Healed;
        public event System.Action Died;

        void Awake() => Current = maxHealth;

        /// <summary>Changes the maximum and optionally tops the health back up.</summary>
        public void SetMax(float value, bool refill)
        {
            maxHealth = Mathf.Max(1f, value);
            Current = refill ? maxHealth : Mathf.Min(Current, maxHealth);
        }

        public void Damage(float amount)
        {
            if (!IsAlive || amount <= 0f) return;

            float lost = Mathf.Min(amount, Current);
            Current -= lost;
            Damaged?.Invoke(lost);

            if (Current <= 0f)
            {
                Current = 0f;
                Died?.Invoke();
            }
        }

        /// <summary>Restores health and returns how much was actually restored.</summary>
        public float Heal(float amount)
        {
            if (!IsAlive || amount <= 0f) return 0f;

            float restored = Mathf.Min(amount, maxHealth - Current);
            if (restored <= 0f) return 0f;

            Current += restored;
            Healed?.Invoke(restored);
            return restored;
        }

        public void Kill() => Damage(Current);
    }
}
