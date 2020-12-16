using UnityEngine;

namespace Junk
{
    public class AccelerationZone : MonoBehaviour
    {
        public Vector3 acceleration;
        public float speedLimit;

        public void OnTriggerStay(Collider other)
        {
            if (other.attachedRigidbody)
                if (other.attachedRigidbody.velocity.magnitude < speedLimit)
                    other.attachedRigidbody.AddForce(acceleration + Random.insideUnitSphere * 6, ForceMode.Acceleration);
        }
    }
}
