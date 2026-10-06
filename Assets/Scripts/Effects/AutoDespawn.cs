using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// For one-shot effects such as explosions: restarts the particle systems each time the effect
    /// is spawned and hands it back to the pool once it has finished.
    /// </summary>
    public sealed class AutoDespawn : MonoBehaviour, IPoolable
    {
        [SerializeField] float lifetime = 1.5f;

        ParticleSystem[] systems;

        void Awake() => systems = GetComponentsInChildren<ParticleSystem>(true);

        public void OnSpawned()
        {
            for (int i = 0; i < systems.Length; i++)
            {
                systems[i].Clear(false);
                systems[i].Play(false);
            }
            PoolManager.Despawn(gameObject, lifetime);
        }

        public void OnDespawned() { }
    }
}
