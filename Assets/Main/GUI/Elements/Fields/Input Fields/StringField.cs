using MPCore;
using TMPro;
using UnityEngine;

namespace MPGUI
{
    public class StringField : MonoBehaviour, IGUISelectable
    {
        [SerializeField] protected int charCount = 4;
        [SerializeField] protected TextMeshProUGUI valueText;
        [SerializeField] protected TextMeshProUGUI description;

        private string value;
        protected string recovery;

        private static readonly char[] cursorAnim = new char[] { ' ', '|' };

        private void Awake()
        {
            value = valueText.text;
            InitValue();
        }

        private void OnDisable()
        {
            if (enabled)
                ManualDeselect();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                CancelInput();
            else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                CommitInput();
            else
                EnterText();
        }

        protected virtual void InitValue()
        {

        }

        protected virtual void ParseValue(string value)
        {

        }

        public void SetField(string valueString, string name = null)
        {
            this.value = valueString;
            recovery = valueString;

            if (name != null)
                description.SetText(name);

            valueText.SetText(valueString);
        }

        private void EnterText()
        {
            char cur = cursorAnim[(int)(Time.unscaledTime * 4) % cursorAnim.Length];
            string frameInput = Input.inputString;

            if (!frameInput.Contains("\b"))
            {
                if (value.Length < charCount)
                    value += frameInput.Substring(0, Mathf.Min(frameInput.Length, charCount - value.Length));
            }
            else if (value.Length > 0)
                value = value.Substring(0, value.Length - 1);

            valueText.SetText($"{value}{cur}");     
        }

        private void CommitInput()
        {
            ManualDeselect();
        }

        private void CancelInput()
        {
            value = recovery;
            ManualDeselect();
        }

        private void ManualDeselect()
        {
            if (gameObject.TryGetComponentInParent(out GUIInputManager ginput))
                ginput.Deselect(this);
        }

        public void OnSelect()
        {
            enabled = true;
            recovery = value;
        }

        public void OnDeselect()
        {
            enabled = false;

            ParseValue(value);
        }
    }
}
