using UnityEngine;
using UnityEngine.UI;

namespace SpaceShooter
{
    /// <summary>A full-screen image that flashes and fades - bombs, boss kills, the player's death.</summary>
    [RequireComponent(typeof(Image))]
    public sealed class ScreenFlash : MonoBehaviour
    {
        [Tooltip("Opacity lost per second.")]
        [SerializeField] float fadeSpeed = 2.2f;

        Image image;
        float alpha;

        void Awake()
        {
            image = GetComponent<Image>();
            image.raycastTarget = false;
            image.enabled = false;
        }

        void OnEnable() => GameEvents.ScreenFlash += Flash;

        void OnDisable() => GameEvents.ScreenFlash -= Flash;

        void Flash(Color color, float strength)
        {
            alpha = Mathf.Max(alpha, Mathf.Clamp01(strength));
            color.a = alpha;
            image.color = color;
            image.enabled = true;
        }

        void Update()
        {
            if (alpha <= 0f) return;

            alpha -= fadeSpeed * Time.unscaledDeltaTime;
            if (alpha <= 0f)
            {
                alpha = 0f;
                image.enabled = false;
                return;
            }

            Color color = image.color;
            color.a = alpha;
            image.color = color;
        }
    }
}
