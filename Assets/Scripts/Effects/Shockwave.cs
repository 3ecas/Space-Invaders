using UnityEngine;

namespace SpaceShooter
{
    /// <summary>A ring that expands and fades. Used for the bomb blast and big explosions.</summary>
    public sealed class Shockwave : MonoBehaviour, IPoolable
    {
        [SerializeField] SpriteRenderer ring;
        [SerializeField] float duration = 0.6f;
        [SerializeField] float startScale = 0.5f;
        [SerializeField] float endScale = 30f;
        [SerializeField] Color color = Color.white;

        float age;
        bool playing;

        public void OnSpawned()
        {
            age = 0f;
            playing = true;
            Apply(0f);
        }

        public void OnDespawned() => playing = false;

        void Update()
        {
            if (!playing) return;

            age += Time.deltaTime;
            float t = duration > 0f ? Mathf.Clamp01(age / duration) : 1f;
            Apply(t);

            if (t < 1f) return;
            playing = false;
            PoolManager.Despawn(gameObject);
        }

        void Apply(float t)
        {
            // Ease-out: fast at first, slowing as it spreads.
            float eased = 1f - (1f - t) * (1f - t);
            float scale = Mathf.Lerp(startScale, endScale, eased);
            transform.localScale = new Vector3(scale, scale, 1f);

            if (ring == null) return;
            Color tint = color;
            tint.a *= 1f - t;
            ring.color = tint;
        }
    }
}
