using UnityEngine;

namespace MPCore
{
    public class ObjectEvent : ScriptableObject
    {
        private readonly ScriptableEvent<object> broadcaster = new ScriptableEvent<object>();

        public void Clear()
        {
            broadcaster.Clear();
        }

        public void Invoke(object o)
        {
            broadcaster.Invoke(o);
        }

        public void Add(ScriptableEvent<object>.SetValue OnValueChange, bool initializeImmediately = false)
        {
            broadcaster.Add(OnValueChange, initializeImmediately);
        }

        public void Remove(ScriptableEvent<object>.SetValue action)
        {
            broadcaster.Remove(action);
        }
    }
}