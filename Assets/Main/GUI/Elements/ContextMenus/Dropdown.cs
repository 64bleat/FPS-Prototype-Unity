using UnityEngine;
using UnityEngine.UI;

namespace MPGUI
{
    public class Dropdown : MonoBehaviour
    {
        private Button owner;

        private void OnDestroy()
        {
            if (owner)
                owner.LockButton(false);
        }

        public ButtonSet SpawnDropdown(RectTransform buttonRT)
        {
            CanvasScaler scaler = buttonRT.GetComponentInParent<CanvasScaler>();
            float canvasScale = scaler ? scaler.scaleFactor : 1;
            Window window = buttonRT.GetComponentInParent<Window>();
            Transform dropParent = window ? window.transform.parent : buttonRT.parent;
            //Vector3 buttonPosition = buttonRT.position;
            //buttonPosition.y -= buttonRT.sizeDelta.y * canvasScale;
            Vector3 buttonPosition = buttonRT.position;
            GameObject dropGO = Instantiate(gameObject, buttonPosition, buttonRT.rotation, dropParent);
            RectTransform dropRT = dropGO.transform as RectTransform;
            Vector2 dropSize = dropRT.sizeDelta;

            dropSize.x = buttonRT.sizeDelta.x;
            dropRT.sizeDelta = dropSize;
            dropRT.SetAsLastSibling();

            if (buttonRT.TryGetComponent(out Button dropB))
            {
                dropB.LockButton(true);

                if (dropGO.TryGetComponent(out Dropdown dropdown))
                    dropdown.owner = dropB;
            }

            return dropGO.GetComponentInChildren<ButtonSet>();
        }
    }
}
