using UnityEngine;

namespace MPCore
{
    public class Respawner : MonoBehaviour
    {
        public GameObject itemToSpawn;
        public float respawnTime = 5f;
        public bool spawnOnAwake = true;

        private float timer = 0;

        private void Awake()
        {
            if (TryGetComponent(out MeshRenderer mr))
                mr.enabled = false;
        }

        private void Start()
        {
            if (spawnOnAwake)
                Instantiate(itemToSpawn, transform, false);
        }

        void Update()
        {
            if (transform.childCount == 0)
            {
                timer += Time.deltaTime;

                if (timer >= respawnTime)
                {
                    GameObject go = Instantiate(itemToSpawn, transform, false);

                    if (go.TryGetComponent(out InventoryPickup ip))
                        ip.countDownDestroy = false;

                    timer = 0;
                }
            }
        }
    }
}
