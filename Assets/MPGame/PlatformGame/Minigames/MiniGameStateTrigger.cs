using UnityEngine;

namespace MPCore
{
    public enum SwitchTo { Start, Stop, Reset }
    public enum CollisionType { Enter, Exit, Stay };

    public class MiniGameStateTrigger : MonoBehaviour
    {
        public Checkmate game;
        public SwitchTo switchStateTo = SwitchTo.Start;
        public CollisionType onCollisionType = CollisionType.Enter;

        //private void SwitchGame(GameObject go)
        //{
        //    switch(switchStateTo)
        //    {
        //        case SwitchTo.Start: game.GameStart(go); return;
        //        case SwitchTo.Stop: game.GameEnd(go); return;
        //        case SwitchTo.Reset: game.GameReset(go); return;
        //    }
        //}

        //// ENTER //////////////////////////////////////////////////////////////
        //private void OnTriggerEnter(Collider other)
        //{
        //    if (other && other.gameObject && onCollisionType == CollisionType.Enter)
        //        SwitchGame(other.gameObject);
        //}

        //private void OnCollisionEnter(Collision collision)
        //{
        //    if (collision != null && collision.gameObject && onCollisionType == CollisionType.Enter)
        //        SwitchGame(collision.gameObject);
        //}

        //// STAY ///////////////////////////////////////////////////////////////
        //private void OnTriggerStay(Collider other)
        //{
        //    if(other && other.gameObject && onCollisionType == CollisionType.Stay)
        //        SwitchGame(other.gameObject);
        //}

        //private void OnCollisionStay(Collision collision)
        //{
        //    if (collision != null && collision.gameObject && onCollisionType == CollisionType.Stay)
        //        SwitchGame(collision.gameObject);
        //}

        //// LEAVE //////////////////////////////////////////////////////////////
        //private void OnTriggerExit(Collider other)
        //{
        //    if (other && other.gameObject && onCollisionType == CollisionType.Exit)
        //        SwitchGame(other.gameObject);
        //}

        //private void OnCollisionExit(Collision collision)
        //{
        //    if (collision != null && collision.gameObject && onCollisionType == CollisionType.Exit)
        //        SwitchGame(collision.gameObject);
        //}
    }
}
