using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementKey : MonoBehaviour
{
    public float transitionTime = 1;

    //public bool Move(Rigidbody mover, Transform next, float keyTime)
    //{
    //    float factor = transitionTime <= 0 ? 1 : keyTime / transitionTime;
        
    //    return Lerp(mover, next, factor);
    //}

    //private bool Lerp(Rigidbody mover, Transform next, float factor)
    //{
    //    mover.MovePosition(Vector3.Lerp(transform.position, next.position, factor));
    //    mover.MoveRotation(Quaternion.Lerp(transform.rotation, next.rotation, factor));

    //    if (transitionTime == 0)
    //        mover.velocity = Vector3.zero;

    //    return factor >= 1f;
    //}
}
