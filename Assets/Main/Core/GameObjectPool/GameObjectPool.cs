using Serialization;
using System.Collections.Generic;
using UnityEngine;

namespace MPCore
{
    public class GameObjectPool : MonoBehaviour
    {
        public static readonly Dictionary<GameObject, GameObjectPool> pools = new Dictionary<GameObject, GameObjectPool>();

        public string resourcePath;
        public GameObject resource;
        public readonly Queue<GameObject> instances = new Queue<GameObject>();

        private void OnDestroy()
        {
            pools.Remove(resource);
        }

        /// <summary>
        /// Disable and return a GameObject to the pool
        /// </summary>
        /// <param name="instance"></param>
        private void DeactivateInstance(GameObject instance)
        {
            instance.SetActive(false);
            instance.transform.parent = transform;
            instances.Enqueue(instance);
        }

        /// <summary>
        /// Get an instance from the pool
        /// </summary>
        private GameObject GetInstance()
        {
            if (instances.Count == 0)
                AddInstance();

            return instances.Dequeue();
        }

        /// <summary>
        /// Add an instance too the pool
        /// </summary>
        public void AddInstance()
        {
            GameObject instance = Instantiate(resource, transform);

            DeactivateInstance(instance);

            if (!instance.TryGetComponent(out PoolReturn pr))
                pr = instance.AddComponent<PoolReturn>();

            instance.name = resource.name;
            pr.parentPool = this;
        }

        /// <summary>
        /// Spawns a GameObject from the pool
        /// </summary>
        public GameObject Spawn(Vector3 position = default, Quaternion rotation = default, Transform parent = null)
        {
            GameObject spawn;

            parent ??= transform;

            if (spawn = GetInstance())
            { 
                spawn.transform.SetPositionAndRotation(position, rotation);
                spawn.transform.SetParent(parent, true);
                spawn.SetActive(true);
            }

            return spawn;
        }

        /// <summary>
        /// Try to get the pool for a given resource. True if pool was found.
        /// </summary>
        public static bool TryGetPool(GameObject resource, int minCount, out GameObjectPool pool)
        {
            return pool = GetPool(resource, minCount);
        }

        /// <summary>
        /// Get the pool for a resource. Makes one if one does not exist
        /// </summary>
        public static GameObjectPool GetPool(GameObject resource, int minCount)
        {
            GameObjectPool pool = null;

            if (resource && !pools.TryGetValue(resource, out pool))
            {// Make Pool
                pool = new GameObject().AddComponent<GameObjectPool>();
                pool.gameObject.AddComponent<XMLSerializeable>();
                pool.name = "Pool: " + resource.name;
                pool.resource = resource;

                if(pool.resource && pool.resource.TryGetComponent(out XMLSerializeable po))
                    pool.resourcePath = po.resourceID;

                pools.Add(resource, pool);
            }

            if (pool)
                for (int i = pool.transform.childCount; i < minCount; i++)
                    pool.AddInstance();

            return pool;
        }
 

        public static void Deactivate(GameObject instance)
        {
            if (instance.TryGetComponent(out PoolReturn pr) && pr.parentPool)
                pr.parentPool.DeactivateInstance(instance);
            else
                Destroy(instance);
        }
    }
}