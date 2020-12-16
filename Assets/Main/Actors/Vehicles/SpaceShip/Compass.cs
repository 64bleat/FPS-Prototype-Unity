using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Compass : MonoBehaviour
{
    public Transform orientation;

    private void Update()
    {
        transform.rotation = orientation ? orientation.rotation : Quaternion.identity;
    }
}
