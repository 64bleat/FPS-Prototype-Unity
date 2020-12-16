using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MPCore.Movers
{
    public class LaunchMover : Mover
    {
        private enum State { forward, fwait, reverse, rwait };
        private State state = State.forward;
        public Vector3 offset;
        public float acceleration, launchSpeed, waitTime, resetSpeed;
        private float launchTime, startTime, resetTime;

        public bool moveOnTrigger = false;


        public override void Awake()
        {
            base.Awake();
            launchTime = launchSpeed / acceleration;
            offset = Vector3.ClampMagnitude(offset, 0.5f * acceleration * Mathf.Pow(launchTime, 2));
            startTime = Time.time;
            resetTime = (rb.position - origin).magnitude / resetSpeed;
            state = State.rwait;
            NextState();
        }

        void FixedUpdate()
        {
            switch (state)
            {
                case State.forward:
                    if (rb.velocity.magnitude < launchSpeed)
                        rb.MovePosition(rb.position + Vector3.ClampMagnitude(offset, acceleration * Time.fixedDeltaTime * (Time.time - startTime)));
                    else
                        NextState();
                    break;
                case State.reverse:
                    rb.MovePosition(rb.position - Vector3.ClampMagnitude(offset, resetSpeed * Time.fixedDeltaTime));
                    break;
                default:
                    break;
            }
        }

        private void NextState()
        {

            switch (state)
            {
                case State.forward:
                    state = State.fwait;

                    Invoke("NextState", waitTime);
                    return;
                case State.fwait:
                    state = State.reverse;
                    resetTime = (rb.position - origin).magnitude / resetSpeed;
                    Invoke("NextState", resetTime);
                    return;
                case State.reverse:
                    state = State.rwait;
                    rb.MovePosition(origin);
                    if(!moveOnTrigger)
                        Invoke("NextState", waitTime);
                    return;
                case State.rwait:
                    state = State.forward;
                    startTime = Time.time;
                    return;
                default:
                    return;
            }
        }

        public void Triggered()
        {
            if (moveOnTrigger && state == State.rwait)
            {
                state = State.forward;
                startTime = Time.time;
            }
        }
    }
}