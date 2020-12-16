using UnityEngine;
using MPWorld;
using MPCore;
using System.Diagnostics;

namespace MPGUI
{
    /// <summary>
    /// Attatches to a camera owned by a CharacterBody and makes your equipment JIGGLE
    /// </summary>
    public class ImpactJiggler : MonoBehaviour
    {
        public bool runSway = true;
        public float bodyImpactScale = 0.13f;
        public float swayScale = 0.6f;
        public float swayPeriod = 5;
        public float gravity = 9.81f;
        public float distanceMax = 0.05f;
        public float deccereration = 1f;

        private IGravityUser gravityUser;
        private CharacterBody body;
        private CharacterInput input;
        private Vector3 lastBodyVelocity;
        private Vector3 velocity;
        private Vector3 originPosition;
        private readonly Stopwatch animTimer = new Stopwatch();

        private void Awake()
        {
            gravityUser = GetComponentInParent<IGravityUser>();
            body = GetComponentInParent<CharacterBody>();
            input = GetComponentInParent<CharacterInput>();
            originPosition = transform.localPosition;
        }

        void OnEnable()
        {
            lastBodyVelocity = gravityUser.Velocity;
        }

        void FixedUpdate()
        {
            if (Time.fixedDeltaTime > 0f)
            {
                Vector3 oLocalPosition = transform.localPosition;
                Vector3 originOffset = transform.localPosition - originPosition;

                // Drag
                velocity -= velocity.normalized * Mathf.Min(velocity.magnitude, deccereration * Time.fixedDeltaTime);

                // Body Impact Velocity
                if (gravityUser != null)
                {
                    velocity -= transform.parent.InverseTransformVector(gravityUser.Velocity - lastBodyVelocity) * bodyImpactScale;
                    lastBodyVelocity = gravityUser.Velocity;
                }

                // Gravity
                velocity -= originOffset.normalized * Mathf.Min(originOffset.magnitude, gravity * Time.fixedDeltaTime);

                // Running Animation
                if (runSway && body && input)
                    if (input.Forward == 0 && input.Right == 0 || body.currentState == CharacterBody.MoveState.Airborne)
                        animTimer.Restart();
                    else if (!input.Walk && !input.Crouch)
                    {
                        float bodySpeedFactor = Mathf.Clamp01(gravityUser.Velocity.magnitude / body.defaultMoveSpeed);
                        float period = (float)animTimer.Elapsed.TotalSeconds * Mathf.PI * swayPeriod;
                        float scale = swayScale * bodySpeedFactor * Time.fixedDeltaTime;

                        originOffset += Vector3.up * Mathf.Sin(period) * scale;
                        
                        if (input.Sprint)
                            originOffset += Vector3.right * Mathf.Sin(period / 2f) * scale;
                    }

                // Apply Velocity
                transform.localPosition = originPosition + Vector3.ClampMagnitude(originOffset + velocity * Time.fixedDeltaTime, distanceMax);

                // Recalculate Velocity
                velocity = (transform.localPosition - oLocalPosition) / Time.fixedDeltaTime;
            }
        }

        public void AddForce(Vector3 acceleration)
        {
            velocity += acceleration;
        }
    }
}