using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MPCore
{
    public class QualifiedEvent<T>
    {
        private readonly Func<bool> predicate;
        private readonly UnityEvent<T> listeners = new UnityEvent<T>();

        public QualifiedEvent(Func<bool> predicate = null, params UnityAction<T>[] actions)
        {
            this.predicate = predicate;

            foreach (var action in actions)
                listeners.AddListener(action);
        }

        public void Invoke(T data)
        {
            if (listeners != null && predicate?.Invoke() != null)
                listeners.Invoke(data);
        }

        public void AddListener(UnityAction<T> call) => listeners.AddListener(call);
        public void RemoveListener(UnityAction<T> call) => listeners.RemoveListener(call);
    }
}
