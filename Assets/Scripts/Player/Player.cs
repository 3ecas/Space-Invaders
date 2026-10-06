using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// The front door to the player's ship. Other systems (enemies, pickups, the HUD) go through
    /// here to reach the ship's individual parts instead of searching for them.
    /// </summary>
    [DefaultExecutionOrder(-90)]
    public sealed class Player : MonoBehaviour
    {
        public static Player Instance { get; private set; }

        /// <summary>True while there is a living ship to aim at.</summary>
        public static bool Exists => Instance != null && Instance.IsAlive;

        /// <summary>Where the ship is - or was, if it has just been destroyed.</summary>
        public static Vector2 Position { get; private set; }

        [SerializeField] GameObject deathEffect;
        [SerializeField] float deathShake = 1f;

        public Health Health { get; private set; }
        public Armor Armor { get; private set; }
        public Shield Shield { get; private set; }
        public PlayerController Controller { get; private set; }
        public PlayerWeapons Weapons { get; private set; }
        public PlayerBombs Bombs { get; private set; }
        public PlayerDamageReceiver Damage { get; private set; }

        public bool IsAlive => Health != null && Health.IsAlive;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            Instance = null;
            Position = Vector2.zero;
        }

        void Awake()
        {
            Instance = this;
            Health = GetComponent<Health>();
            Armor = GetComponent<Armor>();
            Shield = GetComponent<Shield>();
            Controller = GetComponent<PlayerController>();
            Weapons = GetComponent<PlayerWeapons>();
            Bombs = GetComponent<PlayerBombs>();
            Damage = GetComponent<PlayerDamageReceiver>();
            Position = transform.position;
        }

        void OnEnable() => Health.Died += OnDied;

        void OnDisable() => Health.Died -= OnDied;

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void LateUpdate() => Position = transform.position;

        void OnDied()
        {
            Position = transform.position;

            if (deathEffect != null) PoolManager.Spawn(deathEffect, transform.position, Quaternion.identity);
            AudioManager.Play(SfxId.PlayerDeath);
            CameraShake.Shake(deathShake);
            GameEvents.RaiseScreenFlash(Color.white, 0.6f);
            GameEvents.RaisePlayerDied();

            gameObject.SetActive(false);
        }
    }
}
