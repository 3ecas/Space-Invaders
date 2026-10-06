using UnityEngine;

namespace SpaceShooter
{
    /// <summary>Score, kill-combo multiplier and the saved high score.</summary>
    public sealed class ScoreManager : MonoBehaviour
    {
        const string HighScoreKey = "SpaceShooter.HighScore";

        public static ScoreManager Instance { get; private set; }

        [Tooltip("Kill again within this many seconds to keep the combo alive.")]
        [SerializeField] float comboWindow = 3f;
        [Tooltip("Kills in a row needed to raise the multiplier by one.")]
        [SerializeField] int killsPerMultiplier = 6;
        [SerializeField] int maxMultiplier = 8;
        [SerializeField] Color scoreTextColor = new Color(1f, 0.92f, 0.45f);

        public int Score { get; private set; }
        public int HighScore { get; private set; }
        public int Kills { get; private set; }
        public int Multiplier { get; private set; } = 1;
        public int MaxMultiplier => maxMultiplier;

        /// <summary>1 right after a kill, falling to 0 as the combo runs out.</summary>
        public float ComboTime01 => comboWindow > 0f ? Mathf.Clamp01(comboTimer / comboWindow) : 0f;

        int comboKills;
        float comboTimer;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => Instance = null;

        void Awake()
        {
            Instance = this;
            HighScore = PlayerPrefs.GetInt(HighScoreKey, 0);
        }

        void OnEnable()
        {
            GameEvents.EnemyKilled += OnEnemyKilled;
            GameEvents.PlayerDamaged += OnPlayerDamaged;
            GameEvents.GameStateChanged += OnGameStateChanged;
        }

        void OnDisable()
        {
            GameEvents.EnemyKilled -= OnEnemyKilled;
            GameEvents.PlayerDamaged -= OnPlayerDamaged;
            GameEvents.GameStateChanged -= OnGameStateChanged;
        }

        void OnDestroy()
        {
            SaveHighScore();
            if (Instance == this) Instance = null;
        }

        void Update()
        {
            if (comboTimer <= 0f || !GameManager.Playing) return;
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0f) BreakCombo();
        }

        public void AddScore(int amount)
        {
            if (amount <= 0) return;
            Score += amount;
            if (Score > HighScore) HighScore = Score;
        }

        void OnEnemyKilled(Enemy enemy)
        {
            Kills++;
            comboKills++;
            comboTimer = comboWindow;
            Multiplier = Mathf.Min(maxMultiplier, 1 + comboKills / Mathf.Max(1, killsPerMultiplier));

            int points = enemy.Data.scoreValue * Multiplier;
            if (points <= 0) return;
            AddScore(points);
            GameEvents.RaiseFloatingText(enemy.transform.position, "+" + points, scoreTextColor);
        }

        void OnPlayerDamaged(float amount) => BreakCombo();

        void OnGameStateChanged(GameState state)
        {
            if (state == GameState.GameOver) SaveHighScore();
        }

        void BreakCombo()
        {
            comboKills = 0;
            comboTimer = 0f;
            Multiplier = 1;
        }

        void SaveHighScore()
        {
            if (HighScore <= PlayerPrefs.GetInt(HighScoreKey, 0)) return;
            PlayerPrefs.SetInt(HighScoreKey, HighScore);
            PlayerPrefs.Save();
        }
    }
}
