using UnityEngine;
using UnityEngine.UI;

namespace SpaceShooter
{
    /// <summary>
    /// A horizontal bar. The fill is a stretched rectangle whose right edge follows the value,
    /// with an optional "trail" that lags behind so a sudden loss is easy to read.
    /// </summary>
    public sealed class UiBar : MonoBehaviour
    {
        [SerializeField] RectTransform fill;
        [SerializeField] Graphic fillGraphic;
        [Tooltip("Optional second bar behind the fill that catches up slowly after a drop.")]
        [SerializeField] RectTransform trail;
        [SerializeField] float trailSpeed = 0.5f;

        [Header("Colour")]
        [SerializeField] bool changeColor;
        [SerializeField] Color fullColor = Color.green;
        [SerializeField] Color emptyColor = Color.red;

        float value = 1f;
        float trailValue = 1f;
        bool initialised;

        public void Set(float normalized)
        {
            normalized = Mathf.Clamp01(normalized);
            if (initialised && Mathf.Approximately(normalized, value)) return;

            value = normalized;
            SetWidth(fill, value);
            if (changeColor && fillGraphic != null) fillGraphic.color = Color.Lerp(emptyColor, fullColor, value);

            // Gains show instantly; only losses leave a trail.
            if (!initialised || value > trailValue)
            {
                trailValue = value;
                SetWidth(trail, trailValue);
            }
            initialised = true;
        }

        void Update()
        {
            if (trail == null || trailValue <= value) return;
            trailValue = Mathf.MoveTowards(trailValue, value, trailSpeed * Time.unscaledDeltaTime);
            SetWidth(trail, trailValue);
        }

        static void SetWidth(RectTransform rect, float normalized)
        {
            if (rect == null) return;
            Vector2 max = rect.anchorMax;
            max.x = normalized;
            rect.anchorMax = max;
        }
    }
}
