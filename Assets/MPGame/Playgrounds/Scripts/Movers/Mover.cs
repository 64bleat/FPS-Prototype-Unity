using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MPCore.Movers
{
    public class Mover : MonoBehaviour
    {
        [HideInInspector]
        public Rigidbody rb;
        [HideInInspector]
        public Vector3 origin;

        public virtual void Awake()
        {
            if ((rb = GetComponent<Rigidbody>()) == null)
                rb = gameObject.AddComponent<Rigidbody>();

            rb.isKinematic = true;
            rb.useGravity = false;

            origin = transform.position;
        }
        /*private void Awake()
        {
            startMover();
        }

        public virtual void startMover()
        {
            if (rb.GetComponent<Rigidbody>() == null)
                rb = gameObject.AddComponent<Rigidbody>();

            rb.isKinematic = true;
            rb.useGravity = false;

            origin = transform.position;
        }*/
    }
}
