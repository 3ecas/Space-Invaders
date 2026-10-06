using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// The world-space rectangle the camera can see. Anything that needs to know where the screen
    /// edges are (spawning, clamping the player, culling bullets) asks this class.
    /// Lives on the main camera.
    /// </summary>
    [DefaultExecutionOrder(-200)]
    [RequireComponent(typeof(Camera))]
    public sealed class PlayArea : MonoBehaviour
    {
        public static Vector2 Center { get; private set; }
        public static Vector2 HalfSize { get; private set; } = new Vector2(16f, 9f);

        public static float Left => Center.x - HalfSize.x;
        public static float Right => Center.x + HalfSize.x;
        public static float Bottom => Center.y - HalfSize.y;
        public static float Top => Center.y + HalfSize.y;
        public static float Width => HalfSize.x * 2f;
        public static float Height => HalfSize.y * 2f;

        Camera cam;

        void Awake()
        {
            cam = GetComponent<Camera>();
            // Captured once so camera shake doesn't drag the play area around with it.
            Center = transform.position;
            Recalculate();
        }

        void Update() => Recalculate();

        void Recalculate()
        {
            float halfHeight = cam.orthographicSize;
            HalfSize = new Vector2(halfHeight * cam.aspect, halfHeight);
        }

        /// <summary>True when the point is in view. A positive margin grows the area, a negative one shrinks it.</summary>
        public static bool Contains(Vector2 point, float margin = 0f)
        {
            return point.x >= Left - margin && point.x <= Right + margin &&
                   point.y >= Bottom - margin && point.y <= Top + margin;
        }

        /// <summary>Keeps a point inside the view, staying <paramref name="padding"/> units away from the edges.</summary>
        public static Vector2 Clamp(Vector2 point, float padding = 0f)
        {
            point.x = Mathf.Clamp(point.x, Left + padding, Right - padding);
            point.y = Mathf.Clamp(point.y, Bottom + padding, Top - padding);
            return point;
        }

        /// <summary>Converts (-1..1, -1..1) screen-relative coordinates to a world position.</summary>
        public static Vector2 FromNormalized(float x, float y)
        {
            return new Vector2(Center.x + x * HalfSize.x, Center.y + y * HalfSize.y);
        }

        public static float RandomX(float padding = 1f)
        {
            return Random.Range(Left + padding, Right - padding);
        }
    }
}
