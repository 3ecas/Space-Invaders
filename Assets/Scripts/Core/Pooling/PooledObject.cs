using UnityEngine;

namespace SpaceShooter
{
    /// <summary>Bookkeeping the <see cref="PoolManager"/> attaches to every instance it creates.</summary>
    [DisallowMultipleComponent]
    public sealed class PooledObject : MonoBehaviour
    {
        public GameObject Prefab { get; private set; }
        public bool IsSpawned { get; private set; }

        /// <summary>Goes up by one every time the instance is reused. Lets delayed callbacks detect a recycled object.</summary>
        public int SpawnId { get; private set; }

        IPoolable[] poolables;

        internal void Bind(GameObject prefab)
        {
            Prefab = prefab;
            poolables = GetComponentsInChildren<IPoolable>(true);
        }

        internal void NotifySpawned()
        {
            IsSpawned = true;
            SpawnId++;
            for (int i = 0; i < poolables.Length; i++) poolables[i].OnSpawned();
        }

        internal void NotifyDespawned()
        {
            IsSpawned = false;
            for (int i = 0; i < poolables.Length; i++) poolables[i].OnDespawned();
        }
    }
}
