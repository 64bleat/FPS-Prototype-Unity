using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MPCore
{
    [System.Serializable]
    public class ValueEvent<T>
    {
        [SerializeField] private T _value = default;

        /// <summary>
        /// Called upon setting Value. <c>Callback(T old, T new)</c>
        /// </summary>
        public readonly UnityEvent<T,T> OnSet = new UnityEvent<T,T>();

        public T Value
        {
            get => _value;
            set
            {
                T old = _value;

                _value = value;

                OnSet.Invoke(old, value);
            }
        }

        public void Initialize(UnityAction<T, T> call)
        {
            OnSet.AddListener(call);
            call.Invoke(default, _value);
        }

        public static implicit operator T(ValueEvent<T> vt) => vt._value;
    }
}
