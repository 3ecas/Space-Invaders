using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// The weapon bar. Builds one <see cref="WeaponSlotView"/> per weapon in the player's loadout,
    /// so adding a weapon to the loadout adds a box here automatically.
    /// </summary>
    public sealed class WeaponHud : MonoBehaviour
    {
        [Tooltip("A disabled slot in the hierarchy that gets copied for each weapon.")]
        [SerializeField] WeaponSlotView slotTemplate;
        [SerializeField] Transform container;

        WeaponSlotView[] views;
        PlayerWeapons weapons;

        void Start()
        {
            Player player = Player.Instance;
            if (player == null || slotTemplate == null) return;

            weapons = player.Weapons;
            Transform parent = container != null ? container : transform;

            views = new WeaponSlotView[weapons.Slots.Count];
            for (int i = 0; i < views.Length; i++)
            {
                WeaponSlotView view = Instantiate(slotTemplate, parent);
                view.gameObject.SetActive(true);
                view.Bind(weapons.Slots[i], i + 1);
                views[i] = view;
            }
        }

        void Update()
        {
            if (views == null) return;
            for (int i = 0; i < views.Length; i++) views[i].Refresh(i == weapons.CurrentIndex);
        }
    }
}
