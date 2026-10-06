using UnityEngine;

namespace SpaceShooter
{
    public enum PickupType
    {
        /// <summary>Repairs the hull by <c>amount</c>.</summary>
        Health = 0,
        /// <summary>Adds <c>amount</c> armor plating.</summary>
        Armor = 1,
        /// <summary>Raises the energy shield for <c>duration</c> seconds.</summary>
        Shield = 2,
        /// <summary>Raises the power level of every weapon by one.</summary>
        WeaponPower = 3,
        /// <summary>Adds <c>amount</c> bombs.</summary>
        Bomb = 4,
        /// <summary>Boosts fire rate for <c>duration</c> seconds.</summary>
        Overdrive = 5,
        /// <summary>Unlocks <c>weapon</c> and refills its ammo.</summary>
        Weapon = 6
    }

    /// <summary>
    /// One kind of pickup. To add a pickup: create one of these (Create > Space Shooter > Pickup)
    /// and add it to the drop table.
    /// </summary>
    [CreateAssetMenu(menuName = "Space Shooter/Pickup", fileName = "Pickup")]
    public sealed class PickupData : ScriptableObject
    {
        [Tooltip("Text that pops up when it is collected.")]
        public string label = "PICKUP";
        public PickupType type;
        public float amount = 25f;
        public float duration = 8f;
        public WeaponData weapon;

        [Header("Look & sound")]
        public Sprite sprite;
        public Color color = Color.white;
        public SfxId sfx = SfxId.Pickup;
    }
}
