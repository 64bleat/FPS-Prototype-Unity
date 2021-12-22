using UnityEngine;
using UnityEngine.Events;

namespace MPCore
{
	[System.Serializable]
	public class DataValue<T>
	{
		[SerializeField] T _value = default;

		readonly UnityEvent<DeltaValue<T>> OnSet = new();

		public DataValue()
		{
			_value = default;
		}

		public DataValue(T value)
		{
			_value = value;
		}

		public virtual T Value
		{
			get => _value;
			set
			{
				T old = _value;

				_value = value;

				OnSet.Invoke(new DeltaValue<T>(old, value));
			}
		}

		public void Subscribe(UnityAction<DeltaValue<T>> listener, bool initialize = true)
		{
			OnSet.AddListener(listener);

			if(initialize)
				listener.Invoke(new DeltaValue<T>(default, _value));
		}

		public void Unsubscribe(UnityAction<DeltaValue<T>> listener)
		{
			OnSet.RemoveListener(listener);
		}

		public static implicit operator T(DataValue<T> vt) => vt._value;
	}
}
