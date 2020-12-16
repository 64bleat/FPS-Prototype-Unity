using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyTrigger : MonoBehaviour
{
    public void DestroyGameObject()
    {
        Destroy(gameObject);
    }

    public void DestroyGameObjectImmediate()
    {
        DestroyImmediate(gameObject);
    }
}
