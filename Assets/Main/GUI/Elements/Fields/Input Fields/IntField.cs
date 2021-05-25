using System.Reflection;
using UnityEngine;

namespace MPGUI
{
    public sealed class IntField : ObjectField
    {
        [SerializeField] private string format = "D";
        [SerializeField] private int min = 0;
        [SerializeField] private int max = 10;

        protected override void ParseValue(string value)
        {
            if (int.TryParse(value, out int i))
            {
                i = Mathf.Clamp(i, min, max);

                if (TryGetField(out FieldInfo field))
                    field.SetValue(instance, i);
                else if (TryGetProperty(out PropertyInfo prop))
                    prop.SetValue(instance, i);

                SetField(i.ToString(format));
            }
            else
                SetField(recovery);
        }

        protected override void InitValue()
        {
            if (TryGetField(out FieldInfo field))
                SetField((field.GetValue(instance) as int?)?.ToString(format));
            else if (TryGetProperty(out PropertyInfo prop))
                SetField((prop.GetValue(instance) as int?)?.ToString(format));
        }
    }
}
