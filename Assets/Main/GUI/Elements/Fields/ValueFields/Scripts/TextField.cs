using MPCore;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace MPGUI
{
    public class TextField : MonoBehaviour, IGUISelectable
    {
        [SerializeField] private int _charCount = 4;
        [SerializeField] private TextMeshProUGUI _fieldValueText;
        [SerializeField] protected TextMeshProUGUI _fieldNameText;

        private string _text;
        protected string _recoveryText;

        private static readonly char[] _curAnimation = new char[] { ' ', '|' };

        private void Awake()
        {
            _text = _fieldValueText.text;
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
            this._text = valueString;
            _recoveryText = valueString;

            if (name != null)
                _fieldNameText.SetText(name);

            _fieldValueText.SetText(valueString);
        }

        private void EnterText()
        {
            char cur = _curAnimation[(int)(Time.unscaledTime * 4) % _curAnimation.Length];
            string frameInput = Input.inputString;

            if (!frameInput.Contains("\b"))
            {
                if (_text.Length < _charCount)
                    _text += frameInput.Substring(0, Mathf.Min(frameInput.Length, _charCount - _text.Length));
            }
            else if (_text.Length > 0)
                _text = _text.Substring(0, _text.Length - 1);

            _fieldValueText.SetText($"{_text}{cur}");     
        }

        private void CommitInput()
        {
            ManualDeselect();
        }

        private void CancelInput()
        {
            _text = _recoveryText;
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
            _recoveryText = _text;
        }

        public void OnDeselect()
        {
            enabled = false;

            ParseValue(_text);
            InitValue();
        }
    }
}
