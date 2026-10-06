using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceShooter.EditorTools
{
    /// <summary>
    /// Builds the HUD canvas: status bars, score, weapon bar, banners and the title / pause /
    /// game over screens. Everything ends up as ordinary UI objects in the scene, so it can be
    /// restyled by hand afterwards.
    /// </summary>
    public static class HudFactory
    {
        static readonly Color TextColor = new Color(0.92f, 0.96f, 1f);
        static readonly Color DimColor = new Color(0.62f, 0.70f, 0.84f);
        static readonly Color Yellow = SpriteFactory.Hex(0xFDE047);
        static readonly Color PanelColor = new Color(0.03f, 0.05f, 0.11f, 0.78f);

        static readonly Vector2 TopLeft = new Vector2(0f, 1f);
        static readonly Vector2 TopCenter = new Vector2(0.5f, 1f);
        static readonly Vector2 TopRight = new Vector2(1f, 1f);
        static readonly Vector2 Center = new Vector2(0.5f, 0.5f);
        static readonly Vector2 BottomLeft = new Vector2(0f, 0f);
        static readonly Vector2 BottomCenter = new Vector2(0.5f, 0f);
        static readonly Vector2 BottomRight = new Vector2(1f, 0f);

        static Font font;
        static Sprite pixel;

        public static void Build(WaveSpawner waves)
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            pixel = SpriteFactory.Get("FxPixel");

            var canvasObject = new GameObject("HUD", typeof(RectTransform));
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            // Lay everything out for 1920x1080 and scale to whatever the real screen is.
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            Transform root = canvasObject.transform;

            // Order matters: later objects draw on top of earlier ones.
            Image vignette = Box("Damage Vignette", root, Color.clear);
            vignette.sprite = SpriteFactory.Get("UiVignette");
            Stretch(vignette.rectTransform);
            vignette.gameObject.AddComponent<DamageVignette>();

            RectTransform hud = Stretch(Rect("In-Game HUD", root));
            var controller = hud.gameObject.AddComponent<HudController>();
            EditorUtil.Set(controller, "waves", waves);

            BuildStatus(hud, controller);
            BuildScore(hud, controller);
            BuildWeapons(hud, controller);
            BuildAbilities(hud, controller);

            Text waveText = Label("Wave", hud, "", 26, TextAnchor.UpperCenter, DimColor);
            Place(waveText.rectTransform, TopCenter, new Vector2(0f, -26f), new Vector2(400f, 40f));
            EditorUtil.Set(controller, "waveText", waveText);

            BuildFloatingText(root);
            BuildBossBar(root);
            BuildBanner(root);
            BuildNotice(root);

            Image flash = Box("Screen Flash", root, new Color(1f, 1f, 1f, 0f));
            Stretch(flash.rectTransform);
            flash.gameObject.AddComponent<ScreenFlash>();

            var screens = canvasObject.AddComponent<GameScreens>();
            EditorUtil.SetMany(screens, "hudRoot", hud.gameObject, "waves", waves);
            Graphic titlePrompt = BuildTitle(root, screens);
            BuildPause(root, screens);
            Graphic gameOverPrompt = BuildGameOver(root, screens);
            EditorUtil.Set(screens, "blinkers", new[] { titlePrompt, gameOverPrompt });

            Image crosshair = Box("Crosshair", root, new Color(1f, 1f, 1f, 0.9f));
            crosshair.sprite = SpriteFactory.Get("UiCrosshair");
            Place(crosshair.rectTransform, Center, Vector2.zero, new Vector2(48f, 48f));
            crosshair.gameObject.AddComponent<Crosshair>();
        }

        // ---------------------------------------------------------------- in-game HUD

        static void BuildStatus(RectTransform hud, HudController controller)
        {
            const float width = 340f;
            RectTransform panel = Rect("Ship Status", hud);
            Place(panel, TopLeft, new Vector2(40f, -28f), new Vector2(width, 170f));

            Text hullLabel = Label("Hull Label", panel, "HULL", 20, TextAnchor.UpperLeft, DimColor);
            Place(hullLabel.rectTransform, TopLeft, new Vector2(0f, 0f), new Vector2(200f, 26f));
            Text hullValue = Label("Hull Value", panel, "100", 20, TextAnchor.UpperRight, TextColor);
            Place(hullValue.rectTransform, TopRight, new Vector2(0f, 0f), new Vector2(120f, 26f));

            UiBar hull = Bar("Hull Bar", panel, SpriteFactory.Hex(0x4ADE80), true);
            Place((RectTransform)hull.transform, TopLeft, new Vector2(0f, -28f), new Vector2(width, 22f));
            EditorUtil.SetMany(hull, "changeColor", true, "fullColor", SpriteFactory.Hex(0x4ADE80), "emptyColor", SpriteFactory.Hex(0xEF4444));

            Text armorLabel = Label("Armor Label", panel, "ARMOR", 17, TextAnchor.UpperLeft, DimColor);
            Place(armorLabel.rectTransform, TopLeft, new Vector2(0f, -58f), new Vector2(200f, 24f));
            Text armorValue = Label("Armor Value", panel, "0", 17, TextAnchor.UpperRight, TextColor);
            Place(armorValue.rectTransform, TopRight, new Vector2(0f, -58f), new Vector2(120f, 24f));

            UiBar armor = Bar("Armor Bar", panel, SpriteFactory.Hex(0xA9B8CF), true);
            Place((RectTransform)armor.transform, TopLeft, new Vector2(0f, -82f), new Vector2(width, 13f));

            // The shield row only shows while a shield is active.
            RectTransform shieldRow = Rect("Shield Row", panel);
            Place(shieldRow, TopLeft, new Vector2(0f, -104f), new Vector2(width, 36f));
            Text shieldLabel = Label("Shield Label", shieldRow, "SHIELD", 17, TextAnchor.UpperLeft, SpriteFactory.Hex(0x7DD3FC));
            Place(shieldLabel.rectTransform, TopLeft, new Vector2(0f, 0f), new Vector2(200f, 24f));
            UiBar shield = Bar("Shield Bar", shieldRow, SpriteFactory.Hex(0x7DD3FC), false);
            Place((RectTransform)shield.transform, TopLeft, new Vector2(0f, -24f), new Vector2(width, 10f));
            shieldRow.gameObject.SetActive(false);

            EditorUtil.SetMany(controller,
                "healthBar", hull, "healthText", hullValue,
                "armorBar", armor, "armorText", armorValue,
                "shieldBar", shield, "shieldRoot", shieldRow.gameObject);
        }

        static void BuildScore(RectTransform hud, HudController controller)
        {
            RectTransform panel = Rect("Score", hud);
            Place(panel, TopRight, new Vector2(-40f, -22f), new Vector2(520f, 180f));

            Text score = Label("Score Value", panel, "0", 54, TextAnchor.UpperRight, TextColor);
            Place(score.rectTransform, TopRight, new Vector2(0f, 0f), new Vector2(520f, 64f));

            Text best = Label("Best", panel, "BEST 0", 19, TextAnchor.UpperRight, DimColor);
            Place(best.rectTransform, TopRight, new Vector2(0f, -64f), new Vector2(520f, 26f));

            Text multiplier = Label("Multiplier", panel, "x2", 38, TextAnchor.UpperRight, Yellow);
            Place(multiplier.rectTransform, TopRight, new Vector2(0f, -92f), new Vector2(200f, 46f));
            multiplier.enabled = false;

            UiBar combo = Bar("Combo Bar", panel, Yellow, false);
            Place((RectTransform)combo.transform, TopRight, new Vector2(0f, -140f), new Vector2(180f, 7f));

            EditorUtil.SetMany(controller, "scoreText", score, "highScoreText", best, "multiplierText", multiplier, "comboBar", combo);
        }

        static void BuildWeapons(RectTransform hud, HudController controller)
        {
            RectTransform panel = Rect("Weapons", hud);
            Place(panel, BottomLeft, new Vector2(40f, 36f), new Vector2(620f, 170f));

            RectTransform row = Rect("Slots", panel);
            Place(row, BottomLeft, Vector2.zero, new Vector2(620f, 76f));
            var layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 12f;
            layout.childAlignment = TextAnchor.LowerLeft;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            // One slot, kept disabled as the template that WeaponHud copies for every weapon.
            RectTransform slot = Rect("Slot Template", row);
            slot.sizeDelta = new Vector2(72f, 72f);
            Image frame = slot.gameObject.AddComponent<Image>();
            frame.sprite = pixel;
            frame.raycastTarget = false;

            Image inner = Box("Background", slot, PanelColor);
            Stretch(inner.rectTransform, 3f);

            Image icon = Box("Icon", slot, Color.white);
            icon.preserveAspect = true;
            Stretch(icon.rectTransform, 13f);

            Text key = Label("Key", slot, "1", 15, TextAnchor.UpperLeft, DimColor);
            Place(key.rectTransform, TopLeft, new Vector2(7f, -4f), new Vector2(30f, 20f));

            Text ammo = Label("Ammo", slot, "", 16, TextAnchor.LowerRight, TextColor);
            Place(ammo.rectTransform, BottomRight, new Vector2(-6f, 4f), new Vector2(60f, 20f));

            var view = slot.gameObject.AddComponent<WeaponSlotView>();
            EditorUtil.SetMany(view, "frame", frame, "icon", icon, "keyLabel", key, "ammoLabel", ammo);
            slot.gameObject.SetActive(false);

            var weaponHud = panel.gameObject.AddComponent<WeaponHud>();
            EditorUtil.SetMany(weaponHud, "slotTemplate", view, "container", row);

            Text weaponName = Label("Weapon Name", panel, "BLASTER", 26, TextAnchor.LowerLeft, TextColor);
            Place(weaponName.rectTransform, BottomLeft, new Vector2(0f, 88f), new Vector2(400f, 32f));

            Text power = Label("Power", panel, "POWER  1 / 5", 19, TextAnchor.LowerLeft, Yellow);
            Place(power.rectTransform, BottomLeft, new Vector2(0f, 122f), new Vector2(400f, 26f));

            EditorUtil.SetMany(controller, "weaponNameText", weaponName, "powerText", power);
        }

        static void BuildAbilities(RectTransform hud, HudController controller)
        {
            RectTransform panel = Rect("Abilities", hud);
            Place(panel, BottomRight, new Vector2(-40f, 36f), new Vector2(420f, 180f));

            Text bombs = Label("Bombs", panel, "BOMBS  2", 28, TextAnchor.LowerRight, SpriteFactory.Hex(0xFB923C));
            Place(bombs.rectTransform, BottomRight, new Vector2(0f, 0f), new Vector2(420f, 36f));

            UiBar dash = Bar("Dash Bar", panel, SpriteFactory.Hex(0x7DE3FF), false);
            Place((RectTransform)dash.transform, BottomRight, new Vector2(0f, 46f), new Vector2(180f, 8f));
            Text dashLabel = Label("Dash Label", panel, "DASH", 15, TextAnchor.LowerRight, DimColor);
            Place(dashLabel.rectTransform, BottomRight, new Vector2(-190f, 40f), new Vector2(120f, 20f));

            Text overdrive = Label("Overdrive", panel, "OVERDRIVE  0.0", 26, TextAnchor.LowerRight, SpriteFactory.Hex(0xFB7185));
            Place(overdrive.rectTransform, BottomRight, new Vector2(0f, 66f), new Vector2(420f, 34f));
            overdrive.enabled = false;

            Text controls = Label("Controls", panel, "CLASSIC CONTROLS  [TAB]", 14, TextAnchor.LowerRight, new Color(0.62f, 0.70f, 0.84f, 0.6f));
            Place(controls.rectTransform, BottomRight, new Vector2(0f, 106f), new Vector2(420f, 20f));

            EditorUtil.SetMany(controller, "bombText", bombs, "dashBar", dash, "overdriveText", overdrive, "controlsText", controls);
        }

        // ---------------------------------------------------------------- overlays

        static void BuildFloatingText(Transform root)
        {
            RectTransform holder = Stretch(Rect("Floating Text", root));

            Text template = Label("Template", holder, "+100", 26, TextAnchor.MiddleCenter, Yellow);
            Place(template.rectTransform, Center, Vector2.zero, new Vector2(320f, 40f));
            template.gameObject.SetActive(false);

            var manager = holder.gameObject.AddComponent<FloatingTextManager>();
            EditorUtil.Set(manager, "template", template);
        }

        static void BuildBossBar(Transform root)
        {
            RectTransform holder = Rect("Boss Bar", root);
            Place(holder, TopCenter, new Vector2(0f, -66f), new Vector2(760f, 60f));

            RectTransform content = Stretch(Rect("Content", holder));

            Text bossName = Label("Name", content, "DREADNOUGHT", 20, TextAnchor.UpperCenter, SpriteFactory.Hex(0xFF6B81));
            Place(bossName.rectTransform, TopCenter, Vector2.zero, new Vector2(760f, 26f));

            UiBar bar = Bar("Health", content, SpriteFactory.Hex(0xF43F5E), true);
            Place((RectTransform)bar.transform, TopCenter, new Vector2(0f, -30f), new Vector2(760f, 16f));

            var bossBar = holder.gameObject.AddComponent<BossHealthBar>();
            EditorUtil.SetMany(bossBar, "root", content.gameObject, "bar", bar, "nameText", bossName);
        }

        static void BuildBanner(Transform root)
        {
            RectTransform banner = Rect("Banner", root);
            Place(banner, Center, new Vector2(0f, 190f), new Vector2(1400f, 220f));
            var group = banner.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;

            Text title = Label("Title", banner, "WAVE 1", 92, TextAnchor.MiddleCenter, TextColor);
            Place(title.rectTransform, Center, new Vector2(0f, 20f), new Vector2(1400f, 110f));

            Text subtitle = Label("Subtitle", banner, "", 30, TextAnchor.MiddleCenter, TextColor);
            Place(subtitle.rectTransform, Center, new Vector2(0f, -60f), new Vector2(1400f, 40f));

            var waveBanner = banner.gameObject.AddComponent<WaveBanner>();
            EditorUtil.SetMany(waveBanner, "group", group, "title", title, "subtitle", subtitle);
        }

        static void BuildNotice(Transform root)
        {
            Text notice = Label("Notice", root, "", 28, TextAnchor.MiddleCenter, TextColor);
            Place(notice.rectTransform, TopCenter, new Vector2(0f, -130f), new Vector2(1000f, 80f));
            var component = notice.gameObject.AddComponent<NoticeText>();
            EditorUtil.Set(component, "label", notice);
        }

        // ---------------------------------------------------------------- screens

        static Graphic BuildTitle(Transform root, GameScreens screens)
        {
            RectTransform panel = Screen("Title Screen", root, 0.4f);

            Text title = Label("Title", panel, PlayerSettings.productName.ToUpperInvariant(), 124, TextAnchor.MiddleCenter, TextColor);
            Place(title.rectTransform, Center, new Vector2(0f, 250f), new Vector2(1800f, 150f));

            Text tagline = Label("Tagline", panel, "ENDLESS WAVES   -   BIG GUNS   -   BIGGER BOSSES", 28, TextAnchor.MiddleCenter, SpriteFactory.Hex(0x7DE3FF));
            Place(tagline.rectTransform, Center, new Vector2(0f, 150f), new Vector2(1800f, 40f));

            const string help =
                "MOVE      WASD  or  MOUSE\n" +
                "FIRE      LEFT CLICK  or  SPACE      (F toggles auto-fire)\n" +
                "BOMB      RIGHT CLICK  or  B\n" +
                "DASH      SHIFT\n" +
                "WEAPONS      1 - 6,   Q / E   or   MOUSE WHEEL\n" +
                "TAB      switch to twin-stick controls  (WASD moves, mouse aims)";
            Text controls = Label("Controls", panel, help, 25, TextAnchor.MiddleCenter, DimColor);
            controls.lineSpacing = 1.25f;
            Place(controls.rectTransform, Center, new Vector2(0f, -40f), new Vector2(1500f, 280f));

            Text prompt = Label("Prompt", panel, "CLICK  or  PRESS ENTER  TO START", 38, TextAnchor.MiddleCenter, Yellow);
            Place(prompt.rectTransform, Center, new Vector2(0f, -300f), new Vector2(1500f, 60f));

            EditorUtil.Set(screens, "titlePanel", panel.gameObject);
            return prompt;
        }

        static void BuildPause(Transform root, GameScreens screens)
        {
            RectTransform panel = Screen("Pause Screen", root, 0.6f);

            Text title = Label("Title", panel, "PAUSED", 110, TextAnchor.MiddleCenter, TextColor);
            Place(title.rectTransform, Center, new Vector2(0f, 50f), new Vector2(1400f, 140f));

            Text hint = Label("Hint", panel, "ESC  resume          R  restart", 30, TextAnchor.MiddleCenter, DimColor);
            Place(hint.rectTransform, Center, new Vector2(0f, -60f), new Vector2(1400f, 50f));

            panel.gameObject.SetActive(false);
            EditorUtil.Set(screens, "pausePanel", panel.gameObject);
        }

        static Graphic BuildGameOver(Transform root, GameScreens screens)
        {
            RectTransform panel = Screen("Game Over Screen", root, 0.62f);

            Text title = Label("Title", panel, "GAME OVER", 124, TextAnchor.MiddleCenter, SpriteFactory.Hex(0xF43F5E));
            Place(title.rectTransform, Center, new Vector2(0f, 240f), new Vector2(1600f, 150f));

            Text scoreLabel = Label("Score Label", panel, "FINAL SCORE", 26, TextAnchor.MiddleCenter, DimColor);
            Place(scoreLabel.rectTransform, Center, new Vector2(0f, 120f), new Vector2(800f, 40f));

            Text score = Label("Score", panel, "0", 92, TextAnchor.MiddleCenter, TextColor);
            Place(score.rectTransform, Center, new Vector2(0f, 45f), new Vector2(1200f, 110f));

            Text best = Label("Best", panel, "BEST  0", 32, TextAnchor.MiddleCenter, Yellow);
            Place(best.rectTransform, Center, new Vector2(0f, -45f), new Vector2(1200f, 44f));

            Text summary = Label("Summary", panel, "", 26, TextAnchor.MiddleCenter, DimColor);
            Place(summary.rectTransform, Center, new Vector2(0f, -100f), new Vector2(1200f, 40f));

            Text prompt = Label("Prompt", panel, "CLICK  or  PRESS ENTER  TO PLAY AGAIN", 34, TextAnchor.MiddleCenter, Yellow);
            Place(prompt.rectTransform, Center, new Vector2(0f, -260f), new Vector2(1500f, 60f));

            panel.gameObject.SetActive(false);
            EditorUtil.SetMany(screens, "gameOverPanel", panel.gameObject,
                "finalScoreText", score, "bestScoreText", best, "summaryText", summary);
            return prompt;
        }

        /// <summary>A full-screen panel with a dark see-through backdrop.</summary>
        static RectTransform Screen(string name, Transform root, float dim)
        {
            RectTransform panel = Stretch(Rect(name, root));
            Image backdrop = Box("Backdrop", panel, new Color(0.01f, 0.02f, 0.06f, dim));
            Stretch(backdrop.rectTransform);
            return panel;
        }

        // ---------------------------------------------------------------- building blocks

        static RectTransform Rect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = LayerMask.NameToLayer("UI");
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            return rect;
        }

        /// <summary>Makes a rect fill its parent, optionally inset by a margin.</summary>
        static RectTransform Stretch(RectTransform rect, float inset = 0f)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = new Vector2(inset, inset);
            rect.offsetMax = new Vector2(-inset, -inset);
            return rect;
        }

        /// <summary>Pins a rect to a corner or edge of its parent. The pivot matches the anchor.</summary>
        static void Place(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        static Text Label(string name, Transform parent, string content, int size, TextAnchor alignment, Color color)
        {
            RectTransform rect = Rect(name, parent);

            var text = rect.gameObject.AddComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.fontStyle = FontStyle.Bold;
            text.alignment = alignment;
            text.color = color;
            text.text = content;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.supportRichText = false;
            text.raycastTarget = false;

            // A dark outline keeps text readable over explosions and bright backgrounds.
            var outline = rect.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.8f);
            outline.effectDistance = size >= 40 ? new Vector2(3f, -3f) : new Vector2(1.5f, -1.5f);
            return text;
        }

        static Image Box(string name, Transform parent, Color color)
        {
            RectTransform rect = Rect(name, parent);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = pixel;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        /// <summary>A bar made of a dark backing, an optional lagging "trail" and the coloured fill.</summary>
        static UiBar Bar(string name, Transform parent, Color fillColor, bool withTrail)
        {
            Image backing = Box(name, parent, new Color(0f, 0f, 0f, 0.6f));

            RectTransform area = Stretch(Rect("Fill Area", backing.transform), 2f);

            RectTransform trail = null;
            if (withTrail)
            {
                Image trailImage = Box("Trail", area, new Color(1f, 1f, 1f, 0.75f));
                trail = Stretch(trailImage.rectTransform);
            }

            Image fillImage = Box("Fill", area, fillColor);
            RectTransform fill = Stretch(fillImage.rectTransform);

            var bar = backing.gameObject.AddComponent<UiBar>();
            EditorUtil.SetMany(bar, "fill", fill, "fillGraphic", fillImage, "trail", trail);
            return bar;
        }
    }
}
