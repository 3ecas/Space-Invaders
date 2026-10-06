using UnityEngine;

namespace SpaceShooter
{
    /// <summary>A temporary energy bubble. While it is up the ship takes no damage at all.</summary>
    public sealed class Shield : MonoBehaviour
    {
        [SerializeField] SpriteRenderer bubble;
        [Tooltip("Longest the shield can last when pickups are stacked.")]
        [SerializeField] float maxDuration = 20f;
        [Tooltip("The bubble starts blinking this many seconds before it runs out.")]
        [SerializeField] float warningTime = 2.5f;
        [SerializeField] Color color = new Color(0.45f, 0.9f, 1f, 0.9f);

        public bool IsActive => timeLeft > 0f;
        public float TimeLeft => timeLeft;

        /// <summary>1 when freshly activated, falling to 0 as it expires.</summary>
        public float Normalized => fullDuration > 0f ? Mathf.Clamp01(timeLeft / fullDuration) : 0f;

        Vector3 baseScale = Vector3.one;
        float timeLeft;
        float fullDuration;
        float hitPulse;

        void Awake()
        {
            if (bubble == null) return;
            baseScale = bubble.transform.localScale;
            bubble.enabled = false;
        }

        public void Activate(float duration)
        {
            if (duration <= 0f) return;

            timeLeft = Mathf.Min(maxDuration, timeLeft + duration);
            fullDuration = timeLeft;
            hitPulse = 1f;
            AudioManager.Play(SfxId.ShieldUp);
        }

        /// <summary>Called when the bubble stops a hit, so it can flash.</summary>
        public void RegisterBlockedHit()
        {
            hitPulse = 1f;
            AudioManager.Play(SfxId.ShieldBlock);
        }

        void Update()
        {
            float dt = Time.deltaTime;

            if (timeLeft > 0f)
            {
                timeLeft -= dt;
                if (timeLeft <= 0f)
                {
                    timeLeft = 0f;
                    fullDuration = 0f;
                    AudioManager.Play(SfxId.ShieldDown);
                }
            }

            hitPulse = Mathf.MoveTowards(hitPulse, 0f, dt * 4f);
            UpdateBubble();
        }

        void UpdateBubble()
        {
            if (bubble == null) return;

            bool visible = IsActive;
            if (visible && timeLeft < warningTime) visible = Mathf.Repeat(Time.time * 7f, 1f) < 0.6f;

            bubble.enabled = visible;
            if (!visible) return;

            float pulse = 1f + Mathf.Sin(Time.time * 5f) * 0.035f + hitPulse * 0.14f;
            bubble.transform.localScale = baseScale * pulse;

            Color tint = color;
            tint.a *= Mathf.Lerp(0.7f, 1f, hitPulse);
            bubble.color = tint;
        }
    }
}
