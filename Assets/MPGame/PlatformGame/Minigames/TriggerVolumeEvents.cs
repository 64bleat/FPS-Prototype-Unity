using UnityEngine;
using UnityEngine.Events;

namespace Minigames
{
    public class TriggerVolumeEvents : MonoBehaviour
    {
        public UnityEvent<GameObject, GameObject> onTriggerEnter;
        public UnityEvent<GameObject, GameObject> onTriggerExit;

        private void OnTriggerEnter(Collider other)
        {
            onTriggerEnter?.Invoke(other.gameObject, gameObject);
        }

        private void OnTriggerExit(Collider other)
        {
            onTriggerExit?.Invoke(other.gameObject, gameObject);
        }
    }
}
