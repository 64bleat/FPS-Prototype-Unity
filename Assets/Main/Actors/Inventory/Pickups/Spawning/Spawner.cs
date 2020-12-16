using MPWorld;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public float respawnTime = 0.5f;
    public float ejectSpeed = 2f;
    public float speedFluctuation = 0f;
    public float randomAngle = 0f;

    private readonly Queue<GameObjectCount> spawnQueue = new Queue<GameObjectCount>();
    private float respawnCooldown = 0f;
    private Rigidbody attachedRigidbody;

    private class GameObjectCount
    {
        internal GameObject prefab;
        internal int count;
    }

    private void Awake()
    {
        attachedRigidbody = GetComponentInParent<Rigidbody>();
    }

    private void Update()
    {
        respawnCooldown += Time.deltaTime;

        if (spawnQueue.Count != 0 && respawnCooldown >= respawnTime)
        {
            GameObjectCount peek = spawnQueue.Peek();
            GameObject instance = Instantiate(peek.prefab, transform.position, transform.rotation, null);
            Vector3 ejectVelocity = transform.forward * (ejectSpeed + Random.Range(-speedFluctuation, speedFluctuation));

            ejectVelocity = Quaternion.AngleAxis(Random.Range(0, 360), transform.forward) * Quaternion.AngleAxis(Random.Range(0, randomAngle), transform.right) * ejectVelocity;

            if (attachedRigidbody)
                ejectVelocity += attachedRigidbody.GetPointVelocity(transform.position);

            if (instance.GetComponentInChildren<Rigidbody>() is var rb && rb)
                rb.velocity = ejectVelocity;
            else if (instance.GetComponentInChildren<IGravityUser>() is var gu && gu != null)
                gu.Velocity = ejectVelocity;

            if (--peek.count <= 0)
                spawnQueue.Dequeue();

            respawnCooldown = 0;
        }
    }

    public void PushSpawn(GameObject prefab, int count = 1)
    {
        spawnQueue.Enqueue(new GameObjectCount()
        {
            prefab = prefab,
            count = count
        });
    }
}
