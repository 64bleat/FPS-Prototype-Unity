using TMPro;
using UnityEngine;

namespace MPGUI
{
    public class DropdownField : MonoBehaviour, IClickable
    {
        [SerializeField] protected TextMeshProUGUI valueText;
        [SerializeField] protected TextMeshProUGUI description;
        [SerializeField] protected RectTransform dropPosition;
        [SerializeField] protected Dropdown dropdown;

        private void OnEnable()
        {
            InitField();
        }

        protected virtual void InitField()
        {

        }

        protected virtual void OpenMenu()
        {

        }

        public void OnMouseClick(MouseInfo mouse) 
        {
            OpenMenu();
        }

        public void OnMouseRelease(MouseInfo mouse) { }
        public void OnMouseHover(MouseInfo mouse) { }
        public void OnMousePress(MouseInfo mouse) { }
        public void OnMouseHold(MouseInfo mouse) { }
    }
}
