using UnityEngine;
using UnityEngine.Events;

namespace MPCore
{
    public class OnEnableEvents : MonoBehaviour
    {
        public UnityEvent events;
        public UnityEvent disableEvents;

        public void OnEnable()
        {
            events.Invoke();
        }

        public void OnDisable()
        {
            events.Invoke();
        }
    }
}
