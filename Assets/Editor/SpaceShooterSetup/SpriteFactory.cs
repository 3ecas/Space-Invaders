using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SpaceShooter.EditorTools
{
    /// <summary>
    /// Draws all of the game's placeholder art and saves it as PNG sprites in Assets/Art/Sprites.
    /// Nothing here runs in the finished game. Swap any PNG for your own art (keep the file name)
    /// and every prefab that uses it updates automatically.
    /// </summary>
    public static class SpriteFactory
    {
        public const string Folder = "Assets/Art/Sprites";
        public const float PixelsPerUnit = 64f;

        static readonly Color Ink = Hex(0x0B0F1E);
        static readonly Color Panel = Hex(0x0E1626);

        // Weapon signature colours, shared by icons, pickups and projectiles.
        public static readonly Color BlasterColor = Hex(0x38BDF8);
        public static readonly Color SpreadColor = Hex(0xFB923C);
        public static readonly Color VulcanColor = Hex(0xFACC15);
        public static readonly Color RailColor = Hex(0xA78BFA);
        public static readonly Color MissileColor = Hex(0xF87171);
        public static readonly Color PlasmaColor = Hex(0x34D399);

        sealed class Entry
        {
            public string Name;
            public float PixelsPerUnit;
            public Vector2 Pivot;
        }

        static readonly List<Entry> entries = new List<Entry>();

        public static Color Hex(int rgb, float alpha = 1f)
        {
            return new Color(((rgb >> 16) & 255) / 255f, ((rgb >> 8) & 255) / 255f, (rgb & 255) / 255f, alpha);
        }

        /// <summary>Loads a sprite made by <see cref="BuildAll"/>.</summary>
        public static Sprite Get(string name)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{Folder}/{name}.png");
            if (sprite == null) Debug.LogError($"[Space Shooter setup] Missing sprite '{name}'.");
            return sprite;
        }

        public static void BuildAll()
        {
            entries.Clear();
            EditorUtil.EnsureFolder(Folder);

            DrawPlayer();
            DrawEnemies();
            DrawBoss();
            DrawProjectiles();
            DrawPickups();
            DrawWeaponIcons();
            DrawEffects();
            DrawBackground();
            DrawInterface();

            // Import the new files, then apply sprite settings to each one.
            AssetDatabase.Refresh();
            for (int i = 0; i < entries.Count; i++)
            {
                EditorUtility.DisplayProgressBar("Space Shooter setup", "Importing sprites...", i / (float)entries.Count);
                Configure(entries[i]);
            }
            EditorUtility.ClearProgressBar();
        }

        // ---------------------------------------------------------------- saving

        static void Save(string name, SpriteCanvas canvas, bool withFlash = false, float pixelsPerUnit = PixelsPerUnit, Vector2? pivot = null)
        {
            WritePng(name, canvas.ToTexture(), pixelsPerUnit, pivot ?? new Vector2(0.5f, 0.5f));
            // The "_Flash" copy is an all-white silhouette, shown for an instant when the sprite is hit.
            if (withFlash) WritePng(name + "_Flash", canvas.ToTexture(true), pixelsPerUnit, pivot ?? new Vector2(0.5f, 0.5f));
        }

        static void WritePng(string name, Texture2D texture, float pixelsPerUnit, Vector2 pivot)
        {
            File.WriteAllBytes($"{Folder}/{name}.png", texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            entries.Add(new Entry { Name = name, PixelsPerUnit = pixelsPerUnit, Pivot = pivot });
        }

        static void Configure(Entry entry)
        {
            string path = $"{Folder}/{entry.Name}.png";
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                Debug.LogError($"[Space Shooter setup] Could not import '{path}'.");
                return;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = entry.PixelsPerUnit;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = entry.Pivot;
            settings.spriteGenerateFallbackPhysicsShape = false;
            importer.SetTextureSettings(settings);

            importer.SaveAndReimport();
        }

        // ---------------------------------------------------------------- player

        static void DrawPlayer()
        {
            Color hull = Hex(0xE6ECF5), wing = Hex(0x8C9BB5), dark = Hex(0x56627C), accent = Hex(0xFF4D5E), metal = Hex(0x2B3142);

            var c = new SpriteCanvas(112, 112);
            c.RectMirrored(metal, 0.44f, -0.32f, 0.54f, 0.10f);                                        // wing cannons
            c.PolygonMirrored(wing, 0.10f, 0.20f, 0.95f, -0.36f, 0.95f, -0.60f, 0.50f, -0.56f, 0.10f, -0.48f);
            c.PolygonMirrored(accent, 0.78f, -0.25f, 0.95f, -0.36f, 0.95f, -0.60f, 0.78f, -0.585f);    // wing tips
            c.PolygonMirrored(dark, 0.12f, -0.50f, 0.42f, -0.76f, 0.42f, -0.92f, 0.12f, -0.80f);       // tail fins
            c.RectMirrored(metal, 0.03f, -0.94f, 0.13f, -0.76f);                                       // engine nozzles
            c.PolygonMirrored(hull, 0f, 0.96f, 0.09f, 0.72f, 0.17f, 0.30f, 0.20f, -0.30f, 0.15f, -0.76f, 0f, -0.82f);
            c.PolygonMirrored(accent, 0f, 0.96f, 0.09f, 0.72f, 0f, 0.66f);                             // nose cone
            c.ShadeRightHalf(0.2f);
            c.Ellipse(Hex(0x14233D), 0f, 0.33f, 0.105f, 0.235f);                                       // cockpit
            c.Ellipse(Hex(0x35D6FF), 0f, 0.33f, 0.075f, 0.20f);
            c.Ellipse(Hex(0xD8F8FF), -0.025f, 0.42f, 0.025f, 0.075f);
            c.Outline(Ink, 2f);
            Save("PlayerShip", c, true);

            // Engine flame. The pivot sits at the top so it stretches away from the nozzle.
            var flame = new SpriteCanvas(32, 64);
            flame.Fill(Hex(0x38BDF8), -0.5f, -1f, 0.5f, 1f, (x, y) => Teardrop(x, y, 0.46f, 1f));
            flame.Fill(Hex(0xE8FAFF), -0.5f, -1f, 0.5f, 1f, (x, y) => Teardrop(x, y, 0.22f, 0.6f));
            Save("ThrusterFlame", flame, false, PixelsPerUnit, new Vector2(0.5f, 1f));
        }

        static float Teardrop(float x, float y, float width, float length)
        {
            float t = (1f - y) * 0.5f / length;     // 0 at the nozzle, 1 at the tip
            if (t <= 0f || t >= 1f) return 0f;

            float half = width * (1f - t);
            float d = Mathf.Abs(x) / Mathf.Max(half, 0.0001f);
            return d >= 1f ? 0f : (1f - d * d) * Mathf.Pow(1f - t, 0.7f);
        }

        // ---------------------------------------------------------------- enemies
        // Ships are designed nose-up and flipped at the end so they face down the screen.

        static void DrawEnemies()
        {
            // Drone: the common green "popcorn" enemy.
            var c = new SpriteCanvas(72, 72);
            c.PolygonMirrored(Hex(0x4CC764), 0f, 0.85f, 0.30f, 0.35f, 0.85f, 0.05f, 0.85f, -0.35f, 0.45f, -0.60f, 0f, -0.42f);
            c.PolygonMirrored(Hex(0x2E8F4A), 0.30f, 0.35f, 0.85f, 0.05f, 0.85f, -0.12f, 0.30f, 0.12f);
            c.ShadeRightHalf(0.2f);
            c.Circle(Hex(0x12301F), 0f, 0.02f, 0.25f);
            c.Circle(Hex(0xC8FF5A), 0f, 0.02f, 0.16f);
            c.Circle(Hex(0xFFFFFF), -0.05f, 0.07f, 0.05f);
            c.Outline(Ink, 2f);
            c.FlipVertical();
            Save("EnemyDrone", c, true);

            // Dart: a slim orange diver.
            c = new SpriteCanvas(64, 80);
            c.PolygonMirrored(Hex(0xFF9A3C), 0f, 0.97f, 0.16f, 0.25f, 0.70f, -0.55f, 0.28f, -0.42f, 0.12f, -0.90f, 0f, -0.72f);
            c.PolygonMirrored(Hex(0xC8621E), 0.16f, 0.25f, 0.70f, -0.55f, 0.48f, -0.49f, 0.16f, -0.05f);
            c.ShadeRightHalf(0.2f);
            c.Ellipse(Hex(0x3A1A0A), 0f, 0.30f, 0.08f, 0.21f);
            c.Ellipse(Hex(0xFFE08A), 0f, 0.30f, 0.05f, 0.16f);
            c.Outline(Ink, 2f);
            c.FlipVertical();
            Save("EnemyDart", c, true);

            // Gunner: a purple gunship with twin cannons.
            c = new SpriteCanvas(96, 96);
            c.RectMirrored(Hex(0x3A3350), 0.52f, 0.05f, 0.68f, 0.74f);
            c.PolygonMirrored(Hex(0x8B5CF6), 0f, 0.60f, 0.28f, 0.50f, 0.44f, 0.12f, 0.92f, -0.02f, 0.96f, -0.45f, 0.55f, -0.58f, 0.30f, -0.82f, 0f, -0.72f);
            c.PolygonMirrored(Hex(0x6D3FD9), 0.44f, 0.12f, 0.92f, -0.02f, 0.96f, -0.45f, 0.60f, -0.40f);
            c.ShadeRightHalf(0.2f);
            c.Ellipse(Hex(0x1E1338), 0f, 0.12f, 0.17f, 0.24f);
            c.Ellipse(Hex(0xFF7AD9), 0f, 0.12f, 0.12f, 0.18f);
            c.Ellipse(Hex(0xFFE3F6), -0.03f, 0.20f, 0.04f, 0.07f);
            c.CircleMirrored(Hex(0xFFD34D), 0.60f, 0.74f, 0.07f);
            c.Outline(Ink, 2f);
            c.FlipVertical();
            Save("EnemyGunner", c, true);

            // Kamikaze: a red dagger that hunts the player.
            c = new SpriteCanvas(64, 64);
            c.PolygonMirrored(Hex(0xF43F5E), 0f, 0.95f, 0.26f, 0.22f, 0.80f, -0.15f, 0.42f, -0.30f, 0.58f, -0.88f, 0.20f, -0.52f, 0f, -0.74f);
            c.ShadeRightHalf(0.22f);
            c.Circle(Hex(0x4A0E18), 0f, -0.02f, 0.21f);
            c.Circle(Hex(0xFFE14D), 0f, -0.02f, 0.13f);
            c.Outline(Ink, 2f);
            c.FlipVertical();
            Save("EnemyKamikaze", c, true);

            // Bomber: a slow, heavily armoured teal brute.
            c = new SpriteCanvas(144, 144);
            c.RectMirrored(Hex(0x243B40), 0.60f, -0.90f, 0.82f, -0.50f);
            c.PolygonMirrored(Hex(0x3BA7A0), 0f, 0.72f, 0.34f, 0.64f, 0.54f, 0.30f, 0.95f, 0.16f, 0.95f, -0.36f, 0.60f, -0.56f, 0.44f, -0.86f, 0f, -0.76f);
            c.PolygonMirrored(Hex(0x2A7F7A), 0.20f, 0.42f, 0.50f, 0.22f, 0.50f, -0.30f, 0.20f, -0.52f);
            c.PolygonMirrored(Hex(0x2A7F7A), 0.62f, 0.10f, 0.90f, 0.06f, 0.90f, -0.30f, 0.62f, -0.42f);
            c.ShadeRightHalf(0.2f);
            c.Ellipse(Hex(0x0F2A2C), 0f, 0.30f, 0.17f, 0.15f);
            c.Ellipse(Hex(0xFFD34D), 0f, 0.30f, 0.12f, 0.10f);
            c.CircleMirrored(Hex(0x0F2A2C), 0.76f, -0.12f, 0.10f);
            c.CircleMirrored(Hex(0xFF6B4A), 0.76f, -0.12f, 0.06f);
            c.Outline(Ink, 2.5f);
            c.FlipVertical();
            Save("EnemyBomber", c, true);

            // Spinner: a magenta saucer that sprays bullets in rings.
            c = new SpriteCanvas(112, 112);
            c.Circle(Hex(0xE0459B), 0f, 0f, 0.92f);
            c.Circle(Hex(0xA82D74), 0f, 0f, 0.70f);
            c.ShadeSphere(0f, 0f, 0.92f, -0.3f, 0.35f, 1.15f, 0.7f);
            for (int i = 0; i < 6; i++)
            {
                float angle = i * Mathf.PI / 3f;
                float x = Mathf.Cos(angle) * 0.81f, y = Mathf.Sin(angle) * 0.81f;
                c.Circle(Hex(0x2B1030), x, y, 0.10f);
                c.Circle(Hex(0xFFD1F0), x, y, 0.05f);
            }
            c.Circle(Hex(0x2B1030), 0f, 0f, 0.42f);
            c.Circle(Hex(0x7CF3FF), 0f, 0f, 0.31f);
            c.Circle(Hex(0xE8FDFF), -0.09f, 0.10f, 0.09f);
            c.Outline(Ink, 2f);
            Save("EnemySpinner", c, true);

            // Striker: a yellow flying wing that crosses the screen sideways.
            c = new SpriteCanvas(96, 72);
            c.PolygonMirrored(Hex(0xF5C542), 0f, 0.75f, 0.30f, 0.40f, 1.25f, -0.25f, 1.25f, -0.55f, 0.42f, -0.22f, 0.24f, -0.80f, 0f, -0.58f);
            c.PolygonMirrored(Hex(0xC99A1E), 0.30f, 0.40f, 1.25f, -0.25f, 1.25f, -0.40f, 0.30f, 0.15f);
            c.ShadeRightHalf(0.2f);
            c.Ellipse(Hex(0x3D2C05), 0f, 0.16f, 0.12f, 0.26f);
            c.Ellipse(Hex(0xFFF3B0), 0f, 0.16f, 0.07f, 0.19f);
            c.Outline(Ink, 2f);
            c.FlipVertical();
            Save("EnemyStriker", c, true);

            // Supply ship: a classic flying saucer seen from the side. Worth a pickup when shot down.
            c = new SpriteCanvas(128, 72);
            c.Ellipse(Hex(0x7DE3FF), 0f, 0.22f, 0.52f, 0.52f);
            c.Ellipse(Hex(0xDDF7FF), -0.16f, 0.44f, 0.14f, 0.12f);
            c.Ellipse(Hex(0xC9D2E3), 0f, -0.12f, 1.60f, 0.44f);
            c.Ellipse(Hex(0x8E99B3), 0f, -0.28f, 1.30f, 0.26f);
            for (int i = -2; i <= 2; i++) c.Circle(Hex(0xFFD34D), i * 0.52f, -0.13f, 0.09f);
            c.Outline(Ink, 2f);
            Save("EnemySupplyShip", c, true);

            DrawAsteroid("AsteroidLarge", 128, 11);
            DrawAsteroid("AsteroidSmall", 64, 29);
        }

        static void DrawAsteroid(string name, int size, int seed)
        {
            var random = new System.Random(seed);
            float Next() => (float)random.NextDouble();

            const int corners = 11;
            var outline = new float[corners * 2];
            for (int i = 0; i < corners; i++)
            {
                float angle = i * Mathf.PI * 2f / corners + (Next() - 0.5f) * 0.3f;
                float radius = 0.66f + Next() * 0.26f;
                outline[i * 2] = Mathf.Cos(angle) * radius;
                outline[i * 2 + 1] = Mathf.Sin(angle) * radius;
            }

            var c = new SpriteCanvas(size, size);
            c.Polygon(Hex(0x8A8076), outline);
            for (int i = 0; i < 4; i++)
            {
                float angle = Next() * Mathf.PI * 2f;
                float distance = Next() * 0.40f;
                float radius = 0.08f + Next() * 0.11f;
                float x = Mathf.Cos(angle) * distance, y = Mathf.Sin(angle) * distance;
                c.Circle(Hex(0x6B6259), x, y, radius);
                c.Circle(Hex(0x554D45), x + radius * 0.2f, y - radius * 0.2f, radius * 0.7f);
            }
            c.ShadeSphere(0f, 0f, 0.9f, -0.35f, 0.4f, 1.2f, 0.55f);
            c.Outline(Ink, 2f);
            Save(name, c, true);
        }

        static void DrawBoss()
        {
            Color hull = Hex(0xC43C4B), plate = Hex(0x8E2433), deep = Hex(0x6B1A27), metal = Hex(0x2A1016);

            var c = new SpriteCanvas(384, 288, 3);
            c.RectMirrored(metal, 0.30f, -0.96f, 0.55f, -0.60f);                                       // engines
            c.RectMirrored(metal, 0.80f, -0.80f, 0.98f, -0.45f);
            c.PolygonMirrored(plate, 0.45f, 0.35f, 1.28f, 0.02f, 1.30f, -0.42f, 0.95f, -0.62f, 0.45f, -0.50f);     // wings
            c.PolygonMirrored(Hex(0xB8323F), 0.16f, 0.50f, 0.26f, 0.97f, 0.42f, 0.88f, 0.46f, 0.30f);  // forward prongs
            c.PolygonMirrored(hull, 0f, 0.62f, 0.20f, 0.56f, 0.48f, 0.30f, 0.62f, -0.20f, 0.50f, -0.72f, 0.22f, -0.86f, 0f, -0.78f);
            c.PolygonMirrored(plate, 0.12f, 0.40f, 0.38f, 0.22f, 0.46f, -0.20f, 0.36f, -0.60f, 0.12f, -0.68f);
            c.PolygonMirrored(deep, 0.66f, 0.16f, 1.18f, -0.02f, 1.20f, -0.36f, 0.70f, -0.40f);
            c.ShadeRightHalf(0.18f);
            c.CircleMirrored(Hex(0x1F0A10), 0.92f, -0.18f, 0.13f);                                     // wing turrets
            c.CircleMirrored(Hex(0xFF9A3C), 0.92f, -0.18f, 0.075f);
            c.CircleMirrored(Hex(0xFFD34D), 0.33f, 0.90f, 0.05f);                                      // prong tips
            c.RectMirrored(Hex(0xFFD34D), 0.03f, 0.36f, 0.10f, 0.43f);                                 // bridge lights
            c.Circle(Hex(0x1F0A10), 0f, -0.08f, 0.25f);                                                // reactor core
            c.Circle(Hex(0xFF7A1A), 0f, -0.08f, 0.19f);
            c.Circle(Hex(0xFFE08A), 0f, -0.08f, 0.11f);
            c.Circle(Hex(0xFFFFFF), -0.03f, -0.04f, 0.04f);
            c.Outline(Ink, 3f);
            c.FlipVertical();
            Save("Boss", c, true);
        }

        // ---------------------------------------------------------------- projectiles

        static void DrawProjectiles()
        {
            var c = new SpriteCanvas(16, 44);
            c.Capsule(BlasterColor, 0f, -0.62f, 0f, 0.62f, 0.30f);
            c.Capsule(Hex(0xF0FBFF), 0f, -0.50f, 0f, 0.56f, 0.15f);
            Save("ShotBlaster", c);

            c = new SpriteCanvas(20, 20);
            c.Circle(SpreadColor, 0f, 0f, 0.92f);
            c.Circle(Hex(0xFFF3C4), 0f, 0f, 0.52f);
            Save("ShotSpread", c);

            c = new SpriteCanvas(10, 28);
            c.Capsule(VulcanColor, 0f, -0.66f, 0f, 0.66f, 0.30f);
            c.Capsule(Hex(0xFFFDE8), 0f, -0.50f, 0f, 0.60f, 0.14f);
            Save("ShotVulcan", c);

            c = new SpriteCanvas(14, 96);
            c.Capsule(RailColor, 0f, -0.88f, 0f, 0.88f, 0.125f);
            c.Capsule(Hex(0xFFFFFF), 0f, -0.80f, 0f, 0.86f, 0.06f);
            Save("ShotRail", c);

            c = new SpriteCanvas(24, 52);
            c.PolygonMirrored(Hex(0x7A8499), 0.15f, -0.25f, 0.40f, -0.72f, 0.15f, -0.58f);              // fins
            c.Rect(Hex(0xE6ECF5), -0.16f, -0.60f, 0.16f, 0.42f);
            c.Polygon(MissileColor, 0f, 0.94f, 0.16f, 0.42f, -0.16f, 0.42f);
            c.ShadeRightHalf(0.2f);
            c.Circle(Hex(0xFFD34D), 0f, -0.70f, 0.13f);
            c.Outline(Ink, 1.5f);
            Save("ShotMissile", c);

            c = new SpriteCanvas(56, 56);
            c.Glow(PlasmaColor, 0f, 0f, 1f, 1.4f);
            c.Circle(Hex(0x6EE7B7), 0f, 0f, 0.56f);
            c.Circle(Hex(0xF0FFF8), 0f, 0f, 0.32f);
            Save("ShotPlasma", c);

            // Enemy shots get a dark rim so they stay readable over bright explosions.
            c = new SpriteCanvas(24, 24);
            c.Circle(Hex(0x5C0A22), 0f, 0f, 0.96f);
            c.Circle(Hex(0xFF3B5C), 0f, 0f, 0.78f);
            c.Circle(Hex(0xFFD9DF), 0f, 0f, 0.40f);
            Save("EnemyShotOrb", c);

            c = new SpriteCanvas(16, 40);
            c.Polygon(Hex(0x4A0D57), 0f, 0.98f, 0.38f, 0f, 0f, -0.98f, -0.38f, 0f);
            c.Polygon(Hex(0xE879F9), 0f, 0.84f, 0.29f, 0f, 0f, -0.84f, -0.29f, 0f);
            c.Polygon(Hex(0xFDF0FF), 0f, 0.50f, 0.13f, 0f, 0f, -0.50f, -0.13f, 0f);
            Save("EnemyShotBolt", c);

            c = new SpriteCanvas(40, 40);
            c.Circle(Hex(0x2E0854), 0f, 0f, 0.96f);
            c.Circle(Hex(0xA855F7), 0f, 0f, 0.82f);
            c.Circle(Hex(0xF3E8FF), 0f, 0f, 0.42f);
            Save("EnemyShotHeavy", c);
        }

        // ---------------------------------------------------------------- pickups & icons

        static SpriteCanvas Badge(Color border, bool hexagon)
        {
            var c = new SpriteCanvas(64, 64);
            if (hexagon)
            {
                c.Polygon(border, RegularPolygon(6, 0.94f, 30f));
                c.Polygon(Panel, RegularPolygon(6, 0.76f, 30f));
            }
            else
            {
                c.RoundedRect(border, -0.86f, -0.86f, 0.86f, 0.86f, 0.30f);
                c.RoundedRect(Panel, -0.70f, -0.70f, 0.70f, 0.70f, 0.18f);
            }
            return c;
        }

        static float[] RegularPolygon(int sides, float radius, float rotationDegrees)
        {
            var points = new float[sides * 2];
            for (int i = 0; i < sides; i++)
            {
                float angle = (rotationDegrees + i * 360f / sides) * Mathf.Deg2Rad;
                points[i * 2] = Mathf.Cos(angle) * radius;
                points[i * 2 + 1] = Mathf.Sin(angle) * radius;
            }
            return points;
        }

        static void DrawPickups()
        {
            // Stat pickups: rounded squares.
            Color green = Hex(0x4ADE80);
            SpriteCanvas c = Badge(Hex(0x22C55E), false);
            c.Rect(green, -0.14f, -0.48f, 0.14f, 0.48f);
            c.Rect(green, -0.48f, -0.14f, 0.48f, 0.14f);
            SavePickup("PickupHealth", c);

            c = Badge(Hex(0x94A3B8), false);
            c.Polygon(Hex(0xCBD5E1), -0.42f, 0.46f, 0.42f, 0.46f, 0.42f, -0.06f, 0f, -0.54f, -0.42f, -0.06f);
            c.Polygon(Hex(0x64748B), -0.24f, 0.28f, 0.24f, 0.28f, 0.24f, -0.02f, 0f, -0.30f, -0.24f, -0.02f);
            SavePickup("PickupArmor", c);

            c = Badge(Hex(0x38BDF8), false);
            c.Ring(Hex(0x7DD3FC), 0f, 0f, 0.48f, 0.33f);
            c.Circle(Hex(0xE0F2FE), 0f, 0f, 0.14f);
            SavePickup("PickupShield", c);

            Color yellow = Hex(0xFDE047);
            c = Badge(Hex(0xFACC15), false);
            c.Polygon(yellow, 0f, 0.54f, 0.44f, 0.12f, 0.44f, -0.10f, 0f, 0.30f, -0.44f, -0.10f, -0.44f, 0.12f);
            c.Polygon(yellow, 0f, 0.14f, 0.44f, -0.28f, 0.44f, -0.50f, 0f, -0.10f, -0.44f, -0.50f, -0.44f, -0.28f);
            SavePickup("PickupPower", c);

            c = Badge(Hex(0xF97316), false);
            c.Capsule(Hex(0xD6D3D1), 0.12f, 0.16f, 0.32f, 0.40f, 0.05f);
            c.Circle(Hex(0xFDE047), 0.36f, 0.45f, 0.10f);
            c.Circle(Hex(0xEF4444), -0.06f, -0.10f, 0.36f);
            c.Circle(Hex(0xFCA5A5), -0.18f, 0.02f, 0.09f);
            SavePickup("PickupBomb", c);

            c = Badge(Hex(0xFB7185), false);
            c.Polygon(yellow, 0.12f, 0.56f, -0.32f, -0.06f, -0.02f, -0.06f, -0.14f, -0.56f, 0.32f, 0.08f, 0.02f, 0.08f);
            SavePickup("PickupOverdrive", c);

            // Weapon pickups: hexagons in the weapon's colour.
            c = Badge(SpreadColor, true);
            GlyphSpread(c, SpreadColor, 1f);
            SavePickup("PickupSpread", c);

            c = Badge(VulcanColor, true);
            GlyphVulcan(c, VulcanColor, 1f);
            SavePickup("PickupVulcan", c);

            c = Badge(RailColor, true);
            GlyphRail(c, RailColor, 1f);
            SavePickup("PickupRail", c);

            c = Badge(MissileColor, true);
            GlyphMissile(c, MissileColor, 1f);
            SavePickup("PickupMissile", c);

            c = Badge(PlasmaColor, true);
            GlyphPlasma(c, PlasmaColor, 1f);
            SavePickup("PickupPlasma", c);
        }

        static void SavePickup(string name, SpriteCanvas canvas)
        {
            canvas.Outline(Ink, 1.5f);
            Save(name, canvas);
        }

        static void DrawWeaponIcons()
        {
            var c = new SpriteCanvas(64, 64);
            GlyphBlaster(c, BlasterColor, 1.5f);
            Save("IconBlaster", c);

            c = new SpriteCanvas(64, 64);
            GlyphSpread(c, SpreadColor, 1.5f);
            Save("IconSpread", c);

            c = new SpriteCanvas(64, 64);
            GlyphVulcan(c, VulcanColor, 1.5f);
            Save("IconVulcan", c);

            c = new SpriteCanvas(64, 64);
            GlyphRail(c, RailColor, 1.5f);
            Save("IconRail", c);

            c = new SpriteCanvas(64, 64);
            GlyphMissile(c, MissileColor, 1.5f);
            Save("IconMissile", c);

            c = new SpriteCanvas(64, 64);
            GlyphPlasma(c, PlasmaColor, 1.5f);
            Save("IconPlasma", c);
        }

        // The little symbols that identify each weapon. "s" scales them (1 fits inside a pickup badge).

        static void GlyphBlaster(SpriteCanvas c, Color color, float s)
        {
            c.Capsule(color, 0f, -0.40f * s, 0f, 0.40f * s, 0.14f * s);
            c.Capsule(Color.white, 0f, -0.30f * s, 0f, 0.34f * s, 0.06f * s);
        }

        static void GlyphSpread(SpriteCanvas c, Color color, float s)
        {
            // Three shots fanning out of a muzzle.
            c.Circle(color, 0f, -0.40f * s, 0.11f * s);
            c.Capsule(color, 0f, -0.08f * s, 0f, 0.46f * s, 0.085f * s);
            c.Capsule(color, -0.17f * s, -0.14f * s, -0.44f * s, 0.30f * s, 0.085f * s);
            c.Capsule(color, 0.17f * s, -0.14f * s, 0.44f * s, 0.30f * s, 0.085f * s);
        }

        static void GlyphVulcan(SpriteCanvas c, Color color, float s)
        {
            c.Capsule(color, 0f, -0.06f * s, 0f, 0.44f * s, 0.09f * s);
            c.Capsule(color, -0.30f * s, -0.40f * s, -0.30f * s, 0.10f * s, 0.09f * s);
            c.Capsule(color, 0.30f * s, -0.40f * s, 0.30f * s, 0.10f * s, 0.09f * s);
        }

        static void GlyphRail(SpriteCanvas c, Color color, float s)
        {
            c.Capsule(color, 0f, -0.50f * s, 0f, 0.50f * s, 0.075f * s);
            c.Capsule(color, -0.26f * s, -0.24f * s, -0.26f * s, 0.24f * s, 0.045f * s);
            c.Capsule(color, 0.26f * s, -0.24f * s, 0.26f * s, 0.24f * s, 0.045f * s);
        }

        static void GlyphMissile(SpriteCanvas c, Color color, float s)
        {
            c.PolygonMirrored(Hex(0x94A3B8), 0.15f * s, -0.10f * s, 0.38f * s, -0.48f * s, 0.15f * s, -0.38f * s);
            c.Rect(Hex(0xE6ECF5), -0.16f * s, -0.38f * s, 0.16f * s, 0.24f * s);
            c.Polygon(color, 0f, 0.56f * s, 0.16f * s, 0.24f * s, -0.16f * s, 0.24f * s);
            c.Circle(Hex(0xFDE047), 0f, -0.46f * s, 0.10f * s);
        }

        static void GlyphPlasma(SpriteCanvas c, Color color, float s)
        {
            c.Glow(color, 0f, 0f, 0.56f * s, 1.2f);
            c.Circle(color, 0f, 0f, 0.34f * s);
            c.Circle(Hex(0xF0FFF8), 0f, 0f, 0.17f * s);
        }

        // ---------------------------------------------------------------- effects

        static void DrawEffects()
        {
            var c = new SpriteCanvas(64, 64);
            c.Glow(Color.white, 0f, 0f, 1f, 1.6f);
            Save("FxSoftCircle", c);

            c = new SpriteCanvas(128, 128);
            c.Ring(Color.white, 0f, 0f, 0.98f, 0.86f);
            Save("FxRing", c);

            c = new SpriteCanvas(128, 128);
            c.Ring(Color.white, 0f, 0f, 0.98f, 0.93f);
            Save("FxRingThin", c);

            c = new SpriteCanvas(128, 128);
            c.Ring(Color.white, 0f, 0f, 0.98f, 0.93f);
            Save("FxRingThin", c);

            float[] star = { 0f, 1f, 0.16f, 0.16f, 1f, 0f, 0.16f, -0.16f, 0f, -1f, -0.16f, -0.16f, -1f, 0f, -0.16f, 0.16f };

            c = new SpriteCanvas(48, 48);
            c.Glow(Color.white, 0f, 0f, 0.8f, 1.5f);
            c.Polygon(Color.white, star);
            Save("FxMuzzleFlash", c);

            c = new SpriteCanvas(16, 16);
            c.Glow(Color.white, 0f, 0f, 1f, 1.2f);
            c.Circle(Color.white, 0f, 0f, 0.38f);
            Save("FxStar", c);

            // Energy shield bubble: faint in the middle, bright at the rim.
            c = new SpriteCanvas(160, 160);
            c.Fill(Color.white, -1f, -1f, 1f, 1f, (x, y) =>
            {
                float d = Mathf.Sqrt(x * x + y * y) / 0.95f;
                return d >= 1f ? 0f : 0.10f + 0.45f * d * d * d * d;
            });
            c.Ring(Color.white, 0f, 0f, 0.97f, 0.91f);
            Save("FxShieldBubble", c);

            // A plain white square: bar fills, panels, debris.
            c = new SpriteCanvas(8, 8, 1);
            c.Rect(Color.white, -1f, -1f, 1f, 1f);
            Save("FxPixel", c, false, 8f);
        }

        // ---------------------------------------------------------------- background

        static void DrawBackground()
        {
            DrawNebula("BgNebulaA", 3);
            DrawNebula("BgNebulaB", 8);

            DrawPlanet("BgPlanetA", Hex(0xC97B4A), Hex(0x8C4A2F), false, 1.3f, 7f);
            DrawPlanet("BgPlanetB", Hex(0x4A9BC9), Hex(0x2F5F8C), false, 4.1f, 4f);
            DrawPlanet("BgPlanetC", Hex(0xD9C58A), Hex(0xA8885A), true, 2.2f, 9f);
        }

        /// <summary>
        /// Soft clouds from layered Perlin noise, stored as white with varying transparency so the
        /// sprite can be tinted any colour. Tiles seamlessly top-to-bottom for endless scrolling.
        /// </summary>
        static void DrawNebula(string name, int seed)
        {
            const int size = 512;
            const float frequency = 2.4f;
            float offsetX = seed * 37.13f, offsetY = seed * 91.7f;

            // Pass 1: a density value for every pixel.
            var density = new float[size * size];
            double sum = 0.0;
            for (int y = 0; y < size; y++)
            {
                float v = y / (float)size;
                for (int x = 0; x < size; x++)
                {
                    float u = x / (float)size;
                    // Cross-fade the noise with a copy shifted by one tile so top and bottom match.
                    float a = Wisps(u * frequency + offsetX, v * frequency + offsetY);
                    float b = Wisps(u * frequency + offsetX, (v - 1f) * frequency + offsetY);
                    density[y * size + x] = Mathf.Lerp(a, b, v);
                    sum += density[y * size + x];
                }
            }

            // The cross-fade flattens the contrast towards the middle of the tile; stretching the
            // values back out around the average keeps the clouds looking the same everywhere.
            float mean = (float)(sum / density.Length);
            for (int y = 0; y < size; y++)
            {
                float v = y / (float)size;
                float restore = 1f / Mathf.Sqrt((1f - v) * (1f - v) + v * v);
                for (int x = 0; x < size; x++) density[y * size + x] = mean + (density[y * size + x] - mean) * restore;
            }

            // Noise values bunch up around the average, so pick the cut-off points from the actual
            // spread: the thinner 55% of the sky stays empty, and only the densest few percent is solid.
            var sorted = (float[])density.Clone();
            System.Array.Sort(sorted);
            float empty = sorted[(int)(sorted.Length * 0.55f)];
            float solid = sorted[(int)(sorted.Length * 0.985f)];

            // Pass 2: turn density into see-through white cloud.
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                float v = y / (float)size;
                for (int x = 0; x < size; x++)
                {
                    float u = x / (float)size;
                    float alpha = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(empty, solid, density[y * size + x]));
                    float tone = Mathf.Lerp(0.7f, 1f, Fbm(u * 6f + offsetY, v * 6f + offsetX));
                    pixels[y * size + x] = new Color(tone, tone, 1f, alpha);
                }
            }

            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.SetPixels(pixels);
            texture.Apply();
            // 512 pixels stretched over 48 world units: big, blurry and cheap, which is what clouds want.
            WritePng(name, texture, size / 48f, new Vector2(0.5f, 0.5f));
        }

        /// <summary>Noise pushed around by more noise ("domain warping"), which gives wispy, swirling shapes.</summary>
        static float Wisps(float x, float y)
        {
            float warpX = Fbm(x + 5.2f, y + 1.3f);
            float warpY = Fbm(x + 9.7f, y + 6.1f);
            return Fbm(x + warpX * 1.6f, y + warpY * 1.6f);
        }

        static float Fbm(float x, float y)
        {
            float sum = 0f, amplitude = 0.5f, total = 0f;
            for (int octave = 0; octave < 5; octave++)
            {
                sum += Mathf.PerlinNoise(x, y) * amplitude;
                total += amplitude;
                amplitude *= 0.5f;
                x *= 2f;
                y *= 2f;
            }
            return sum / total;
        }

        static void DrawPlanet(string name, Color bandA, Color bandB, bool rings, float seed, float bandFrequency)
        {
            float radius = rings ? 0.55f : 0.92f;
            var ringColor = new Color(0.86f, 0.8f, 0.66f, 0.75f);
            var c = new SpriteCanvas(256, 256, 2);

            if (rings) DrawRingHalf(c, ringColor, true);

            c.Paint(-radius, -radius, radius, radius, (x, y) =>
            {
                float nx = x / radius, ny = y / radius;
                float d2 = nx * nx + ny * ny;
                if (d2 > 1f) return new Color(0f, 0f, 0f, 0f);

                float nz = Mathf.Sqrt(1f - d2);
                float band = 0.5f + 0.5f * Mathf.Sin(ny * bandFrequency + Mathf.Sin(nx * 2.3f + seed) * 0.8f + seed);
                Color surface = Color.Lerp(bandA, bandB, band);

                // Lit from the upper left; the far side falls into shadow like a crescent.
                float light = 0.10f + 0.90f * Mathf.Clamp01(-0.55f * nx + 0.45f * ny + 0.70f * nz);
                return new Color(surface.r * light, surface.g * light, surface.b * light, 1f);
            });

            if (rings) DrawRingHalf(c, ringColor, false);
            Save(name, c);
        }

        /// <summary>Half of a flat ring: the back half is drawn before the planet, the front half after.</summary>
        static void DrawRingHalf(SpriteCanvas c, Color color, bool back)
        {
            const float outerX = 0.96f, outerY = 0.24f, innerX = 0.68f, innerY = 0.17f;
            c.Fill(color, -outerX, -outerY, outerX, outerY, (x, y) =>
            {
                if (back != y >= 0f) return 0f;
                float outer = (x * x) / (outerX * outerX) + (y * y) / (outerY * outerY);
                float inner = (x * x) / (innerX * innerX) + (y * y) / (innerY * innerY);
                return outer <= 1f && inner >= 1f ? 1f : 0f;
            });
        }

        // ---------------------------------------------------------------- interface

        static void DrawInterface()
        {
            var c = new SpriteCanvas(64, 64);
            c.Ring(Color.white, 0f, 0f, 0.60f, 0.50f);
            c.Rect(Color.white, -0.05f, 0.68f, 0.05f, 0.94f);
            c.Rect(Color.white, -0.05f, -0.94f, 0.05f, -0.68f);
            c.Rect(Color.white, 0.68f, -0.05f, 0.94f, 0.05f);
            c.Rect(Color.white, -0.94f, -0.05f, -0.68f, 0.05f);
            c.Circle(Color.white, 0f, 0f, 0.08f);
            c.Outline(Ink, 1f);
            Save("UiCrosshair", c);

            // Screen-edge vignette: clear in the middle, solid at the corners. Tinted red by the HUD.
            c = new SpriteCanvas(256, 256, 1);
            c.Fill(Color.white, -1f, -1f, 1f, 1f, (x, y) => Mathf.SmoothStep(0.45f, 1f, Mathf.Sqrt(x * x + y * y) / 1.4142f));
            Save("UiVignette", c);
        }
    }
}
