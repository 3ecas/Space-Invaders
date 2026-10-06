using UnityEngine;
using UnityEngine.UI;

namespace SpaceShooter
{
    /// <summary>Shows the right overlay for each game state: title, pause or game over.</summary>
    public sealed class GameScreens : MonoBehaviour
    {
        [SerializeField] GameObject titlePanel;
        [SerializeField] GameObject pausePanel;
        [SerializeField] GameObject gameOverPanel;
        [Tooltip("The in-game HUD, hidden while the title screen is up.")]
        [SerializeField] GameObject hudRoot;

        [Header("Game over details")]
        [SerializeField] WaveSpawner waves;
        [SerializeField] Text finalScoreText;
        [SerializeField] Text bestScoreText;
        [SerializeField] Text summaryText;

        [Header("\"Press to start\" prompts")]
        [SerializeField] Graphic[] blinkers;
        [SerializeField] float blinkSpeed = 3f;

        void Awake() => Apply(GameState.Title);

        void OnEnable() => GameEvents.GameStateChanged += Apply;

        void OnDisable() => GameEvents.GameStateChanged -= Apply;

        void Apply(GameState state)
        {
            if (titlePanel != null) titlePanel.SetActive(state == GameState.Title);
            if (pausePanel != null) pausePanel.SetActive(state == GameState.Paused);
            if (gameOverPanel != null) gameOverPanel.SetActive(state == GameState.GameOver);
            if (hudRoot != null) hudRoot.SetActive(state != GameState.Title);

            if (state == GameState.GameOver) FillGameOver();
        }

        void FillGameOver()
        {
            ScoreManager score = ScoreManager.Instance;
            if (score == null) return;

            if (finalScoreText != null) finalScoreText.text = score.Score.ToString("N0");

            if (bestScoreText != null)
            {
                bool record = score.Score > 0 && score.Score >= score.HighScore;
                bestScoreText.text = record ? "NEW HIGH SCORE!" : "BEST  " + score.HighScore.ToString("N0");
            }

            if (summaryText != null)
            {
                int wave = waves != null ? waves.Wave : 0;
                summaryText.text = "REACHED WAVE " + wave + "   -   KILLS " + score.Kills;
            }
        }

        void Update()
        {
            if (blinkers == null) return;

            float alpha = 0.35f + 0.65f * Mathf.Abs(Mathf.Sin(Time.unscaledTime * blinkSpeed));
            foreach (Graphic graphic in blinkers)
            {
                if (graphic == null || !graphic.gameObject.activeInHierarchy) continue;
                Color color = graphic.color;
                color.a = alpha;
                graphic.color = color;
            }
        }
    }
}
