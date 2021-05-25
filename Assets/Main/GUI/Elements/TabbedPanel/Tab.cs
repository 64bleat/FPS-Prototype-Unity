using UnityEngine;

namespace MPGUI
{
    public class Tab : MonoBehaviour, IClickable
    {
        [SerializeField] private GameObject selectView;
        [SerializeField] private GameObject panel;

        private void Awake()
        {
            if (transform.GetSiblingIndex() == 0)
                Select(0);
        }

        private void Select(int index)
        {
            Transform parent = transform.parent;
            int count = parent.childCount;

            for (int i = 0; i < count; i++)
                if (parent.GetChild(i).TryGetComponent(out Tab tab))
                    tab.selectView.SetActive(i == index);

            SelectPanel(index);
        }

        private void SelectPanel(int index)
        {
            Transform parent = panel.transform.parent;
            int count = parent.childCount;

            for (int i = 0; i < count; i++)
                parent.GetChild(i).gameObject.SetActive(i == index);
        }

        public void OnMousePress(MouseInfo mouse) 
        {
            Select(transform.GetSiblingIndex());
        }

        public void OnMouseClick(MouseInfo mouse) { }
        public void OnMouseHold(MouseInfo mouse) { }
        public void OnMouseHover(MouseInfo mouse) { }
        public void OnMouseRelease(MouseInfo mouse) { }
    }
}
