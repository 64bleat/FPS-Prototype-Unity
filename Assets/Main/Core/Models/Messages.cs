using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace MPCore
{
    public static class Messages
    {
        private static readonly Dictionary<Type, dynamic> listeners = new Dictionary<Type, dynamic>();

        public static void Subscribe<T>(UnityAction<T> call)
        {
            Type type = typeof(T);

            if (listeners.TryGetValue(type, out dynamic dynEvent))
                    ((UnityEvent<T>)dynEvent).AddListener(call);
            else
            {
                UnityEvent<T> typeEvent = new UnityEvent<T>();
                typeEvent.AddListener(call);
                listeners.Add(type, typeEvent);
            }
        }

        public static void Unsubscribe<T>(UnityAction<T> call)
        {
            Type type = typeof(T);

            ((UnityEvent<T>)listeners[type]).RemoveListener(call);
        }

        public static void Publish<T>(T data)
        {
            Type type = typeof(T);

            if(listeners.TryGetValue(type, out dynamic dynEvent))
                ((UnityEvent<T>)dynEvent)?.Invoke(data);
        }

        public static void Clear()
        {
            listeners.Clear();
        }
    }
}
