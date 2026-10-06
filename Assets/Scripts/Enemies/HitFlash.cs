using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// Flashes a sprite white for an instant when hit, by briefly swapping in an all-white copy
    /// of the sprite. Also gives it a small scale "punch".
    /// </summary>
    public sealed class HitFlash : MonoBehaviour
    {
        [SerializeField] SpriteRenderer target;
        [Tooltip("An all-white silhouette of the normal sprite.")]
        [SerializeField] Sprite flashSprite;
        [SerializeField] float duration = 0.06f;
        [SerializeField] float punchScale = 1.12f;

        Sprite normalSprite;
        Vector3 baseScale = Vector3.one;
        float timeLeft;

        void Awake()
        {
            if (target == null) return;
            normalSprite = target.sprite;
            baseScale = target.transform.localScale;
        }

        public void Flash()
        {
            if (target == null) return;
            if (flashSprite != null) target.sprite = flashSprite;
            target.transform.localScale = baseScale * punchScale;
            timeLeft = duration;
        }

        public void ResetFlash()
        {
            timeLeft = 0f;
            Restore();
        }

        void Update()
        {
            if (timeLeft <= 0f) return;
            timeLeft -= Time.deltaTime;
            if (timeLeft <= 0f) Restore();
        }

        void Restore()
        {
            if (target == null) return;
            if (normalSprite != null) target.sprite = normalSprite;
            target.transform.localScale = baseScale;
        }
    }
}
