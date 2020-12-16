using MPCore;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MPGUI
{
    public class GUIWindow : MonoBehaviour, IGUIClickable
    {
        [SerializeField] private Vector2 minDimensions = new Vector2(100, 50);
        [SerializeField] private Sprite 
            activeGradient = null,
            inactiveGradient = null;
        [SerializeField] private Color 
            activeTextColor = new Color(1, 1, 1, 1),
            inactiveTextColor = new Color(0.75f, 0.75f, 0.75f, 1);
        [SerializeField]
        private RectTransform
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
        private RectTransform rtransform;
        private bool active = true;

        public bool Active
        {
            get => active;
            set
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
                                    w.Active = false;
                    }

                    active = value;
                }
            }
        }

        private void Awake()
        {
            input = GetComponentInParent<InputManager>();
            rtransform = transform as RectTransform;
        }

        public Vector2 OffsetMin
        {
            get => rtransform.offsetMin;
            set
            {
                rtransform.offsetMin = new Vector2(Mathf.Clamp(value.x, 0, rtransform.offsetMax.x - minDimensions.x), Mathf.Clamp(value.y, 0, rtransform.offsetMax.y - minDimensions.y));
            }
        }

        public Vector2 OffsetMax
        {
            get => rtransform.offsetMax;
            set
            {
                Rect parent = (transform.parent as RectTransform).rect;

                rtransform.offsetMax = new Vector2(Mathf.Clamp(value.x, rtransform.offsetMin.x + minDimensions.x, parent.width), Mathf.Clamp(value.y, rtransform.offsetMin.y + minDimensions.y, parent.height));
            }
        }

        public Vector2 AnchoredPosition
        {
            get => rtransform.anchoredPosition;
            set
            {
                Rect parent = (rtransform.parent as RectTransform).rect;
                Rect me = rtransform.rect;
                rtransform.anchoredPosition = new Vector2(Mathf.Clamp(value.x, 0, parent.width - me.width), Mathf.Clamp(value.y, 0, parent.height - me.height));
            }
        }

        public void Close()
        {
            Destroy(gameObject);
        }


        #region IGUICLickable
        public void OnMouseClick(MouseInfo mouse) {}

        public void OnMouseHold(MouseInfo mouse)
        {
            Transform mtrans = mouse.downInfo.gameObject.transform;
            Vector3 mouseDelta = new Vector3(input.MousePositionDelta.x, input.MousePositionDelta.y, 0);

            //Pan
            if(mtrans.Equals(title))
                AnchoredPosition += input.MousePositionDelta;
            //left
            if (mtrans.Equals(edgeLeft) || mtrans.Equals(cornerTopLeft) || mtrans.Equals(cornerBottomLeft))
                OffsetMin += new Vector2(1, 0) * Vector3.Dot(mouseDelta, rtransform.right);
            //Bottom
            if (mtrans.Equals(edgeDown) || mtrans.Equals(cornerBottomLeft) || mtrans.Equals(cornerBottomRight))
                OffsetMin += new Vector2(0, 1) * Vector3.Dot(mouseDelta, rtransform.up);
            //right
            if (mtrans.Equals(edgeRight) || mtrans.Equals(cornerTopRight) || mtrans.Equals(cornerBottomRight))
                OffsetMax += new Vector2(1, 0) * Vector3.Dot(mouseDelta, rtransform.right);
            //Top
            if (mtrans.Equals(edgeUp) || mtrans.Equals(cornerTopLeft) || mtrans.Equals(cornerTopRight))
                OffsetMax += new Vector2(0, 1) * Vector3.Dot(mouseDelta, rtransform.up);
        }

        public void OnMouseHover(MouseInfo mouse) { }
        public void OnMouseRelease(MouseInfo mouse) { }

        public void OnMousePress(MouseInfo mouse)
        {
            Active = true;
        }
        #endregion
    }
}
