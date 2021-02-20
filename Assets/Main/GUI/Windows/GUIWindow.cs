using MPCore;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MPGUI
{
    public class GUIWindow : MonoBehaviour, IGUIClickable
    {
        [SerializeField] private Vector2 minDimensions = new Vector2(100, 50);
        [SerializeField] private Sprite activeGradient = null;
        [SerializeField] private Sprite inactiveGradient = null;
        [SerializeField] private Color activeTextColor = new Color(1, 1, 1, 1);
        [SerializeField] private Color inactiveTextColor = new Color(0.75f, 0.75f, 0.75f, 1);
        [SerializeField] private RectTransform
            edgeLeft = null,
            edgeRight = null,
            edgeUp = null,
            edgeDown = null,
            cornerTopLeft = null,
            cornerTopRight = null,
            cornerBottomLeft = null,
            cornerBottomRight = null;
        public RectTransform
            title = null,
            panel = null;

        private InputManager input;
        private RectTransform rect;
        private bool active = true;

        private void Awake()
        {
            input = GetComponentInParent<InputManager>();
            rect = transform as RectTransform;
        }

        public void OnMouseClick(MouseInfo mouse) { }
        public void OnMouseHold(MouseInfo mouse)
        {
            Transform focus = mouse.downInfo.gameObject.transform;
            Vector3 mouseDelta = new Vector3(input.MousePositionDelta.x, input.MousePositionDelta.y, 0);

            //Pan
            if (focus.Equals(title))
                AnchoredPosition += input.MousePositionDelta;
            //left
            if (focus.Equals(edgeLeft) || focus.Equals(cornerTopLeft) || focus.Equals(cornerBottomLeft))
                OffsetMin += new Vector2(1, 0) * Vector3.Dot(mouseDelta, rect.right);
            //Bottom
            if (focus.Equals(edgeDown) || focus.Equals(cornerBottomLeft) || focus.Equals(cornerBottomRight))
                OffsetMin += new Vector2(0, 1) * Vector3.Dot(mouseDelta, rect.up);
            //right
            if (focus.Equals(edgeRight) || focus.Equals(cornerTopRight) || focus.Equals(cornerBottomRight))
                OffsetMax += new Vector2(1, 0) * Vector3.Dot(mouseDelta, rect.right);
            //Top
            if (focus.Equals(edgeUp) || focus.Equals(cornerTopLeft) || focus.Equals(cornerTopRight))
                OffsetMax += new Vector2(0, 1) * Vector3.Dot(mouseDelta, rect.up);
        }
        public void OnMouseHover(MouseInfo mouse) { }
        public void OnMouseRelease(MouseInfo mouse) { }
        public void OnMousePress(MouseInfo mouse)
        {
            SetActive(true);
        }

        private void SetActive(bool value)
        {
            if (value || value != active)
            {
                Image image = title.GetComponent<Image>();
                TextMeshProUGUI text = title.GetComponentInChildren<TextMeshProUGUI>();

                if (image)
                    image.sprite = value ? activeGradient : inactiveGradient;

                if (text)
                    text.color = value ? activeTextColor : inactiveTextColor;

                if (value == true)
                {
                    transform.SetAsLastSibling();

                    if (transform.parent)
                        foreach (GUIWindow w in transform.parent.GetComponentsInChildren<GUIWindow>())
                            if (!w.Equals(this))
                                w.SetActive(false);
                }

                active = value;
            }
        }

        public Vector2 OffsetMin
        {
            get => rect.offsetMin;
            set
            {
                rect.offsetMin = new Vector2(
                    Mathf.Clamp(value.x, 0, rect.offsetMax.x - minDimensions.x), 
                    Mathf.Clamp(value.y, 0, rect.offsetMax.y - minDimensions.y));
            }
        }

        public Vector2 OffsetMax
        {
            get => rect.offsetMax;
            set
            {
                Rect parent = (transform.parent as RectTransform).rect;

                rect.offsetMax = new Vector2(
                    Mathf.Clamp(value.x, rect.offsetMin.x + minDimensions.x, parent.width),
                    Mathf.Clamp(value.y, rect.offsetMin.y + minDimensions.y, parent.height));
            }
        }

        public Vector2 AnchoredPosition
        {
            get => rect.anchoredPosition;
            set
            {
                Rect parent = (rect.parent as RectTransform).rect;
                Rect me = rect.rect;

                rect.anchoredPosition = new Vector2(
                    Mathf.Clamp(value.x, 0, parent.width - me.width),
                    Mathf.Clamp(value.y, 0, parent.height - me.height));
            }
        }
    }
}
