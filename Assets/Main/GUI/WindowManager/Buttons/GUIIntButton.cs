﻿using MPCore;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace MPGUI
{
    public class GUIIntButton : MonoBehaviour
    {
        public int value;
        public TextMeshProUGUI description;
        public TextMeshProUGUI valueName;
        public UnityEvent<int> OnValueChange;

        private string inputString;

        private void OnValidate()
        {
            valueName.SetText(value.ToString());
        }

        private void OnDisable()
        {
            inputString = "";
            valueName.SetText(value.ToString());
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                inputString = "";
                Deselect();
            }
            else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                Deselect();
            else
            {
                if (!Input.inputString.Contains("\b"))
                    inputString += Input.inputString;
                else if (inputString.Length > 0)
                    inputString = inputString.Substring(0, inputString.Length - 1);

                string text = inputString;
                text += (int)(Time.unscaledTime * 3) % 2 == 0 ? " " : "|";

                valueName.SetText(text);
            }
        }

        public void SetValue(int n)
        {
            float oldVal = value;

            value = n;
            valueName.SetText(n.ToString());

            if (n != oldVal)
                OnValueChange?.Invoke(n);
        }

        private void Deselect()
        {
            if (GetComponent<GUISelectableEvents>() is var selector && selector)
                foreach (GUIInputManager ginput in GetComponentsInParent<GUIInputManager>())
                    ginput.Deselect(selector);

            enabled = false;
        }

        public void BeginReassignment()
        {
            if (GetComponentInParent<InputManager>() is var input && input)
                input.enabled = false;

            inputString = value.ToString();
            enabled = true;
        }

        public void CommitReassignment()
        {
            if (GetComponentInParent<InputManager>() is var input && input)
                input.enabled = true;

            if (int.TryParse(inputString, out int n))
                SetValue(n);
            else
                SetValue(value);

            enabled = false;
        }
    }
}