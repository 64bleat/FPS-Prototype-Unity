using MPCore;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MPGUI
{
    public class KeyBindButton : ValueButton
    {
        private static readonly List<KeyCode> comboBuffer = new List<KeyCode>();

        private KeyBind key;
        private bool awaitingInput = false;

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

                        if (Input.GetKeyDown(k) && !comboBuffer.Contains(k))
                            comboBuffer.Add(k);
                    }

                if (comboBuffer.Count != 0 && wait)
                    CommitReassignment();
            }
        }

        public void SetValue(KeyBind k)
        {
            if (k)
            {
                key = k;
                SetLabel(k.name);
                SetValueText(k.GetComboString());
            }
        }

        public void CancelReassignment()
        {
            awaitingInput = false;
            SetValueText(key.GetComboString());
        }

        public void BeginReassignment()
        {
            if (GetComponentInParent<InputManager>() is var input && input)
                input.enabled = false;

            comboBuffer.Clear();
            awaitingInput = true;
            SetValueText("???");
        }

        private void CommitReassignment()
        {
            if (awaitingInput)
            {
                if (GetComponentInParent<InputManager>() is var input && input)
                    input.enabled = true;

                if (comboBuffer.Count != 0)
                    key.keyCombo = comboBuffer.ToArray();

                awaitingInput = false;
            }

            SetValueText(key.GetComboString());

            foreach (GUIInputManager ginput in GetComponentsInParent<GUIInputManager>())
                ginput.Deselect(GetComponent<IGUISelectable>());
        }
    }
}
