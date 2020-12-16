using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Accelerator : MonoBehaviour
{
    public Vector3 angularVelocity;

    private Rigidbody rb;
   
    void Awake()
    {
        rb = GetComponentInParent<Rigidbody>();
    }

    void FixedUpdate()
    {
        rb.MoveRotation(rb.rotation * Quaternion.Euler(angularVelocity * Time.fixedDeltaTime));
    }
}
