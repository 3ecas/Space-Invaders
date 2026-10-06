using UnityEngine;

namespace SpaceShooter
{
    /// <summary>Breaks into smaller enemies when destroyed - asteroids cracking apart, for example.</summary>
    [RequireComponent(typeof(Enemy))]
    public sealed class EnemySplitOnDeath : EnemyBehaviour
    {
        [SerializeField] EnemyData childData;
        [SerializeField] int count = 2;
        [Tooltip("The pieces fly off within this many degrees either side of straight down.")]
        [SerializeField] float spreadAngle = 65f;
        [SerializeField] float scatterRadius = 0.4f;

        void Awake() => GetComponent<Enemy>().Killed += OnKilled;

        void OnKilled(Enemy parent)
        {
            if (childData == null) return;

            for (int i = 0; i < count; i++)
            {
                // Spread the pieces evenly, with a little randomness so it doesn't look mechanical.
                float t = count > 1 ? i / (float)(count - 1) : 0.5f;
                float angle = Mathf.Lerp(-spreadAngle, spreadAngle, t) + Random.Range(-12f, 12f);

                EnemySpawnInfo info = EnemySpawnInfo.Default;
                info.direction = GameMath.Rotate(Vector2.down, angle);
                info.phase = Random.value * 6f;

                Enemy.Spawn(childData, parent.Position + Random.insideUnitCircle * scatterRadius, info, parent.Difficulty);
            }
        }
    }
}
