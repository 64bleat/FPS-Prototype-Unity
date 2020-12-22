using UnityEngine;

public class Respawner : MonoBehaviour
{
    public GameObject itemToSpawn;
    public float respawnTime = 5f;
    public bool spawnOnAwake = true;

    private float timer = 0;

    private void Awake()
    {
        if(spawnOnAwake)
            Instantiate(itemToSpawn, transform, false);

        if(TryGetComponent(out MeshRenderer mr))
            mr.enabled = false;
    }

    void Update()
    {
        if(transform.childCount == 0)
        {
            timer += Time.deltaTime;

            if(timer >= respawnTime)
            {
                Instantiate(itemToSpawn, transform, false);
                timer = 0;
            }
        }
    }
}
