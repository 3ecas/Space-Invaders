using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// Flies in a straight line, optionally weaving from side to side.
    /// Covers simple divers, drifting asteroids and ships that cross the screen.
    /// </summary>
    public sealed class StraightMovement : EnemyBehaviour
    {
        [Tooltip("How far it weaves to each side. 0 = dead straight.")]
        [SerializeField] float swayAmplitude;
        [SerializeField] float swayFrequency = 2.5f;
        [Tooltip("Each one gets a random speed within this fraction of its normal speed.")]
        [SerializeField, Range(0f, 0.5f)] float speedVariance;
        [Tooltip("Turn the sprite to face the direction of travel.")]
        [SerializeField] bool faceTravelDirection;
        [Tooltip("Come back in from the top after leaving the bottom, instead of flying away for good.")]
        [SerializeField] bool wrapToTop;

        Vector2 origin;
        Vector2 direction;
        Vector2 perpendicular;
        float travelled;
        float time;
        float speedFactor;

        protected override void OnBegin()
        {
            Vector2 wanted = Owner.SpawnInfo.direction;
            direction = wanted.sqrMagnitude > 0.001f ? wanted.normalized : Vector2.down;
            perpendicular = new Vector2(-direction.y, direction.x);

            origin = Owner.Position;
            travelled = 0f;
            time = Owner.SpawnInfo.phase;
            speedFactor = 1f + Random.Range(-speedVariance, speedVariance);

            transform.rotation = faceTravelDirection ? GameMath.FaceDown(direction) : Quaternion.identity;
        }

        public override void Tick(float deltaTime)
        {
            time += deltaTime;
            travelled += Owner.MoveSpeed * speedFactor * deltaTime;

            Vector2 position = origin + direction * travelled;
            if (swayAmplitude > 0f) position += perpendicular * (Mathf.Sin(time * swayFrequency) * swayAmplitude);

            if (wrapToTop && position.y < PlayArea.Bottom - 1.5f)
            {
                origin = new Vector2(PlayArea.RandomX(1.5f), PlayArea.Top + 1.5f);
                travelled = 0f;
                position = origin;
            }

            transform.position = position;
        }
    }
}
