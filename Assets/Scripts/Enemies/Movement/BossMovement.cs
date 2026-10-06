using UnityEngine;

namespace SpaceShooter
{
    /// <summary>Lowers the boss into view, then sweeps it from side to side near the top of the screen.</summary>
    public sealed class BossMovement : EnemyBehaviour
    {
        [Tooltip("Height it settles at, from -1 (bottom of the screen) to 1 (top).")]
        [SerializeField, Range(-1f, 1f)] float hoverHeight = 0.45f;
        [Tooltip("How far it sweeps sideways, as a fraction of half the screen width.")]
        [SerializeField, Range(0f, 1f)] float sweepWidth = 0.55f;
        [Tooltip("Seconds for a full left-right-left sweep.")]
        [SerializeField] float sweepPeriod = 8f;
        [SerializeField] float bobAmount = 0.5f;
        [SerializeField] float bobPeriod = 3.3f;

        /// <summary>True once the boss has finished its entrance.</summary>
        public bool HasArrived { get; private set; }

        float time;

        protected override void OnBegin()
        {
            HasArrived = false;
            time = 0f;
            transform.rotation = Quaternion.identity;
        }

        public override void Tick(float deltaTime)
        {
            Vector2 position = Owner.Position;
            float hoverY = PlayArea.Center.y + hoverHeight * PlayArea.HalfSize.y;

            if (!HasArrived)
            {
                position.y = Mathf.MoveTowards(position.y, hoverY, Owner.MoveSpeed * deltaTime);
                transform.position = position;
                if (Mathf.Approximately(position.y, hoverY)) HasArrived = true;
                return;
            }

            time += deltaTime;
            var target = new Vector2(
                PlayArea.Center.x + Mathf.Sin(time * Mathf.PI * 2f / sweepPeriod) * sweepWidth * PlayArea.HalfSize.x,
                hoverY + Mathf.Sin(time * Mathf.PI * 2f / bobPeriod) * bobAmount);

            transform.position = Vector2.Lerp(position, target, GameMath.Smoothing(3f, deltaTime));
        }
    }
}
