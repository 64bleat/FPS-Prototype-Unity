using MPCore;
using System.Collections.Generic;
using UnityEngine;

namespace MPGUI
{
    /// <summary> Activates a given GameObject and deactivates all other siblings. </summary>
    public class PageSwitcher : MonoBehaviour
    {
        public List<SwitchBind> switches = new List<SwitchBind>();
        public PageSwitcher mainPage;

        [System.Serializable]
        public class SwitchBind
        {
            public KeyBind switchKey;
            public PageSwitcher switchTo;
        }

        private void OnEnable()
        {
            if (gameObject.TryGetComponentInParent(out InputManager input))
                foreach (SwitchBind bind in switches)
                    input.Bind(bind.switchKey.name, bind.switchTo.SwitchToThis, this, KeyPressType.Down);
        }

        private void OnDisable()
        {
            if (gameObject.TryGetComponentInParent(out InputManager input))
                input.Unbind(this);
        }

        /// <summary> Make all other transforms in the target's child group inactive. </summary>
        public void Switch(PageSwitcher target)
        {
            Transform parent = target.transform.parent;

            if(parent)
                for (int i = 0, count = parent.childCount; i < count; i++)
                    if (parent.GetChild(i).TryGetComponent(out PageSwitcher ps))
                        ps.gameObject.SetActive(ps == this);

            if (target.mainPage)
                target.mainPage.SwitchToThis();
        }

        public void SwitchToThis()
        {
            Switch(this);
        }
    }
}
