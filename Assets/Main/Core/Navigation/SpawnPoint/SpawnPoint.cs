using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    private static readonly List<SpawnPoint> points = new List<SpawnPoint>();

    private void Awake()
    {
        if (gameObject.activeSelf)
            points.Add(this);
    }

    void OnEnable()
    {
        if(!points.Contains(this))
            points.Add(this);
    }

    void OnDisable()
    {
        points.Remove(this);
    }

    public static SpawnPoint GetSpawnPoint()
    {
        if (points.Count != 0)
            return points[Random.Range(0, points.Count)];
        else
            return null;
    }
}
