using MPWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MPCore
{
    public class SpawnPoint : MonoBehaviour
    {
        private const float cooldown = 1f;
        private static readonly List<SpawnPoint> points = new List<SpawnPoint>();

        [NonSerialized] public float lastSpawnTime = -cooldown * 2f;
        private readonly HashSet<Collider> overlaps = new HashSet<Collider>();

        private void Awake()
        {
            points.Add(this);
        }

        void OnDestroy()
        {
            points.Remove(this);
        }

        public static SpawnPoint GetRandomSpawnPoint()
        {
            if (points.Count != 0)
            {
                float time = Time.time;
                int count = points.Count;
                int index = Random.Range(0, count);

                for (int i = 0; i < count; i++)
                {
                    int pick = (index + i) % count;
                    SpawnPoint spawn = points[pick];

                    if (spawn.gameObject.activeInHierarchy 
                        && spawn.overlaps.Count == 0 
                        && time - spawn.lastSpawnTime >= cooldown)
                        return points[pick];
                }
            }

            return null;
        }

        public GameObject Spawn(GameObject reference)
        {
            GameObject instance = Instantiate(reference, transform.position, transform.rotation);

            if (instance.TryGetComponent(out Collider c))
                instance.transform.position += transform.up * c.bounds.extents.y;

            // Match SpawnPoint Velocity
            if (instance.TryGetComponentInChildren(out IGravityUser playerGU))
                if (gameObject.TryGetComponentInParent(out Rigidbody spawnRb))
                    playerGU.Velocity = spawnRb.GetPointVelocity(instance.transform.position);
                else if (gameObject.TryGetComponentInParent(out IGravityUser spawnGu))
                    playerGU.Velocity = spawnGu.Velocity;

            return instance;
        }

        private void OnTriggerEnter(Collider other)
        {
            overlaps.Add(other);
        }

        private void OnTriggerExit(Collider other)
        {
            overlaps.Remove(other);
        }
    }
}
