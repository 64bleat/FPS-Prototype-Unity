using UnityEngine;
using UnityEngine.Events;

namespace MPWorld
{
    /// <summary>
    /// Called when the gameObject is interacted with
    /// </summary>
    public class InteractEvents : MonoBehaviour, IInteractable
    {
        public UnityEvent startEvents, stayEvents, endEvents;

        public void OnInteractEnd(GameObject other, RaycastHit hit)
        {
            endEvents?.Invoke();
        }
        public void OnInteractHold(GameObject other, RaycastHit hit)
        {
            stayEvents?.Invoke();
        }
        public void OnInteractStart(GameObject other, RaycastHit hit)
        {
            startEvents?.Invoke();
        }
    }
}
