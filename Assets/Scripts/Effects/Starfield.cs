using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// Layers of stars streaming down the screen. Far layers move slowly and near ones fast
    /// (parallax), which is what sells the feeling of flying forward. During a speed boost the
    /// stars stretch into streaks.
    /// </summary>
    public sealed class Starfield : MonoBehaviour
    {
        [System.Serializable]
        public class Layer
        {
            public int count = 60;
            [Tooltip("Speed compared to the world scroll speed. Small = far away.")]
            public float speedFactor = 0.3f;
            public Vector2 sizeRange = new Vector2(0.05f, 0.1f);
            public Color color = Color.white;
            public int sortingOrder = -90;
        }

        [SerializeField] Sprite starSprite;
        [SerializeField] Material material;
        [SerializeField] Layer[] layers = new Layer[0];
        [Tooltip("How much stars stretch into streaks while the scroll speed is boosted.")]
        [SerializeField] float streakAmount = 1.2f;

        const float Margin = 1f;

        Transform[] stars;
        float[] speeds;
        float[] sizes;

        void Start()
        {
            int total = 0;
            foreach (Layer layer in layers) total += Mathf.Max(0, layer.count);

            stars = new Transform[total];
            speeds = new float[total];
            sizes = new float[total];

            int index = 0;
            foreach (Layer layer in layers)
            {
                for (int i = 0; i < layer.count; i++)
                {
                    var star = new GameObject("Star");
                    star.transform.SetParent(transform, false);
                    star.transform.position = new Vector3(
                        Random.Range(PlayArea.Left - Margin, PlayArea.Right + Margin),
                        Random.Range(PlayArea.Bottom - Margin, PlayArea.Top + Margin),
                        0f);

                    var sprite = star.AddComponent<SpriteRenderer>();
                    sprite.sprite = starSprite;
                    if (material != null) sprite.sharedMaterial = material;
                    sprite.sortingOrder = layer.sortingOrder;

                    // Vary the brightness a little so the sky doesn't look uniform.
                    Color tint = layer.color;
                    tint.a *= Random.Range(0.55f, 1f);
                    sprite.color = tint;

                    stars[index] = star.transform;
                    speeds[index] = layer.speedFactor * Random.Range(0.85f, 1.15f);
                    sizes[index] = Random.Range(layer.sizeRange.x, layer.sizeRange.y);
                    index++;
                }
            }
        }

        void Update()
        {
            if (stars == null) return;

            float distance = WorldScroll.Speed * Time.deltaTime;
            float boost = Mathf.Max(0f, WorldScroll.SpeedFactor - 1f);
            float bottom = PlayArea.Bottom - Margin;
            float wrapHeight = PlayArea.Height + Margin * 2f;

            for (int i = 0; i < stars.Length; i++)
            {
                Transform star = stars[i];
                Vector3 position = star.position;
                position.y -= distance * speeds[i];

                if (position.y < bottom)
                {
                    position.y += wrapHeight;
                    position.x = Random.Range(PlayArea.Left - Margin, PlayArea.Right + Margin);
                }
                star.position = position;

                float size = sizes[i];
                star.localScale = new Vector3(size, size * (1f + boost * speeds[i] * streakAmount), 1f);
            }
        }
    }
}
