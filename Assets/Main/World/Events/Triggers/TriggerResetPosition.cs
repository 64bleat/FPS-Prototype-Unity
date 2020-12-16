using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerResetPosition : MonoBehaviour
{
    // Start is called before the first frame update
    private Vector3 originPosition;
    private Quaternion originRotation;

    void Start()
    {
        originPosition = transform.position;
        originRotation = transform.rotation;
    }

    public void ResetPosition()
    {
        Rigidbody rigidbody = GetComponent<Rigidbody>();

        transform.position = originPosition;
        transform.rotation = originRotation;

        if(rigidbody)
        {
            rigidbody.velocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
        }
        
    }
}
