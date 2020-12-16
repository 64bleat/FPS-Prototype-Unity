using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MPGUI
{
    public class GUISelectableEvents : MonoBehaviour, IGUISelectable
    {
        public UnityEvent onSelect;
        public UnityEvent onDeselect;

        private static readonly List<IGUISelectable> selector = new List<IGUISelectable>();

        public void SelectThis()
        {
            if (GetComponentInParent<GUIInputManager>() is var input && input)
            {
                selector.Clear();
                selector.Add(this);
                input.Select(selector);
            }
        }

        public void DeselectThis()
        {
            if (GetComponentInParent<GUIInputManager>() is var input && input)
                input.Deselect(this);
        }

        public void OnDeselect()
        {
            onDeselect?.Invoke();
        }

        public void OnSelect()
        {
            onSelect?.Invoke();
        }
    }
}
