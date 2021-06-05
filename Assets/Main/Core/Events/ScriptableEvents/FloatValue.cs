using UnityEngine;
using UnityEngine.Events;

namespace MPCore
{
    public class FloatValue : ScriptableObject
    {
        [SerializeField] private float value;
        [SerializeField] private float min;
        [SerializeField] private float max;

        public UnityEvent<float> callback;

        public void OnValidate()
        {
            min = Mathf.Min(min, max);
            value = Mathf.Clamp(value, min, max);
        }

        public float Value
        {
            get
            {
                return value;
            }
            set
            {
                value = Mathf.Clamp(value, min, max);
                this.value = value;
                callback?.Invoke(value);
            }
        }

        public static implicit operator float(FloatValue fv)
        {
            return fv.value;
        }
    }
}
