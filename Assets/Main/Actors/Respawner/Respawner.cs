using UnityEngine;

namespace MPCore
{
	public class Respawner : MonoBehaviour
	{
		public GameObject itemToSpawn;
		[SerializeField] float respawnTime = 5f;
		[SerializeField] bool spawnOnAwake = true;

		float _timer = 0;

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.red;
			int drawCount = 0;

			if(itemToSpawn)
			{
				if(itemToSpawn.transform.TryGetComponent(out MeshFilter mr) && mr.sharedMesh)
				{
					drawCount++;
					Gizmos.DrawMesh(mr.sharedMesh, 0, transform.position, transform.rotation, itemToSpawn.transform.lossyScale);
				}

				foreach(Transform t in itemToSpawn.transform)
					if(t.TryGetComponent(out mr) && mr.sharedMesh)
					{
						drawCount++;
						Gizmos.DrawMesh(mr.sharedMesh, 0, transform.position, transform.rotation, t.lossyScale);
					}
			}

			if(drawCount == 0)
				Gizmos.DrawCube(transform.position, Vector3.one * 0.3f);
		}

		private void Start()
		{
			Messages.Publish(this);

			if(spawnOnAwake)
				Instantiate(itemToSpawn, transform, false);
		}

		void Update()
		{
			if(transform.childCount == 0)
			{
				_timer += Time.deltaTime;

				if(_timer >= respawnTime)
				{
					GameObject instance = Instantiate(itemToSpawn, transform, false);

					if(instance.TryGetComponent(out InventoryPickup ip))
						ip.countDownDestroy = false;

					_timer = 0;
				}
			}
		}
	}
}
