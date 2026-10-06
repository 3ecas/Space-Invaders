using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpaceShooter
{
    /// <summary>Owns the game flow: title screen, playing, pause, game over and restart.</summary>
    [DefaultExecutionOrder(-100)]
    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        /// <summary>True only while the game is running (not paused and not on a menu).</summary>
        public static bool Playing => Instance != null && Instance.State == GameState.Playing;

        // Set just before a restart so the reloaded scene jumps straight back into the action.
        static bool skipTitleOnce;

        [SerializeField] WaveSpawner waveSpawner;

        [Tooltip("Seconds of slow motion between the ship exploding and the game over screen.")]
        [SerializeField] float gameOverDelay = 1.5f;
        [Tooltip("Seconds the game over screen ignores input, so a held fire button doesn't skip it.")]
        [SerializeField] float restartInputDelay = 0.8f;
        [SerializeField] bool hideCursorWhilePlaying = true;

        public GameState State { get; private set; } = GameState.Title;

        /// <summary>Seconds spent actually playing this run.</summary>
        public float PlayTime { get; private set; }

        float stateChangedAt;
        Coroutine hitStopRoutine;
        bool playerDead;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            Instance = null;
            skipTitleOnce = false;
        }

        void Awake()
        {
            Instance = this;
            Time.timeScale = 1f;
        }

        void OnEnable() => GameEvents.PlayerDied += OnPlayerDied;

        void OnDisable() => GameEvents.PlayerDied -= OnPlayerDied;

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            Time.timeScale = 1f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        void Start()
        {
            if (skipTitleOnce)
            {
                skipTitleOnce = false;
                StartGame();
            }
            else
            {
                SetState(GameState.Title);
            }
        }

        void Update()
        {
            GameInput input = GameInput.Instance;
            if (input == null) return;

            switch (State)
            {
                case GameState.Title:
                    if (input.ConfirmPressed) StartGame();
                    break;

                case GameState.Playing:
                    PlayTime += Time.deltaTime;
                    if (!playerDead && input.PausePressed) SetState(GameState.Paused);
                    break;

                case GameState.Paused:
                    if (input.PausePressed) SetState(GameState.Playing);
                    else if (input.RestartPressed) Restart();
                    break;

                case GameState.GameOver:
                    bool ready = Time.unscaledTime - stateChangedAt >= restartInputDelay;
                    if (ready && (input.ConfirmPressed || input.RestartPressed)) Restart();
                    break;
            }
        }

        public void StartGame()
        {
            PlayTime = 0f;
            playerDead = false;
            SetState(GameState.Playing);
            if (waveSpawner != null) waveSpawner.Begin();
        }

        public void Restart()
        {
            skipTitleOnce = true;
            Time.timeScale = 1f;

            Scene scene = SceneManager.GetActiveScene();
            if (scene.buildIndex < 0)
            {
                Debug.LogError($"Can't restart: add '{scene.name}' to File > Build Profiles > Scene List.");
                skipTitleOnce = false;
                return;
            }
            SceneManager.LoadScene(scene.buildIndex);
        }

        /// <summary>Freezes the action for a split second. Makes big hits feel heavy.</summary>
        public void HitStop(float seconds)
        {
            if (State != GameState.Playing || playerDead || seconds <= 0f) return;
            if (hitStopRoutine != null) StopCoroutine(hitStopRoutine);
            hitStopRoutine = StartCoroutine(HitStopRoutine(seconds));
        }

        IEnumerator HitStopRoutine(float seconds)
        {
            Time.timeScale = 0.05f;
            yield return new WaitForSecondsRealtime(seconds);
            if (State == GameState.Playing && !playerDead) Time.timeScale = 1f;
            hitStopRoutine = null;
        }

        void OnPlayerDied()
        {
            if (playerDead) return;
            playerDead = true;
            StartCoroutine(GameOverRoutine());
        }

        IEnumerator GameOverRoutine()
        {
            // A beat of slow motion to watch the explosion before the menu appears.
            Time.timeScale = 0.35f;
            yield return new WaitForSecondsRealtime(gameOverDelay);
            SetState(GameState.GameOver);
        }

        void SetState(GameState next)
        {
            State = next;
            stateChangedAt = Time.unscaledTime;
            Time.timeScale = next == GameState.Paused ? 0f : 1f;

            bool playing = next == GameState.Playing;
            if (hideCursorWhilePlaying) Cursor.visible = !playing;
            Cursor.lockState = playing ? CursorLockMode.Confined : CursorLockMode.None;

            GameEvents.RaiseGameStateChanged(next);
        }
    }
}
