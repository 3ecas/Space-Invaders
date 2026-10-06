using UnityEngine;

namespace SpaceShooter
{
    public enum ControlScheme
    {
        /// <summary>The ship follows the mouse or WASD and always fires straight ahead.</summary>
        Classic = 0,
        /// <summary>WASD moves the ship while it turns to aim at the mouse cursor.</summary>
        TwinStick = 1
    }

    /// <summary>Moves the ship, aims it and handles the dash.</summary>
    public sealed class PlayerController : MonoBehaviour
    {
        const string SchemePrefKey = "SpaceShooter.ControlScheme";

        [Header("Movement")]
        [SerializeField] float moveSpeed = 13f;
        [SerializeField] float acceleration = 110f;
        [Tooltip("Top speed while the ship is chasing the mouse cursor.")]
        [SerializeField] float mouseFollowSpeed = 26f;
        [Tooltip("Higher = the ship snaps to the cursor more eagerly.")]
        [SerializeField] float mouseFollowSharpness = 14f;
        [Tooltip("Gap kept between the ship and the screen edges.")]
        [SerializeField] float edgePadding = 0.8f;

        [Header("Controls")]
        [SerializeField] ControlScheme scheme = ControlScheme.Classic;
        [Tooltip("Pixels the mouse must travel before it takes over steering from the keyboard.")]
        [SerializeField] float mouseTakeoverPixels = 30f;
        [Tooltip("Degrees per second the ship turns towards the cursor in twin-stick mode.")]
        [SerializeField] float turnSpeed = 1080f;

        [Header("Dash")]
        [SerializeField] float dashSpeed = 38f;
        [SerializeField] float dashDuration = 0.16f;
        [SerializeField] float dashCooldown = 1.1f;
        [SerializeField] GameObject dashEffect;

        [Header("Visuals")]
        [Tooltip("Child holding the ship sprite. It tilts when the ship strafes.")]
        [SerializeField] Transform visual;
        [SerializeField] float bankAngle = 14f;
        [SerializeField, Range(0f, 0.5f)] float bankSquash = 0.18f;

        public Vector2 Velocity { get; private set; }
        public Vector2 AimDirection { get; private set; } = Vector2.up;
        public ControlScheme Scheme => scheme;
        public float MaxSpeed => moveSpeed;
        public bool IsDashing => dashTimeLeft > 0f;

        /// <summary>True while the ship is following the mouse cursor (classic controls only).</summary>
        public bool MouseSteering { get; private set; }

        /// <summary>1 when the dash is ready, 0 right after using it.</summary>
        public float DashReady01 => dashCooldown > 0f ? 1f - Mathf.Clamp01(dashCooldownLeft / dashCooldown) : 1f;

        Vector2 dashDirection;
        float dashTimeLeft;
        float dashCooldownLeft;
        float mouseTravel;
        float mouseQuietTime;
        float bank;

        void Awake()
        {
            if (PlayerPrefs.HasKey(SchemePrefKey)) scheme = (ControlScheme)PlayerPrefs.GetInt(SchemePrefKey);
        }

        void Update()
        {
            float dt = Time.deltaTime;
            GameInput input = GameInput.Instance;
            GameManager game = GameManager.Instance;

            if (input != null && game != null && game.State != GameState.GameOver && input.ToggleControlsPressed)
            {
                ToggleScheme();
            }

            if (input == null || !GameManager.Playing || dt <= 0f)
            {
                Velocity = Vector2.zero;
                UpdateVisual(dt);
                return;
            }

            if (dashCooldownLeft > 0f) dashCooldownLeft -= dt;

            Vector2 keys = input.Move;
            UpdateSteeringSource(input, keys);

            Vector2 position = transform.position;

            if (!IsDashing && dashCooldownLeft <= 0f && input.DashPressed) StartDash(input, keys, position);

            if (IsDashing)
            {
                dashTimeLeft -= dt;
                Velocity = dashDirection * dashSpeed;
                position += Velocity * dt;
            }
            else if (scheme == ControlScheme.Classic && MouseSteering)
            {
                // Ease towards the cursor, but never faster than the follow speed.
                Vector2 target = PlayArea.Clamp(input.PointerWorld, edgePadding);
                Vector2 eased = Vector2.Lerp(position, target, GameMath.Smoothing(mouseFollowSharpness, dt));
                Vector2 step = Vector2.ClampMagnitude(eased - position, mouseFollowSpeed * dt);
                Velocity = step / dt;
                position += step;
            }
            else
            {
                Velocity = Vector2.MoveTowards(Velocity, keys * moveSpeed, acceleration * dt);
                position += Velocity * dt;
            }

            Vector2 clamped = PlayArea.Clamp(position, edgePadding);
            if (!Mathf.Approximately(clamped.x, position.x)) Velocity = new Vector2(0f, Velocity.y);
            if (!Mathf.Approximately(clamped.y, position.y)) Velocity = new Vector2(Velocity.x, 0f);
            transform.position = clamped;

            UpdateAim(input, dt);
            UpdateVisual(dt);
        }

