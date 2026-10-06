using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace SpaceShooter.EditorTools
{
    /// <summary>Builds Assets/Scenes/Game.unity: camera, scrolling background, game systems, player and HUD.</summary>
    public static class SceneFactory
    {
        public const string ScenePath = "Assets/Scenes/Game.unity";
        const string PostProcessingPath = "Assets/Settings/GamePostProcessing.asset";

        /// <summary>Fills the given (empty, already open) scene with the game and saves it.</summary>
        public static void Build(GameAssets a, Scene scene)
        {
            EditorUtil.EnsureFolder("Assets/Scenes");
            a.PostProcessing = BuildPostProcessing();

            BuildCamera();
            BuildRendering(a);
            BuildBackground(a);
            WaveSpawner waves = BuildSystems(a);

            var player = (GameObject)PrefabUtility.InstantiatePrefab(a.PlayerPrefab, scene);
            player.transform.position = new Vector3(0f, -5.5f, 0f);

            HudFactory.Build(waves);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AddToBuildSettings();
        }

        // ---------------------------------------------------------------- camera & rendering

        static void BuildCamera()
        {
            var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);

            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            // 9 units from the centre to the top edge: the play field is 18 units tall.
            camera.orthographicSize = 9f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = SpriteFactory.Hex(0x050816);
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 50f;
            camera.allowHDR = true;

            cameraObject.AddComponent<AudioListener>();

            var cameraData = cameraObject.AddComponent<UniversalAdditionalCameraData>();
            cameraData.renderPostProcessing = true;

            cameraObject.AddComponent<PlayArea>();
            cameraObject.AddComponent<CameraShake>();
        }

        static void BuildRendering(GameAssets a)
        {
            // The game's sprites are unlit, but a global light means lit sprites you add later just work.
            var lightObject = new GameObject("Global Light 2D");
            var light = lightObject.AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Global;
            light.intensity = 1f;

            var volumeObject = new GameObject("Post Processing");
            var volume = volumeObject.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.sharedProfile = a.PostProcessing;
        }

        /// <summary>Bloom makes bright things glow; a slight vignette darkens the corners.</summary>
        static VolumeProfile BuildPostProcessing()
        {
            EditorUtil.EnsureFolder("Assets/Settings");
            AssetDatabase.DeleteAsset(PostProcessingPath);

            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, PostProcessingPath);

            var bloom = profile.Add<Bloom>();
            bloom.threshold.Override(0.8f);
            bloom.intensity.Override(1.2f);
            bloom.scatter.Override(0.6f);
            Embed(bloom, profile);

            var vignette = profile.Add<Vignette>();
            vignette.intensity.Override(0.3f);
            vignette.smoothness.Override(0.45f);
            Embed(vignette, profile);

            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            return profile;
        }

        static void Embed(VolumeComponent component, VolumeProfile profile)
        {
            component.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
            AssetDatabase.AddObjectToAsset(component, profile);
        }

        // ---------------------------------------------------------------- background

        static void BuildBackground(GameAssets a)
        {
            var background = new GameObject("Background");

            SpriteRenderer[] farClouds = Nebula(a, background, "Nebula Far", "BgNebulaA", new Color(0.50f, 0.30f, 0.95f, 0.42f), 0.10f, -100, false);
            Nebula(a, background, "Nebula Near", "BgNebulaB", new Color(0.15f, 0.60f, 0.75f, 0.26f), 0.20f, -99, true);

            var theme = background.AddComponent<SectorTheme>();
            EditorUtil.SetMany(theme, "targets", farClouds, "palette", new[]
            {
                new Color(0.50f, 0.30f, 0.95f, 0.42f),   // violet
                new Color(0.95f, 0.35f, 0.35f, 0.38f),   // crimson
                new Color(0.25f, 0.80f, 0.55f, 0.38f),   // emerald
                new Color(0.95f, 0.65f, 0.25f, 0.38f),   // amber
                new Color(0.30f, 0.55f, 1.00f, 0.42f)    // azure
            });

            var starsObject = new GameObject("Starfield");
            starsObject.transform.SetParent(background.transform, false);
            var stars = starsObject.AddComponent<Starfield>();
            EditorUtil.SetMany(stars, "starSprite", SpriteFactory.Get("FxStar"), "material", a.SpriteMaterial);
            SetStarLayers(stars,
                (70, 0.12f, new Vector2(0.45f, 0.70f), new Color(0.70f, 0.78f, 1f, 0.55f), -96),
                (45, 0.35f, new Vector2(0.65f, 1.00f), new Color(0.85f, 0.90f, 1f, 0.80f), -95),
                (22, 0.90f, new Vector2(0.90f, 1.40f), new Color(1f, 1f, 1f, 0.95f), -94));

            var planetsObject = new GameObject("Planets");
            planetsObject.transform.SetParent(background.transform, false);
            var planets = planetsObject.AddComponent<DecorSpawner>();
            EditorUtil.SetMany(planets, "material", a.SpriteMaterial, "sprites", new[]
            {
                SpriteFactory.Get("BgPlanetA"), SpriteFactory.Get("BgPlanetB"), SpriteFactory.Get("BgPlanetC")
            });
        }

        /// <summary>Two copies of a cloud texture stacked vertically, leap-frogging forever.</summary>
        static SpriteRenderer[] Nebula(GameAssets a, GameObject parent, string name, string sprite, Color tint,
            float speedFactor, int order, bool mirrored)
        {
            const float tileHeight = 48f;

            var layer = new GameObject(name);
            layer.transform.SetParent(parent.transform, false);

            var renderers = new SpriteRenderer[2];
            var tiles = new Transform[2];
            for (int i = 0; i < 2; i++)
            {
                var tile = new GameObject("Tile " + (i + 1));
                tile.transform.SetParent(layer.transform, false);
                tile.transform.localPosition = new Vector3(0f, i * tileHeight, 0f);
                // Stretched sideways so the clouds still reach the edges on very wide screens.
                tile.transform.localScale = new Vector3(1.6f, 1f, 1f);

                var renderer = tile.AddComponent<SpriteRenderer>();
                renderer.sprite = SpriteFactory.Get(sprite);
                renderer.sharedMaterial = a.SpriteMaterial;
                renderer.sortingOrder = order;
                renderer.color = tint;
                renderer.flipX = mirrored;

                renderers[i] = renderer;
                tiles[i] = tile.transform;
            }

            var scroller = layer.AddComponent<ScrollingLayer>();
            EditorUtil.SetMany(scroller, "speedFactor", speedFactor, "tileHeight", tileHeight, "tiles", tiles);
            return renderers;
        }

        static void SetStarLayers(Starfield stars, params (int count, float speed, Vector2 size, Color color, int order)[] layers)
        {
            var serialized = new SerializedObject(stars);
            SerializedProperty array = serialized.FindProperty("layers");
            array.arraySize = layers.Length;

            for (int i = 0; i < layers.Length; i++)
            {
                SerializedProperty element = array.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("count").intValue = layers[i].count;
                element.FindPropertyRelative("speedFactor").floatValue = layers[i].speed;
                element.FindPropertyRelative("sizeRange").vector2Value = layers[i].size;
                element.FindPropertyRelative("color").colorValue = layers[i].color;
                element.FindPropertyRelative("sortingOrder").intValue = layers[i].order;
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        // ---------------------------------------------------------------- systems

        static WaveSpawner BuildSystems(GameAssets a)
        {
            var systems = new GameObject("Systems");

            T Make<T>(string name) where T : Component
            {
                var holder = new GameObject(name);
                holder.transform.SetParent(systems.transform, false);
                return holder.AddComponent<T>();
            }

            Make<PoolManager>("Pools");
            Make<GameInput>("Input");
            Make<WorldScroll>("World Scroll");
            Make<ScoreManager>("Score");

            var audio = Make<AudioManager>("Audio");
            EditorUtil.Set(audio, "library", a.SfxLibrary);

            var waves = Make<WaveSpawner>("Wave Spawner");
            EditorUtil.SetMany(waves,
                "roster", new[] { a.Drone, a.Dart, a.DartWing, a.Gunner, a.Kamikaze, a.AsteroidLarge, a.Bomber, a.Striker, a.Spinner },
                "bosses", new[] { a.Boss });

            var bonus = waves.gameObject.AddComponent<BonusShipSpawner>();
            EditorUtil.Set(bonus, "bonusShip", a.SupplyShip);

            var pickups = Make<PickupSpawner>("Pickup Spawner");
            EditorUtil.SetMany(pickups, "pickupPrefab", a.PickupPrefab, "dropTable", a.DropTable);

            var game = Make<GameManager>("Game Manager");
            EditorUtil.Set(game, "waveSpawner", waves);

            return waves;
        }

        // ---------------------------------------------------------------- build settings

        static void AddToBuildSettings()
        {
            // The game scene goes first so it is the one a built player starts in.
            var scenes = new List<EditorBuildSettingsScene> { new EditorBuildSettingsScene(ScenePath, true) };
            foreach (EditorBuildSettingsScene existing in EditorBuildSettings.scenes)
            {
                if (existing.path != ScenePath) scenes.Add(existing);
            }
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
