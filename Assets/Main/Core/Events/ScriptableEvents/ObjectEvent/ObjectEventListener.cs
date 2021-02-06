using UnityEngine;
using UnityEngine.Events;

namespace MPCore
{
    public class ObjectEventListener : MonoBehaviour
    {
        public ObjectEvent channel;
        public UnityEvent events;

        private void Awake()
        {
            channel.Add(Invoke);
        }

        private void OnDestroy()
        {
            channel.Remove(Invoke);
        }

        private void Invoke(object o)
        {
            events?.Invoke();
        }
    }
}
