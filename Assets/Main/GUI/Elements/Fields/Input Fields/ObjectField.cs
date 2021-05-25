using System;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MPGUI
{
    public class ObjectField : StringField
    {
        [SerializeField] protected Object instance;
        [SerializeField] protected string fieldName;

        private void Awake()
        {
            InitValue();
        }

        public void SetReference(Object instance, string fieldName)
        {
            this.instance = instance;
            this.fieldName = fieldName;
            InitValue();
        }

        protected override void ParseValue(string value)
        {
            if (TryGetField(out FieldInfo field))
                field.SetValue(instance, value);
            else if (TryGetProperty(out PropertyInfo prop))
                prop.SetValue(instance, value);

            SetField(value.ToString());
        }

        protected override void InitValue()
        {
            if (TryGetField(out FieldInfo field))
                SetField(field.GetValue(instance).ToString());
            else if (TryGetProperty(out PropertyInfo prop))
                SetField(prop.GetValue(instance).ToString());
        }

        protected bool TryGetField(out FieldInfo field)
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
