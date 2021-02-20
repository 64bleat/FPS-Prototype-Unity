using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class RigidbodyAnimator : MonoBehaviour
{
    public Vector3 position;
    public Vector3 rotation;

    private Rigidbody rb;

    private void Awake()
    {
        TryGetComponent(out rb);
    }

    private void FixedUpdate()
    {
        if (transform.parent)
            rb.MovePosition(transform.parent.TransformPoint(position));
        else
            rb.MovePosition(position);

        rb.MoveRotation(Quaternion.Euler(rotation));
    }
}
