using System.Collections.Generic;
using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// The ship's arsenal: which weapons it carries, which one is selected, firing, ammo,
    /// the shared power level and the temporary overdrive boost.
    /// </summary>
    public sealed class PlayerWeapons : MonoBehaviour
    {
        [Tooltip("Every weapon the ship can carry, in number-key order. The first one is the starting weapon.")]
        [SerializeField] WeaponData[] loadout;
        [SerializeField] Transform muzzle;
        [SerializeField] MuzzleFlash muzzleFlash;

        [Header("Power")]
        [SerializeField] int maxPowerLevel = 5;
        [Tooltip("Fire-rate multiplier while overdrive is active.")]
        [SerializeField] float overdriveFireRate = 1.8f;

        [Header("Options")]
        [Tooltip("Fire continuously without holding the button (toggle in game with F).")]
        [SerializeField] bool autoFire;

        public IReadOnlyList<WeaponSlot> Slots => slots;
        public int CurrentIndex { get; private set; }
        public WeaponSlot Current => slots.Count > 0 ? slots[CurrentIndex] : null;

        public int PowerLevel { get; private set; } = 1;
        public int MaxPowerLevel => maxPowerLevel;
        public bool IsMaxPower => PowerLevel >= maxPowerLevel;

        public bool AutoFire => autoFire;
        public bool OverdriveActive => overdriveTimeLeft > 0f;
        public float OverdriveTimeLeft => overdriveTimeLeft;

        readonly List<WeaponSlot> slots = new List<WeaponSlot>();
        PlayerController controller;
        float cooldown;
        float overdriveTimeLeft;

        void Awake()
        {
            controller = GetComponent<PlayerController>();

            if (loadout == null) return;
            foreach (WeaponData data in loadout)
            {
                if (data == null) continue;
                var slot = new WeaponSlot(data) { Unlocked = slots.Count == 0 };
                slots.Add(slot);
            }
        }

        void Update()
        {
            if (!GameManager.Playing) return;

            GameInput input = GameInput.Instance;
            if (input == null || slots.Count == 0) return;

            float dt = Time.deltaTime;
            if (overdriveTimeLeft > 0f) overdriveTimeLeft -= dt;

            if (input.ToggleAutoFirePressed)
            {
                autoFire = !autoFire;
                GameEvents.RaiseAnnouncement(autoFire ? "AUTO-FIRE ON" : "AUTO-FIRE OFF");
            }

            HandleSwitching(input);

            cooldown -= dt;
            if ((autoFire || input.FireHeld) && cooldown <= 0f) Fire();
        }

        // ---------------------------------------------------------------- firing

        void Fire()
        {
            WeaponSlot slot = Current;
            if (!slot.Usable)
            {
                Select(0);
                slot = Current;
                if (!slot.Usable) return;
            }

            WeaponData data = slot.Data;
            if (data.projectilePrefab == null) return;

            float rate = data.GetFireRate(PowerLevel) * (OverdriveActive ? overdriveFireRate : 1f);
            cooldown = 1f / Mathf.Max(0.1f, rate);

            Vector2 aim = controller != null ? controller.AimDirection : Vector2.up;
            Vector2 origin = muzzle != null ? muzzle.position : transform.position;

            BulletPatterns.Fan(data.projectilePrefab, Team.Player, origin, aim,
                data.GetProjectileCount(PowerLevel), data.angleStep, data.lateralSpacing,
                data.projectileSpeed, data.GetDamage(PowerLevel), data.projectileLifetime, data.randomSpread);

            AudioManager.Play(data.fireSfx);
            if (data.fireShake > 0f) CameraShake.Shake(data.fireShake);
            if (muzzleFlash != null) muzzleFlash.Flash(data.color);

            if (data.infiniteAmmo) return;

            slot.Ammo = Mathf.Max(0, slot.Ammo - 1);
            if (slot.Ammo == 0)
            {
                GameEvents.RaiseAnnouncement(data.displayName.ToUpperInvariant() + " EMPTY");
                Select(0);
            }
        }

        // ---------------------------------------------------------------- switching

        void HandleSwitching(GameInput input)
        {
            int key = input.WeaponSlotPressed;
            if (key >= 0 && key < slots.Count)
            {
                if (slots[key].Usable) Select(key);
                return;
            }

            int direction = input.WeaponCycle;
            if (direction != 0) Cycle(direction);
        }

        /// <summary>Steps to the next (+1) or previous (-1) weapon that can actually be fired.</summary>
        public void Cycle(int direction)
        {
            int count = slots.Count;
            for (int i = 1; i < count; i++)
            {
                int index = ((CurrentIndex + direction * i) % count + count) % count;
                if (!slots[index].Usable) continue;
                Select(index);
                return;
            }
        }

        void Select(int index)
        {
            if (index == CurrentIndex || index < 0 || index >= slots.Count) return;

            CurrentIndex = index;
            // A tiny delay so swapping weapons can't be used to cheat the fire rate.
            cooldown = Mathf.Max(cooldown, 0.1f);
            AudioManager.Play(SfxId.WeaponSwitch);
        }

        // ---------------------------------------------------------------- pickups

        /// <summary>Unlocks a weapon and tops up its ammo.</summary>
        public void GiveWeapon(WeaponData data)
        {
            int index = IndexOf(data);
            if (index < 0) return;

            WeaponSlot slot = slots[index];
            bool wasUsable = slot.Usable;

            slot.Unlocked = true;
            if (!data.infiniteAmmo) slot.Ammo = Mathf.Min(data.maxAmmo, slot.Ammo + data.ammoPerPickup);

            // Jump to the new toy, unless the player is busy with another special weapon.
            if (!wasUsable || CurrentIndex == 0) Select(index);
        }

        /// <summary>Raises the power level of every weapon. Returns false when already maxed out.</summary>
        public bool UpgradePower()
        {
            if (IsMaxPower) return false;
            PowerLevel++;
            return true;
        }

        public void ActivateOverdrive(float duration)
        {
            overdriveTimeLeft = Mathf.Max(overdriveTimeLeft, duration);
        }

        int IndexOf(WeaponData data)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].Data == data) return i;
            }
            return -1;
        }
    }
}
