using UnityEngine;
using UnityEngine.Events;

namespace MPCore
{
    [System.Serializable]
    public class DataValue<T>
    {
        [SerializeField] private T _value = default;

        /// <summary>
        /// Called upon setting Value. <c>Callback(T old, T new)</c>
        /// </summary>
        public readonly UnityEvent<DeltaValue<T>> OnSet = new UnityEvent<DeltaValue<T>>();

        public T Value
        {
            get => _value;
            set
            {
                T old = _value;

                _value = value;

                OnSet.Invoke(new DeltaValue<T>(old, value));
            }
        }

        public void Initialize(UnityAction<DeltaValue<T>> call)
        {
            OnSet.AddListener(call);
            call.Invoke(new DeltaValue<T>(default, _value));
        }

        public static implicit operator T(DataValue<T> vt) => vt._value;
    }
}
