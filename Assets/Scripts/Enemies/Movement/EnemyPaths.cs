using System.Collections.Generic;
using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// The swooping routes that <see cref="PathMovement"/> enemies fly. Each route is a handful of
    /// waypoints in screen-relative coordinates: x and y run from -1 to 1 across the visible area,
    /// so anything beyond that is off-screen. Add a new array here to add a new route.
    /// </summary>
    public static class EnemyPaths
    {
        static readonly Vector2[][] routes =
        {
            // S-curve: in at the top-left, across the middle, out at the bottom-right.
            new[] { P(-0.7f, 1.25f), P(-0.7f, 0.6f), P(-0.3f, 0.1f), P(0.4f, 0.2f), P(0.75f, -0.2f), P(0.6f, -0.8f), P(0.5f, -1.3f) },

            // Loop: down the left, a full loop mid-screen, then out the bottom.
            new[] { P(-0.5f, 1.25f), P(-0.5f, 0.4f), P(-0.2f, -0.1f), P(0.2f, 0.1f), P(0.1f, 0.55f), P(-0.25f, 0.4f), P(-0.15f, -0.3f), P(0.1f, -1.3f) },

            // U-turn: sweeps down to the middle and climbs back out of the top.
            new[] { P(-0.85f, 1.25f), P(-0.8f, 0.3f), P(-0.45f, -0.35f), P(0f, -0.5f), P(0.45f, -0.35f), P(0.8f, 0.3f), P(0.85f, 1.3f) },

            // Diagonal dive from the upper-left edge to the bottom-right.
            new[] { P(-1.2f, 0.75f), P(-0.6f, 0.6f), P(0f, 0.25f), P(0.5f, -0.3f), P(0.8f, -1.3f) },

            // Zig-zag down the whole screen.
            new[] { P(-0.6f, 1.25f), P(-0.6f, 0.8f), P(0.6f, 0.45f), P(-0.6f, 0.05f), P(0.6f, -0.35f), P(-0.2f, -0.8f), P(-0.2f, -1.3f) },

            // Sweep across the top third, then dive.
            new[] { P(-1.2f, 0.55f), P(-0.3f, 0.7f), P(0.5f, 0.6f), P(0.8f, 0.2f), P(0.4f, -0.3f), P(0.3f, -1.3f) }
        };

        public static int Count => routes.Length;

        static Vector2 P(float x, float y) => new Vector2(x, y);

        /// <summary>
        /// Fills <paramref name="output"/> with a smooth world-space curve through the route's waypoints.
        /// A <paramref name="side"/> of -1 mirrors the route left-to-right.
        /// </summary>
        public static void Sample(int index, float side, List<Vector2> output, int samplesPerSegment = 10)
        {
            output.Clear();
            Vector2[] route = routes[Mathf.Abs(index) % routes.Length];
            float mirror = side < 0f ? -1f : 1f;
            int last = route.Length - 1;

            for (int i = 0; i < last; i++)
            {
                // Catmull-Rom needs the neighbours on either side of the current segment.
                Vector2 p0 = route[Mathf.Max(i - 1, 0)];
                Vector2 p1 = route[i];
                Vector2 p2 = route[i + 1];
                Vector2 p3 = route[Mathf.Min(i + 2, last)];

                for (int s = 0; s < samplesPerSegment; s++)
                {
                    Vector2 point = CatmullRom(p0, p1, p2, p3, s / (float)samplesPerSegment);
                    output.Add(PlayArea.FromNormalized(point.x * mirror, point.y));
                }
            }

            Vector2 end = route[last];
            output.Add(PlayArea.FromNormalized(end.x * mirror, end.y));
        }

        static Vector2 CatmullRom(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;
            return 0.5f * (2f * p1 + (p2 - p0) * t + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 + (3f * p1 - p0 - 3f * p2 + p3) * t3);
        }
    }
}
