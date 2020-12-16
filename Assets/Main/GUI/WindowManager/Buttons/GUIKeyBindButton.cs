using MPCore;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

namespace MPGUI
{
    public class GUIKeyBindButton : MonoBehaviour
    {
        public KeyBind key;
        public TextMeshProUGUI description;
        public TextMeshProUGUI keyname;

        private bool awaitingInput = false;
        private readonly List<KeyCode> newCombo = new List<KeyCode>();

        private void Awake()
        {
            SetValue(key);
        }

        private void OnDisable()
        {
            CancelReassignment();
        }

        private void Update()
        {
            if (awaitingInput)
            {
                bool wait = true;

                foreach (KeyCode k in Enum.GetValues(typeof(KeyCode)))
                    if (Input.GetKey(k))
                    {
                        wait = false;

                        if (Input.GetKeyDown(k) && !newCombo.Contains(k))
                            newCombo.Add(k);
                    }

                if (newCombo.Count != 0 && wait)
                    CommitReassignment();
            }
        }

        public void SetValue(KeyBind k)
        {
            if (k)
            {
                key = k;
                description.text = k.name;
                keyname.text = k.GetComboString();
            }
        }

        public void CancelReassignment()
        {
            awaitingInput = false;
            keyname.text = key.GetComboString();
        }

        public void BeginReassignment()
        {
            if (GetComponentInParent<InputManager>() is var input && input)
                input.enabled = false;

            newCombo.Clear();
            awaitingInput = true;
            keyname.text = "???";
        }

        private void CommitReassignment()
        {
            if (awaitingInput)
            {
                if (GetComponentInParent<InputManager>() is var input && input)
                    input.enabled = true;

                if (newCombo.Count != 0)
                    key.keyCombo = newCombo.ToArray();

                awaitingInput = false;
            }

            keyname.text = key.GetComboString();

            foreach (GUIInputManager ginput in GetComponentsInParent<GUIInputManager>())
                ginput.Deselect(GetComponent<IGUISelectable>());
        }
    }
}
