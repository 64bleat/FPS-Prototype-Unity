using Serialization;
using System.Collections.Generic;
using UnityEngine;

namespace MPCore
{
    public class GameObjectPool : MonoBehaviour
    {
        public static readonly Dictionary<GameObject, GameObjectPool> openPools = new Dictionary<GameObject, GameObjectPool>();

        public string resourcePath;
        public GameObject resource;
        public readonly Queue<GameObject> availableInstances = new Queue<GameObject>();

        private void OnDestroy()
        {
            openPools.Remove(resource);
        }

        private void DisableInstance(GameObject instance)
        {
            instance.SetActive(false);
            instance.transform.parent = transform;
            availableInstances.Enqueue(instance);
        }

        private GameObject PullInstance()
        {
            if (availableInstances.Count == 0)
                AddInstance();

            return availableInstances.Dequeue();
        }

        public void AddInstance()
        {
            GameObject instance = Instantiate(resource, transform);

            DisableInstance(instance);

            if (!instance.TryGetComponent(out PoolReturn pr))
                pr = instance.AddComponent<PoolReturn>();

            instance.name = resource.name;
            pr.returnPool = this;
        }

        public GameObject Spawn(Vector3 position = default, Quaternion rotation = default, Transform parent = null)
        {
            GameObject spawn;

            parent ??= transform;

            if (spawn = PullInstance())
            { 
                spawn.transform.SetPositionAndRotation(position, rotation);
                spawn.transform.SetParent(parent, true);
                spawn.SetActive(true);
            }

            return spawn;
        }

        public static bool TryGetPool(GameObject prefab, int assureCount, out GameObjectPool pool)
        {
            return pool = GetPool(prefab, assureCount);
        }

        public static GameObjectPool GetPool(GameObject prefab, int assureCount)
        {
            GameObjectPool pool = null;

            if (prefab && !openPools.TryGetValue(prefab, out pool))
            {// Make Pool
                pool = new GameObject().AddComponent<GameObjectPool>();
                pool.gameObject.AddComponent<XMLSerializeable>();
                pool.name = "Pool: " + prefab.name;
                pool.resource = prefab;

                if(pool.resource && pool.resource.TryGetComponent(out XMLSerializeable po))
                    pool.resourcePath = po.resourceID;

                openPools.Add(prefab, pool);
            }

            if (pool)
                for (int i = pool.transform.childCount; i < assureCount; i++)
                    pool.AddInstance();

            return pool;
        }

        public static void Return(GameObject gameObject)
        {
            if (gameObject.TryGetComponent(out PoolReturn pr) && pr.returnPool)
                pr.returnPool.DisableInstance(gameObject);
            else
                Destroy(gameObject);
        }
    }
}