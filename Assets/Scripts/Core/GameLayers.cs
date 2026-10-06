using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// Physics layers the game relies on. The names must exist in
    /// Project Settings > Tags and Layers (the setup tool creates them).
    /// </summary>
    public static class GameLayers
    {
        public const string PlayerName = "Player";
        public const string EnemyName = "Enemy";

        static bool resolved;
        static int playerLayer;
        static int enemyLayer;
        static int playerMask;
        static int enemyMask;

        public static int Player { get { Resolve(); return playerLayer; } }
        public static int Enemy { get { Resolve(); return enemyLayer; } }
        public static int PlayerMask { get { Resolve(); return playerMask; } }
        public static int EnemyMask { get { Resolve(); return enemyMask; } }

        /// <summary>Layers a projectile fired by <paramref name="owner"/> is allowed to hit.</summary>
        public static int HitMaskFor(Team owner) => owner == Team.Player ? EnemyMask : PlayerMask;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => resolved = false;

        static void Resolve()
        {
            if (resolved) return;
            resolved = true;

            playerLayer = LayerMask.NameToLayer(PlayerName);
            enemyLayer = LayerMask.NameToLayer(EnemyName);
            playerMask = LayerMask.GetMask(PlayerName);
            enemyMask = LayerMask.GetMask(EnemyName);

            if (playerLayer < 0 || enemyLayer < 0)
            {
                Debug.LogError($"Missing physics layers '{PlayerName}' and/or '{EnemyName}'. " +
                               "Add them in Project Settings > Tags and Layers, or run Tools > Space Shooter > Rebuild Game.");
            }
        }
    }
}
