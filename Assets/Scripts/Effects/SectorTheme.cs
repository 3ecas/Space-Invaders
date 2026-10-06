using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// Shifts the colour of the backdrop each time a boss is defeated, so every "sector" of the
    /// journey looks a little different.
    /// </summary>
    public sealed class SectorTheme : MonoBehaviour
    {
        [SerializeField] SpriteRenderer[] targets;
        [SerializeField] Color[] palette = { Color.white };
        [SerializeField] float fadeSeconds = 4f;

        int index;
        Color current;
        Color goal;

        void Awake()
        {
            current = goal = palette != null && palette.Length > 0 ? palette[0] : Color.white;
            Apply();
        }

        void OnEnable() => GameEvents.BossDefeated += OnBossDefeated;

        void OnDisable() => GameEvents.BossDefeated -= OnBossDefeated;

        void OnBossDefeated(Enemy boss)
        {
            if (palette == null || palette.Length == 0) return;
            index = (index + 1) % palette.Length;
            goal = palette[index];
        }

        void Update()
        {
            if (current == goal) return;

            float step = fadeSeconds > 0f ? Time.deltaTime / fadeSeconds : 1f;
            current = new Color(
                Mathf.MoveTowards(current.r, goal.r, step),
                Mathf.MoveTowards(current.g, goal.g, step),
                Mathf.MoveTowards(current.b, goal.b, step),
                Mathf.MoveTowards(current.a, goal.a, step));
            Apply();
        }

        void Apply()
        {
            if (targets == null) return;
            foreach (SpriteRenderer target in targets)
            {
                if (target != null) target.color = current;
            }
        }
    }
}
