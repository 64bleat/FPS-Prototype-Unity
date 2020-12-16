using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MPCore.Movers
{
    public class SinMover : MonoBehaviour
    {
        public float moveDistance, moveSpeed;
        public Vector3 moveDirection;

        private Rigidbody rb;
        private Vector3 origin;

        // Use this for initialization
        void Start()
        {
            rb = GetComponent<Rigidbody>();

            moveDirection = moveDirection.normalized;
            origin = transform.position;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            rb.MovePosition(origin + moveDirection * moveDistance * Mathf.Sin(Time.time / moveSpeed));
        }
    }
}
