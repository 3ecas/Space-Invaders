using UnityEngine;
using UnityEngine.UI;

namespace SpaceShooter
{
    /// <summary>Small messages near the bottom of the screen ("AUTO-FIRE ON", "RAILGUN EMPTY"...).</summary>
    public sealed class NoticeText : MonoBehaviour
    {
        [SerializeField] Text label;
        [SerializeField] float holdSeconds = 1.6f;
        [SerializeField] float fadeSeconds = 0.4f;

        float timeLeft;

        void Awake() => SetAlpha(0f);

        void OnEnable() => GameEvents.Announcement += Show;

        void OnDisable() => GameEvents.Announcement -= Show;

        void Show(string message)
        {
            if (label == null) return;
            label.text = message;
            timeLeft = holdSeconds + fadeSeconds;
            SetAlpha(1f);
        }

        void Update()
        {
            if (timeLeft <= 0f) return;
            timeLeft -= Time.unscaledDeltaTime;
            SetAlpha(fadeSeconds > 0f ? Mathf.Clamp01(timeLeft / fadeSeconds) : 0f);
        }

        void SetAlpha(float alpha)
        {
            if (label == null) return;
            Color color = label.color;
            color.a = alpha;
            label.color = color;
        }
    }
}
