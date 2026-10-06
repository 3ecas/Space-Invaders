using UnityEngine;
using UnityEngine.UI;

namespace SpaceShooter
{
    /// <summary>
    /// Keeps the on-screen readouts in step with the game. It simply looks at the player and the
    /// managers every frame; text is only rebuilt when the number behind it actually changes.
    /// </summary>
    public sealed class HudController : MonoBehaviour
    {
        [Header("Ship status")]
        [SerializeField] UiBar healthBar;
        [SerializeField] UiBar armorBar;
        [SerializeField] UiBar shieldBar;
        [SerializeField] GameObject shieldRoot;
        [SerializeField] Text healthText;
        [SerializeField] Text armorText;

        [Header("Score")]
        [SerializeField] Text scoreText;
        [SerializeField] Text highScoreText;
        [SerializeField] Text multiplierText;
        [SerializeField] UiBar comboBar;

        [Header("Wave")]
        [SerializeField] WaveSpawner waves;
        [SerializeField] Text waveText;

        [Header("Weapons & abilities")]
        [SerializeField] Text weaponNameText;
        [SerializeField] Text powerText;
        [SerializeField] Text bombText;
        [SerializeField] Text overdriveText;
        [SerializeField] UiBar dashBar;
        [SerializeField] Text controlsText;

        int shownScore = -1;
        int shownHighScore = -1;
        int shownMultiplier = -1;
        int shownWave = -1;
        int shownHealth = -1;
        int shownArmor = -1;
        int shownBombs = -1;
        int shownPower = -1;
        int shownWeapon = -1;
        int shownOverdrive = -1;
        int shownControls = -1;

        void Update()
        {
            UpdateScore();
            UpdateWave();

            Player player = Player.Instance;
            if (player == null) return;

            UpdateStatus(player);
            UpdateWeapons(player);
        }

        void UpdateScore()
        {
            ScoreManager score = ScoreManager.Instance;
            if (score == null) return;

            if (score.Score != shownScore)
            {
                shownScore = score.Score;
                if (scoreText != null) scoreText.text = shownScore.ToString("N0");
            }

            if (score.HighScore != shownHighScore)
            {
                shownHighScore = score.HighScore;
                if (highScoreText != null) highScoreText.text = "BEST " + shownHighScore.ToString("N0");
            }

            if (score.Multiplier != shownMultiplier)
            {
                shownMultiplier = score.Multiplier;
                if (multiplierText != null)
                {
                    multiplierText.text = "x" + shownMultiplier;
                    multiplierText.enabled = shownMultiplier > 1;
                }
            }

            if (comboBar != null) comboBar.Set(score.Multiplier > 1 ? score.ComboTime01 : 0f);
        }

        void UpdateWave()
        {
            if (waves == null || waveText == null || waves.Wave == shownWave) return;
            shownWave = waves.Wave;
            waveText.text = shownWave > 0 ? "WAVE " + shownWave : "";
        }

        void UpdateStatus(Player player)
        {
            if (healthBar != null) healthBar.Set(player.Health.Normalized);
            if (armorBar != null) armorBar.Set(player.Armor.Normalized);

            int health = Mathf.CeilToInt(player.Health.Current);
            if (health != shownHealth)
            {
                shownHealth = health;
                if (healthText != null) healthText.text = health.ToString();
            }

            int armor = Mathf.CeilToInt(player.Armor.Current);
            if (armor != shownArmor)
            {
                shownArmor = armor;
                if (armorText != null) armorText.text = armor.ToString();
            }

            bool shielded = player.Shield.IsActive;
            if (shieldRoot != null && shieldRoot.activeSelf != shielded) shieldRoot.SetActive(shielded);
            if (shielded && shieldBar != null) shieldBar.Set(player.Shield.Normalized);

            if (dashBar != null) dashBar.Set(player.Controller.DashReady01);

            int bombs = player.Bombs.Count;
            if (bombs != shownBombs)
            {
                shownBombs = bombs;
                if (bombText != null) bombText.text = "BOMBS  " + bombs;
            }

            int controls = (int)player.Controller.Scheme;
            if (controls != shownControls)
            {
                shownControls = controls;
                if (controlsText != null)
                {
                    controlsText.text = player.Controller.Scheme == ControlScheme.Classic
                        ? "CLASSIC CONTROLS  [TAB]"
                        : "TWIN-STICK CONTROLS  [TAB]";
                }
            }
        }

        void UpdateWeapons(Player player)
        {
            PlayerWeapons weapons = player.Weapons;

            if (weapons.CurrentIndex != shownWeapon && weapons.Current != null)
            {
                shownWeapon = weapons.CurrentIndex;
                if (weaponNameText != null)
                {
                    weaponNameText.text = weapons.Current.Data.displayName.ToUpperInvariant();
                    weaponNameText.color = weapons.Current.Data.color;
                }
            }

            if (weapons.PowerLevel != shownPower)
            {
                shownPower = weapons.PowerLevel;
                if (powerText != null)
                {
                    powerText.text = weapons.IsMaxPower ? "POWER  MAX" : "POWER  " + shownPower + " / " + weapons.MaxPowerLevel;
                }
            }

            // Tenths of a second, so the text only changes ten times a second.
            int overdrive = weapons.OverdriveActive ? Mathf.CeilToInt(weapons.OverdriveTimeLeft * 10f) : 0;
            if (overdrive != shownOverdrive)
            {
                shownOverdrive = overdrive;
                if (overdriveText != null)
                {
                    overdriveText.enabled = overdrive > 0;
                    if (overdrive > 0) overdriveText.text = "OVERDRIVE  " + (overdrive / 10f).ToString("0.0");
                }
            }
        }
    }
}
