using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MPCore.Movers
{
    public class TrigMover : Mover
    {
        public enum FType { sin, cos, tan };
        public LinkedList<Trig> functionList;
        //private float start;

        public override void Awake()
        {
            base.Awake();
            //start = Time.time;
            functionList = new LinkedList<Trig>();

            functionList.AddFirst(new Trig(FType.sin, Vector3.up * 2.5f, 2.5f, 2.5f));
            functionList.AddFirst(new Trig(FType.cos, Vector3.right * 5f, 5f, 0f));
        }

        private void FixedUpdate()
        {
            Vector3 off = Vector3.zero;

            foreach (Trig t in functionList)
                off += t.value;

            rb.MovePosition(origin + off);
        }

        public class Trig
        {
            FType t;
            Vector3 offset;
            float cycle;
            float phase;
            float center;
            float start;

            public Trig(FType t, Vector3 offset, float cycle, float center)
            {
                this.t = t;
                this.offset = offset;
                this.cycle = cycle;
                this.center = center;
                this.start = Time.fixedDeltaTime;
            }

            public Vector3 value
            {
                get
                {
                    switch (t)
                    {
                        case FType.sin:
                            return offset * Mathf.Sin((Time.time - start) * 2f * Mathf.PI / cycle) + offset.normalized * center;
                        case FType.cos:
                            return offset * Mathf.Cos((Time.time - start) * 2f * Mathf.PI / cycle) + offset.normalized * center;
                        case FType.tan:
                            return offset * Mathf.Tan((Time.time - start) * 2f * Mathf.PI / cycle) + offset.normalized * center;
                        default:
                            return Vector3.zero;
                    }
                }
            }
        }
    }
}
