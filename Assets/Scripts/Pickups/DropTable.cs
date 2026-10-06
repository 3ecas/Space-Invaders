using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// The list of pickups enemies can drop and how likely each one is.
    /// The odds lean towards what the player needs: more repairs when the hull is low,
    /// no power-ups once power is maxed, and so on.
    /// </summary>
    [CreateAssetMenu(menuName = "Space Shooter/Drop Table", fileName = "DropTable")]
    public sealed class DropTable : ScriptableObject
    {
        [System.Serializable]
        public struct Entry
        {
            public PickupData pickup;
            [Tooltip("Higher = drops more often compared to the other entries.")]
            public float weight;
        }

        public Entry[] entries = new Entry[0];

        public PickupData Roll(Player player)
        {
            float total = 0f;
            for (int i = 0; i < entries.Length; i++) total += WeightOf(entries[i], player);
            if (total <= 0f) return null;

            float roll = Random.value * total;
            for (int i = 0; i < entries.Length; i++)
            {
                float weight = WeightOf(entries[i], player);
                if (weight <= 0f) continue;
                roll -= weight;
                if (roll <= 0f) return entries[i].pickup;
            }
            return null;
        }

        static float WeightOf(Entry entry, Player player)
        {
            if (entry.pickup == null || entry.weight <= 0f) return 0f;
            if (player == null) return entry.weight;

            float weight = entry.weight;
            switch (entry.pickup.type)
            {
                case PickupType.Health:
                    float hull = player.Health.Normalized;
                    if (hull >= 0.999f) weight *= 0.15f;
                    else if (hull < 0.35f) weight *= 3f;
                    break;

                case PickupType.Armor:
                    if (player.Armor.IsFull) weight *= 0.2f;
                    break;

                case PickupType.Shield:
                    if (player.Shield.IsActive) weight *= 0.3f;
                    break;

                case PickupType.WeaponPower:
                    if (player.Weapons.IsMaxPower) weight = 0f;
                    break;

                case PickupType.Bomb:
                    if (player.Bombs.IsFull) weight = 0f;
                    break;
            }
            return weight;
        }
    }
}
