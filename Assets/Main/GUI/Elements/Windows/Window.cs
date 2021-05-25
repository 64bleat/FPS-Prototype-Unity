using MPCore;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MPGUI
{
    /// <summary>
    /// A generic, multi-purpose GUI window
    /// </summary>
    public class Window : MonoBehaviour, IClickable
    {
        [SerializeField] private WindowStyle style;
        [SerializeField] private RectTransform panel;
        [SerializeField] private RectTransform title;

        private bool active = true;

        private void OnEnable()
        {
            SetActive(true);
        }

        public string Title
        {
            get
            {
                if (title.TryGetComponentInChildren(out TextMeshProUGUI text))
                    return text.text;
                else
                    return string.Empty;
            }

            set
            {
                if (title.TryGetComponentInChildren(out TextMeshProUGUI text))
                    text.SetText(value);
            }
        }

        public RectTransform Contents => panel;

        public void CloseWindow()
        {
            int sibling = transform.parent.childCount;
            int thisIndex = transform.GetSiblingIndex();
            Window newTop = null;

            while(sibling-- > 0)
                if(sibling != thisIndex)
                {
                    Transform child = transform.parent.GetChild(sibling);

                    if (child.TryGetComponent(out newTop))
                        break;
                }

            if (newTop)
                newTop.SetActive(true);

            Destroy(gameObject);
        }

        private void SetActive(bool value)
        {
            if (value || value != active)
            {
                if (title.TryGetComponent(out Image titleBackground))
                    titleBackground.sprite = value ? style.activeTitleBackground : style.inactiveTitleBackground;

                if (title.TryGetComponentInChildren(out TextMeshProUGUI titleText))
                    titleText.color = value ? style.activeTextColor : style.inactiveTextColor;

                if (value == true)
                {
                    // Push to Front
                    transform.SetAsLastSibling();

                    // Deactivate Sibling Windows
                    if (transform.parent)
                    {
                        int siblingIndex = transform.GetSiblingIndex();
                        int childCount = transform.parent.childCount;

                        for (int i = 0; i < childCount; i++)
                            if (i != siblingIndex && transform.parent.GetChild(i).TryGetComponent(out Window window))
                                window.SetActive(false);
                    }
                }

                active = value;
            }
        }

        public void OnMouseClick(MouseInfo mouse) { }
        public void OnMouseHold(MouseInfo mouse) { }
        public void OnMouseHover(MouseInfo mouse) { }
        public void OnMouseRelease(MouseInfo mouse) { }
        public void OnMousePress(MouseInfo mouse)
        {
            SetActive(true);
        }

        /// <summary>
        /// Spawns a clone of this <c>Window</c> under the given parent
        /// </summary>
        /// <param name="parent"><c>RectTransform</c> under which to spawn the <c>Window</c></param>
        public void SpawnWindow(RectTransform parent)
        {


            if (TryGetDuplicate(parent, out Window dupe))
            {
                dupe.SetActive(true);
                return;
            }

            GameObject go = Instantiate(gameObject, parent);
            RectTransform rt = go.transform as RectTransform;

            rt.position = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        }

        /// <summary>
        /// Checks children of <c>parent</c> to see if any windows have the same title as this one
        /// </summary>
        /// <param name="parent"><c>RectTransform</c> under which to search for duplicates</param>
        /// <returns><c>true</c> if this window is already spawned under <c>parent</c></returns>
        private bool TryGetDuplicate(RectTransform parent, out Window dupe)
        {
            int count = parent.childCount;


            for(int i = 0; i < count; i++)
            {
                Transform sib = parent.GetChild(i);

                if (sib.TryGetComponent(out Window window))
                    if (window.Title == this.Title)
                    {
                        dupe = window;
                        return true;
                    }
            }

            dupe = null;
            return false;
        }
    }
}
