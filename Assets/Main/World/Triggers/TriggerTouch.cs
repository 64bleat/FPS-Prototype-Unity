using MPCore;
using UnityEngine;
using UnityEngine.Events;

namespace MPWorld
{
    public class TriggerTouch : MonoBehaviour, ITouchable
    {
        public float retriggerTime = 0.1f;
        public float triggerDelay = 0f;
        public UnityEvent interactEvent;

        private float lastEventTime = 0f;

        public virtual void OnTouch(GameObject instigator, Collision c)
        {
            if (Time.time - lastEventTime > retriggerTime)
            {
                Invoke("DoEvent", triggerDelay);
                lastEventTime = Time.time + triggerDelay;
            }
        }

        #pragma warning disable IDE0051
        private void DoEvent()
        {
            interactEvent.Invoke();
        }
    }
}
