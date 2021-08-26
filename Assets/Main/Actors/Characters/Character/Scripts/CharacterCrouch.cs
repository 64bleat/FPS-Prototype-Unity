using UnityEngine;

namespace MPCore
{
    /// <summary>
    /// <para>Handles character crouching behaviour.</para>
    /// <para>Shrinks or grows the attached <c>CapsuleCollider</c> to match the desired height</para>
    /// </summary>
    /// <remarks>
    /// <para>Growing tests for overlaps to avoid clipping.</para>
    /// <para>When <c>CharacterInput.autoCrouch</c> is off, the overlap test will not be performed
    ///     once the standing height is reached. When it is on, overlaps while standing will
    ///     make the character crouch automatically.</para>
    /// </remarks>
    public class CharacterCrouch : MonoBehaviour
    {
        private static readonly Collider[] overlapBuffer = new Collider[10];
        private static readonly string[] collisionLayers = { "Default", "Physical", "Player" };

        private InputManager input;
        private CharacterInput cInput;
        private CapsuleCollider cap;
        private CharacterBody body;
        private float dampVelocity;
        private int layermask;

        private void Awake()
        {
            TryGetComponent(out input);
            TryGetComponent(out cInput);
            TryGetComponent(out cap);
            TryGetComponent(out body);

            layermask = LayerMask.GetMask(collisionLayers);

            input.Bind("Crouch", () => dampVelocity = 0, this, KeyPressType.Down);
            input.Bind("Crouch", () => dampVelocity = 0, this, KeyPressType.Up);
        }


        private void FixedUpdate()
        {
            bool crouched = cInput.Crouch;

            float crouchHeight = body.defaultCrouchHeight - body.defaultStepOffset;
            float standHeight = body.defaultHeight - body.defaultStepOffset;

            if (crouched)
            {
                cap.height = Mathf.SmoothDamp(cap.height, crouchHeight, ref dampVelocity, 0.05f, 10f, Time.fixedDeltaTime);
            }
            else
            {
                if (cInput.autoCrouch || standHeight - cap.height > 0.001f)
                {
                    Vector3 position = transform.position;
                    Vector3 up = transform.up;
                    Vector3 point1 = position + up * (standHeight - cap.height / 2);
                    int count = Physics.OverlapCapsuleNonAlloc(position, point1, cap.radius, overlapBuffer, layermask, QueryTriggerInteraction.Ignore);

                    if (IsBlocked(overlapBuffer, count))
                        standHeight = crouchHeight;
                }

                cap.height = Mathf.SmoothDamp(cap.height, standHeight, ref dampVelocity, 0.05f, 10f, Time.fixedDeltaTime);
            }
        }

        private bool IsBlocked(Collider[] colliders, int count)
        {
            for(int i = 0; i < count; i++)
                if (colliders[i].gameObject != gameObject)
                    return true;

            return false;
        }
    }
}
