using UnityEngine;
using UnityEngine.Events;

namespace MPCore
{
    public class TimedEvents : MonoBehaviour
    {
        public float duration = 1f;
        public UnityEvent events;

        private float enableTime;

        private void OnEnable()
        {
            enableTime = Time.time;
        }

        private void Update()
        {
            float time = Time.time - enableTime;

            if (time >= duration)
            {
                events?.Invoke();
                enabled = false;
            }
        }
    }
}
