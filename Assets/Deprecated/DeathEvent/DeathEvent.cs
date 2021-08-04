using UnityEngine;

namespace MPCore
{
    public class DeathEvent : ScriptableObject
    {
        private readonly ScriptableEvent<DeathInfo> broadcaster = new ScriptableEvent<DeathInfo>();

        public void Clear()
        {
            broadcaster.Clear();
        }

        public void Invoke(DeathInfo o)
        {
            broadcaster.Invoke(o);
        }

        public void Add(ScriptableEvent<DeathInfo>.SetValue OnValueChange, bool initializeImmediately = false)
        {
            broadcaster.Add(OnValueChange, initializeImmediately);
        }

        public void Remove(ScriptableEvent<DeathInfo>.SetValue action)
        {
            broadcaster.Remove(action);
        }
    }
}
