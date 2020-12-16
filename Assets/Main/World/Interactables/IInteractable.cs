using UnityEngine;

namespace MPCore
{
    public interface IInteractable
    {
        void OnInteractStart(GameObject other, RaycastHit hit);
        void OnInteractHold(GameObject other, RaycastHit hit);
        void OnInteractEnd(GameObject other, RaycastHit hit);
    }
}
