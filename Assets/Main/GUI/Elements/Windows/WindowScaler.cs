using MPCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MPGUI
{
    public class WindowScaler : MonoBehaviour, IClickable
    {
        [SerializeField] private Vector2 minDimensions = new Vector2(100, 50);
        [SerializeField] private RectTransform
            left,
            right,
            top,
            bottom,
            topLeft,
            topRight,
            bottomLeft,
            bottomRight,
            title;

        private RectTransform rt;
        private InputManager input;
        private Vector3 cursorOffset;
        private float canvasScale;

        private void Awake()
        {
            rt = transform as RectTransform;
            input = GetComponentInParent<InputManager>();

            ClampSize();
        }

        private void SetPosition(Vector3 position)
        {
            position -= cursorOffset;
            position = ConstrainPosition(position);
            rt.position = position;
        }

        /// <summary>
        /// Constrains the position to fit inside the parent <c>RectTransform</c>
        /// </summary>
        private Vector3 ConstrainPosition(Vector3 position)
        {
            RectTransform parent = transform.parent as RectTransform;
            Rect parentRect = parent.rect;
            Rect windowRect = rt.rect;
            Vector2 deltaMin = (Vector2)position + windowRect.min * canvasScale;
            Vector2 deltaMax = (Vector2)position + (windowRect.max - parentRect.size) * canvasScale;

            position -= Vector3.Min(Vector3.zero, deltaMin);
            position -= Vector3.Max(Vector3.zero, deltaMax);

            return position;
        }

        private void ClampSize()
        {
            RectTransform parent = transform.parent as RectTransform;
            Rect parentRect = parent.rect;
            Rect windowRect = rt.rect;

            rt.sizeDelta = Vector2.Min(parentRect.size, windowRect.size);
        }

        public void OnMouseHold(MouseInfo mouse)
        {
            Transform focus = mouse.downInfo.gameObject.transform;
            Vector3 mouseDelta = new Vector3(input.MousePositionDelta.x, input.MousePositionDelta.y, 0);
            Vector2 delta = new Vector2(
                Vector3.Dot(mouseDelta, rt.right),
                Vector3.Dot(mouseDelta, rt.up));
            Vector2 deltaOffsetMin = Vector3.zero;
            Vector2 deltaOffsetMax = Vector3.zero;
            Rect current = rt.rect;
            Vector2 maxDelta = minDimensions - current.size;

            //Pan
            if (focus == title)
                SetPosition(mouse.holdInfo.screenPosition);

            //left
            if (focus == left
                || focus == topLeft
                || focus == bottomLeft)
                deltaOffsetMin.x = delta.x;

            //Bottom
            if (focus == bottom
                || focus == bottomLeft
                || focus == bottomRight)
                deltaOffsetMin.y = delta.y;

            //right
            if (focus == right
                || focus == topRight
                || focus == bottomRight)
                deltaOffsetMax.x = delta.x;

            //Top
            if (focus == top
                || focus == topLeft
                || focus == topRight)
                deltaOffsetMax.y = delta.y;

            deltaOffsetMax = Vector2.Max(deltaOffsetMax, maxDelta);
            deltaOffsetMin = Vector2.Min(deltaOffsetMin, -maxDelta);

            rt.offsetMin += deltaOffsetMin;
            rt.offsetMax += deltaOffsetMax;
        }
        public void OnMouseClick(MouseInfo mouse) { }
        public void OnMouseHover(MouseInfo mouse) { }
        public void OnMousePress(MouseInfo mouse) 
        {
            canvasScale = 1;

            if (transform.TryGetComponentInParent(out CanvasScaler cs))
                canvasScale = cs.scaleFactor;

            cursorOffset = transform.InverseTransformPoint(mouse.downInfo.screenPosition) * canvasScale;
        }
        public void OnMouseRelease(MouseInfo mouse) { }
    }
}
