using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace SpaceShooter.EditorTools
{
    /// <summary>
    /// Assembles the prefabs: effects, projectiles, enemies, the pickup and the player's ship.
    /// Each prefab is built as a temporary scene object, saved to Assets/Prefabs and removed again.
    /// </summary>
    public static class PrefabFactory
    {
        public const string Folder = "Assets/Prefabs";
        public const string MaterialFolder = "Assets/Art/Materials";

        const string UrpMaterials = "Packages/com.unity.render-pipelines.universal/Runtime/Materials/";

        // Draw order within the default sorting layer (higher = in front).
        public const int OrderPickup = 5;
        public const int OrderEnemy = 10;
        public const int OrderPlayerShot = 15;
        public const int OrderPlayer = 20;
        public const int OrderEffect = 30;
        public const int OrderEnemyShot = 40;

        static int playerLayer;
        static int enemyLayer;

        public static void BuildAll(GameAssets a)
        {
            playerLayer = LayerMask.NameToLayer(GameLayers.PlayerName);
            enemyLayer = LayerMask.NameToLayer(GameLayers.EnemyName);

            EditorUtil.EnsureFolder(MaterialFolder);
            EditorUtil.EnsureFolder(Folder + "/Effects");
            EditorUtil.EnsureFolder(Folder + "/Projectiles");
            EditorUtil.EnsureFolder(Folder + "/Enemies");

            BuildMaterials(a);
            BuildEffects(a);
            BuildProjectiles(a);
            BuildEnemies(a);
            BuildPickup(a);
            BuildPlayer(a);
        }

        // ---------------------------------------------------------------- materials

        static void BuildMaterials(GameAssets a)
        {
            // Sprites use URP's unlit sprite material, so they look the same with or without 2D lights.
            a.SpriteMaterial = AssetDatabase.LoadAssetAtPath<Material>(UrpMaterials + "Sprite-Unlit-Default.mat");
            if (a.SpriteMaterial == null) Debug.LogError("[Space Shooter setup] Could not find URP's Sprite-Unlit-Default material.");

            a.ParticleMaterial = ParticleMaterial("ParticleGlow", SpriteFactory.Get("FxSoftCircle").texture);
            a.RingMaterial = ParticleMaterial("ParticleRing", SpriteFactory.Get("FxRingThin").texture);
        }

        /// <summary>An additive (glowing) particle material using the given texture.</summary>
        static Material ParticleMaterial(string name, Texture texture)
        {
            string path = $"{MaterialFolder}/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (material == null)
            {
                // Start from URP's own particle material, which is already set up as transparent.
                var template = AssetDatabase.LoadAssetAtPath<Material>(UrpMaterials + "ParticlesUnlit.mat");
                if (template != null)
                {
                    material = new Material(template);
                }
                else
                {
                    material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
                    material.SetFloat("_Surface", 1f);
                    material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                    material.SetOverrideTag("RenderType", "Transparent");
                }
                AssetDatabase.CreateAsset(material, path);
            }

            material.SetTexture("_BaseMap", texture);
            material.SetTexture("_MainTex", texture);
            material.SetColor("_BaseColor", Color.white);

            // Additive blending: overlapping particles get brighter, like light.
            material.SetFloat("_Blend", 2f);
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)BlendMode.One);
            material.SetFloat("_SrcBlendAlpha", (float)BlendMode.One);
            material.SetFloat("_DstBlendAlpha", (float)BlendMode.One);
            material.SetFloat("_ZWrite", 0f);
            material.renderQueue = (int)RenderQueue.Transparent;

            EditorUtility.SetDirty(material);
            return material;
        }

        // ---------------------------------------------------------------- shared helpers

        static GameObject Child(GameObject parent, string name, Vector3 localPosition = default)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent.transform, false);
            child.transform.localPosition = localPosition;
            child.layer = parent.layer;
            return child;
        }

        static SpriteRenderer AddSprite(GameAssets a, GameObject target, string spriteName, int order)
        {
            var renderer = target.AddComponent<SpriteRenderer>();
            renderer.sprite = SpriteFactory.Get(spriteName);
            renderer.sharedMaterial = a.SpriteMaterial;
            renderer.sortingOrder = order;
            return renderer;
        }

        // ---------------------------------------------------------------- effects

        static void BuildEffects(GameAssets a)
        {
            Color white = Color.white;
            Color yellow = SpriteFactory.Hex(0xFFE08A), orange = SpriteFactory.Hex(0xFF8A2A), red = SpriteFactory.Hex(0xC2281E);
            Color cyan = SpriteFactory.Hex(0x7DE3FF), blue = SpriteFactory.Hex(0x2A6BFF), navy = SpriteFactory.Hex(0x1A2A8C);
            Color mint = SpriteFactory.Hex(0xB8FFE0), green = SpriteFactory.Hex(0x34D399), forest = SpriteFactory.Hex(0x0F7A55);
            Color pink = SpriteFactory.Hex(0xFFC2D0), crimson = SpriteFactory.Hex(0xFF3B5C);

            a.FxHitSpark = Explosion(a, "FxHitSpark", 0.28f, white, yellow, orange, 3, 4, false, 0.22f);
            a.FxHitSparkEnemy = Explosion(a, "FxHitSparkEnemy", 0.3f, white, pink, crimson, 3, 3, false, 0.22f);
            a.FxExplosionSmall = Explosion(a, "FxExplosionSmall", 0.75f, yellow, orange, red, 9, 6, true, 0.45f);
            a.FxExplosionMedium = Explosion(a, "FxExplosionMedium", 1.15f, yellow, orange, red, 14, 10, true, 0.6f);
            a.FxExplosionLarge = Explosion(a, "FxExplosionLarge", 1.9f, yellow, orange, red, 22, 18, true, 0.85f);
            a.FxExplosionBoss = Explosion(a, "FxExplosionBoss", 4.2f, yellow, orange, red, 46, 40, true, 1.5f);
            a.FxExplosionPlasma = Explosion(a, "FxExplosionPlasma", 1.5f, mint, green, forest, 14, 8, true, 0.55f);
            a.FxPlayerHit = Explosion(a, "FxPlayerHit", 0.7f, white, cyan, blue, 7, 8, false, 0.35f);
            a.FxPlayerDeath = Explosion(a, "FxPlayerDeath", 2.4f, cyan, blue, navy, 30, 26, true, 1.1f);
            a.FxDash = Explosion(a, "FxDash", 0.6f, white, cyan, blue, 6, 0, true, 0.3f);

            // The bomb's shockwave is a sprite, so it can sweep cleanly across the whole screen.
            var blast = new GameObject("FxBombBlast");
            SpriteRenderer ring = AddSprite(a, blast, "FxRing", OrderEffect + 2);
            var shockwave = blast.AddComponent<Shockwave>();
            EditorUtil.SetMany(shockwave, "ring", ring, "duration", 0.75f, "startScale", 0.6f, "endScale", 30f, "color", new Color(0.75f, 0.95f, 1f, 1f));
            a.FxBombBlast = EditorUtil.SavePrefab(blast, Folder + "/Effects/FxBombBlast.prefab");
        }

        /// <summary>
        /// A burst made of up to four layers: glowing fireball blobs, fast streaking sparks,
        /// an expanding ring and a brief white flash.
        /// </summary>
        static GameObject Explosion(GameAssets a, string name, float size, Color hot, Color mid, Color cold,
            int blobs, int sparks, bool ring, float life)
        {
            var root = new GameObject(name);

            ParticleSystem fire = NewSystem(root, a.ParticleMaterial, OrderEffect, life);
            ParticleSystem.MainModule main = fire.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(life * 0.45f, life);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.6f * size, 4.5f * size);
            main.startSize = new ParticleSystem.MinMaxCurve(0.5f * size, 1.25f * size);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            Burst(fire, blobs);
            EmitFromCircle(fire, 0.12f * size);
            Drag(fire, 3.5f);
            Fade(fire, new[] { Color.white, hot, mid, cold }, new[] { 0f, 0.15f, 0.5f, 1f }, 1f);
            Resize(fire, 0.45f, 1f, 0.05f);

            if (sparks > 0)
            {
                ParticleSystem spark = NewSystem(Child(root, "Sparks"), a.ParticleMaterial, OrderEffect + 1, life);
                main = spark.main;
                main.startLifetime = new ParticleSystem.MinMaxCurve(life * 0.35f, life * 0.9f);
                main.startSpeed = new ParticleSystem.MinMaxCurve(5f * size, 13f * size);
                main.startSize = new ParticleSystem.MinMaxCurve(0.10f + 0.05f * size, 0.16f + 0.09f * size);
                Burst(spark, sparks);
                EmitFromCircle(spark, 0.1f * size);
                Drag(spark, 2.5f);
                Fade(spark, new[] { Color.white, hot, mid }, new[] { 0f, 0.3f, 1f }, 1f);

                // Stretch each spark along its direction of travel so it reads as a streak.
                var renderer = spark.GetComponent<ParticleSystemRenderer>();
                renderer.renderMode = ParticleSystemRenderMode.Stretch;
                renderer.velocityScale = 0.05f;
                renderer.lengthScale = 1.6f;
            }

            if (ring)
            {
                ParticleSystem wave = NewSystem(Child(root, "Ring"), a.RingMaterial, OrderEffect - 1, life);
                main = wave.main;
                main.startLifetime = Mathf.Min(0.45f, life * 0.7f);
                main.startSpeed = 0f;
                main.startSize = 4.2f * size;
                Burst(wave, 1);
                EmitFromPoint(wave);
                Fade(wave, new[] { hot, mid }, new[] { 0f, 1f }, 0.55f);
                Resize(wave, 0.12f, 0.75f, 1f);
            }

            ParticleSystem flash = NewSystem(Child(root, "Flash"), a.ParticleMaterial, OrderEffect + 2, life);
            main = flash.main;
            main.startLifetime = 0.12f;
            main.startSpeed = 0f;
            main.startSize = 2.6f * size;
            Burst(flash, 1);
            EmitFromPoint(flash);
            Fade(flash, new[] { Color.white, hot }, new[] { 0f, 1f }, 0.45f);

            var despawn = root.AddComponent<AutoDespawn>();
            EditorUtil.Set(despawn, "lifetime", life + 0.3f);

            return EditorUtil.SavePrefab(root, $"{Folder}/Effects/{name}.prefab");
        }

        static ParticleSystem NewSystem(GameObject target, Material material, int order, float duration)
        {
            var system = target.AddComponent<ParticleSystem>();

            ParticleSystem.MainModule main = system.main;
            main.duration = Mathf.Max(0.1f, duration);
            main.loop = false;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startColor = Color.white;
            main.gravityModifier = 0f;
            main.maxParticles = 128;

            ParticleSystem.EmissionModule emission = system.emission;
            emission.rateOverTime = 0f;

            var renderer = target.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = material;
            renderer.sortingOrder = order;
            return system;
        }

        static void Burst(ParticleSystem system, int count)
        {
            ParticleSystem.EmissionModule emission = system.emission;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)count) });
        }

        static void EmitFromCircle(ParticleSystem system, float radius)
        {
            ParticleSystem.ShapeModule shape = system.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = Mathf.Max(0.01f, radius);
        }

        static void EmitFromPoint(ParticleSystem system)
        {
            ParticleSystem.ShapeModule shape = system.shape;
            shape.enabled = false;
        }

        /// <summary>Makes particles slow down as they fly, so a burst blooms outwards then hangs.</summary>
        static void Drag(ParticleSystem system, float amount)
        {
            ParticleSystem.LimitVelocityOverLifetimeModule limit = system.limitVelocityOverLifetime;
            limit.enabled = true;
            limit.limit = 1000f;
            limit.dampen = 0f;
            limit.drag = amount;
            limit.multiplyDragByParticleSize = false;
            limit.multiplyDragByParticleVelocity = false;
        }

        static void Fade(ParticleSystem system, Color[] colors, float[] times, float startAlpha)
        {
            var colorKeys = new GradientColorKey[colors.Length];
            for (int i = 0; i < colors.Length; i++) colorKeys[i] = new GradientColorKey(colors[i], times[i]);

            var gradient = new Gradient();
            gradient.SetKeys(colorKeys, new[]
            {
                new GradientAlphaKey(startAlpha, 0f),
                new GradientAlphaKey(startAlpha * 0.8f, 0.45f),
                new GradientAlphaKey(0f, 1f)
            });

            ParticleSystem.ColorOverLifetimeModule color = system.colorOverLifetime;
            color.enabled = true;
            color.color = new ParticleSystem.MinMaxGradient(gradient);
        }

        static void Resize(ParticleSystem system, float start, float middle, float end)
        {
            var curve = new AnimationCurve(new Keyframe(0f, start), new Keyframe(0.25f, middle), new Keyframe(1f, end));

            ParticleSystem.SizeOverLifetimeModule size = system.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, curve);
        }

        // ---------------------------------------------------------------- projectiles

        static void BuildProjectiles(GameAssets a)
        {
            a.ShotBlaster = Shot(a, "ShotBlaster", 0.14f, OrderPlayerShot, a.FxHitSpark);
            a.ShotSpread = Shot(a, "ShotSpread", 0.14f, OrderPlayerShot, a.FxHitSpark);
            a.ShotVulcan = Shot(a, "ShotVulcan", 0.10f, OrderPlayerShot, a.FxHitSpark);

            a.ShotRail = Shot(a, "ShotRail", 0.16f, OrderPlayerShot, a.FxHitSpark, go =>
            {
                // Punches through everything in its path.
                EditorUtil.Set(go.GetComponent<Projectile>(), "pierceCount", 99);
            });

            a.ShotMissile = Shot(a, "ShotMissile", 0.2f, OrderPlayerShot, a.FxExplosionSmall, go =>
            {
                EditorUtil.SetMany(go.GetComponent<Projectile>(),
                    "explosionRadius", 1.7f, "splashDamage", 0.6f, "hitSfx", SfxId.ExplosionSmall, "hitShake", 0.08f);
                go.AddComponent<HomingProjectile>();

                var trail = go.AddComponent<TrailRenderer>();
                trail.sharedMaterial = a.ParticleMaterial;
                trail.time = 0.28f;
                trail.startWidth = 0.3f;
                trail.endWidth = 0f;
                trail.startColor = new Color(1f, 0.75f, 0.3f, 0.9f);
                trail.endColor = new Color(1f, 0.3f, 0.1f, 0f);
                trail.sortingOrder = OrderPlayerShot - 1;
                trail.minVertexDistance = 0.1f;
            });

            a.ShotPlasma = Shot(a, "ShotPlasma", 0.38f, OrderPlayerShot, a.FxExplosionPlasma, go =>
            {
                EditorUtil.SetMany(go.GetComponent<Projectile>(),
                    "alignToVelocity", false, "explosionRadius", 2.5f, "splashDamage", 0.7f,
                    "hitSfx", SfxId.ExplosionMedium, "hitShake", 0.15f);
            });

            a.EnemyShotOrb = Shot(a, "EnemyShotOrb", 0.14f, OrderEnemyShot, a.FxHitSparkEnemy, go =>
            {
                EditorUtil.Set(go.GetComponent<Projectile>(), "alignToVelocity", false);
            });
            a.EnemyShotBolt = Shot(a, "EnemyShotBolt", 0.12f, OrderEnemyShot, a.FxHitSparkEnemy);
            a.EnemyShotHeavy = Shot(a, "EnemyShotHeavy", 0.26f, OrderEnemyShot, a.FxHitSparkEnemy, go =>
            {
                EditorUtil.Set(go.GetComponent<Projectile>(), "alignToVelocity", false);
            });
        }

        static Projectile Shot(GameAssets a, string name, float radius, int order, GameObject hitEffect,
            System.Action<GameObject> customise = null)
        {
            var go = new GameObject(name);
            AddSprite(a, go, name, order);

            var projectile = go.AddComponent<Projectile>();
            EditorUtil.SetMany(projectile, "radius", radius, "hitEffect", hitEffect);
            customise?.Invoke(go);

            return EditorUtil.SavePrefab(go, $"{Folder}/Projectiles/{name}.prefab").GetComponent<Projectile>();
        }

        // ---------------------------------------------------------------- enemies

        static void BuildEnemies(GameAssets a)
        {
            a.EnemyDrone = EnemyPrefab(a, "EnemyDrone", "EnemyDrone", 0.42f, 3f, (go, visual) =>
            {
                go.AddComponent<PathMovement>();
            });

            a.EnemyDart = EnemyPrefab(a, "EnemyDart", "EnemyDart", 0.36f, 3f, (go, visual) =>
            {
                var move = go.AddComponent<StraightMovement>();
                EditorUtil.SetMany(move, "swayAmplitude", 1.6f, "swayFrequency", 2.4f, "speedVariance", 0.1f);

                var shooter = go.AddComponent<EnemyShooter>();
                EditorUtil.SetMany(shooter, "bulletPrefab", a.EnemyShotBolt, "pattern", FirePattern.Forward,
                    "muzzle", Child(go, "Muzzle", new Vector3(0f, -0.55f)).transform,
                    "interval", 3.4f, "intervalJitter", 1.4f, "firstShotDelay", 1.2f, "bulletSpeed", 9f, "damage", 8f);
            });

            a.EnemyGunner = EnemyPrefab(a, "EnemyGunner", "EnemyGunner", 0.55f, 3f, (go, visual) =>
            {
                go.AddComponent<HoverMovement>();

                var shooter = go.AddComponent<EnemyShooter>();
                EditorUtil.SetMany(shooter, "bulletPrefab", a.EnemyShotOrb, "pattern", FirePattern.Aimed,
                    "muzzle", Child(go, "Muzzle", new Vector3(0f, -0.55f)).transform,
                    "interval", 2.6f, "intervalJitter", 0.6f, "firstShotDelay", 1.4f,
                    "burstCount", 3, "burstSpacing", 0.16f, "bulletSpeed", 7.5f, "damage", 10f, "aimError", 4f);
            });

            a.EnemyKamikaze = EnemyPrefab(a, "EnemyKamikaze", "EnemyKamikaze", 0.36f, 10f, (go, visual) =>
            {
                go.AddComponent<ChaseMovement>();
            });

            a.EnemyAsteroidLarge = EnemyPrefab(a, "EnemyAsteroidLarge", "AsteroidLarge", 0.78f, 3f, (go, visual) =>
            {
                var move = go.AddComponent<StraightMovement>();
                EditorUtil.Set(move, "speedVariance", 0.25f);
                Tumble(visual, 70f);

                var split = go.AddComponent<EnemySplitOnDeath>();
                EditorUtil.SetMany(split, "childData", a.AsteroidSmall, "count", 3);
            });

            a.EnemyAsteroidSmall = EnemyPrefab(a, "EnemyAsteroidSmall", "AsteroidSmall", 0.38f, 3f, (go, visual) =>
            {
                var move = go.AddComponent<StraightMovement>();
                EditorUtil.Set(move, "speedVariance", 0.3f);
                Tumble(visual, 140f);
            });

            a.EnemyBomber = EnemyPrefab(a, "EnemyBomber", "EnemyBomber", 0.95f, 4f, (go, visual) =>
            {
                var move = go.AddComponent<StraightMovement>();
                EditorUtil.SetMany(move, "swayAmplitude", 0.9f, "swayFrequency", 0.8f);

                var shooter = go.AddComponent<EnemyShooter>();
                EditorUtil.SetMany(shooter, "bulletPrefab", a.EnemyShotOrb, "pattern", FirePattern.Aimed,
                    "muzzle", Child(go, "Muzzle", new Vector3(0f, -0.9f)).transform,
                    "interval", 3f, "intervalJitter", 0.5f, "firstShotDelay", 1.5f,
                    "burstCount", 2, "burstSpacing", 0.3f, "bulletsPerShot", 5, "angleStep", 13f,
                    "bulletSpeed", 6.5f, "damage", 12f);
            });

            a.EnemySpinner = EnemyPrefab(a, "EnemySpinner", "EnemySpinner", 0.78f, 3f, (go, visual) =>
            {
                var move = go.AddComponent<HoverMovement>();
                EditorUtil.SetMany(move, "stayDuration", 22f, "wanderRange", new Vector2(5f, 1.5f));

                var spin = visual.AddComponent<Spin>();
                EditorUtil.Set(spin, "speed", 50f);

                var shooter = go.AddComponent<EnemyShooter>();
                EditorUtil.SetMany(shooter, "bulletPrefab", a.EnemyShotOrb, "pattern", FirePattern.Ring,
                    "interval", 2.8f, "intervalJitter", 0.4f, "firstShotDelay", 1.6f,
                    "burstCount", 4, "burstSpacing", 0.22f, "bulletsPerShot", 10, "spinStep", 13f,
                    "bulletSpeed", 5.5f, "damage", 10f);
            });

            a.EnemyStriker = EnemyPrefab(a, "EnemyStriker", "EnemyStriker", 0.42f, 3f, (go, visual) =>
            {
                var move = go.AddComponent<StraightMovement>();
                EditorUtil.SetMany(move, "swayAmplitude", 0.7f, "swayFrequency", 2f, "faceTravelDirection", true);

                var shooter = go.AddComponent<EnemyShooter>();
                EditorUtil.SetMany(shooter, "bulletPrefab", a.EnemyShotBolt, "pattern", FirePattern.Aimed,
                    "interval", 1.5f, "intervalJitter", 0.4f, "firstShotDelay", 0.8f, "bulletSpeed", 8.5f, "damage", 9f, "aimError", 6f);
            });

            a.EnemySupplyShip = EnemyPrefab(a, "EnemySupplyShip", "EnemySupplyShip", 0f, 4f, (go, visual) =>
            {
                // A wide, flat ship: a box fits it better than a circle.
                var box = go.AddComponent<BoxCollider2D>();
                box.isTrigger = true;
                box.size = new Vector2(1.9f, 0.7f);

                var move = go.AddComponent<StraightMovement>();
                EditorUtil.SetMany(move, "swayAmplitude", 0.45f, "swayFrequency", 3f);
            });

            a.EnemyBoss = EnemyPrefab(a, "EnemyBoss", "Boss", 1.45f, 30f, (go, visual) =>
            {
                // Extra colliders for the wings.
                foreach (float x in new[] { -1.9f, 1.9f })
                {
                    var wing = go.AddComponent<CircleCollider2D>();
                    wing.isTrigger = true;
                    wing.radius = 0.95f;
                    wing.offset = new Vector2(x, 0.3f);
                }

                var move = go.AddComponent<BossMovement>();

                var brain = go.AddComponent<BossBrain>();
                Transform left = Child(go, "Muzzle Left", new Vector3(-2.07f, 0.4f)).transform;
                Transform right = Child(go, "Muzzle Right", new Vector3(2.07f, 0.4f)).transform;
                Transform core = Child(go, "Muzzle Core", new Vector3(0f, 0.18f)).transform;
                EditorUtil.SetMany(brain, "movement", move,
                    "boltPrefab", a.EnemyShotBolt, "orbPrefab", a.EnemyShotOrb, "heavyPrefab", a.EnemyShotHeavy,
                    "sideMuzzles", new[] { left, right }, "coreMuzzle", core, "minionData", a.Kamikaze);
            });
        }

        static void Tumble(GameObject visual, float speed)
        {
            var spin = visual.AddComponent<Spin>();
            EditorUtil.SetMany(spin, "speed", speed, "randomize", true);
        }

        /// <summary>
        /// The parts every enemy shares: a sprite, a trigger collider, health and a hit flash.
        /// <paramref name="addBehaviours"/> then adds whatever makes this particular enemy tick.
        /// </summary>
        static Enemy EnemyPrefab(GameAssets a, string name, string sprite, float colliderRadius, float despawnMargin,
            System.Action<GameObject, GameObject> addBehaviours)
        {
            var go = new GameObject(name) { layer = enemyLayer };

            GameObject visual = Child(go, "Visual");
            SpriteRenderer renderer = AddSprite(a, visual, sprite, OrderEnemy);

            // Kinematic: scripts move it, physics only reports overlaps.
            var body = go.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;

            if (colliderRadius > 0f)
            {
                var circle = go.AddComponent<CircleCollider2D>();
                circle.isTrigger = true;
                circle.radius = colliderRadius;
            }

            go.AddComponent<Health>();

            var flash = go.AddComponent<HitFlash>();
            EditorUtil.SetMany(flash, "target", renderer, "flashSprite", SpriteFactory.Get(sprite + "_Flash"));

            var enemy = go.AddComponent<Enemy>();
            EditorUtil.SetMany(enemy, "hitFlash", flash, "despawnMargin", despawnMargin);

            addBehaviours(go, visual);

            return EditorUtil.SavePrefab(go, $"{Folder}/Enemies/{name}.prefab").GetComponent<Enemy>();
        }

        // ---------------------------------------------------------------- pickup

        static void BuildPickup(GameAssets a)
        {
            var go = new GameObject("Pickup");

            GameObject glowObject = Child(go, "Glow");
            glowObject.transform.localScale = new Vector3(2.4f, 2.4f, 1f);
            SpriteRenderer glow = AddSprite(a, glowObject, "FxSoftCircle", OrderPickup - 1);

            SpriteRenderer icon = AddSprite(a, Child(go, "Icon"), "PickupHealth", OrderPickup);

            var pickup = go.AddComponent<Pickup>();
            EditorUtil.SetMany(pickup, "icon", icon, "glow", glow);

            a.PickupPrefab = EditorUtil.SavePrefab(go, Folder + "/Pickup.prefab").GetComponent<Pickup>();
        }

        // ---------------------------------------------------------------- player

        static void BuildPlayer(GameAssets a)
        {
            var go = new GameObject("Player") { layer = playerLayer };

            GameObject visual = Child(go, "Visual");
            SpriteRenderer ship = AddSprite(a, visual, "PlayerShip", OrderPlayer);

            GameObject flameObject = Child(visual, "Thruster Flame", new Vector3(0f, -0.78f));
            flameObject.transform.localScale = new Vector3(1.15f, 0.85f, 1f);
            AddSprite(a, flameObject, "ThrusterFlame", OrderPlayer - 1);

            Transform muzzle = Child(go, "Muzzle", new Vector3(0f, 0.85f)).transform;

            GameObject flashObject = Child(go, "Muzzle Flash", new Vector3(0f, 0.95f));
            AddSprite(a, flashObject, "FxMuzzleFlash", OrderPlayer + 1);
            var muzzleFlash = flashObject.AddComponent<MuzzleFlash>();

            GameObject bubbleObject = Child(go, "Shield Bubble");
            SpriteRenderer bubble = AddSprite(a, bubbleObject, "FxShieldBubble", OrderPlayer + 2);
            bubble.enabled = false;

            var body = go.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.sleepMode = RigidbodySleepMode2D.NeverSleep;

            // A hitbox much smaller than the sprite: near misses feel fair, as in classic shooters.
            var hitbox = go.AddComponent<CircleCollider2D>();
            hitbox.isTrigger = true;
            hitbox.radius = 0.3f;

            var health = go.AddComponent<Health>();
            EditorUtil.Set(health, "maxHealth", 100f);

            go.AddComponent<Armor>();

            var shield = go.AddComponent<Shield>();
            EditorUtil.Set(shield, "bubble", bubble);

            var controller = go.AddComponent<PlayerController>();
            EditorUtil.SetMany(controller, "visual", visual.transform, "dashEffect", a.FxDash);

            var damage = go.AddComponent<PlayerDamageReceiver>();
            EditorUtil.SetMany(damage, "blinkRenderers", new[] { ship }, "hitEffect", a.FxPlayerHit);

            var weapons = go.AddComponent<PlayerWeapons>();
            EditorUtil.SetMany(weapons,
                "loadout", new[] { a.Blaster, a.Spread, a.Vulcan, a.Rail, a.Missile, a.Plasma },
                "muzzle", muzzle, "muzzleFlash", muzzleFlash);

            var bombs = go.AddComponent<PlayerBombs>();
            EditorUtil.Set(bombs, "blastEffect", a.FxBombBlast);

            var player = go.AddComponent<Player>();
            EditorUtil.Set(player, "deathEffect", a.FxPlayerDeath);

            var flame = flameObject.AddComponent<ThrusterFlame>();
            EditorUtil.Set(flame, "controller", controller);

            a.PlayerPrefab = EditorUtil.SavePrefab(go, Folder + "/Player.prefab");
        }
    }
}
