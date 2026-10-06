using UnityEngine;

namespace SpaceShooter
{
    /// <summary>Rotates an object steadily. Tumbling asteroids, spinning turrets and the like.</summary>
    public sealed class Spin : MonoBehaviour, IPoolable
    {
        [Tooltip("Degrees per second. Positive is counter-clockwise.")]
        [SerializeField] float speed = 90f;
        [Tooltip("Pick a random speed and direction between -speed and +speed each time it spawns.")]
        [SerializeField] bool randomize;

        float current;

        void Awake() => current = speed;

        public void OnSpawned()
        {
            if (!randomize) return;
            current = Random.Range(speed * 0.4f, speed) * (Random.value < 0.5f ? -1f : 1f);
            transform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
        }

        public void OnDespawned() { }

        void Update() => transform.Rotate(0f, 0f, current * Time.deltaTime);
    }
}
