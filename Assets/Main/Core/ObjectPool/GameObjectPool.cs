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
            if (resource && pools.ContainsKey(resource))
                pools.Remove(resource);
        }

        private void DisableMember(GameObject instance)
        {
            instance.SetActive(false);
            availableInstances.Push(instance);
        }

        private GameObject EnableMember()
        {
            if (availableInstances.Count == 0)
                AddMember();

            return availableInstances.Pop();
        }

        private void AddMember()
        {
            if (resource && Instantiate(resource, transform) is var instance && instance)
            {
                instance.name = resource.name;
                DisableMember(instance);
            }
        }

        public static bool TryGetPool(GameObject prefab, int assureCount, out GameObjectPool pool)
        {
            pool = GetPool(prefab, assureCount);

            return pool != null;
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
                    pool.AddMember();

            return pool;
        }

        public GameObject Spawn(Vector3 position = default, Quaternion rotation = default, Transform parent = null)
        {
            GameObject spawn;

            if (spawn = EnableMember())
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

        public static void DestroyMember(GameObject gameObject)
        {
            if (gameObject.GetComponentInParent<GameObjectPool>() is var pool && pool)
                pool.DisableMember(gameObject);
            else
                Destroy(gameObject);
        }
    }
}