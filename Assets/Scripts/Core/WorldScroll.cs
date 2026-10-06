using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// How fast the world streams past the ship. The background layers and drifting pickups read
    /// <see cref="Speed"/>, so one number controls the whole "flying forward" feeling.
    /// </summary>
    public sealed class WorldScroll : MonoBehaviour
    {
        static WorldScroll instance;

        /// <summary>Current scroll speed in world units per second.</summary>
        public static float Speed { get; private set; } = 3f;

        /// <summary>Current speed relative to cruising speed (1 = normal, higher while boosting).</summary>
        public static float SpeedFactor { get; private set; } = 1f;

        [SerializeField] float cruiseSpeed = 3f;
        [Tooltip("How quickly the speed eases in and out of a boost.")]
        [SerializeField] float easing = 2.5f;

        float boostMultiplier = 1f;
        float boostTimeLeft;
        float current;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            instance = null;
            Speed = 3f;
            SpeedFactor = 1f;
        }

        void Awake()
        {
            instance = this;
            current = cruiseSpeed;
            Speed = current;
            SpeedFactor = 1f;
        }

        void OnDestroy()
        {
            if (instance == this) instance = null;
        }

        /// <summary>Temporarily speeds up the scrolling - a short "warp" between waves.</summary>
        public static void Boost(float multiplier, float duration)
        {
            if (instance == null) return;
            instance.boostMultiplier = Mathf.Max(1f, multiplier);
            instance.boostTimeLeft = duration;
        }

        void Update()
        {
            float dt = Time.deltaTime;
            if (boostTimeLeft > 0f) boostTimeLeft -= dt;

            float target = boostTimeLeft > 0f ? cruiseSpeed * boostMultiplier : cruiseSpeed;
            current = Mathf.Lerp(current, target, GameMath.Smoothing(easing, dt));

            Speed = current;
            SpeedFactor = cruiseSpeed > 0f ? current / cruiseSpeed : 1f;
        }
    }
}
