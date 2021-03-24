using MPCore;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MPGUI
{
    /// <summary> Activates a given GameObject and deactivates all other siblings. </summary>
    public class Page : MonoBehaviour
    {
        public List<SwitchBind> switches = new List<SwitchBind>();
        [FormerlySerializedAs("mainPage")]
        public Page startingSubPage;

        [System.Serializable]
        public class SwitchBind
        {
            public KeyBind switchKey;
            public Page switchTo;
        }

        private void OnEnable()
        {
            if (gameObject.TryGetComponentInParent(out InputManager input))
                foreach (SwitchBind bind in switches)
                    input.Bind(bind.switchKey.name, bind.switchTo.SwitchToThis, this, KeyPressType.Down);

            if (startingSubPage)
                startingSubPage.SwitchToThis();
        }

        private void OnDisable()
        {
            if (gameObject.TryGetComponentInParent(out InputManager input))
                input.Unbind(this);
        }

        /// <summary> Make all other transforms in the target's child group inactive. </summary>
        private void Switch(Page target)
        {
            Transform parent = target.transform.parent;

            if(parent)
                for (int i = 0, count = parent.childCount; i < count; i++)
                    if (parent.GetChild(i).TryGetComponent(out Page sibPage))
                        sibPage.gameObject.SetActive(sibPage == this);

            if (target.startingSubPage)
                target.startingSubPage.SwitchToThis();
        }

        public void SwitchToThis()
        {
            Switch(this);
        }
    }
}
