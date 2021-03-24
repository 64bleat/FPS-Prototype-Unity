using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MPGUI
{
    /// <summary>
    /// Assign events to a button click, or just a button animator for other IGUIClickables
    /// </summary>
    public class Button : MonoBehaviour, IClickable
    {
        [SerializeField] private Sprite pressImage = null;
        [SerializeField] private bool directClicksOnly = false;
        public UnityEvent clickEvents = null;

        private Sprite unpressSprite;

        private void Awake()
        {
            if(TryGetComponent(out Image image))
                unpressSprite = image.sprite;
        }

        public void OnMouseClick(MouseInfo mouse)
        {
            if(!directClicksOnly || gameObject.Equals(mouse.upInfo.gameObject))
                clickEvents.Invoke();

            if(unpressSprite && TryGetComponent(out Image image))
                image.sprite = unpressSprite;
        }

        public void OnMouseHold(MouseInfo mouse)
        {
        }

        public void OnMouseHover(MouseInfo mouse)
        {
        }

        public void OnMousePress(MouseInfo mouse)
        {
            if(pressImage && TryGetComponent(out Image image))
                image.sprite = pressImage;
        }

        public void OnMouseRelease(MouseInfo mouse)
        {
            if(TryGetComponent(out Image image))
                image.sprite = unpressSprite;
        }
    }
}
