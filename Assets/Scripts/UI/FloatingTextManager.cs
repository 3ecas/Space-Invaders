using UnityEngine;
using UnityEngine.UI;

namespace SpaceShooter
{
    /// <summary>
    /// The little pop-up texts in the game world ("+150", "+25 HULL"). They are UI labels that
    /// follow a world position, recycled from a fixed pool.
    /// Requires a Screen Space - Overlay canvas.
    /// </summary>
    public sealed class FloatingTextManager : MonoBehaviour
    {
        [Tooltip("A disabled Text in the hierarchy that gets copied to fill the pool.")]
        [SerializeField] Text template;
        [SerializeField] int poolSize = 48;
        [SerializeField] float lifetime = 0.9f;
        [Tooltip("World units per second the text floats upwards.")]
        [SerializeField] float riseSpeed = 1.8f;

        struct Item
        {
            public Text Label;
            public RectTransform Rect;
            public Vector3 World;
            public Color Color;
            public float Age;
            public bool Active;
        }

        Item[] items;
        Camera cam;
        int next;

        void Awake()
        {
            if (template == null) return;

            items = new Item[Mathf.Max(1, poolSize)];
            for (int i = 0; i < items.Length; i++)
            {
                Text label = Instantiate(template, transform);
                label.gameObject.SetActive(false);
                items[i] = new Item { Label = label, Rect = label.rectTransform };
            }
        }

        void OnEnable() => GameEvents.FloatingText += Show;

        void OnDisable() => GameEvents.FloatingText -= Show;

        void Show(Vector3 worldPosition, string message, Color color)
        {
            if (items == null) return;

            // Round-robin: when the pool is full the oldest text is the one that gets replaced.
            int index = next;
            next = (next + 1) % items.Length;

            Item item = items[index];
            item.World = worldPosition + (Vector3)(Random.insideUnitCircle * 0.3f);
            item.Color = color;
            item.Age = 0f;
            item.Active = true;
            item.Label.text = message;
            item.Label.color = color;
            item.Label.gameObject.SetActive(true);
            items[index] = item;
        }

        void LateUpdate()
        {
            if (items == null) return;
            if (cam == null) cam = Camera.main;
            if (cam == null) return;

            float dt = Time.deltaTime;

            for (int i = 0; i < items.Length; i++)
            {
                if (!items[i].Active) continue;

                Item item = items[i];
                item.Age += dt;

                if (item.Age >= lifetime)
                {
                    item.Active = false;
                    item.Label.gameObject.SetActive(false);
                    items[i] = item;
                    continue;
                }

                item.World.y += riseSpeed * dt;

                float t = item.Age / lifetime;
                // On an overlay canvas, UI world space is screen pixels.
                item.Rect.position = cam.WorldToScreenPoint(item.World);

                float pop = 1f + 0.4f * Mathf.Clamp01(1f - t * 6f);
                item.Rect.localScale = new Vector3(pop, pop, 1f);

                Color color = item.Color;
                color.a = 1f - t * t;
                item.Label.color = color;

                items[i] = item;
            }
        }
    }
}
