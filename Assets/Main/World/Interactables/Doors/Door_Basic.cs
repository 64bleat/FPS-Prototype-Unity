using UnityEngine;
using UnityEngine.Serialization;

namespace MPWorld
{
    /// <summary> A script to open and close a hinged door upon interacting with it. </summary>
    [RequireComponent(typeof(AudioSource))]
    public class Door_Basic : MonoBehaviour, IInteractable
    {
        public bool clockwise = true;
        public float motorForce = 80f;
        public float motorVelocity = 2.0f;
        public float snapClosedAngle = 1.0f;
        public AudioClip openSound;
        public AudioClip closeSound;

        private Quaternion closeRotation;
        private Vector3 closePosition;
        private new Rigidbody rigidbody;
        private AudioSource audioSource;
        private HingeJoint hinge;
        private int interactDirection;
        private float lastDebounce = -1;
        private const float debounceTime = 0.25f;


        public void Start()
        {
            rigidbody = GetComponent<Rigidbody>();
            audioSource = GetComponent<AudioSource>();
            hinge = GetComponent<HingeJoint>();

            closeRotation = transform.rotation;
            closePosition = transform.position;
            rigidbody.constraints = RigidbodyConstraints.FreezeAll;
            interactDirection = clockwise ? -1 : 1;
            enabled = false;
        }

        public void FixedUpdate()
        {
            float deltaDebounce = Time.time - lastDebounce;
            float currentAngle = Quaternion.Angle(closeRotation, rigidbody.rotation);

            if (deltaDebounce > debounceTime && currentAngle < snapClosedAngle)
            {
                rigidbody.rotation = closeRotation;
                rigidbody.position = closePosition;
                rigidbody.constraints = RigidbodyConstraints.FreezeAll;
                interactDirection = clockwise ? -1 : 1;

                audioSource.PlayOneShot(closeSound);

                enabled = false;
            }
        }

        public void OnInteractStart(GameObject other, RaycastHit hit)
        {
            interactDirection *= -1;

            JointMotor motor = hinge.motor;
            motor.force = motorForce;
            motor.targetVelocity = motorVelocity * interactDirection;
            motor.freeSpin = false;
            hinge.motor = motor;
            hinge.useMotor = true;

            if (!enabled)
                audioSource.PlayOneShot(openSound);

            enabled = true;
            lastDebounce = Time.time;
            rigidbody.constraints = RigidbodyConstraints.None;
        }

        public void OnInteractEnd(GameObject other, RaycastHit hit)
        {
            hinge.useMotor = false;
        }

        public void OnInteractHold(GameObject other, RaycastHit hit)
        {

        }
    }
}
