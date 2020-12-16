using MPCore.MPCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MPCore.MPCore
{
    public class MiniGame : MonoBehaviour
    {
        public StateMachine State { get; } = new StateMachine();
        public GameObject Player { get; internal set; } = null;

        private void Update()
        {
            State.Update();
        }

        private void FixedUpdate()
        {
            State.FixedUpdate();
        }

        public virtual bool GameStart(GameObject player)
        {
            return false;
        }

        public virtual bool GameEnd(GameObject player)
        {
            return false;
        }

        public virtual bool GameReset(GameObject player)
        {
            return false;
        }
    }
}
