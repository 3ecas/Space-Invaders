using System.Collections.Generic;
using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// Follows one of the curved routes in <see cref="EnemyPaths"/>. A group spawned one after
    /// another on the same route forms the snaking chains typical of scrolling shooters.
    /// </summary>
    public sealed class PathMovement : EnemyBehaviour
    {
        [SerializeField] bool faceTravelDirection = true;
        [Tooltip("Degrees per second the sprite can turn while following the curve.")]
        [SerializeField] float turnSpeed = 540f;

        readonly List<Vector2> points = new List<Vector2>(96);
        readonly List<float> distances = new List<float>(96);
        float travelled;
        float totalLength;
        int segment;

        protected override void OnBegin()
        {
            EnemyPaths.Sample(Owner.SpawnInfo.pathIndex, Owner.SpawnInfo.side, points);

            // Measure the curve so the enemy moves at a steady speed along it.
            distances.Clear();
            distances.Add(0f);
            totalLength = 0f;
            for (int i = 1; i < points.Count; i++)
            {
                totalLength += Vector2.Distance(points[i - 1], points[i]);
                distances.Add(totalLength);
            }

            travelled = 0f;
            segment = 0;
            transform.position = points[0];
            transform.rotation = faceTravelDirection && points.Count > 1
                ? GameMath.FaceDown(points[1] - points[0])
                : Quaternion.identity;
        }

        public override void Tick(float deltaTime)
        {
            travelled += Owner.MoveSpeed * deltaTime;
            if (travelled >= totalLength)
            {
                Owner.Leave();
                return;
            }

            while (segment < points.Count - 2 && distances[segment + 1] < travelled) segment++;

            Vector2 from = points[segment];
            Vector2 to = points[segment + 1];
            float length = distances[segment + 1] - distances[segment];
            float t = length > 0f ? (travelled - distances[segment]) / length : 0f;

            transform.position = Vector2.Lerp(from, to, t);

            if (faceTravelDirection && (to - from).sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, GameMath.FaceDown(to - from), turnSpeed * deltaTime);
            }
        }
    }
}
