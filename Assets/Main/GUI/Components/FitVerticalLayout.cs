using UnityEngine;
using UnityEngine.UI;

namespace MPGUI
{
    /// <summary>
    ///     Fits a RectTransform to its vertically aligned children
    /// </summary>
    public class FitVerticalLayout : MonoBehaviour
    {
        private void Update()
        {
            Resize();
        }

        private void Resize()
        {
            RectTransform rt = transform as RectTransform;
            int childCount = transform.childCount;
            Vector2 size = rt.sizeDelta;

            VerticalLayoutGroup v = GetComponent<VerticalLayoutGroup>();
            float spacing = v.spacing;

            size.y = 0;

            for (int i = 0; i < childCount; i++)
            {
                RectTransform child = rt.GetChild(i) as RectTransform;
                
                if(child.gameObject.activeSelf)
                    size.y += child.sizeDelta.y;
            }

            size.y += spacing * (childCount - 1);

            rt.sizeDelta = size;
        }
    }
}
