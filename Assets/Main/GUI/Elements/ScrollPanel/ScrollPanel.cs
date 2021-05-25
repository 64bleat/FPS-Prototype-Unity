using MPCore;
using UnityEngine;

namespace MPGUI
{
    /// <summary>
    ///     A viewport panel with horizontal and vertical scroll bars
    /// </summary>
    public class ScrollPanel : MonoBehaviour, IClickable
    {
        [SerializeField] private float scrollDistance = 16f;
        [SerializeField] private RectTransform viewPanel;
        [SerializeField] private RectTransform verticalScrollBar;
        [SerializeField] private RectTransform verticalScrollArea;
        [SerializeField] private RectTransform verticalScrollButton;
        [SerializeField] private RectTransform horizontalScrollArea;
        [SerializeField] private RectTransform horizontalScrollButton;
        private RectTransform scrollPanel;
        private InputManager input;

        private void Awake()
        {
            scrollPanel = transform as RectTransform;
            input = GetComponentInParent<InputManager>();
        }

        private void OnEnable()
        {
            Update();
        }

        private void Update()
        {
            if (!Validate())
                return;

            RectTransform content = viewPanel.GetChild(0) as RectTransform;

            if (verticalScrollBar)
            {
                float viewHeight = viewPanel.rect.height;
                float viewWidth = scrollPanel.rect.width;
                float contentHeight = Mathf.Max(float.Epsilon, content.rect.height);
                bool isVerticalActive = contentHeight > viewHeight;
                float scaleRatio = Mathf.Clamp01(viewHeight / contentHeight);
                float areaHeight = verticalScrollArea.rect.height;
                float buttonHeight = Mathf.Max(scaleRatio * areaHeight, 1f);
                float buttonWidth = verticalScrollButton.sizeDelta.x;

                if (isVerticalActive)
                    viewWidth -= verticalScrollBar.rect.width;

                // Set Vertical Scroll Visibility
                verticalScrollBar.gameObject.SetActive(isVerticalActive);
                viewPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, viewWidth);

                // Scale Vertical Scroll Button
                verticalScrollButton.sizeDelta = new Vector2(buttonWidth, buttonHeight);

                // Re-Orient Vertical Scroll
                MoveVertical(0);
            }

            //Rescale Horizontal Scroll
            if (horizontalScrollArea && horizontalScrollButton)
            {
                float left = horizontalScrollButton.offsetMin.x;
                float length = Mathf.Max(Mathf.Clamp01(viewPanel.rect.width / content.rect.width) * horizontalScrollArea.rect.width, 8);

                horizontalScrollButton.offsetMax = new Vector2(left + length, horizontalScrollButton.offsetMax.y);
                horizontalScrollButton.anchoredPosition = new Vector2(Mathf.Clamp(horizontalScrollButton.anchoredPosition.x, 0, horizontalScrollArea.rect.width - horizontalScrollButton.rect.width), 0);
                content.anchoredPosition = new Vector2(-horizontalScrollButton.offsetMin.x / horizontalScrollArea.rect.width * content.rect.width, content.anchoredPosition.y);
            }
        }

        public void OnMouseClick(MouseInfo mouse)
        {
        }
        public void OnMouseHold(MouseInfo mouse)
        {
            RectTransform button = mouse.downInfo.gameObject.transform as RectTransform;

            // Move Vertical Scroll Button
            if (button == verticalScrollButton || button == verticalScrollArea)
            {
                float mouseDelta = Vector3.Dot(input.MousePositionDelta, verticalScrollArea.up);

                MoveVertical(mouseDelta);
            }
            // Move Horizontal Scroll Button
            else if (button == horizontalScrollButton)
            {
                float max = horizontalScrollArea.rect.width - horizontalScrollButton.rect.width;
                horizontalScrollButton.anchoredPosition = new Vector2(Mathf.Clamp(horizontalScrollButton.anchoredPosition.x + Vector3.Dot(input.MousePositionDelta, horizontalScrollArea.right), 0, max), 0);
            }
        }
        public void OnMouseHover(MouseInfo mouse)
        {
            float scrollDelta = Input.mouseScrollDelta.y * scrollDistance;

            MoveVertical(scrollDelta);
        }
        public void OnMousePress(MouseInfo mouse)
        {
            RectTransform button = mouse.downInfo.gameObject.transform as RectTransform;

            if (button == verticalScrollArea)
            {
                Vector3 relativeMouse = verticalScrollArea.parent.InverseTransformPoint(mouse.downInfo.screenPosition);
                float clickPosition = relativeMouse.y - verticalScrollArea.offsetMax.y;
                float buttonHeight = verticalScrollButton.sizeDelta.y;
                float desiredPosition = clickPosition + buttonHeight / 2;
                float deltaPosition = desiredPosition - verticalScrollButton.offsetMax.y;
                MoveVertical(deltaPosition);
            }
        }
        public void OnMouseRelease(MouseInfo mouse) { }

        public void MoveVertical(float deltaDistance)
        {
            RectTransform content = viewPanel.GetChild(0) as RectTransform;
            float positionDesired = verticalScrollButton.anchoredPosition.y + deltaDistance;
            float positionMax = verticalScrollArea.rect.height - verticalScrollButton.rect.height;
            float positionClamped = Mathf.Clamp(positionDesired, -positionMax, 0);

            float targetHeight = content.rect.height;
            float areaHeight = verticalScrollArea.rect.height;
            float buttonTop = verticalScrollButton.offsetMax.y;
            float targetPositionY = -buttonTop / areaHeight * targetHeight;

            verticalScrollButton.anchoredPosition = new Vector2(0, positionClamped);
            content.anchoredPosition = new Vector2(content.anchoredPosition.x, targetPositionY);
        }

        private bool Validate()
        {
            int viewChildren = viewPanel.childCount;
            bool pass = viewPanel && viewChildren == 1;

            if (pass)
                return true;

            Debug.LogError($"Scroll Panel {gameObject.name} has {viewChildren} children but must only have 1.", gameObject);

            enabled = false;

            return false;
        }
    }
}
