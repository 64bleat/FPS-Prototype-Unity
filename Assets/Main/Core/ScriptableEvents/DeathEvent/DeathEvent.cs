using UnityEngine;

namespace MPCore
{
    public class DeathEvent : ScriptableObject
    {
        private readonly ScriptableEvent<DeathEventParameters> broadcaster = new ScriptableEvent<DeathEventParameters>();

        public void Clear()
        {
            broadcaster.Clear();
        }

        public void Invoke(DeathEventParameters o)
        {
            broadcaster.Invoke(o);
        }

        public void Add(ScriptableEvent<DeathEventParameters>.SetValue OnValueChange, bool initializeImmediately = false)
        {
            broadcaster.Add(OnValueChange, initializeImmediately);
        }

        public void Remove(ScriptableEvent<DeathEventParameters>.SetValue action)
        {
            broadcaster.Remove(action);
        }
    }
}
