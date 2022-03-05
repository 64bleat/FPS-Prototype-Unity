using MPCore;
using UnityEngine;

namespace MPWorld
{
    public class CannonZone : MonoBehaviour
    {
        public float ejectSpeed = 30f;
        public float onTime = 0.5f;
        private float awakeTime = 0;

        private Rigidbody attachedRigidbody;

        public void OnEnable()
        {
            attachedRigidbody = GetComponent<Rigidbody>() ?? GetComponentInParent<Rigidbody>();
        }

        public void Reset()
        {
            if (Time.time - awakeTime > onTime)
            {
                awakeTime = Time.time;
                GetComponent<AudioSource>().Play();
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (onTime <= 0 || Time.time - awakeTime < onTime)
            {
                IGravityUser thing = other.gameObject.GetComponent<IGravityUser>();
                Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();

                Vector3 startVel = attachedRigidbody ? attachedRigidbody.GetPointVelocity(transform.position) : Vector3.zero;
                startVel += transform.up * ejectSpeed;

                if (other.TryGetComponent(out CharacterBody body))
                    body.currentState = CharacterBody.MoveState.Airborne;

                if (thing != null)
                    thing.Velocity = startVel;

                if (rb)
                    rb.velocity = startVel;
            }
        }
    }
}
