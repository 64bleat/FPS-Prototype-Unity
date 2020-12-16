using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MPGUI
{
    /// <summary>
    /// Assign events to a button click, or just a button animator for other IGUIClickables
    /// </summary>
    public class GUIButton : MonoBehaviour, IGUIClickable
    {
        public Sprite pressImage = null;
        public bool directClicksOnly = false;
        public UnityEvent clickEvents = null;

        private Image image;
        private Sprite unpressSprite;

        private void Awake()
        {
            image = GetComponent<Image>();

            if(image)
                unpressSprite = image.sprite;
        }

        public void OnMouseClick(MouseInfo mouse)
        {
            if(!directClicksOnly || gameObject.Equals(mouse.upInfo.gameObject))
                clickEvents.Invoke();

            if(image && unpressSprite)
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
            if(image && pressImage)
                image.sprite = pressImage;
        }

        public void OnMouseRelease(MouseInfo mouse)
        {
            if(image)
                image.sprite = unpressSprite;
        }
    }
}
