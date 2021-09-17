using UnityEngine;
using UnityEngine.Events;

namespace MPCore
{
    public class StartEvents : MonoBehaviour
    {
        public UnityEvent events;

        void Start()
        {
            events?.Invoke();
        }
    }
}
