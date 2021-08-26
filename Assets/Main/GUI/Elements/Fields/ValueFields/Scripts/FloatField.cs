using MPCore;
using System.Reflection;
using UnityEngine;

namespace MPGUI
{
    public class FloatField : GenericField<float>
    {
        [SerializeField] private string _format = "P0";
        [SerializeField] private float _min = 0;
        [SerializeField] private float _max = 1;

        protected override float Parse(string text)
        {
            if (!float.TryParse(text, out float value))
                value = default;

            value = Mathf.Clamp(value, _min, _max);

            return value;
        }

        protected override string Write(float value)
        {
            return value.ToString(_format);
        }

        public void SetRange(float min, float max)
        {
            _min = min;
            _max = max;
        }

        public void SetFormat(string format)
        {
            _format = format;
        }
    }
}
