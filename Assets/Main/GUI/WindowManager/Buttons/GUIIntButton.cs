using MPCore;
using System;
using TMPro;
using UnityEngine;

namespace MPGUI
{
    public class GUIIntButton : MonoBehaviour
    {
        public int value;
        public TextMeshProUGUI description;
        public TextMeshProUGUI valueName;
        public Action<int> OnValueChange;

        private bool awaitingInput = false;
        private string inputString;

        private void Awake()
        {
            SetValue(value);
        }

        private void OnDisable()
        {
            inputString = "";
            Deselect();
        }

        private void Update()
        {
            if (awaitingInput)
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
        }

        public void BeginReassignment()
        {
            if (GetComponentInParent<InputManager>() is var input && input)
                input.enabled = false;

            inputString = value.ToString();
            awaitingInput = true;
        }

        public void CommitReassignment()
        {
            if (GetComponentInParent<InputManager>() is var input && input)
                input.enabled = true;

            if (int.TryParse(inputString, out int n))
                SetValue(n);
            else
                SetValue(value);

            awaitingInput = false;
        }
    }
}