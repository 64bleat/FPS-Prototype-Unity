using MPCore;
using UnityEngine;

namespace MPGUI
{
    public sealed class IntField : GenericField<int>
    {
        [SerializeField] private string _format = "D";
        [SerializeField] private int _min = 0;
        [SerializeField] private int _max = 10;

        protected override int Parse(string text)
        {
            if (!int.TryParse(text, out int value))
                value = default;

            value = Mathf.Clamp(value, _min, _max);

            return value;
        }

        protected override string Write(int value)
        {
            return value.ToString(_format);
        }
    }
}
