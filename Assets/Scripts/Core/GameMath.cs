using UnityEngine;

namespace SpaceShooter
{
    public static class GameMath
    {
        /// <summary>Rotates a 2D vector counter-clockwise by the given degrees.</summary>
        public static Vector2 Rotate(Vector2 v, float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            float sin = Mathf.Sin(radians);
            float cos = Mathf.Cos(radians);
            return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
        }

        /// <summary>Rotation that points a sprite drawn facing up (+Y) along <paramref name="direction"/>.</summary>
        public static Quaternion FaceUp(Vector2 direction)
        {
            return Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f);
        }

        /// <summary>Rotation that points a sprite drawn facing down (-Y) along <paramref name="direction"/>.</summary>
        public static Quaternion FaceDown(Vector2 direction)
        {
            return Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90f);
        }

        /// <summary>Frame-rate independent smoothing factor for Lerp-style easing.</summary>
        public static float Smoothing(float sharpness, float deltaTime)
        {
            return 1f - Mathf.Exp(-sharpness * deltaTime);
        }
    }
}
