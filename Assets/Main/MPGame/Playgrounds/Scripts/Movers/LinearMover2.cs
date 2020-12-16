using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MPCore.Movers
{
    public class LinearMover2 : Mover
    {
        private enum State  {forward, fwait, reverse, rwait};
        private State state = State.rwait;
        public Vector3 offset;
        public float waitTime, moveTime;
        public bool forwardOnTrigger = false;

        public override void Awake() 
        {
            base.Awake();

            if(!forwardOnTrigger)
                Invoke("nextState", 0f);
        }

        void FixedUpdate()
        {
            switch (state)
            {
            case State.forward:
                    rb.MovePosition(rb.position + offset / moveTime * Time.fixedDeltaTime);
                    break;
                case State.reverse:
                    rb.MovePosition(rb.position - offset / moveTime * Time.fixedDeltaTime);
                    break;
                default:
                    break;
            }
        }

       void nextState()
        {

            switch (state)
            {
                case State.forward:
                    state = State.fwait;
                    rb.MovePosition(origin + offset);
                    Invoke("nextState", waitTime);
                    return;
                case State.fwait:
                    state = State.reverse;
                    Invoke("nextState", moveTime);
                    return;
                case State.reverse:
                    state = State.rwait;
                    rb.MovePosition(origin);
                    if (!forwardOnTrigger)
                        Invoke("nextState", waitTime);
                    return;
                case State.rwait:
                    state = State.forward;
                    Invoke("nextState", moveTime);
                    return;
                default:
                    return;
            }
        }

        public void trigged()
        {
            if (forwardOnTrigger && state == State.rwait)
            {
                state = State.forward;
                Invoke("nextState", moveTime);
            }
        }
    }
}
