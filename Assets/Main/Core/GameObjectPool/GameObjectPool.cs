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

		void OnDestroy()
		{
			pools.Remove(resource);
		}

		/// <summary>
		/// Disable and return a GameObject to the pool
		/// </summary>
		void DeactivateInstance(GameObject instance)
		{
			instance.SetActive(false);
			instance.transform.parent = transform;
			instances.Enqueue(instance);
		}

		/// <summary>
		/// Get an instance from the pool
		/// </summary>
		GameObject GetInstance()
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

			if (!instance.TryGetComponent(out PoolReturn poolReturn))
				poolReturn = instance.AddComponent<PoolReturn>();

			instance.name = resource.name;
			poolReturn.parentPool = this;
		}

		/// <summary>
		/// Spawns a GameObject from the pool
		/// </summary>
		public GameObject Spawn(Vector3 position = default, Quaternion rotation = default, Transform parent = null)
		{
			GameObject instance;

			parent ??= transform;

			if (instance = GetInstance())
			{ 
				instance.transform.SetPositionAndRotation(position, rotation);
				instance.transform.SetParent(parent, true);
				instance.SetActive(true);
			}

			return instance;
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

			// Make Pool
			if (resource && !pools.TryGetValue(resource, out pool))
			{
				pool = new GameObject().AddComponent<GameObjectPool>();
				pool.gameObject.AddComponent<XMLSerializeable>();
				pool.name = $"Pool: {resource.name}";
				pool.resource = resource;

				if(pool.resource.TryGetComponent(out XMLSerializeable po))
					pool.resourcePath = po.resourceID;

				pools.Add(resource, pool);
			}

			// Ensure instance count
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