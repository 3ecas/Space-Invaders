using UnityEngine;
using UnityEngine.UI;

namespace SpaceShooter
{
    /// <summary>The wide bar at the top of the screen that appears during a boss fight.</summary>
    public sealed class BossHealthBar : MonoBehaviour
    {
        [Tooltip("Child that holds the visuals. It is switched off when no boss is around.")]
        [SerializeField] GameObject root;
        [SerializeField] UiBar bar;
        [SerializeField] Text nameText;

        Enemy boss;

        void Awake()
        {
            if (root != null) root.SetActive(false);
        }

        void OnEnable()
        {
            GameEvents.BossSpawned += OnBossSpawned;
            GameEvents.BossDefeated += OnBossGone;
        }

        void OnDisable()
        {
            GameEvents.BossSpawned -= OnBossSpawned;
            GameEvents.BossDefeated -= OnBossGone;
        }

        void OnBossSpawned(Enemy spawned)
        {
            boss = spawned;
            if (root != null) root.SetActive(true);
            if (nameText != null) nameText.text = spawned.Data.displayName.ToUpperInvariant();
            if (bar != null) bar.Set(1f);
        }

        void OnBossGone(Enemy defeated)
        {
            boss = null;
            if (root != null) root.SetActive(false);
        }

        void Update()
        {
            if (boss == null) return;

            if (!boss.IsAlive)
            {
                OnBossGone(boss);
                return;
            }
            if (bar != null) bar.Set(boss.Health.Normalized);
        }
    }
}
