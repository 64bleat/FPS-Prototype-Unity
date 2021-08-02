using MPCore;
using UnityEngine;
using UnityEngine.Events;

namespace MPWorld
{
    public class TriggerEntryEvents : MonoBehaviour
    {
        public UnityEvent enterEvents;
        public UnityEvent exitEvents;

        public void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.GetComponentInChildren<MPCore.Character>())
                enterEvents?.Invoke();
        }

        public void OnTriggerExit(Collider other)
        {
            if (other.gameObject.GetComponentInChildren<MPCore.Character>())
                exitEvents?.Invoke();
        }
    }
}
