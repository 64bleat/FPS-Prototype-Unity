using MPCore;
using System;
using TMPro;
using UnityEngine;

namespace MPGUI
{
    public class GUIFloatButton : MonoBehaviour
    {
        public float value;
        public TextMeshProUGUI description;
        public TextMeshProUGUI valueName;
        public Action<float> OnValueChange;

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

        public void SetValue(float f)
        {
            float oldVal = value;

            value = f;
            valueName.SetText(f.ToString());

            if (f != oldVal)
                OnValueChange?.Invoke(f);
        }

        private void Deselect()
        {
            if(GetComponent<GUISelectableEvents>() is var selector && selector)
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

            if (float.TryParse(inputString, out float f))
                SetValue(f);
            else
                SetValue(value);

            awaitingInput = false;
        }
    }
}