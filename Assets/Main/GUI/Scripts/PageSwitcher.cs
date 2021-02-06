using MPCore;
using System;
using System.Collections;
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
            // Bind Keys
            if (GetComponentInParent<InputManager>() is var input && input)
                foreach (var bind in switches)
                    input.Bind(bind.switchKey.name, () => Switch(bind.switchTo), this, KeyPressType.Down);
        }

        private void OnDisable()
        {
            // Unbind Keys
            if (GetComponentInParent<InputManager>() is var input && input)
                input.Unbind(this);
        }

        /// <summary> Make all other transforms in the target's child group inactive. </summary>
        public void Switch(PageSwitcher target)
        {
            if (target && target.transform.parent is var parent && parent)
                for (int i = 0, ie = parent.childCount; i < ie; i++)
                    if (parent.GetChild(i) is var t && t.GetComponent<PageSwitcher>())
                        t.gameObject.SetActive(t.Equals(target.transform));

            if (target.mainPage)
                target.mainPage.SwitchToThis();
        }

        public void SwitchToThis()
        {
            Switch(this);
        }
    }
}
