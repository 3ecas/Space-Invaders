using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// Every so often sends a supply ship across the top of the screen. It doesn't fight back,
    /// but shooting it down before it escapes is worth a guaranteed pickup.
    /// </summary>
    public sealed class BonusShipSpawner : MonoBehaviour
    {
        [SerializeField] EnemyData bonusShip;
        [Tooltip("Shortest and longest wait between supply ships, in seconds.")]
        [SerializeField] Vector2 interval = new Vector2(20f, 34f);

        float timer;

        void OnEnable() => timer = Random.Range(interval.x, interval.y);

        void Update()
        {
            if (bonusShip == null || !GameManager.Playing || !Player.Exists) return;

            timer -= Time.deltaTime;
            if (timer > 0f) return;
            timer = Random.Range(interval.x, interval.y);

            float side = Random.value < 0.5f ? -1f : 1f;
            var position = new Vector2(
                side > 0f ? PlayArea.Right + 2f : PlayArea.Left - 2f,
                PlayArea.Top - Random.Range(1.5f, 3.5f));

            EnemySpawnInfo info = EnemySpawnInfo.Default;
            info.direction = new Vector2(-side, 0f);
            info.side = side;

            Enemy.Spawn(bonusShip, position, info, DifficultyScale.Normal);
        }
    }
}
