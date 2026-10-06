using UnityEngine;

namespace SpaceShooter
{
    /// <summary>Now and then lets a planet drift past in the background. Pure scenery.</summary>
    public sealed class DecorSpawner : MonoBehaviour
    {
        [SerializeField] Sprite[] sprites;
        [SerializeField] Material material;
        [Tooltip("Shortest and longest wait between planets, in seconds.")]
        [SerializeField] Vector2 interval = new Vector2(12f, 28f);
        [SerializeField] Vector2 scaleRange = new Vector2(0.7f, 1.6f);
        [Tooltip("Speed compared to the world scroll speed.")]
        [SerializeField] float speedFactor = 0.28f;
        [SerializeField] Color tint = new Color(0.62f, 0.64f, 0.75f, 1f);
        [SerializeField] int sortingOrder = -80;

        SpriteRenderer planet;
        float timer;
        float halfHeight;
        bool drifting;

        void Start()
        {
            var holder = new GameObject("Planet");
            holder.transform.SetParent(transform, false);

            planet = holder.AddComponent<SpriteRenderer>();
            if (material != null) planet.sharedMaterial = material;
            planet.sortingOrder = sortingOrder;
            planet.color = tint;
            planet.enabled = false;

            timer = Random.Range(2f, 6f);
        }

        void Update()
        {
            if (planet == null || sprites == null || sprites.Length == 0) return;

            if (!drifting)
            {
                timer -= Time.deltaTime;
                if (timer <= 0f) Launch();
                return;
            }

            Transform body = planet.transform;
            body.position += Vector3.down * (WorldScroll.Speed * speedFactor * Time.deltaTime);

            if (body.position.y < PlayArea.Bottom - halfHeight - 1f)
            {
                drifting = false;
                planet.enabled = false;
                timer = Random.Range(interval.x, interval.y);
            }
        }

        void Launch()
        {
            Sprite sprite = sprites[Random.Range(0, sprites.Length)];
            if (sprite == null) return;

            float scale = Random.Range(scaleRange.x, scaleRange.y);
            halfHeight = sprite.bounds.extents.y * scale;

            Transform body = planet.transform;
            body.localScale = new Vector3(scale, scale, 1f);
            body.rotation = Quaternion.Euler(0f, 0f, Random.Range(-25f, 25f));
            body.position = new Vector3(PlayArea.RandomX(0f), PlayArea.Top + halfHeight + 1f, 0f);

            planet.sprite = sprite;
            planet.flipX = Random.value < 0.5f;
            planet.enabled = true;
            drifting = true;
        }
    }
}
