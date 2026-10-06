using UnityEngine;
using UnityEngine.UI;

namespace SpaceShooter
{
    /// <summary>The big centre-screen announcements: "WAVE 3", boss warnings, "WAVE CLEARED".</summary>
    public sealed class WaveBanner : MonoBehaviour
    {
        [SerializeField] CanvasGroup group;
        [SerializeField] Text title;
        [SerializeField] Text subtitle;

        [Header("Timing")]
        [SerializeField] float fadeIn = 0.2f;
        [SerializeField] float fadeOut = 0.45f;

        [Header("Colours")]
        [SerializeField] Color waveColor = new Color(0.55f, 0.9f, 1f);
        [SerializeField] Color clearedColor = new Color(0.6f, 1f, 0.65f);
        [SerializeField] Color bossColor = new Color(1f, 0.3f, 0.3f);
        [SerializeField] Color victoryColor = new Color(1f, 0.85f, 0.35f);

        float age;
        float hold;
        bool showing;
        bool bossWave;

        void Awake()
        {
            if (group != null) group.alpha = 0f;
        }

        void OnEnable()
        {
            GameEvents.WaveStarted += OnWaveStarted;
            GameEvents.WaveCleared += OnWaveCleared;
            GameEvents.BossDefeated += OnBossDefeated;
        }

        void OnDisable()
        {
            GameEvents.WaveStarted -= OnWaveStarted;
            GameEvents.WaveCleared -= OnWaveCleared;
            GameEvents.BossDefeated -= OnBossDefeated;
        }

        void OnWaveStarted(int wave, bool isBoss)
        {
            bossWave = isBoss;
            if (isBoss) Show("WARNING", "BOSS APPROACHING", bossColor, 2.2f);
            else Show("WAVE " + wave, "", waveColor, 1.2f);
        }

        void OnWaveCleared(int wave)
        {
            // Boss waves already get their own "destroyed" banner.
            if (!bossWave) Show("WAVE CLEARED", "", clearedColor, 0.9f);
        }

        void OnBossDefeated(Enemy boss)
        {
            Show("BOSS DESTROYED", "ENTERING NEXT SECTOR", victoryColor, 2.4f);
        }

        void Show(string titleText, string subtitleText, Color color, float holdSeconds)
        {
            if (title != null)
            {
                title.text = titleText;
                title.color = color;
            }
            if (subtitle != null) subtitle.text = subtitleText;

            age = 0f;
            hold = holdSeconds;
            showing = true;
        }

        void Update()
        {
            if (!showing || group == null) return;

            age += Time.unscaledDeltaTime;

            float alpha;
            if (age < fadeIn) alpha = age / fadeIn;
            else if (age < fadeIn + hold) alpha = 1f;
            else alpha = 1f - (age - fadeIn - hold) / fadeOut;

            if (alpha <= 0f && age > fadeIn)
            {
                showing = false;
                alpha = 0f;
            }

            group.alpha = Mathf.Clamp01(alpha);

            // Starts slightly oversized and settles: a quick "slam" onto the screen.
            float pop = 1f + 0.25f * (1f - Mathf.Clamp01(age / Mathf.Max(0.01f, fadeIn * 1.5f)));
            transform.localScale = new Vector3(pop, pop, 1f);
        }
    }
}
