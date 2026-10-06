using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SpaceShooter.EditorTools
{
    /// <summary>
    /// Creates the ScriptableObject assets that hold the game's tuning numbers: weapons, enemies,
    /// pickups, the drop table and the sound library. After the first build these are yours to
    /// edit in the Inspector - select one under Assets/Data and change the values.
    /// </summary>
    public static class DataFactory
    {
        public const string Folder = "Assets/Data";

        /// <summary>
        /// Step 1: make sure every asset exists (still empty), so prefabs can point at them.
        /// </summary>
        public static void CreateAssets(GameAssets a)
        {
            EditorUtil.EnsureFolder(Folder + "/Weapons");
            EditorUtil.EnsureFolder(Folder + "/Enemies");
            EditorUtil.EnsureFolder(Folder + "/Pickups");

            a.Blaster = WeaponAsset("Blaster");
            a.Spread = WeaponAsset("SpreadShot");
            a.Vulcan = WeaponAsset("Vulcan");
            a.Rail = WeaponAsset("Railgun");
            a.Missile = WeaponAsset("Missiles");
            a.Plasma = WeaponAsset("PlasmaCannon");

            a.Drone = EnemyAsset("Drone");
            a.Dart = EnemyAsset("Dart");
            a.DartWing = EnemyAsset("DartWing");
            a.Gunner = EnemyAsset("Gunner");
            a.Kamikaze = EnemyAsset("Kamikaze");
            a.AsteroidLarge = EnemyAsset("AsteroidLarge");
            a.AsteroidSmall = EnemyAsset("AsteroidSmall");
            a.Bomber = EnemyAsset("Bomber");
            a.Spinner = EnemyAsset("Spinner");
            a.Striker = EnemyAsset("Striker");
            a.SupplyShip = EnemyAsset("SupplyShip");
            a.Boss = EnemyAsset("Boss");

            a.Health = PickupAsset("Repair");
            a.Armor = PickupAsset("ArmorPlating");
            a.Shield = PickupAsset("EnergyShield");
            a.Power = PickupAsset("PowerUp");
            a.Bomb = PickupAsset("Bomb");
            a.Overdrive = PickupAsset("Overdrive");
            a.SpreadCrate = PickupAsset("WeaponSpreadShot");
            a.VulcanCrate = PickupAsset("WeaponVulcan");
            a.RailCrate = PickupAsset("WeaponRailgun");
            a.MissileCrate = PickupAsset("WeaponMissiles");
            a.PlasmaCrate = PickupAsset("WeaponPlasmaCannon");

            a.DropTable = EditorUtil.LoadOrCreate<DropTable>(Folder + "/DropTable.asset");
            a.SfxLibrary = EditorUtil.LoadOrCreate<SfxLibrary>(Folder + "/SfxLibrary.asset");
        }

        static WeaponData WeaponAsset(string name) => EditorUtil.LoadOrCreate<WeaponData>($"{Folder}/Weapons/{name}.asset");
        static EnemyData EnemyAsset(string name) => EditorUtil.LoadOrCreate<EnemyData>($"{Folder}/Enemies/{name}.asset");
        static PickupData PickupAsset(string name) => EditorUtil.LoadOrCreate<PickupData>($"{Folder}/Pickups/{name}.asset");

        /// <summary>Step 2: fill the assets in, now that the prefabs they refer to exist.</summary>
        public static void Fill(GameAssets a, Dictionary<SfxId, AudioClip> clips)
        {
            FillWeapons(a);
            FillEnemies(a);
            FillPickups(a);
            FillSfx(a, clips);

            foreach (Object asset in new Object[]
            {
                a.Blaster, a.Spread, a.Vulcan, a.Rail, a.Missile, a.Plasma,
                a.Drone, a.Dart, a.DartWing, a.Gunner, a.Kamikaze, a.AsteroidLarge, a.AsteroidSmall,
                a.Bomber, a.Spinner, a.Striker, a.SupplyShip, a.Boss,
                a.Health, a.Armor, a.Shield, a.Power, a.Bomb, a.Overdrive,
                a.SpreadCrate, a.VulcanCrate, a.RailCrate, a.MissileCrate, a.PlasmaCrate,
                a.DropTable, a.SfxLibrary
            })
            {
                EditorUtility.SetDirty(asset);
            }
        }

        // ---------------------------------------------------------------- weapons

        static void FillWeapons(GameAssets a)
        {
            // The starting gun: never runs dry, and gains a whole extra barrel per power level.
            WeaponData w = a.Blaster;
            w.displayName = "Blaster";
            w.icon = SpriteFactory.Get("IconBlaster");
            w.color = SpriteFactory.BlasterColor;
            w.projectilePrefab = a.ShotBlaster;
            w.fireRate = 9f; w.damage = 7f; w.projectileSpeed = 30f; w.projectileLifetime = 1.2f;
            w.projectilesPerShot = 2; w.angleStep = 2f; w.lateralSpacing = 0.36f; w.randomSpread = 0f;
            w.infiniteAmmo = true; w.ammoPerPickup = 0; w.maxAmmo = 0;
            w.projectilesPerLevel = 1f; w.damagePerLevel = 0.08f; w.fireRatePerLevel = 0.04f;
            w.fireSfx = SfxId.Blaster; w.fireShake = 0f;

            // A wide fan of pellets: great against swarms, weak at range.
            w = a.Spread;
            w.displayName = "Spread Shot";
            w.icon = SpriteFactory.Get("IconSpread");
            w.color = SpriteFactory.SpreadColor;
            w.projectilePrefab = a.ShotSpread;
            w.fireRate = 3.2f; w.damage = 9f; w.projectileSpeed = 24f; w.projectileLifetime = 0.9f;
            w.projectilesPerShot = 5; w.angleStep = 9f; w.lateralSpacing = 0f; w.randomSpread = 0f;
            w.infiniteAmmo = false; w.ammoPerPickup = 70; w.maxAmmo = 210;
            w.projectilesPerLevel = 1f; w.damagePerLevel = 0.1f; w.fireRatePerLevel = 0.04f;
            w.fireSfx = SfxId.Spread; w.fireShake = 0.03f;

            // A stream of fast, light rounds.
            w = a.Vulcan;
            w.displayName = "Vulcan";
            w.icon = SpriteFactory.Get("IconVulcan");
            w.color = SpriteFactory.VulcanColor;
            w.projectilePrefab = a.ShotVulcan;
            w.fireRate = 18f; w.damage = 7f; w.projectileSpeed = 38f; w.projectileLifetime = 1f;
            w.projectilesPerShot = 1; w.angleStep = 3f; w.lateralSpacing = 0.22f; w.randomSpread = 3.5f;
            w.infiniteAmmo = false; w.ammoPerPickup = 350; w.maxAmmo = 900;
            w.projectilesPerLevel = 0.5f; w.damagePerLevel = 0.1f; w.fireRatePerLevel = 0.06f;
            w.fireSfx = SfxId.Vulcan; w.fireShake = 0f;

            // Slow but devastating, and it passes straight through everything in line.
            w = a.Rail;
            w.displayName = "Railgun";
            w.icon = SpriteFactory.Get("IconRail");
            w.color = SpriteFactory.RailColor;
            w.projectilePrefab = a.ShotRail;
            w.fireRate = 1.7f; w.damage = 70f; w.projectileSpeed = 70f; w.projectileLifetime = 0.8f;
            w.projectilesPerShot = 1; w.angleStep = 0f; w.lateralSpacing = 0.5f; w.randomSpread = 0f;
            w.infiniteAmmo = false; w.ammoPerPickup = 24; w.maxAmmo = 72;
            w.projectilesPerLevel = 0.5f; w.damagePerLevel = 0.2f; w.fireRatePerLevel = 0.05f;
            w.fireSfx = SfxId.Rail; w.fireShake = 0.18f;

            // Homing missiles that find their own targets and explode on impact.
            w = a.Missile;
            w.displayName = "Missiles";
            w.icon = SpriteFactory.Get("IconMissile");
            w.color = SpriteFactory.MissileColor;
            w.projectilePrefab = a.ShotMissile;
            w.fireRate = 2.4f; w.damage = 26f; w.projectileSpeed = 9f; w.projectileLifetime = 3f;
            w.projectilesPerShot = 2; w.angleStep = 24f; w.lateralSpacing = 0.5f; w.randomSpread = 0f;
            w.infiniteAmmo = false; w.ammoPerPickup = 44; w.maxAmmo = 132;
            w.projectilesPerLevel = 0.5f; w.damagePerLevel = 0.12f; w.fireRatePerLevel = 0.05f;
            w.fireSfx = SfxId.Missile; w.fireShake = 0.02f;

            // A slow ball of plasma with a big blast radius.
            w = a.Plasma;
            w.displayName = "Plasma Cannon";
            w.icon = SpriteFactory.Get("IconPlasma");
            w.color = SpriteFactory.PlasmaColor;
            w.projectilePrefab = a.ShotPlasma;
            w.fireRate = 1.3f; w.damage = 55f; w.projectileSpeed = 15f; w.projectileLifetime = 2.5f;
            w.projectilesPerShot = 1; w.angleStep = 16f; w.lateralSpacing = 0f; w.randomSpread = 0f;
            w.infiniteAmmo = false; w.ammoPerPickup = 18; w.maxAmmo = 54;
            w.projectilesPerLevel = 0.5f; w.damagePerLevel = 0.15f; w.fireRatePerLevel = 0.05f;
            w.fireSfx = SfxId.Plasma; w.fireShake = 0.08f;
        }

        // ---------------------------------------------------------------- enemies

        static void FillEnemies(GameAssets a)
        {
            //            asset            name               prefab                 hp     speed  score  ram    dies   drop
            Stats(a.Drone, "Drone", a.EnemyDrone, 12f, 7.5f, 50, 15f, true, 0.05f);
            Stats(a.Dart, "Dart", a.EnemyDart, 16f, 6.5f, 60, 18f, true, 0.05f);
            Stats(a.DartWing, "Dart Wing", a.EnemyDart, 16f, 6.5f, 60, 18f, true, 0.05f);
            Stats(a.Gunner, "Gunner", a.EnemyGunner, 45f, 6f, 150, 25f, true, 0.12f);
            Stats(a.Kamikaze, "Kamikaze", a.EnemyKamikaze, 14f, 9f, 80, 28f, true, 0.05f);
            Stats(a.AsteroidLarge, "Asteroid", a.EnemyAsteroidLarge, 45f, 3.2f, 80, 30f, true, 0.06f);
            Stats(a.AsteroidSmall, "Asteroid Chunk", a.EnemyAsteroidSmall, 14f, 4.6f, 30, 15f, true, 0.02f);
            Stats(a.Bomber, "Bomber", a.EnemyBomber, 170f, 1.7f, 400, 40f, false, 0.5f);
            Stats(a.Spinner, "Spinner", a.EnemySpinner, 120f, 5f, 350, 30f, false, 0.4f);
            Stats(a.Striker, "Striker", a.EnemyStriker, 24f, 8.5f, 100, 20f, true, 0.07f);
            Stats(a.SupplyShip, "Supply Ship", a.EnemySupplyShip, 60f, 6f, 300, 0f, false, 0f);
            Stats(a.Boss, "Dreadnought", a.EnemyBoss, 4500f, 2.5f, 5000, 45f, false, 0f);

            //         asset            first wave  cost   weight  pattern                   group size              interval  spacing
            Waves(a.Drone, 1, 1f, 1.4f, SpawnPattern.Stream, new Vector2Int(6, 10), 0.3f, 1.8f);
            Waves(a.Dart, 1, 1f, 1.2f, SpawnPattern.Scatter, new Vector2Int(5, 9), 0.3f, 1.8f);
            Waves(a.DartWing, 3, 1f, 0.7f, SpawnPattern.VFormation, new Vector2Int(5, 9), 0.3f, 1.5f);
            Waves(a.Gunner, 2, 3f, 1f, SpawnPattern.Line, new Vector2Int(2, 4), 0.3f, 3.2f);
            Waves(a.Kamikaze, 3, 1.5f, 1f, SpawnPattern.Scatter, new Vector2Int(4, 8), 0.28f, 1.8f);
            Waves(a.AsteroidLarge, 2, 2f, 0.7f, SpawnPattern.Scatter, new Vector2Int(3, 6), 0.5f, 2.5f);
            Waves(a.AsteroidSmall, 1, 1f, 0f, SpawnPattern.Scatter, new Vector2Int(1, 1), 0.3f, 1.8f);
            Waves(a.Bomber, 4, 6f, 0.6f, SpawnPattern.Line, new Vector2Int(1, 2), 0.3f, 7f);
            Waves(a.Spinner, 6, 6f, 0.6f, SpawnPattern.Scatter, new Vector2Int(1, 2), 1.5f, 5f);
            Waves(a.Striker, 4, 1.5f, 0.9f, SpawnPattern.Sides, new Vector2Int(4, 8), 0.4f, 1.8f);
            Waves(a.SupplyShip, 1, 1f, 0f, SpawnPattern.Sides, new Vector2Int(1, 1), 0.3f, 1.8f);
            Waves(a.Boss, 1, 1f, 0f, SpawnPattern.Line, new Vector2Int(1, 1), 0.3f, 1.8f);

            Death(a.Drone, a.FxExplosionSmall, SfxId.ExplosionSmall, 0.06f);
            Death(a.Dart, a.FxExplosionSmall, SfxId.ExplosionSmall, 0.06f);
            Death(a.DartWing, a.FxExplosionSmall, SfxId.ExplosionSmall, 0.06f);
            Death(a.Gunner, a.FxExplosionMedium, SfxId.ExplosionMedium, 0.15f);
            Death(a.Kamikaze, a.FxExplosionSmall, SfxId.ExplosionSmall, 0.1f);
            Death(a.AsteroidLarge, a.FxExplosionMedium, SfxId.ExplosionMedium, 0.15f);
            Death(a.AsteroidSmall, a.FxExplosionSmall, SfxId.ExplosionSmall, 0.04f);
            Death(a.Bomber, a.FxExplosionLarge, SfxId.ExplosionLarge, 0.4f);
            Death(a.Spinner, a.FxExplosionLarge, SfxId.ExplosionLarge, 0.35f);
            Death(a.Striker, a.FxExplosionSmall, SfxId.ExplosionSmall, 0.08f);
            Death(a.SupplyShip, a.FxExplosionMedium, SfxId.ExplosionMedium, 0.2f);
            Death(a.Boss, a.FxExplosionBoss, SfxId.ExplosionLarge, 1f);

            // Special cases.
            a.SupplyShip.guaranteedDrops = 2;
            a.SupplyShip.blocksWaveClear = false;
            a.Boss.guaranteedDrops = 4;
            a.Bomber.guaranteedDrops = 0;
        }

        static void Stats(EnemyData data, string name, Enemy prefab, float health, float speed, int score,
            float contactDamage, bool dieOnContact, float dropChance)
        {
            data.displayName = name;
            data.prefab = prefab;
            data.maxHealth = health;
            data.moveSpeed = speed;
            data.scoreValue = score;
            data.contactDamage = contactDamage;
            data.dieOnContact = dieOnContact;
            data.dropChance = dropChance;
            data.guaranteedDrops = 0;
            data.blocksWaveClear = true;
        }

        static void Waves(EnemyData data, int firstWave, float cost, float weight, SpawnPattern pattern,
            Vector2Int groupSize, float interval, float spacing)
        {
            data.firstWave = firstWave;
            data.threatCost = cost;
            data.spawnWeight = weight;
            data.pattern = pattern;
            data.groupSize = groupSize;
            data.spawnInterval = interval;
            data.spacing = spacing;
        }

        static void Death(EnemyData data, GameObject effect, SfxId sfx, float shake)
        {
            data.deathEffect = effect;
            data.deathSfx = sfx;
            data.deathShake = shake;
        }

        // ---------------------------------------------------------------- pickups

        static void FillPickups(GameAssets a)
        {
            Fill(a.Health, "+30 HULL", PickupType.Health, 30f, 0f, null, "PickupHealth", SpriteFactory.Hex(0x4ADE80), SfxId.Heal);
            Fill(a.Armor, "+50 ARMOR", PickupType.Armor, 50f, 0f, null, "PickupArmor", SpriteFactory.Hex(0xCBD5E1), SfxId.Pickup);
            Fill(a.Shield, "SHIELD UP", PickupType.Shield, 0f, 8f, null, "PickupShield", SpriteFactory.Hex(0x7DD3FC), SfxId.Pickup);
            Fill(a.Power, "POWER UP", PickupType.WeaponPower, 1f, 0f, null, "PickupPower", SpriteFactory.Hex(0xFDE047), SfxId.PowerUp);
            Fill(a.Bomb, "+1 BOMB", PickupType.Bomb, 1f, 0f, null, "PickupBomb", SpriteFactory.Hex(0xFB923C), SfxId.Pickup);
            Fill(a.Overdrive, "OVERDRIVE", PickupType.Overdrive, 0f, 9f, null, "PickupOverdrive", SpriteFactory.Hex(0xFB7185), SfxId.PowerUp);

            Fill(a.SpreadCrate, "SPREAD SHOT", PickupType.Weapon, 0f, 0f, a.Spread, "PickupSpread", SpriteFactory.SpreadColor, SfxId.Pickup);
            Fill(a.VulcanCrate, "VULCAN", PickupType.Weapon, 0f, 0f, a.Vulcan, "PickupVulcan", SpriteFactory.VulcanColor, SfxId.Pickup);
            Fill(a.RailCrate, "RAILGUN", PickupType.Weapon, 0f, 0f, a.Rail, "PickupRail", SpriteFactory.RailColor, SfxId.Pickup);
            Fill(a.MissileCrate, "MISSILES", PickupType.Weapon, 0f, 0f, a.Missile, "PickupMissile", SpriteFactory.MissileColor, SfxId.Pickup);
            Fill(a.PlasmaCrate, "PLASMA CANNON", PickupType.Weapon, 0f, 0f, a.Plasma, "PickupPlasma", SpriteFactory.PlasmaColor, SfxId.Pickup);

            a.DropTable.entries = new[]
            {
                Drop(a.Health, 10f),
                Drop(a.Armor, 8f),
                Drop(a.Power, 7f),
                Drop(a.Shield, 5f),
                Drop(a.Bomb, 4f),
                Drop(a.Overdrive, 4f),
                Drop(a.SpreadCrate, 5f),
                Drop(a.VulcanCrate, 5f),
                Drop(a.MissileCrate, 4f),
                Drop(a.RailCrate, 3.5f),
                Drop(a.PlasmaCrate, 3f)
            };
        }

        static void Fill(PickupData data, string label, PickupType type, float amount, float duration, WeaponData weapon,
            string sprite, Color color, SfxId sfx)
        {
            data.label = label;
            data.type = type;
            data.amount = amount;
            data.duration = duration;
            data.weapon = weapon;
            data.sprite = SpriteFactory.Get(sprite);
            data.color = color;
            data.sfx = sfx;
        }

        static DropTable.Entry Drop(PickupData pickup, float weight)
        {
            return new DropTable.Entry { pickup = pickup, weight = weight };
        }

        // ---------------------------------------------------------------- sound

        static void FillSfx(GameAssets a, Dictionary<SfxId, AudioClip> clips)
        {
            var entries = new List<SfxLibrary.Entry>();

            void Add(SfxId id, float volume, float pitchVariance, float minInterval)
            {
                clips.TryGetValue(id, out AudioClip clip);
                entries.Add(new SfxLibrary.Entry
                {
                    id = id,
                    clip = clip,
                    volume = volume,
                    pitchMin = 1f - pitchVariance,
                    pitchMax = 1f + pitchVariance,
                    minInterval = minInterval
                });
            }

            //  sound                  volume  pitch +/-  shortest gap between plays
            Add(SfxId.Blaster, 0.30f, 0.06f, 0.05f);
            Add(SfxId.Spread, 0.40f, 0.06f, 0.05f);
            Add(SfxId.Vulcan, 0.26f, 0.08f, 0.04f);
            Add(SfxId.Rail, 0.60f, 0.04f, 0.05f);
            Add(SfxId.Missile, 0.40f, 0.08f, 0.06f);
            Add(SfxId.Plasma, 0.55f, 0.05f, 0.05f);

            Add(SfxId.EnemyShoot, 0.22f, 0.10f, 0.07f);
            Add(SfxId.Hit, 0.22f, 0.15f, 0.045f);
            Add(SfxId.ExplosionSmall, 0.45f, 0.15f, 0.05f);
            Add(SfxId.ExplosionMedium, 0.60f, 0.12f, 0.06f);
            Add(SfxId.ExplosionLarge, 0.85f, 0.08f, 0.08f);

            Add(SfxId.PlayerHurt, 0.75f, 0.05f, 0.1f);
            Add(SfxId.PlayerDeath, 1.00f, 0.02f, 0.2f);
            Add(SfxId.ShieldUp, 0.60f, 0.02f, 0.1f);
            Add(SfxId.ShieldBlock, 0.50f, 0.10f, 0.06f);
            Add(SfxId.ShieldDown, 0.55f, 0.02f, 0.1f);
            Add(SfxId.Dash, 0.50f, 0.08f, 0.05f);
            Add(SfxId.Bomb, 1.00f, 0.02f, 0.2f);
            Add(SfxId.WeaponSwitch, 0.40f, 0.03f, 0.03f);

            Add(SfxId.Pickup, 0.55f, 0.03f, 0.04f);
            Add(SfxId.PowerUp, 0.55f, 0.02f, 0.05f);
            Add(SfxId.Heal, 0.55f, 0.03f, 0.05f);

            Add(SfxId.WaveStart, 0.50f, 0f, 0.2f);
            Add(SfxId.BossWarning, 0.60f, 0f, 0.5f);
            Add(SfxId.GameOver, 0.70f, 0f, 0.5f);
            Add(SfxId.Confirm, 0.50f, 0f, 0.05f);

            a.SfxLibrary.entries = entries.ToArray();
        }
    }
}
