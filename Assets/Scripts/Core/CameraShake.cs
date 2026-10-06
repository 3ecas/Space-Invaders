using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// Trauma-based screen shake. Call <see cref="Shake"/> with a value from 0 to 1; hits stack up
    /// and fade out on their own. Lives on the main camera.
    /// </summary>
    public sealed class CameraShake : MonoBehaviour
    {
        static CameraShake instance;

        [SerializeField] float maxOffset = 0.55f;
        [SerializeField] float maxRoll = 1.6f;
        [Tooltip("How much trauma drains away per second.")]
        [SerializeField] float decay = 1.7f;
        [SerializeField] float frequency = 26f;
        [Tooltip("Scales every shake. 0 turns screen shake off.")]
        [SerializeField, Range(0f, 2f)] float intensity = 1f;

        float trauma;
        Vector3 restPosition;
        float seed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => instance = null;

        void Awake()
        {
            instance = this;
            restPosition = transform.localPosition;
            seed = Random.value * 100f;
        }

        void OnDestroy()
        {
            if (instance == this) instance = null;
        }

        public static void Shake(float amount)
        {
            if (instance == null || amount <= 0f) return;
            instance.trauma = Mathf.Clamp01(instance.trauma + amount);
        }

        void LateUpdate()
        {
            if (trauma <= 0f)
            {
                transform.localPosition = restPosition;
                transform.localRotation = Quaternion.identity;
                return;
            }

            // Squaring the trauma makes small hits subtle and big hits dramatic.
            float power = trauma * trauma * intensity;
            float time = Time.unscaledTime * frequency;

            float x = (Mathf.PerlinNoise(seed, time) * 2f - 1f) * maxOffset * power;
            float y = (Mathf.PerlinNoise(seed + 17.3f, time) * 2f - 1f) * maxOffset * power;
            float roll = (Mathf.PerlinNoise(seed + 41.7f, time) * 2f - 1f) * maxRoll * power;

            transform.localPosition = restPosition + new Vector3(x, y, 0f);
            transform.localRotation = Quaternion.Euler(0f, 0f, roll);

            trauma = Mathf.Max(0f, trauma - decay * Time.unscaledDeltaTime);
        }
    }
}
