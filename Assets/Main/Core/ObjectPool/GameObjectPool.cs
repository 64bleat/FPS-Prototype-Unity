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
        public readonly Stack<GameObject> availableInstances = new Stack<GameObject>();

        private void OnDestroy()
        {
            pools.Remove(resource);
        }

        private void DisableInstance(GameObject instance)
        {
            instance.SetActive(false);
            availableInstances.Push(instance);
        }

        private GameObject EnableInstance()
        {
            if (availableInstances.Count == 0)
                AddInstance();

            return availableInstances.Pop();
        }

        private void AddInstance()
        {
            GameObject instance = Instantiate(resource, transform);

            instance.name = resource.name;
            DisableInstance(instance);
        }

        public GameObject Spawn(Vector3 position = default, Quaternion rotation = default, Transform parent = null)
        {
            GameObject spawn;

            if (spawn = EnableInstance())
            {
                if (parent)
                {
                    position = parent.TransformPoint(position);
                    rotation *= parent.rotation;
                }

                spawn.transform.SetPositionAndRotation(position, rotation);
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

            if (prefab && !pools.TryGetValue(prefab, out pool))
            {// Make Pool
                pool = new GameObject().AddComponent<GameObjectPool>();
                pool.gameObject.AddComponent<XMLSerializeable>();
                pool.name = "Pool: " + prefab.name;
                pool.resource = prefab;

                if(pool.resource && pool.resource.TryGetComponent(out XMLSerializeable po))
                    pool.resourcePath = po.resourceID;

                pools.Add(prefab, pool);
            }

            if (pool)
                for (int i = pool.transform.childCount; i < assureCount; i++)
                    pool.AddInstance();

            return pool;
        }

        public static void DestroyMember(GameObject gameObject)
        {
            Transform parent = gameObject.transform.parent;

            if (parent && parent.TryGetComponent(out GameObjectPool pool))
                pool.DisableInstance(gameObject);
            else
                Destroy(gameObject);
        }
    }
}