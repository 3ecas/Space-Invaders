using UnityEngine;

namespace SpaceShooter
{
    /// <summary>Makes the engine flame flicker, and stretch when the ship pushes forward.</summary>
    public sealed class ThrusterFlame : MonoBehaviour
    {
        [SerializeField] PlayerController controller;
        [SerializeField] float flickerSpeed = 22f;
        [SerializeField, Range(0f, 1f)] float flickerAmount = 0.25f;
        [Tooltip("Extra length when flying forward at full speed.")]
        [SerializeField] float thrustStretch = 0.6f;

        Vector3 baseScale;
        float seed;

        void Awake()
        {
            baseScale = transform.localScale;
            seed = Random.value * 50f;
        }

        void Update()
        {
            float thrust = 0f;
            if (controller != null && controller.MaxSpeed > 0f)
            {
                thrust = Mathf.Clamp(controller.Velocity.y / controller.MaxSpeed, -0.5f, 1f);
            }

            float flicker = 1f + (Mathf.PerlinNoise(seed, Time.time * flickerSpeed) - 0.5f) * 2f * flickerAmount;
            float length = Mathf.Max(0.2f, flicker * (1f + thrust * thrustStretch));

            transform.localScale = new Vector3(baseScale.x * Mathf.Lerp(1f, flicker, 0.5f), baseScale.y * length, baseScale.z);
        }
    }
}
