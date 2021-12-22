using UnityEngine;
using UnityEngine.Serialization;

namespace MPCore
{
	public class Respawner : MonoBehaviour
	{
		public InventoryPickup itemPrefab;
		[SerializeField] float _respawnTime = 5f;
		[SerializeField] bool _spawnOnAwake = true;

		float _timer = 0;
		InventoryPickup _instance;

		void OnValidate()
		{
			if (itemPrefab)
				gameObject.name = $"Respawner '{itemPrefab.inventory.displayName}'";
			else
				gameObject.name = "Respawner";
		}

		void OnDrawGizmos()
		{
			Gizmos.color = Color.red;
			int drawCount = 0;

			if(itemPrefab)
			{
				if(itemPrefab.transform.TryGetComponent(out MeshFilter mr) && mr.sharedMesh)
				{
					drawCount++;
					Gizmos.DrawMesh(mr.sharedMesh, 0, transform.position, transform.rotation, itemPrefab.transform.lossyScale);
				}

				foreach(Transform t in itemPrefab.transform)
					if(t.TryGetComponent(out mr) && mr.sharedMesh)
					{
						drawCount++;
						Gizmos.DrawMesh(mr.sharedMesh, 0, transform.position, transform.rotation, t.lossyScale);
					}
			}

			if(drawCount == 0)
				Gizmos.DrawCube(transform.position, Vector3.one * 0.3f);
		}

		void Start()
		{
			MessageBus.Publish(this);

			if(_spawnOnAwake && itemPrefab)
				_instance = Instantiate(itemPrefab, transform, false);
		}

		void Update()
		{
			if(!_instance && itemPrefab)
			{
				_timer += Time.deltaTime;

				if(_timer >= _respawnTime)
				{
					_instance = Instantiate(itemPrefab, transform, false);
					_instance.countDownDestroy = false;

					_timer = 0f;
				}
			}
		}
	}
}
