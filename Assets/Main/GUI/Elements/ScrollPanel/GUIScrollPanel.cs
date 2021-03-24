using MPGUI;
using MPCore;
using UnityEngine;

public class GUIScrollPanel : MonoBehaviour, IClickable
{
    public RectTransform target;
    public RectTransform headerTarget;
    public RectTransform viewPanel;
    public RectTransform verticalScrollBar;
    public RectTransform verticalScrollArea;
    public RectTransform verticalScrollButton;
    public RectTransform horizontalScrollArea;
    public RectTransform horizontalScrollButton;

    private RectTransform rect;
    private InputManager input;

    private void Awake()
    {
        rect = transform as RectTransform;
        input = GetComponentInParent<InputManager>();
    }

    private void Update()
    {
        if (target && viewPanel)
        {
            // Show/Hide Vertical Scroll
            if (verticalScrollBar)
            {
                float size = rect.rect.width;
                verticalScrollBar.gameObject.SetActive(target.rect.height > viewPanel.rect.height);
                size -= verticalScrollBar.gameObject.activeSelf ? verticalScrollBar.rect.width : 0f;
                viewPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
            }

            //Rescale Vertical Scroll
            if (verticalScrollArea && verticalScrollButton)
            {
                float top = verticalScrollButton.offsetMax.y;
                float height = Mathf.Max(Mathf.Clamp01(viewPanel.rect.height / target.rect.height) * verticalScrollArea.rect.height, 8);

                verticalScrollButton.offsetMin = new Vector2(verticalScrollButton.offsetMin.x, top - height);
                verticalScrollButton.anchoredPosition = new Vector2(0, Mathf.Clamp(verticalScrollButton.anchoredPosition.y, -verticalScrollArea.rect.height + verticalScrollButton.rect.height, 0));
                target.anchoredPosition = new Vector2(target.anchoredPosition.x, -verticalScrollButton.offsetMax.y / verticalScrollArea.rect.height * target.rect.height);
            }

            //Rescale Horizontal Scroll
            if (horizontalScrollArea && horizontalScrollButton)
            { 
                float left = horizontalScrollButton.offsetMin.x;
                float length = Mathf.Max(Mathf.Clamp01(viewPanel.rect.width / target.rect.width) * horizontalScrollArea.rect.width, 8);

                horizontalScrollButton.offsetMax = new Vector2(left + length, horizontalScrollButton.offsetMax.y);
                horizontalScrollButton.anchoredPosition = new Vector2(Mathf.Clamp(horizontalScrollButton.anchoredPosition.x, 0, horizontalScrollArea.rect.width - horizontalScrollButton.rect.width), 0);
                target.anchoredPosition = new Vector2(-horizontalScrollButton.offsetMin.x / horizontalScrollArea.rect.width * target.rect.width, target.anchoredPosition.y);
            }

            // Move Table
            //target.anchoredPosition = new Vector2(-horizontalScrollButton.offsetMin.x / horizontalScrollArea.rect.width * target.rect.width, -verticalScrollButton.offsetMax.y / verticalScrollArea.rect.height * target.rect.height);

            // Move Header
            if (headerTarget)
                headerTarget.anchoredPosition = new Vector2(target.anchoredPosition.x, 0);     
        }
    }

    public float YValue 
    { 
        get => throw new System.NotImplementedException(); 
        set => throw new System.NotImplementedException(); 
    }

    public void OnMouseClick(MouseInfo mouse) { }
    public void OnMouseHold(MouseInfo mouse) 
    {
        RectTransform hoverRect = mouse.downInfo.gameObject.transform as RectTransform;

        // Move Vertical Scroll Button
        if (hoverRect.Equals(verticalScrollButton))
        {
            float max = verticalScrollArea.rect.height - verticalScrollButton.rect.height;
            verticalScrollButton.anchoredPosition = new Vector2(0, Mathf.Clamp(verticalScrollButton.anchoredPosition.y + Vector3.Dot(input.MousePositionDelta, verticalScrollArea.up), -max, 0));
        }
        // Move Horizontal Scroll Button
        else if (hoverRect.Equals(horizontalScrollButton))
        {
            float max = horizontalScrollArea.rect.width - horizontalScrollButton.rect.width;
            horizontalScrollButton.anchoredPosition = new Vector2(Mathf.Clamp(horizontalScrollButton.anchoredPosition.x + Vector3.Dot(input.MousePositionDelta, horizontalScrollArea.right), 0, max), 0);
        }
    }
    public void OnMouseHover(MouseInfo mouse) 
    {
        verticalScrollButton.anchoredPosition += Input.mouseScrollDelta * 16;
    }
    public void OnMousePress(MouseInfo mouse) { }
    public void OnMouseRelease(MouseInfo mouse) { }
}
