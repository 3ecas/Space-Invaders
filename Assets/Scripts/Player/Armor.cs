using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// Plating that soaks up part of every hit until it wears out.
    /// While any armor is left, only a fraction of incoming damage reaches the hull.
    /// </summary>
    public sealed class Armor : MonoBehaviour
    {
        [SerializeField] float maxArmor = 100f;
        [SerializeField] float startingArmor;
        [Tooltip("Share of each hit the armor takes instead of the hull.")]
        [SerializeField, Range(0f, 1f)] float absorption = 0.6f;

        public float Max => maxArmor;
        public float Current { get; private set; }
        public float Normalized => maxArmor > 0f ? Mathf.Clamp01(Current / maxArmor) : 0f;
        public bool IsFull => Current >= maxArmor;

        void Awake() => Current = Mathf.Clamp(startingArmor, 0f, maxArmor);

        public void Add(float amount)
        {
            if (amount > 0f) Current = Mathf.Min(maxArmor, Current + amount);
        }

        /// <summary>Takes its share of a hit and returns the damage that gets through to the hull.</summary>
        public float Absorb(float damage)
        {
            if (Current <= 0f || damage <= 0f) return damage;

            float absorbed = Mathf.Min(damage * absorption, Current);
            Current -= absorbed;
            return damage - absorbed;
        }
    }
}