        public void ToggleScheme()
        {
            scheme = scheme == ControlScheme.Classic ? ControlScheme.TwinStick : ControlScheme.Classic;
            MouseSteering = false;
            mouseTravel = 0f;

            PlayerPrefs.SetInt(SchemePrefKey, (int)scheme);
            GameEvents.RaiseAnnouncement(scheme == ControlScheme.Classic
                ? "CLASSIC CONTROLS\nmouse or WASD steers"
                : "TWIN-STICK CONTROLS\nWASD moves, mouse aims");
        }

        /// <summary>Works out whether the keyboard or the mouse is steering right now (classic controls).</summary>
        void UpdateSteeringSource(GameInput input, Vector2 keys)
        {
            if (scheme != ControlScheme.Classic)
            {
                MouseSteering = false;
                return;
            }

            // Any movement key hands control to the keyboard immediately.
            if (keys.sqrMagnitude > 0.01f)
            {
                MouseSteering = false;
                mouseTravel = 0f;
                return;
            }

            if (MouseSteering || !input.HasPointer) return;

            // The mouse only takes over after a deliberate movement, so the small jitter from
            // clicking the fire button doesn't yank the ship away from a keyboard player.
            float moved = input.PointerDelta.magnitude;
            if (moved > 0.5f)
            {
                mouseTravel += moved;
                mouseQuietTime = 0f;
            }
            else
            {
                mouseQuietTime += Time.unscaledDeltaTime;
                if (mouseQuietTime > 0.25f) mouseTravel = 0f;
            }

            if (mouseTravel >= mouseTakeoverPixels)
            {
                MouseSteering = true;
                mouseTravel = 0f;
            }
        }

        void StartDash(GameInput input, Vector2 keys, Vector2 position)
        {
            Vector2 direction = keys;
            if (direction.sqrMagnitude < 0.01f && scheme == ControlScheme.Classic && MouseSteering)
            {
                Vector2 toCursor = input.PointerWorld - position;
                if (toCursor.sqrMagnitude > 0.25f) direction = toCursor;
            }
            if (direction.sqrMagnitude < 0.01f) direction = Velocity;
            if (direction.sqrMagnitude < 0.01f) direction = Vector2.up;

            dashDirection = direction.normalized;
            dashTimeLeft = dashDuration;
            dashCooldownLeft = dashCooldown;

            AudioManager.Play(SfxId.Dash);
            if (dashEffect != null) PoolManager.Spawn(dashEffect, position, Quaternion.identity);
        }

        void UpdateAim(GameInput input, float dt)
        {
            Quaternion targetRotation = Quaternion.identity;

            if (scheme == ControlScheme.TwinStick)
            {
                Vector2 toCursor = input.PointerWorld - (Vector2)transform.position;
                if (toCursor.sqrMagnitude > 0.04f) targetRotation = GameMath.FaceUp(toCursor);
                else targetRotation = transform.rotation;
            }

            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * dt);
            AimDirection = transform.up;
        }

        void UpdateVisual(float dt)
        {
            if (visual == null) return;

            // Lean into sideways movement. In twin-stick mode the ship already rotates, so stay level.
            float targetBank = scheme == ControlScheme.Classic ? Mathf.Clamp(Velocity.x / moveSpeed, -1f, 1f) : 0f;
            bank = Mathf.Lerp(bank, targetBank, GameMath.Smoothing(12f, dt));

            visual.localRotation = Quaternion.Euler(0f, 0f, -bank * bankAngle);
            visual.localScale = new Vector3(1f - Mathf.Abs(bank) * bankSquash, 1f, 1f);
        }
    }
}
