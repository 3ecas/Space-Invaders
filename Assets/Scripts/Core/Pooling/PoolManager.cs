using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// Recycles GameObjects instead of creating and destroying them. With hundreds of bullets and
    /// enemies on screen this avoids garbage-collection hitches.
    /// Use <see cref="Spawn(GameObject, Vector3, Quaternion)"/> in place of Instantiate and
    /// <see cref="Despawn(GameObject)"/> in place of Destroy.
    /// </summary>
    [DefaultExecutionOrder(-300)]
    public sealed class PoolManager : MonoBehaviour
    {
        static PoolManager instance;
        static bool quitting;

        readonly Dictionary<GameObject, Stack<PooledObject>> pools = new Dictionary<GameObject, Stack<PooledObject>>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            instance = null;
            quitting = false;
        }

        void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this);
                return;
            }
            instance = this;
        }

        void OnDestroy()
        {
            if (instance == this) instance = null;
        }

        void OnApplicationQuit() => quitting = true;

        static PoolManager GetOrCreate()
        {
            if (instance == null && !quitting)
            {
                instance = new GameObject("[Pools]").AddComponent<PoolManager>();
            }
            return instance;
        }

        public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab == null) return null;
            PoolManager manager = GetOrCreate();
            if (manager == null) return null;

            if (!manager.pools.TryGetValue(prefab, out Stack<PooledObject> stack))
            {
                stack = new Stack<PooledObject>();
                manager.pools.Add(prefab, stack);
            }

            PooledObject pooled = null;
            while (stack.Count > 0 && pooled == null) pooled = stack.Pop();

            if (pooled == null)
            {
                GameObject created = Instantiate(prefab, position, rotation, manager.transform);
                if (!created.TryGetComponent(out pooled)) pooled = created.AddComponent<PooledObject>();
                pooled.Bind(prefab);
            }
            else
            {
                pooled.transform.SetPositionAndRotation(position, rotation);
                pooled.gameObject.SetActive(true);
            }

            pooled.NotifySpawned();
            return pooled.gameObject;
        }

        public static T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component
        {
            if (prefab == null) return null;
            GameObject spawned = Spawn(prefab.gameObject, position, rotation);
            return spawned != null ? spawned.GetComponent<T>() : null;
        }

        public static void Despawn(GameObject target)
        {
            if (target == null) return;

            if (instance == null || !target.TryGetComponent(out PooledObject pooled) || pooled.Prefab == null)
            {
                // Not ours (or we are shutting down): fall back to a plain destroy.
                Destroy(target);
                return;
            }

            if (!pooled.IsSpawned) return;

            pooled.NotifyDespawned();
            target.SetActive(false);

            if (instance.pools.TryGetValue(pooled.Prefab, out Stack<PooledObject> stack)) stack.Push(pooled);
        }

        public static void Despawn(GameObject target, float delay)
        {
            if (target == null) return;
            if (delay <= 0f)
            {
                Despawn(target);
                return;
            }

            if (instance == null || !target.TryGetComponent(out PooledObject pooled))
            {
                Destroy(target, delay);
                return;
            }

            instance.StartCoroutine(instance.DespawnAfter(pooled, delay));
        }

        IEnumerator DespawnAfter(PooledObject pooled, float delay)
        {
            int spawnId = pooled.SpawnId;
            yield return new WaitForSeconds(delay);
            // Only despawn if it is still the same "life" we were asked to end.
            if (pooled != null && pooled.IsSpawned && pooled.SpawnId == spawnId) Despawn(pooled.gameObject);
        }
    }
}
