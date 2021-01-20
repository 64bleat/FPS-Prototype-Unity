using UnityEngine;

namespace MPCore
{
    public class StringEvent : ScriptableObject
    {
        private readonly ScriptableEvent<string> broadcaster = new ScriptableEvent<string>();

        public void Clear()
        {
            broadcaster.Clear();
        }

        public void Invoke(string o)
        {
            broadcaster.Invoke(o);
        }

        public void Add(ScriptableEvent<string>.SetValue OnValueChange, bool initializeImmediately = false)
        {
            broadcaster.Add(OnValueChange, initializeImmediately);
        }

        public void Remove(ScriptableEvent<string>.SetValue action)
        {
            broadcaster.Remove(action);
        }
    }
}
