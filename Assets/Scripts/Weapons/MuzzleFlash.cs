using UnityEngine;

namespace SpaceShooter
{
    /// <summary>A sprite that pops for a couple of frames each time the ship fires.</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class MuzzleFlash : MonoBehaviour
    {
        [SerializeField] float duration = 0.05f;
        [SerializeField] Vector2 scaleRange = new Vector2(0.8f, 1.25f);

        SpriteRenderer sprite;
        Vector3 baseScale;
        float timeLeft;

        void Awake()
        {
            sprite = GetComponent<SpriteRenderer>();
            baseScale = transform.localScale;
            sprite.enabled = false;
        }

        public void Flash(Color color)
        {
            sprite.color = color;
            sprite.enabled = true;
            transform.localScale = baseScale * Random.Range(scaleRange.x, scaleRange.y);
            transform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(-20f, 20f));
            timeLeft = duration;
        }

        void Update()
        {
            if (timeLeft <= 0f) return;
            timeLeft -= Time.deltaTime;
            if (timeLeft <= 0f) sprite.enabled = false;
        }
    }
}
