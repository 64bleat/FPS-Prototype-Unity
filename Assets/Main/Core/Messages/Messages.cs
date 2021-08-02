using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace MPCore
{
    public static class Messages
    {
        private static readonly Dictionary<Guid, dynamic> listeners = new Dictionary<Guid, dynamic>();
        public static void AddListener<T>(UnityAction<T> call)
        {
            Guid guid = typeof(T).GUID;

            if (listeners.TryGetValue(guid, out dynamic dynEvent))
                    ((UnityEvent<T>)dynEvent).AddListener(call);
            else
            {
                UnityEvent<T> typeEvent = new UnityEvent<T>();
                typeEvent.AddListener(call);
                listeners.Add(guid, typeEvent);
            }
        }

        public static void RemoveListener<T>(UnityAction<T> call)
        {
            Guid guid = typeof(T).GUID;

            ((UnityEvent<T>)listeners[guid]).RemoveListener(call);
        }

        public static void Invoke<T>(T data)
        {
            Guid guid = typeof(T).GUID;

            ((UnityEvent<T>)listeners[guid]).Invoke(data);
        }

        public static void Clear()
        {
            listeners.Clear();
        }
    }
}
