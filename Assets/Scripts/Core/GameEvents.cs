using System;
using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// Global one-shot notifications. Systems raise these instead of calling each other directly,
    /// which keeps scripts independent (the HUD never needs a reference to the wave spawner, etc.).
    /// Subscribe in OnEnable and unsubscribe in OnDisable.
    /// </summary>
    public static class GameEvents
    {
        public static event Action<GameState> GameStateChanged;

        /// <summary>Wave number, and whether it is a boss wave.</summary>
        public static event Action<int, bool> WaveStarted;
        public static event Action<int> WaveCleared;

        public static event Action<Enemy> EnemyKilled;
        public static event Action<Enemy> BossSpawned;
        public static event Action<Enemy> BossDefeated;

        public static event Action<float> PlayerDamaged;
        public static event Action PlayerDied;
        public static event Action<PickupData> PickupCollected;

        /// <summary>World position, text, colour.</summary>
        public static event Action<Vector3, string, Color> FloatingText;
        /// <summary>Colour and peak opacity of a full-screen flash.</summary>
        public static event Action<Color, float> ScreenFlash;
        /// <summary>A short message for the centre banner.</summary>
        public static event Action<string> Announcement;

        public static void RaiseGameStateChanged(GameState state) => GameStateChanged?.Invoke(state);
        public static void RaiseWaveStarted(int wave, bool isBoss) => WaveStarted?.Invoke(wave, isBoss);
        public static void RaiseWaveCleared(int wave) => WaveCleared?.Invoke(wave);
        public static void RaiseEnemyKilled(Enemy enemy) => EnemyKilled?.Invoke(enemy);
        public static void RaiseBossSpawned(Enemy boss) => BossSpawned?.Invoke(boss);
        public static void RaiseBossDefeated(Enemy boss) => BossDefeated?.Invoke(boss);
        public static void RaisePlayerDamaged(float amount) => PlayerDamaged?.Invoke(amount);
        public static void RaisePlayerDied() => PlayerDied?.Invoke();
        public static void RaisePickupCollected(PickupData pickup) => PickupCollected?.Invoke(pickup);
        public static void RaiseFloatingText(Vector3 position, string text, Color color) => FloatingText?.Invoke(position, text, color);
        public static void RaiseScreenFlash(Color color, float strength) => ScreenFlash?.Invoke(color, strength);
        public static void RaiseAnnouncement(string message) => Announcement?.Invoke(message);

        // Keeps things clean when "Enter Play Mode" is set to skip domain reload.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            GameStateChanged = null;
            WaveStarted = null;
            WaveCleared = null;
            EnemyKilled = null;
            BossSpawned = null;
            BossDefeated = null;
            PlayerDamaged = null;
            PlayerDied = null;
            PickupCollected = null;
            FloatingText = null;
            ScreenFlash = null;
            Announcement = null;
        }
    }
}
