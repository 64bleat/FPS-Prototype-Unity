using System;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MPGUI
{
    public class BoolField : MonoBehaviour, IClickable
    {
        [SerializeField] private Object instance;
        [SerializeField] private string fieldName;
        [SerializeField] private Image check;
        [SerializeField] private TextMeshProUGUI description;

        private void OnEnable()
        {
            InitValue();
        }

        public void SetReference(Object instance, string fieldName, string description)
        {
            this.instance = instance;
            this.fieldName = fieldName;
            this.description.SetText(description);
            InitValue();
        }

        public void SetValue(bool value, string description = null)
        {
            Value = value;
            check.enabled = value;
        }

        private void InitValue()
        {
            check.enabled = Value;
        }

        public void OnMouseClick(MouseInfo mouse) 
        {
            SetValue(!check.enabled);
        }

        public void OnMouseHold(MouseInfo mouse) { }

        public void OnMouseHover(MouseInfo mouse) { }

        public void OnMousePress(MouseInfo mouse) { }

        public void OnMouseRelease(MouseInfo mouse) { }

        private bool Value
        {
            get 
            {
                if (TryGetField(out FieldInfo field))
                    return (bool)field.GetValue(instance);
                else if (TryGetProperty(out PropertyInfo prop))
                    return (bool)prop.GetValue(instance);
                else
                    return false;
            }

            set
            {
                if (TryGetField(out FieldInfo field))
                    field.SetValue(instance, value);
                else if (TryGetProperty(out PropertyInfo prop))
                    prop.SetValue(instance, value);
            }
        }

        private bool TryGetField(out FieldInfo field)
        {
            field = null;

            if (instance != null)
            {
                Type type = instance.GetType();

                field = type.GetField(fieldName);
            }

            return field != null;
        }

        protected bool TryGetProperty(out PropertyInfo prop)
        {
            prop = null;

            if (instance != null)
            {
                Type type = instance.GetType();

                prop = type.GetProperty(fieldName);
            }

            return prop != null;
        }
    }
}
