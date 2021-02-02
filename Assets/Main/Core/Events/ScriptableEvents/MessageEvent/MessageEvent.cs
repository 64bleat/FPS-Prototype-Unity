using UnityEngine;

namespace MPCore
{
    public class MessageEvent : ScriptableObject
    {
        private readonly ScriptableEvent<MessageEventParameters> broadcaster = new ScriptableEvent<MessageEventParameters>();

        public void Clear()
        {
            broadcaster.Clear();
        }

        public void Invoke(MessageEventParameters message)
        {
            broadcaster.Invoke(message);
        }

        public void Invoke(string message)
        {
            broadcaster.Invoke(new MessageEventParameters() { message = message });
        }

        public void Add(ScriptableEvent<MessageEventParameters>.SetValue OnValueChange, bool initializeImmediately = false)
        {
            broadcaster.Add(OnValueChange, initializeImmediately);
        }

        public void Remove(ScriptableEvent<MessageEventParameters>.SetValue action)
        {
            broadcaster.Remove(action);
        }
    }

    public struct MessageEventParameters
    {
        public string message;
        public Color color;
        public Color imageColor;
    }
}
