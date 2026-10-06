using UnityEngine;

namespace SpaceShooter
{
    /// <summary>What each pickup type actually does. Add a case here when you add a new <see cref="PickupType"/>.</summary>
    public static class PickupEffects
    {
        // Points awarded when a pickup can't do anything useful (e.g. power-up at max power).
        const int ConsolationScore = 500;

        public static void Apply(PickupData data, Player player)
        {
            if (data == null || player == null) return;

            switch (data.type)
            {
                case PickupType.Health:
                    player.Health.Heal(data.amount);
                    break;

                case PickupType.Armor:
                    player.Armor.Add(data.amount);
                    break;

                case PickupType.Shield:
                    player.Shield.Activate(data.duration);
                    break;

                case PickupType.WeaponPower:
                    if (!player.Weapons.UpgradePower()) AwardConsolation();
                    break;

                case PickupType.Bomb:
                    if (player.Bombs.IsFull) AwardConsolation();
                    else player.Bombs.Add(Mathf.Max(1, Mathf.RoundToInt(data.amount)));
                    break;

                case PickupType.Overdrive:
                    player.Weapons.ActivateOverdrive(data.duration);
                    break;

                case PickupType.Weapon:
                    player.Weapons.GiveWeapon(data.weapon);
                    break;
            }
        }

        static void AwardConsolation()
        {
            if (ScoreManager.Instance != null) ScoreManager.Instance.AddScore(ConsolationScore);
        }
    }
}
