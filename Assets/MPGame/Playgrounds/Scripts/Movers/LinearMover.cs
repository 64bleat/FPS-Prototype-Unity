using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MPCore.Movers
{
    public class LinearMover : MonoBehaviour
    {
        public float moveDistance, moveSpeed, waitTime;
        public Vector3 moveDirection;

        private float lastWait;
        private Rigidbody rb;
        private Vector3 origin;
        int direction;

        // Use this for initialization
        void Start()
        {
            rb = GetComponent<Rigidbody>();

            moveDirection = moveDirection.normalized;
            origin = rb.position;
            direction = 1;
            lastWait = Time.time;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (Time.time > lastWait + waitTime)
            {
                Vector3 movement = rb.position + moveDirection.normalized * direction * moveSpeed * Time.fixedDeltaTime;

                if ((movement - origin).magnitude > moveDistance)
                {
                    movement = origin + moveDirection.normalized * direction * moveDistance;
                    direction *= -1;
                    lastWait = Time.time;
                }

                rb.MovePosition(movement);
            }

            //CharacterController[] cc = GetComponentsInChildren<CharacterController>();

            //foreach (CharacterController c in cc)
            //    c.transform.position += rb.velocity * Time.fixedDeltaTime;
        }
    }
}
