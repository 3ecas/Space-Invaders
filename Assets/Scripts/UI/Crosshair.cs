using UnityEngine;
using UnityEngine.UI;

namespace SpaceShooter
{
    /// <summary>
    /// Replaces the hidden system cursor while playing. Shown when the mouse matters:
    /// in twin-stick mode (it is the aim point) and while the ship is following the mouse.
    /// Requires a Screen Space - Overlay canvas.
    /// </summary>
    [RequireComponent(typeof(Graphic))]
    public sealed class Crosshair : MonoBehaviour
    {
        [SerializeField] float spinSpeed = 45f;

        Graphic graphic;
        RectTransform rect;

        void Awake()
        {
            graphic = GetComponent<Graphic>();
            graphic.raycastTarget = false;
            rect = (RectTransform)transform;
        }

        void LateUpdate()
        {
            GameInput input = GameInput.Instance;
            Player player = Player.Instance;

            bool visible = GameManager.Playing && input != null && input.HasPointer && player != null && player.IsAlive &&
                           (player.Controller.Scheme == ControlScheme.TwinStick || player.Controller.MouseSteering);

            if (graphic.enabled != visible) graphic.enabled = visible;
            if (!visible) return;

            rect.position = input.PointerScreen;
            rect.Rotate(0f, 0f, spinSpeed * Time.unscaledDeltaTime);
        }
    }
}
