using System.Collections;
using System.Collections.Generic;
    using MPCore;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MPGUI
{
    public class HoverHelp : MonoBehaviour, IClickable
    {
        [SerializeField] [TextArea(4, 16)] private string message;
        [SerializeField] private GameObject _helpBubble;
        [SerializeField] private float waitTime = 0.66f;
        [SerializeField] private float fadeScale = 3f;
        private RectTransform helpBubble;
        private Vector3 lastpoint;
        private float hoverTime = 0f;

        private void Awake()
        {
            helpBubble = Instantiate(_helpBubble, transform).transform as RectTransform;
            helpBubble.gameObject.SetActive(false);

            if (helpBubble.TryGetComponentInChildren(out TextMeshProUGUI text))
                text.SetText(message);
        }

        private void Update()
        {
            helpBubble.position = lastpoint;
            float fade = Mathf.Clamp01((hoverTime - waitTime) * fadeScale);

            if (helpBubble.transform.TryGetComponentInChildren(out Image image))
            {
                Color c = image.color;
                c.a = fade;
                image.color = c;
            }

            if(helpBubble.transform.TryGetComponentInChildren(out TextMeshProUGUI text))
            {
                Color c = text.color;
                c.a = fade;
                text.color = c;
            }
        }

        public void OnMouseHover(MouseInfo mouse)
        {
            Vector3 screenPos = mouse.downInfo.screenPosition;
            bool ready = (lastpoint - screenPos).sqrMagnitude < 0.01f;

            helpBubble.gameObject.SetActive(ready);
            enabled = ready;


            if (ready)
                hoverTime += Time.unscaledDeltaTime;
            else
                hoverTime = 0f;

            lastpoint = screenPos;
        }
        public void OnMouseHold(MouseInfo mouse) { }
        public void OnMouseClick(MouseInfo mouse) { }
        public void OnMousePress(MouseInfo mouse) { }
        public void OnMouseRelease(MouseInfo mouse) { }
    }
}
