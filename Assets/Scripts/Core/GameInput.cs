using UnityEngine;
using UnityEngine.InputSystem;

namespace SpaceShooter
{
    /// <summary>
    /// The one place that talks to the Input System. Every other script reads these properties,
    /// so rebinding a key means changing a single line in <see cref="BuildActions"/>.
    /// </summary>
    [DefaultExecutionOrder(-250)]
    public sealed class GameInput : MonoBehaviour
    {
        public static GameInput Instance { get; private set; }

        InputAction move;
        InputAction fire;
        InputAction bomb;
        InputAction dash;
        InputAction pause;
        InputAction confirm;
        InputAction restart;
        InputAction nextWeapon;
        InputAction previousWeapon;
        InputAction toggleControls;
        InputAction toggleAutoFire;
        InputAction[] all;
        Camera cam;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => Instance = null;

        // ---------------------------------------------------------------- movement / actions

        /// <summary>WASD / arrow keys / left stick, never longer than 1.</summary>
        public Vector2 Move => Vector2.ClampMagnitude(move.ReadValue<Vector2>(), 1f);

        public bool FireHeld => fire.IsPressed();
        public bool BombPressed => bomb.WasPressedThisFrame();
        public bool DashPressed => dash.WasPressedThisFrame();
        public bool PausePressed => pause.WasPressedThisFrame();
        public bool ConfirmPressed => confirm.WasPressedThisFrame();
        public bool RestartPressed => restart.WasPressedThisFrame();
        public bool ToggleControlsPressed => toggleControls.WasPressedThisFrame();
        public bool ToggleAutoFirePressed => toggleAutoFire.WasPressedThisFrame();

        /// <summary>+1 = next weapon, -1 = previous weapon, 0 = no change (Q / E or the mouse wheel).</summary>
        public int WeaponCycle
        {
            get
            {
                int direction = 0;
                if (nextWeapon.WasPressedThisFrame()) direction++;
                if (previousWeapon.WasPressedThisFrame()) direction--;

                Mouse mouse = Mouse.current;
                if (mouse != null)
                {
                    float scroll = mouse.scroll.ReadValue().y;
                    if (scroll > 0.01f) direction--;
                    else if (scroll < -0.01f) direction++;
                }
                return Mathf.Clamp(direction, -1, 1);
            }
        }

        /// <summary>Index of the number key pressed this frame (key "1" = 0), or -1.</summary>
        public int WeaponSlotPressed
        {
            get
            {
                Keyboard keyboard = Keyboard.current;
                if (keyboard == null) return -1;
                for (int i = 0; i < 9; i++)
                {
                    if (keyboard[(Key)((int)Key.Digit1 + i)].wasPressedThisFrame) return i;
                }
                return -1;
            }
        }

        // ---------------------------------------------------------------- pointer

        public bool HasPointer => Mouse.current != null;

        public Vector2 PointerScreen
        {
            get
            {
                Mouse mouse = Mouse.current;
                return mouse != null ? mouse.position.ReadValue() : new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            }
        }

        /// <summary>How far the mouse moved this frame, in pixels.</summary>
        public Vector2 PointerDelta
        {
            get
            {
                Mouse mouse = Mouse.current;
                return mouse != null ? mouse.delta.ReadValue() : Vector2.zero;
            }
        }

        public Vector2 PointerWorld
        {
            get
            {
                if (cam == null) cam = Camera.main;
                if (cam == null) return Vector2.zero;
                Vector3 screen = PointerScreen;
                screen.z = -cam.transform.position.z;
                return cam.ScreenToWorldPoint(screen);
            }
        }

        // ---------------------------------------------------------------- setup

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
            BuildActions();
        }

        void BuildActions()
        {
            move = new InputAction("Move", InputActionType.Value);
            move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/rightArrow");
            move.AddBinding("<Gamepad>/leftStick");

            fire = Button("Fire", "<Mouse>/leftButton", "<Keyboard>/space", "<Gamepad>/rightTrigger");
            bomb = Button("Bomb", "<Mouse>/rightButton", "<Keyboard>/b", "<Gamepad>/buttonWest");
            dash = Button("Dash", "<Keyboard>/leftShift", "<Keyboard>/rightShift", "<Gamepad>/buttonSouth");
            pause = Button("Pause", "<Keyboard>/escape", "<Keyboard>/p", "<Gamepad>/start");
            confirm = Button("Confirm", "<Keyboard>/enter", "<Keyboard>/numpadEnter", "<Keyboard>/space",
                "<Mouse>/leftButton", "<Gamepad>/buttonSouth");
            restart = Button("Restart", "<Keyboard>/r");
            nextWeapon = Button("Next Weapon", "<Keyboard>/e", "<Gamepad>/rightShoulder");
            previousWeapon = Button("Previous Weapon", "<Keyboard>/q", "<Gamepad>/leftShoulder");
            toggleControls = Button("Toggle Controls", "<Keyboard>/tab");
            toggleAutoFire = Button("Toggle Auto Fire", "<Keyboard>/f");

            all = new[]
            {
                move, fire, bomb, dash, pause, confirm, restart,
                nextWeapon, previousWeapon, toggleControls, toggleAutoFire
            };
        }

        static InputAction Button(string name, params string[] bindings)
        {
            var action = new InputAction(name, InputActionType.Button);
            foreach (string binding in bindings) action.AddBinding(binding);
            return action;
        }

        void OnEnable()
        {
            if (all == null) return;
            foreach (InputAction action in all) action.Enable();
        }

        void OnDisable()
        {
            if (all == null) return;
            foreach (InputAction action in all) action.Disable();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (all == null) return;
            foreach (InputAction action in all) action.Dispose();
        }
    }
}
