using UnityEngine;
using UnityEngine.Events;
using MPConsole;

namespace MPWorld
{
    public class TriggerEvent : MonoBehaviour
    {
        public bool destroyOnActivate;
        public float retriggerTime = 0;

        public UnityEvent awakeEvents;
        public UnityEvent touchEvents;
        public UnityEvent leaveEvents;

        private float lastTrigger = 0;

        private void Awake()
        {
            awakeEvents.Invoke();
        }

        public void DestroyObject(GameObject target)
        {
            Destroy(target);
        }


        public void ConsoleCommand(string s)
        {
            Console.Command(s);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer != LayerMask.NameToLayer("Player"))
                return;

            if (Time.time - lastTrigger > retriggerTime)
            {
                lastTrigger = Time.time;
                touchEvents.Invoke();
            }

            if (destroyOnActivate)
                Destroy(gameObject);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.layer != LayerMask.NameToLayer("Player"))
                return;

            if (Time.time - lastTrigger > retriggerTime)
            {
                lastTrigger = Time.time;
                touchEvents.Invoke();
            }

            if (destroyOnActivate)
                Destroy(gameObject);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer != LayerMask.NameToLayer("Player"))
                return;

            leaveEvents.Invoke();
        }

        private void OnCollisionExit(Collision collision)
        {
            if (collision.gameObject.layer != LayerMask.NameToLayer("Player"))
                return;

            leaveEvents.Invoke();
        }
    }
}
