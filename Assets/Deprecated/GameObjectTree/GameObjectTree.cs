using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectTree : MonoBehaviour
{
    public List<ObjectPotential> potentialObjects;

    [System.Serializable]
    public struct ObjectPotential
    {
        public GameObject prefab;
        public float potential;
    }

    public void Spawn()
    {
        float total = 0f;

        foreach (var po in potentialObjects)
            total += po.potential;

        float target = Rand() * total;
        float counter = 0;

        foreach(var po in potentialObjects)
            if((counter += po.potential) >= target)
            {
                return;
            }

        return;
    }
    
    private float Rand(int seed = 0)
    {
        return Random.value;
    }
}
