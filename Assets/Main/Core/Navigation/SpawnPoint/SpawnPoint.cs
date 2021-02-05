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
