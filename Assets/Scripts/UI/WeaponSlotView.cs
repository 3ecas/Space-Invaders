using UnityEngine;
using UnityEngine.UI;

namespace SpaceShooter
{
    /// <summary>One box in the weapon bar: icon, number key and remaining ammo.</summary>
    public sealed class WeaponSlotView : MonoBehaviour
    {
        [SerializeField] Image frame;
        [SerializeField] Image icon;
        [SerializeField] Text keyLabel;
        [SerializeField] Text ammoLabel;

        [Header("Colours")]
        [SerializeField] Color selectedFrame = new Color(1f, 1f, 1f, 0.95f);
        [SerializeField] Color normalFrame = new Color(1f, 1f, 1f, 0.25f);
        [SerializeField] Color lockedIcon = new Color(1f, 1f, 1f, 0.15f);

        WeaponSlot slot;
        int shownAmmo = int.MinValue;
        bool shownSelected;
        bool shownUsable;
        bool initialised;

        public void Bind(WeaponSlot weaponSlot, int keyNumber)
        {
            slot = weaponSlot;
            initialised = false;
            shownAmmo = int.MinValue;

            if (icon != null) icon.sprite = slot.Data.icon;
            if (keyLabel != null) keyLabel.text = keyNumber.ToString();
        }

        public void Refresh(bool selected)
        {
            if (slot == null) return;

            bool usable = slot.Usable;
            if (!initialised || selected != shownSelected || usable != shownUsable)
            {
                initialised = true;
                shownSelected = selected;
                shownUsable = usable;

                if (frame != null) frame.color = selected ? selectedFrame : normalFrame;
                if (icon != null) icon.color = usable ? Color.white : lockedIcon;
                transform.localScale = selected ? new Vector3(1.12f, 1.12f, 1f) : Vector3.one;
            }

            if (ammoLabel == null) return;

            // -1 stands for "infinite", -2 for "not found yet".
            int ammo = slot.Data.infiniteAmmo ? -1 : slot.Unlocked ? slot.Ammo : -2;
            if (ammo == shownAmmo) return;
            shownAmmo = ammo;

            if (ammo == -1) ammoLabel.text = "∞";
            else if (ammo == -2) ammoLabel.text = "";
            else ammoLabel.text = ammo.ToString();
        }
    }
}
