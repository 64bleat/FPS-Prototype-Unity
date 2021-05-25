using System.Reflection;
using UnityEngine;

namespace MPGUI
{
    public class FloatField : ObjectField
    {
        [SerializeField] private string format = "P0";
        [SerializeField] private float min = 0;
        [SerializeField] private float max = 1;

        public void SetReference(Object instance, string fieldName, string description, string format)
        {
            this.instance = instance;
            this.fieldName = fieldName;
            this.description.SetText(description);
            this.format = format;
            InitValue();
        }

        public void SetRange(float min, float max)
        {
            this.min = min;
            this.max = max;
            InitValue();
        }

        protected override void ParseValue(string value)
        {
            if (float.TryParse(value, out float f))
            {
                f = Mathf.Clamp(f, min, max);

                if (TryGetField(out FieldInfo field))
                    field.SetValue(instance, f);
                else if (TryGetProperty(out PropertyInfo prop))
                    prop.SetValue(instance, f);

                SetField(f.ToString(format));
            }
            else
                SetField(recovery);
        }

        protected override void InitValue()
        {
            if (TryGetField(out FieldInfo field))
                SetField((field.GetValue(instance) as float?)?.ToString(format));
            else if (TryGetProperty(out PropertyInfo prop))
                SetField((prop.GetValue(instance) as float?)?.ToString(format));
        }
    }
}
