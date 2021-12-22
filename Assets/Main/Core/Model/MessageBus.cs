using System;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

namespace MPCore
{
	public static class MessageBus
	{
		static readonly Dictionary<Type, dynamic> _listeners = new Dictionary<Type, dynamic>();

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		static void RuntimeInit()
		{
			_listeners.Clear();
		}

		public static void Subscribe<T>(UnityAction<T> call)
		{
			Type type = typeof(T);

			if (_listeners.TryGetValue(type, out dynamic dynEvent))
					((UnityEvent<T>)dynEvent).AddListener(call);
			else
			{
				UnityEvent<T> typeEvent = new UnityEvent<T>();
				typeEvent.AddListener(call);
				_listeners.Add(type, typeEvent);
			}
		}

		public static void Unsubscribe<T>(UnityAction<T> call)
		{
			Type type = typeof(T);

			((UnityEvent<T>)_listeners[type]).RemoveListener(call);
		}

		public static void Publish<T>(T data = default)
		{
			Type type = typeof(T);

			if(_listeners.TryGetValue(type, out dynamic dynEvent))
				((UnityEvent<T>)dynEvent)?.Invoke(data);
		}

		public static void Clear()
		{
			_listeners.Clear();
		}
	}
}
