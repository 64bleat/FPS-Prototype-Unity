using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchDestroyer : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        GameObject.Destroy(collision.gameObject);
    }
}
