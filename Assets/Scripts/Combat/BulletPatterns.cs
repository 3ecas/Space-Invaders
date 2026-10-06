using UnityEngine;

namespace SpaceShooter
{
    /// <summary>Shared helpers for launching projectiles in common shapes.</summary>
    public static class BulletPatterns
    {
        /// <summary>A single shot.</summary>
        public static Projectile Fire(Projectile prefab, Team team, Vector2 origin, Vector2 direction,
            float speed, float damage, float lifetime)
        {
            Projectile shot = PoolManager.Spawn(prefab, origin, Quaternion.identity);
            if (shot != null) shot.Launch(team, direction, speed, damage, lifetime);
            return shot;
        }

        /// <summary>
        /// Several shots centred on <paramref name="direction"/>. <paramref name="angleStep"/> fans
        /// them out, <paramref name="lateralSpacing"/> places them side by side; use either or both.
        /// </summary>
        public static void Fan(Projectile prefab, Team team, Vector2 origin, Vector2 direction, int count,
            float angleStep, float lateralSpacing, float speed, float damage, float lifetime, float randomSpread = 0f)
        {
            if (count <= 0) return;

            direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.up;
            var right = new Vector2(direction.y, -direction.x);
            float middle = (count - 1) * 0.5f;

            for (int i = 0; i < count; i++)
            {
                float offset = i - middle;
                float angle = -offset * angleStep;
                if (randomSpread > 0f) angle += Random.Range(-randomSpread, randomSpread);

                Fire(prefab, team, origin + right * (offset * lateralSpacing), GameMath.Rotate(direction, angle),
                    speed, damage, lifetime);
            }
        }

        /// <summary>Shots evenly spaced around a full circle.</summary>
        public static void Ring(Projectile prefab, Team team, Vector2 origin, int count, float angleOffset,
            float speed, float damage, float lifetime)
        {
            if (count <= 0) return;

            float step = 360f / count;
            for (int i = 0; i < count; i++)
            {
                Fire(prefab, team, origin, GameMath.Rotate(Vector2.down, angleOffset + step * i), speed, damage, lifetime);
            }
        }
    }
}
