using UnityEngine;

namespace MPCore
{
    /// <summary>
    /// Interface for writing Components to be interacted with, usually by pressing 'e'
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// Called when a character begins interacting with the implementing Component's GameObject.
        /// </summary>
        /// <param name="other"> the character that is interacting with this Component</param>
        /// <param name="hit"> the point on the GameObject where the character is interacting </param>
        void OnInteractStart(GameObject other, RaycastHit hit);
        /// <summary>
        /// Called on Interactor.Update on the Component OnInteractStart was called on
        /// while the character holds the interact button and the interaction remains valid.
        /// </summary>
        /// <param name="other"> the character that is interacting with this Component</param>
        /// <param name="hit"> the point on the GameObject where the character is interacting </param>
        void OnInteractHold(GameObject other, RaycastHit hit);
        /// <summary>
        /// Called when the character stops interacting with the Component OnInteractStart was called on
        /// or when that interaction becomes invalid.
        /// </summary>
        /// <remarks>
        /// By default this will not call if the GameObject was destroyed while interacting with it.
        /// If calling this method is critical, add it to OnDestroy in your Component</remarks>
        /// <param name="other"> the character that is interacting with this Component</param>
        /// <param name="hit"> the point on the GameObject where the character is interacting </param>
        void OnInteractEnd(GameObject other, RaycastHit hit);
    }
}
