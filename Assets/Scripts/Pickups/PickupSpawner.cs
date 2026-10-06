using UnityEngine;

namespace SpaceShooter
{
    /// <summary>Listens for enemies dying and decides whether they leave a pickup behind.</summary>
    public sealed class PickupSpawner : MonoBehaviour
    {
        [SerializeField] Pickup pickupPrefab;
        [SerializeField] DropTable dropTable;
        [Tooltip("Multiplies every enemy's drop chance. Raise it for a more generous game.")]
        [SerializeField] float dropRateMultiplier = 1f;
        [Tooltip("A drop is forced after this many kills without one, so dry spells can't last forever.")]
        [SerializeField] int pityKills = 24;
        [SerializeField] int maxPickupsOnScreen = 14;

        int killsSinceDrop;

        void OnEnable() => GameEvents.EnemyKilled += OnEnemyKilled;

        void OnDisable() => GameEvents.EnemyKilled -= OnEnemyKilled;

        void OnEnemyKilled(Enemy enemy)
        {
            if (pickupPrefab == null || dropTable == null) return;

            EnemyData data = enemy.Data;
            Vector2 position = enemy.Position;

            for (int i = 0; i < data.guaranteedDrops; i++) Drop(position);

            if (data.dropChance <= 0f || Pickup.ActiveCount >= maxPickupsOnScreen) return;

            killsSinceDrop++;
            if (Random.value < data.dropChance * dropRateMultiplier || killsSinceDrop >= pityKills)
            {
                killsSinceDrop = 0;
                Drop(position);
            }
        }

        void Drop(Vector2 position)
        {
            PickupData data = dropTable.Roll(Player.Instance);
            if (data != null) Spawn(data, position);
        }

        public Pickup Spawn(PickupData data, Vector2 position)
        {
            position = PlayArea.Clamp(position, 0.8f);
            Pickup pickup = PoolManager.Spawn(pickupPrefab, position, Quaternion.identity);
            if (pickup != null) pickup.Init(data);
            return pickup;
        }
    }
}
