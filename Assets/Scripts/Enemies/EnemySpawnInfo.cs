using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// Instructions the spawner hands to a freshly spawned enemy. Each behaviour reads the parts
    /// it cares about and ignores the rest.
    /// </summary>
    public struct EnemySpawnInfo
    {
        /// <summary>Initial direction of travel.</summary>
        public Vector2 direction;
        /// <summary>Where a hovering enemy should settle.</summary>
        public Vector2 anchor;
        /// <summary>Offsets weaving motion so a group ripples instead of moving as a block.</summary>
        public float phase;
        /// <summary>-1 or +1. Mirrors flight paths left/right.</summary>
        public float side;
        /// <summary>Which route from <see cref="EnemyPaths"/> to fly.</summary>
        public int pathIndex;
        /// <summary>This enemy's position within its group.</summary>
        public int index;
        public int groupSize;

        public static EnemySpawnInfo Default => new EnemySpawnInfo
        {
            direction = Vector2.down,
            side = 1f,
            groupSize = 1
        };
    }

    /// <summary>Multipliers the wave director applies so later waves hit harder.</summary>
    public readonly struct DifficultyScale
    {
        public readonly float Health;
        public readonly float Speed;
        public readonly float BulletSpeed;
        public readonly float Damage;

        public DifficultyScale(float health, float speed, float bulletSpeed, float damage)
        {
            Health = health;
            Speed = speed;
            BulletSpeed = bulletSpeed;
            Damage = damage;
        }

        public static DifficultyScale Normal => new DifficultyScale(1f, 1f, 1f, 1f);
    }
}
