using MPCore;
using UnityEngine;

namespace MPCore
{
    public class LookAtTarget : AIAction
    {
        public float angularVelocity = 180;
        public float slowAngle = 45f;

        private CharacterBody body;
        private InputManager input;
        private CharacterAI ai;

        public override void InstanceAwake()
        {
            body = gameObject.GetComponent<CharacterBody>();
            ai = gameObject.GetComponent<CharacterAI>();
            input = gameObject.GetComponent<InputManager>();
        }

        public override float? Priority(IGOAPAction successor)
        {
            if (ai && ai.target)
                return 1;
            else
                return null;
        }

        /*  Update is simple. Keep going as long as there is a target.       */
        public override GOAPStatus Update()
        {
            if (ai && ai.target)
            {
                input.Move(Look);

                return GOAPStatus.Running;
            }
            else 
                return GOAPStatus.Fail;
        }

        /*  Look is passed to the character's InputManager. Mouse movement
            in the InputManager takes a function so simulate human mouse
            movement. This function tells how much the mouse has to move
            to put the crosshair over the target.                            */
        private Vector2 Look(float time)
        {
            if (ai.target)
            {
                CharacterBody targetBody = ai.target.GetComponent<CharacterBody>();
                Vector3 lookDir = ai.lookDestination - body.cameraSlot.position;
                Vector3 lookDirX = Vector3.ProjectOnPlane(lookDir, body.transform.up);
                Vector3 lookDirY = Vector3.ProjectOnPlane(lookDir, body.transform.right);
                float currentAngleY = Mathf.PingPong(Vector3.Angle(body.transform.forward, body.cameraSlot.forward), 90) * Mathf.Sign(Vector3.Dot(body.transform.up, body.cameraSlot.forward));
                float desiredY = Mathf.PingPong(Vector3.Angle(body.transform.forward, lookDirY), 90) * Mathf.Sign(Vector3.Dot(body.transform.up, lookDirY));
                float mouseVelocity = angularVelocity * Mathf.Clamp01(Vector3.Angle(body.cameraSlot.forward, lookDir) / Mathf.Max(1f, slowAngle));
                Vector2 mouseDir = new Vector2(
                    Vector3.Angle(body.transform.forward, lookDirX) * Mathf.Sign(Vector3.Dot(body.transform.right, lookDirX)),
                    desiredY - currentAngleY);

                return mouseDir.normalized * Mathf.Min(mouseDir.magnitude, mouseVelocity * Time.deltaTime);
            }
            else
                return Vector2.zero;
        }
    }
}
