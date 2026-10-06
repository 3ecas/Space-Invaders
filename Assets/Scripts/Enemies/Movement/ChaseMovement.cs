using UnityEngine;

namespace SpaceShooter
{
    /// <summary>Homes in on the player. Turning is limited, so quick sidesteps make it overshoot.</summary>
    public sealed class ChaseMovement : EnemyBehaviour
    {
        [Tooltip("Degrees per second it can turn. Lower = easier to dodge.")]
        [SerializeField] float turnRate = 150f;
        [Tooltip("Seconds it flies straight before locking on.")]
        [SerializeField] float lockOnDelay = 0.4f;
        [Tooltip("Seconds to build up to full speed.")]
        [SerializeField] float rampUpTime = 0.9f;
        [SerializeField] bool faceHeading = true;

        Vector2 heading;
        float age;

        protected override void OnBegin()
        {
            Vector2 wanted = Owner.SpawnInfo.direction;
            heading = wanted.sqrMagnitude > 0.001f ? wanted.normalized : Vector2.down;
            age = 0f;
            transform.rotation = faceHeading ? GameMath.FaceDown(heading) : Quaternion.identity;
        }

        public override void Tick(float deltaTime)
        {
            age += deltaTime;
            Vector2 position = Owner.Position;

            if (age >= lockOnDelay && Player.Exists)
            {
                float angle = Vector2.SignedAngle(heading, Player.Position - position);
                float maxTurn = turnRate * deltaTime;
                heading = GameMath.Rotate(heading, Mathf.Clamp(angle, -maxTurn, maxTurn));
            }

            float ramp = rampUpTime > 0f ? Mathf.Clamp01(age / rampUpTime) : 1f;
            float speed = Owner.MoveSpeed * Mathf.Lerp(0.4f, 1f, ramp);

            transform.position = position + heading * (speed * deltaTime);
            if (faceHeading) transform.rotation = GameMath.FaceDown(heading);
        }
    }
}
