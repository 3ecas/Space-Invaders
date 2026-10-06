using UnityEngine;
using UnityEngine.Rendering;

namespace SpaceShooter.EditorTools
{
    /// <summary>Everything the setup tool creates, passed between its build steps.</summary>
    public sealed class GameAssets
    {
        // Materials
        public Material SpriteMaterial;
        public Material ParticleMaterial;
        public Material RingMaterial;
        public VolumeProfile PostProcessing;

        // Weapons
        public WeaponData Blaster, Spread, Vulcan, Rail, Missile, Plasma;

        // Enemy definitions
        public EnemyData Drone, Dart, DartWing, Gunner, Kamikaze, AsteroidLarge, AsteroidSmall;
        public EnemyData Bomber, Spinner, Striker, SupplyShip, Boss;

        // Pickup definitions
        public PickupData Health, Armor, Shield, Power, Bomb, Overdrive;
        public PickupData SpreadCrate, VulcanCrate, RailCrate, MissileCrate, PlasmaCrate;
        public DropTable DropTable;
        public SfxLibrary SfxLibrary;

        // Effect prefabs
        public GameObject FxHitSpark, FxHitSparkEnemy;
        public GameObject FxExplosionSmall, FxExplosionMedium, FxExplosionLarge, FxExplosionBoss, FxExplosionPlasma;
        public GameObject FxPlayerHit, FxPlayerDeath, FxDash, FxBombBlast;

        // Projectile prefabs
        public Projectile ShotBlaster, ShotSpread, ShotVulcan, ShotRail, ShotMissile, ShotPlasma;
        public Projectile EnemyShotOrb, EnemyShotBolt, EnemyShotHeavy;

        // Enemy prefabs
        public Enemy EnemyDrone, EnemyDart, EnemyGunner, EnemyKamikaze, EnemyAsteroidLarge, EnemyAsteroidSmall;
        public Enemy EnemyBomber, EnemySpinner, EnemyStriker, EnemySupplyShip, EnemyBoss;

        // Other prefabs
        public Pickup PickupPrefab;
        public GameObject PlayerPrefab;
    }
}
