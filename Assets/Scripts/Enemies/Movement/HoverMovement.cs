using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// Flies in, parks in the upper part of the screen and drifts around there while it shoots.
    /// After a while it gives up and dives off the bottom.
    /// </summary>
    public sealed class HoverMovement : EnemyBehaviour
    {
        [Tooltip("Seconds to ease towards a new spot. Lower = snappier.")]
        [SerializeField] float smoothTime = 0.7f;
        [Tooltip("How far from its parking spot it drifts, sideways and vertically.")]
        [SerializeField] Vector2 wanderRange = new Vector2(3f, 1.2f);
        [Tooltip("Shortest and longest pause between drifts.")]
        [SerializeField] Vector2 wanderInterval = new Vector2(1.5f, 3.5f);
        [Tooltip("Seconds before it leaves. 0 = stays until destroyed.")]
        [SerializeField] float stayDuration = 16f;
        [SerializeField] float leaveAcceleration = 9f;

        Vector2 anchor;
        Vector2 target;
        Vector2 velocity;
        float wanderTimer;
        float age;
        float leaveSpeed;

        protected override void OnBegin()
        {
            Vector2 position = Owner.Position;

            anchor = Owner.SpawnInfo.anchor;
            if (anchor == Vector2.zero) anchor = new Vector2(position.x, PlayArea.Top - Random.Range(2.5f, 6f));
            anchor = PlayArea.Clamp(anchor, 1.5f);

            target = anchor;
            velocity = Vector2.zero;
            wanderTimer = Random.Range(wanderInterval.x, wanderInterval.y);
            age = 0f;
            leaveSpeed = 0f;
            transform.rotation = Quaternion.identity;
        }

        public override void Tick(float deltaTime)
        {
            age += deltaTime;
            Vector2 position = Owner.Position;

            if (stayDuration > 0f && age > stayDuration)
            {
                leaveSpeed += leaveAcceleration * deltaTime;
                transform.position = position + Vector2.down * (leaveSpeed * deltaTime);
                return;
            }

            wanderTimer -= deltaTime;
            if (wanderTimer <= 0f)
            {
                wanderTimer = Random.Range(wanderInterval.x, wanderInterval.y);
                var offset = new Vector2(Random.Range(-wanderRange.x, wanderRange.x), Random.Range(-wanderRange.y, wanderRange.y));
                target = PlayArea.Clamp(anchor + offset, 1.5f);
            }

            transform.position = Vector2.SmoothDamp(position, target, ref velocity, smoothTime, Owner.MoveSpeed, deltaTime);
        }
    }
}
