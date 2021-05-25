using MPCore;
using UnityEngine;
using UnityEngine.Events;

namespace MPGUI
{
    public class FloatButton : AbstractValueButton
    {
        public float value;
        public string displayFormat = "F2";
        public UnityEvent<float> OnValueChange;

        private string inputString;

        private void OnValidate()
        {
            SetValueText(value);
        }

        private void OnDisable()
        {
            inputString = "";
            SetValueText(value);
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

                SetValueText(text);
            }
        }

        public void SetValue(float value)
        {
            if (this.value != value)
                OnValueChange?.Invoke(value);

            SetValueText(value);

            this.value = value;
        }

        public void SetValueText(float value)
        {
            SetValueText(value.ToString(displayFormat));
        }

        private void Deselect()
        {
            if(TryGetComponent(out GUISelectableEvents selector))
                foreach (GUIInputManager ginput in GetComponentsInParent<GUIInputManager>())
                    ginput.Deselect(selector);

            enabled = false;
        }

        public void BeginReassignment()
        {
            if (gameObject.TryGetComponentInParent(out InputManager input))
                input.enabled = false;

            inputString = value.ToString();
            enabled = true;
        }

        public void CommitReassignment()
        {
            if(gameObject.TryGetComponentInParent(out InputManager input))
                input.enabled = true;

            if (float.TryParse(inputString, out float f))
                SetValue(f);
            else
                SetValue(value);

            enabled = false;
        }
    }
}