/******************************************************************************      
 * author: David Martinez Copyright 2020 all rights reserved
 * description: A simple interactable that calls UnityEvents.
 *****************************************************************************/
using UnityEngine;
using UnityEngine.Events;

namespace MPCore
{
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
