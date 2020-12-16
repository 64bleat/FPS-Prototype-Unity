using MPCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MPWorld
{
    [RequireComponent(typeof(Rigidbody))]
    public class SpaceShip : MonoBehaviour
    {
        public FloatLever rotationLever;
        public FloatLever speedLever;
        public FloatLever rollLever;
        public Vector2Lever turnLever;
        public BoolLever antiGravityButton;
        public BoolLever autoCorrectButton;
        public BoolLever reverseButton;
        public BoolLever panLever;

        public float maxSpeed = 10f;
        public float maxAcceleration = 500f;
        public float maxRollAcceleration = 0.5f;
        public float maxAngularAcceleration = 0.5f;

        public float autoLevelPower = 2f;
        public float autoLevelScale = 10f;

        private Rigidbody rb;

        private void OnEnable()
        {
            rb = GetComponent<Rigidbody>();

            if (!rb)
                enabled = false;
        }


        void FixedUpdate()
        {
            float currentSpeed = Mathf.Abs(Vector3.Dot(rb.velocity, -transform.forward));
            float acceleration = speedLever.Value * maxAcceleration;
            Vector3 oppDir = Vector3.ProjectOnPlane(rb.velocity, transform.forward);
            float speedFactor = Mathf.Clamp01(Mathf.Abs(currentSpeed) / maxSpeed);
            Vector3 sideVelocity = Vector3.ProjectOnPlane(rb.velocity, transform.forward);

            rb.useGravity = !antiGravityButton.BoolValue;

            //forward acceleration
            //if (idealSpeed > 0.25f && currentSpeed < idealSpeed)
                rb.AddForce(-transform.forward * acceleration * (reverseButton.BoolValue ? -0.5f : 1f) * (1f-speedFactor), ForceMode.Acceleration);

            //focus Acceleration
            if(antiGravityButton.BoolValue && !panLever.BoolValue)
                rb.AddForce(-sideVelocity * 10, ForceMode.Acceleration);

            if (!panLever.BoolValue)
            {
                rb.AddForceAtPosition(transform.up * maxAngularAcceleration * rotationLever.Value * turnLever.Value.y, rb.worldCenterOfMass - transform.forward * 10f, ForceMode.Acceleration);
                rb.AddForceAtPosition(-transform.right * maxAngularAcceleration * rotationLever.Value * turnLever.Value.x, rb.worldCenterOfMass - transform.forward * 10f, ForceMode.Acceleration);
            }
            else
            {
                rb.AddForce((transform.up * turnLever.Value.y - transform.right * turnLever.Value.x) * rotationLever.Value * maxAcceleration * 0.25f, ForceMode.Acceleration);
            }

            //Roll
            rb.AddForceAtPosition(transform.right * maxRollAcceleration * (rollLever.Value - 0.5f) * 2f, rb.worldCenterOfMass - transform.up * 10f, ForceMode.Acceleration);

            if (autoCorrectButton.BoolValue)
            {
                Vector3 leveling = Vector3.ProjectOnPlane(-Physics.gravity.normalized - rb.transform.up, rb.transform.up);
                leveling = leveling.normalized * Mathf.Pow(leveling.magnitude, autoLevelPower);


                rb.AddForceAtPosition(leveling, rb.worldCenterOfMass + transform.up * autoLevelScale, ForceMode.Acceleration);
            }
        }
    }
}
