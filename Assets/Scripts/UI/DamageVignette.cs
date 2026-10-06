using UnityEngine;
using UnityEngine.UI;

namespace SpaceShooter
{
    /// <summary>
    /// Red edges around the screen: a sharp pulse when the ship is hit and a slow heartbeat
    /// while the hull is dangerously low.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public sealed class DamageVignette : MonoBehaviour
    {
        [SerializeField] Color color = new Color(1f, 0.1f, 0.1f, 1f);
        [SerializeField, Range(0f, 1f)] float hitAlpha = 0.75f;
        [SerializeField] float fadeSpeed = 1.6f;
        [Tooltip("Hull fraction below which the warning heartbeat starts.")]
        [SerializeField, Range(0f, 1f)] float lowHealthThreshold = 0.3f;
        [SerializeField, Range(0f, 1f)] float lowHealthAlpha = 0.22f;

        Image image;
        float hitPulse;

        void Awake()
        {
            image = GetComponent<Image>();
            image.raycastTarget = false;
            Apply(0f);
        }

        void OnEnable() => GameEvents.PlayerDamaged += OnPlayerDamaged;

        void OnDisable() => GameEvents.PlayerDamaged -= OnPlayerDamaged;

        void OnPlayerDamaged(float amount) => hitPulse = hitAlpha;

        void Update()
        {
            hitPulse = Mathf.MoveTowards(hitPulse, 0f, fadeSpeed * Time.unscaledDeltaTime);

            float warning = 0f;
            Player player = Player.Instance;
            if (player != null && player.IsAlive && player.Health.Normalized < lowHealthThreshold)
            {
                warning = lowHealthAlpha * (0.6f + 0.4f * Mathf.Sin(Time.unscaledTime * 5f));
            }

            Apply(Mathf.Max(hitPulse, warning));
        }

        void Apply(float alpha)
        {
            bool visible = alpha > 0.001f;
            if (image.enabled != visible) image.enabled = visible;
            if (!visible) return;

            Color tint = color;
            tint.a = alpha;
            image.color = tint;
        }
    }
}
