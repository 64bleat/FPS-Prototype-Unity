using MPGUI;
using System;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace MPCore
{
    public abstract class GenericField<T> : MonoBehaviour, IGUISelectable
    {
        [SerializeField] int _charCount = 4;
        [SerializeField] TextMeshProUGUI _textValue;
        [SerializeField] TextMeshProUGUI _textName;

        private string _inputText;
        private string _recoveryText;
        private ReflectionValue<T> _value;

        private static readonly char[] _curAnimation = new char[] { ' ', '|' };

        private void Awake()
        {
            enabled = false;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                CancelInput();
            else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                CommitInput();
            else
                InputText();
        }

        private void DisplayValue()
        {
            if(_value != null)
            {
                _textValue.SetText(Write(_value.Value));
                _textName.SetText(_value.DisplayName);
            }
            else
            {
                _textValue.SetText("ERR");
                _textName.SetText("NO INSTANCE");
            }

            _inputText = _textValue.text;
            _recoveryText = _textValue.text;
        }

        protected abstract T Parse(string text);

        protected virtual string Write(T value)
        {
            return value.ToString();
        }

        public void SetReference(object instance, string fieldName)
        {
            _value = new ReflectionValue<T>(instance, fieldName);
            DisplayValue();
        }

        public void CommitText(string text, string name = "")
        {
            _inputText = text;
            _recoveryText = text;
            _textName.SetText(name);
            _textValue.SetText(text);
        }

        private void InputText()
        {
            int curFrame = (int)(Time.unscaledTime * 4) % _curAnimation.Length;
            char cur = _curAnimation[curFrame];
            string frameInput = Input.inputString;

            if (!frameInput.Contains("\b"))
            {
                if (_inputText.Length < _charCount)
                    _inputText += frameInput.Substring(0, Mathf.Min(frameInput.Length, _charCount - _inputText.Length));
            }
            else if (_inputText.Length > 0)
                _inputText = _inputText.Substring(0, _inputText.Length - 1);

            _textValue.SetText($"{_inputText}{cur}");
        }

        private void CommitInput()
        {
            ManualDeselect();
        }

        private void CancelInput()
        {
            _inputText = _recoveryText;
            ManualDeselect();
        }

        private void ManualDeselect()
        {
            if (gameObject.TryGetComponentInParent(out GUIInputManager ginput))
                ginput.Deselect(this);
        }

        #region IGUISelectable
        public void OnSelect()
        {
            enabled = true;
            _recoveryText = _inputText;
        }

        public void OnDeselect()
        {
            enabled = false;

            if (_value != null)
                _value.Value = Parse(_inputText);

            DisplayValue();
        }
        #endregion
    }
}
