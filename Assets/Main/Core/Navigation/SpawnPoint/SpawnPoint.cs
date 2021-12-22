using MPWorld;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MPCore
{
	[RequireComponent(typeof(CapsuleCollider))]
	public class SpawnPoint : MonoBehaviour
	{
		const float WAIT_TIME = 1f;

		NavModel _navModel;
		float _lastSpawnTime = -WAIT_TIME * 2f;
		readonly HashSet<Collider> _overlapBuffer = new HashSet<Collider>();
		CapsuleCollider _cap;

		void OnValidate()
		{
			gameObject.name = "PlayerSpawn";
		}

		void Awake()
		{
			_navModel = Models.GetModel<NavModel>();
			_cap = GetComponent<CapsuleCollider>();
		}

		void Start()
		{
			MessageBus.Publish(this);
		}

		void OnEnable()
		{
			_navModel.spawnPoints.Add(this);
		}

		void OnDisable()
		{
			_navModel.spawnPoints.Remove(this);
		}

		public static SpawnPoint GetRandomSpawnPoint()
		{
			NavModel navModel = Models.GetModel<NavModel>();

			if (navModel.spawnPoints.Count != 0)
			{
				float time = Time.time;
				int count = navModel.spawnPoints.Count;
				int index = Random.Range(0, count);

				for (int i = 0; i < count; i++)
				{
					int pick = (index + i) % count;
					SpawnPoint spawn = navModel.spawnPoints[pick];

					if (spawn.gameObject.activeInHierarchy 
						&& spawn._overlapBuffer.Count == 0 
						&& time - spawn._lastSpawnTime >= WAIT_TIME)
						return navModel.spawnPoints[pick];
				}
			}

			return null;
		}

		/// <summary>
		/// Instantiate a Prefab at this spawn point
		/// </summary>
		public T Spawn<T>(T reference) where T: Component
		{
			Vector3 position = transform.TransformPoint(_cap.center);
			T instance = Instantiate(reference, position, transform.rotation);

			// Match SpawnPoint Velocity
			if (instance.TryGetComponentInChildren(out IGravityUser playerGU))
				if (gameObject.TryGetComponentInParent(out Rigidbody spawnRb))
					playerGU.Velocity = spawnRb.GetPointVelocity(instance.transform.position);
				else if (gameObject.TryGetComponentInParent(out IGravityUser spawnGu))
					playerGU.Velocity = spawnGu.Velocity;

			if (instance)
				_lastSpawnTime = Time.time;

			return instance;
		}

		void OnTriggerEnter(Collider other)
		{
			_overlapBuffer.Add(other);
		}

		void OnTriggerExit(Collider other)
		{
			_overlapBuffer.Remove(other);
		}

		void OnDrawGizmos()
		{
			CapsuleCollider cap = GetComponent<CapsuleCollider>();

			Vector3 center = transform.TransformPoint(cap.center);
			Vector3 offset = transform.up * (cap.height / 2 - cap.radius);
			Vector3 forward = transform.forward * cap.radius;

			Color color = Color.green;
			color.a = 0.25f;
			Gizmos.color = color;
			Gizmos.DrawSphere(center - offset, cap.radius);
			Gizmos.DrawSphere(center, cap.radius);
			Gizmos.DrawSphere(center + offset, cap.radius);
			Gizmos.DrawSphere(center + offset + forward, cap.radius / 2);
		}
	}
}
