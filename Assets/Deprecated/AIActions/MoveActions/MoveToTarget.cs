using Unity.Jobs;
using UnityEngine;

namespace MPCore
{
    public class MoveToTarget : AIAction
    {
        public float angularVelocity = 180;
        public float slowAngle = 45f;
        public float sprintDistance = 25f;
        public float walkDistance = 6;
        public float backupDistance = 1f;
        public float satisfiedDistance = 3f;


        private readonly string[] eyeLayers = { "Default", "Physical", "Player" };
        private int eyeMask;

        private CharacterBody body;
        private CharacterAI ai;
        private InputManager input;
        private JobHandle pathJob;

        public override void InstanceAwake()
        {
            body = gameObject.GetComponent<CharacterBody>();
            ai = gameObject.GetComponent<CharacterAI>();
            input = gameObject.GetComponent<InputManager>();

            eyeMask = LayerMask.GetMask(eyeLayers);
        }

        public override float? Priority(IGOAPAction successor)
        {
            if (ai && ai.target && input && body)
                return 1;
            else
                return null;
        }

        public override GOAPStatus Update()
        {
            //if (ai && ai.target)
            //{
            //    //pursure path
            //    if (ai.targetPath != null && ai.targetPath.Length > 0)
            //        while (ai.targetPathIndex < ai.targetPath.Length - 1
            //            && Vector3.ProjectOnPlane(ai.targetPath[ai.targetPathIndex] - gameObject.transform.position, gameObject.transform.up).magnitude < 1f)
            //            ai.destination = ai.targetPath[++ai.targetPathIndex];
            //    else if (ai.target && ai.targetPath == null && pathJob.IsCompleted == true)
            //        pathJob = Navigator.RequestPath(gameObject.transform.position, ai.target.transform.position, p =>
            //        {
            //            ai.targetPath = p;
            //            ai.targetPathIndex = 0;
            //        });

            //    //move begins here
            //    if (ai.targetPath != null)
            //    {
            //        CharacterBody cb;
            //        float distance = Vector3.Distance(ai.targetPath[ai.targetPathIndex], gameObject.transform.position);

            //        for (int i = ai.targetPathIndex; i < ai.targetPath.Length - 1; i++)
            //            distance += Vector3.Distance(ai.targetPath[i], ai.targetPath[i + 1]);

            //        if (ai.destination != null && (distance > satisfiedDistance || distance < backupDistance))
            //        {
            //            Vector3 direction = (Vector3)ai.destination - gameObject.transform.position * Mathf.Sign(distance - backupDistance);
            //            Vector3 offsetH = Vector3.ProjectOnPlane(direction, gameObject.transform.up);
            //            float fAngle = Vector3.Angle(gameObject.transform.forward, offsetH);
            //            float rAngle = Vector3.Angle(gameObject.transform.right, offsetH);

            //            if (fAngle < 67.5f)
            //                input.Press("Forward");
            //            else if (fAngle > 157.5f)
            //                input.Press("Reverse");

            //            if (rAngle < 67.5f)
            //                input.Press("Right");
            //            else if (rAngle > 157.5f)
            //                input.Press("Left");

            //            if (distance < walkDistance)
            //                input.Press("Walk");
            //            else if (distance > sprintDistance)
            //                input.Press("Sprint");

            //            if (body && body.currentState == CharacterBody.MoveState.Grounded)
            //            {
            //                Vector3 jumpTest = body.cameraSlot.position + body.transform.forward * body.cap.radius * 1.5f;

            //                if (Physics.Raycast(jumpTest, -body.transform.up, out RaycastHit hit, body.cap.height, eyeMask, QueryTriggerInteraction.Ignore)
            //                    && Mathf.Abs(hit.distance - body.cap.height) > body.stepOffset)
            //                {
            //                    input.Press("Jump", 0.125f);
            //                    input.Press("Forward", 0.25f);
            //                }
            //            }
            //        }

            //        if (distance < 5 && (cb = ai.target.GetComponent<CharacterBody>()))
            //            ai.lookDestination = cb.cameraSlot.position;  
            //        else
            //            ai.lookDestination = (Vector3)ai.destination + body.transform.up * body.cap.height / 2;

            //        input.Move(Look);
            //    }
            //}

            return GOAPStatus.Running;
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
