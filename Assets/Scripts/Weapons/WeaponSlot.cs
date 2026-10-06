namespace SpaceShooter
{
    /// <summary>The player's copy of a weapon: whether it has been found yet and how much ammo is left.</summary>
    public sealed class WeaponSlot
    {
        public readonly WeaponData Data;
        public bool Unlocked;
        public int Ammo;

        public WeaponSlot(WeaponData data)
        {
            Data = data;
        }

        public bool HasAmmo => Data.infiniteAmmo || Ammo > 0;
        public bool Usable => Unlocked && HasAmmo;
    }
}
