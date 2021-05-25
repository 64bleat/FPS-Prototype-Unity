using TMPro;
using UnityEngine;

namespace MPCore
{
    [RequireComponent(typeof(RectTransform))]
    public class TextFit : MonoBehaviour
    {
        [SerializeField] private float padding = 0;
        [SerializeField] private float minWidth = 0;
        [SerializeField] private float maxWidth = 10000;

        private void Update()
        {
            Resize();
        }

        public void Resize()
        {
            if(transform.TryGetComponentInChildren(out TextMeshProUGUI text))
            {
                RectTransform rect = transform as RectTransform;
                Bounds bound = text.textBounds;
                Vector2 size = rect.sizeDelta;

                size.x = Mathf.Clamp(bound.size.x + padding, minWidth, maxWidth);
                rect.sizeDelta = size;
            }
        }
    }
}
