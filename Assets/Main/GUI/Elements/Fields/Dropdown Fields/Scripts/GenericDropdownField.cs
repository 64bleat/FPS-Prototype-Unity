using MPGUI;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Dropdown = MPGUI.Dropdown;

namespace MPCore
{
    /// <summary>
    /// Base for all dropdown fields
    /// </summary>
    public abstract class GenericDropdownField<T> : MonoBehaviour, IClickable
    {
        const string NULL = "NULL";

        Dropdown _dropdown;
        [SerializeField] TextMeshProUGUI _txtValue;
        [SerializeField] TextMeshProUGUI _txtDescription;
        [SerializeField] RectTransform _dropPosition;

        private ReflectionValue<T> _value;
        private readonly List<T> _options = new();

        private void DisplayValue()
        {
            if(_value != null)
            {
                _txtValue.SetText(Write(_value.Value));
                _txtDescription.SetText(_value.DisplayName);
            }
            else
            {
                _txtValue.SetText("ERR");
                _txtDescription.SetText("NO INSTANCE");
            }
        }

        /// <summary>
        /// Set the reference the value this dropdown changes
        /// </summary>
        public void SetReference(object instance, string fieldName, string displayName = null)
        {
            GUIModel gui = Models.GetModel<GUIModel>();

            _dropdown = gui.dropdown;
            _value = new ReflectionValue<T>(instance, fieldName, displayName);
            DisplayValue();
        }

        public void AddOption(T option)
        {
            _options.Add(option);
        }

        public void AddOptions(IEnumerable<T> options)
        {
            _options.AddRange(options);
        }

        public void RemoveOption(T option)
        {
            _options.Remove(option);
        }

        public void ClearOptions()
        {
            _options.Clear();
        }

        /// <summary>
        /// String representation of the options
        /// </summary>
        /// <remarks>
        /// default is to simply output value.ToString() 
        /// </remarks>
        protected virtual string Write(T value)
        {
            return value != null ? value.ToString() : NULL;
        }

        /// <summary>
        /// Color representation of the options' select buttons
        /// </summary>
        /// <remarks>
        /// default is to grey out unselected options
        /// </remarks>
        protected virtual Color Colorize(T option, T current)
        {
            return current.Equals(option) ? Color.white : Color.grey;
        }

        /// <summary>
        /// Called just before opening the dropdown for on-the-fly changes
        /// </summary>
        protected virtual void InitDropdown()
        {

        }

        protected virtual void SetValue(T value)
        {
            _value.Value = value;
        }

        private void OpenMenu()
        {
            ButtonSet set = _dropdown.SpawnDropdown(_dropPosition);
            T current = _value.Value;

            foreach(T option in _options)
            {
                T value = option;
                void OnPress()
                {
                    SetValue(value);
                    DisplayValue();
                }
                GameObject button = set.AddButton(Write(value), OnPress);

                if (button.TryGetComponent(out Image image))
                    image.color = Colorize(value, current);
            }
        }

        public void OnMouseClick(MouseInfo mouse)
        {
            InitDropdown();

            if (_options.Count > 0)
                OpenMenu();
        }

        public void OnMousePress(MouseInfo mouse) { }
        public void OnMouseHover(MouseInfo mouse) { }
        public void OnMouseHold(MouseInfo mouse) { }
        public void OnMouseRelease(MouseInfo mouse) { }
    }
}
