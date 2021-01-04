using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Jobs;
using UnityEngine;
using MPConsole;
using UnityEngine.Profiling;
using MPWorld;

//#pragma warning disable IDE0052 // Remove unread private members
//#pragma warning disable IDE0044 // Add readonly modifier
//#pragma warning disable IDE0051 // Remove unused private members
//#pragma warning disable UNT0001 // Empty Unity message
//#pragma warning disable IDE0059 // Unnecessary assignment of a value
//#pragma warning disable CS0414 // Unnecessary assignment of a value
namespace MPCore
{
    [ContainsConsoleCommands]
    public class CharacterAI2 : MonoBehaviour
    {
        public float viewAngle = 45;
        public LineRenderer debugLine;


        //private LineRenderer line;
        private CharacterBody body;
        private InputManager input;
        private Character character;

        private static readonly string[] layers = { "Default", "Physical", "Player" };
        private static int layerMask;

        // Path
        private JobHandle pathJob = default;
        private readonly List<Vector3> path = new List<Vector3>();
        private float pathPosition = 0;
        // Targeting
        private float nextTargetTime = 0;
        private float targetSatisfactionDistance = 1f;
        //private Component lookTarget;
        private SightInfo sight;
        private TouchInfo touch;
        private Vector3 moveDest;
        // Looking
        //private Vector3 lookDir;
        private const float angularVelocity = 420;
        private const float slowAngle = 45f;
        //Combat
        public bool hostile = true;

        private LineRenderer line;

        private static readonly HashSet<Type> attackTargets = new HashSet<Type>() { typeof(Character) };
        private static readonly Dictionary<Type, float> satisfactionDistances = new Dictionary<Type, float>()
        {
            {typeof(Character), 3f },
            {typeof(InventoryObject), 0.5f }
        };

        public struct SightInfo
        {
            public dynamic target;
            public Vector3 focalPoint;
            public Vector3 lookDirection;
        }

        public struct TouchInfo
        {
            public GameObject target;
            public float time;
            public Vector3 normal;
            public Vector3 point;
        }

        private void Awake()
        {
            body = GetComponent<CharacterBody>();
            input = GetComponent<InputManager>();
            character = GetComponent<Character>();

            layerMask = LayerMask.GetMask(layers);

            //lookDir = body.cameraSlot.forward;

            PauseManager.Add(OnPauseUnPause);
            MPConsole.Console.RegisterInstance(this);
            character.OnPlayerSet += OnSetPlayer;
        }

        private void OnEnable()
        {
            input.OnMouseMove += OnMouseMove;
            character.OnHit += OnHit;
        }

        private void OnDisable()
        {
            input.OnMouseMove -= OnMouseMove;
            character.OnHit -= OnHit;
        }

        private void OnDestroy()
        {
            if (line)
                Destroy(line.gameObject);

            PauseManager.Remove(OnPauseUnPause);
            MPConsole.Console.RemoveInstance(this);
            character.OnPlayerSet -= OnSetPlayer;
        }

        void Update()
        {
            // Invalid State
            if (Time.timeScale < float.Epsilon)
                return;

            // Request Path
            Profiler.BeginSample("RequestPath");
            if (pathJob.IsCompleted
                && (path.Count == 0
                || nextTargetTime < Time.time
                || Vector3.Distance(transform.position, path[path.Count - 1]) < targetSatisfactionDistance))
                pathJob = RequestNewPath();
            Profiler.EndSample();

            // Move Along Path
            Profiler.BeginSample("MoveAlongPath");
            if (path.Count > 0)
                MoveAlongPath();
            Profiler.EndSample();

            Profiler.BeginSample("MouseMove");
            //lookTarget = FindTarget(attackTargets);
            sight.target = FindTarget(attackTargets);
            Profiler.EndSample();

            if (Debugger.enabled)
            {
                if (line)
                {
                    line.positionCount = path.Count;

                    for (int i = 0; i < path.Count; i++)
                        line.SetPosition(i, path[i]);

                }
                else if (debugLine)
                    line = Instantiate(debugLine).GetComponent<LineRenderer>();
            }
        }

        private void OnSetPlayer(bool isPlayer)
        {
            enabled = !isPlayer;
        }

        private JobHandle RequestNewPath()
        {
            Component moveTarget = FindTarget();
            Vector3 moveDestination = GetMoveDestination(moveTarget);

            nextTargetTime = Time.time + 10f;
            path.Clear();
            pathPosition = 0;

            if (!moveTarget || !satisfactionDistances.TryGetValue(moveTarget.GetType(), out targetSatisfactionDistance))
                targetSatisfactionDistance = 0.5f;

            return Navigator.RequestPath(transform.position, moveDestination, path, body.cap.height / 2f);
        }

        private Vector3 GetMoveDestination(Component moveTarget)
        {
            if (moveTarget)
                return moveTarget.transform.position;
            else
                return Navigator.RandomPoint(body.cap.height / 2);
        }

        private static bool TryMax(float p, ref float priority) => priority != p && (priority = Mathf.Max(p, priority)) == p;

        private Component FindTarget(HashSet<Type> types = null)
        {
            float bestPriority = float.MinValue;
            Component bestTarget = null;

            foreach (Component candidate in AiInterestPoints.interestPoints)
                if (candidate && !candidate.Equals(character) && (types == null || types.Contains(candidate.GetType())))
                {
                    float priority = float.MinValue;

                    if (candidate is Character && IsTargetVisible(candidate, viewAngle, out _))
                        priority = 100f - Vector3.Distance(transform.position, candidate.transform.position);
                    else if (candidate is InventoryObject io)
                        if (io.inventory is HealthPickup hp && character.Health < 100 && character.Health != 0)
                            priority = (100f - character.Health) * 5f - Vector3.Distance(transform.position, candidate.transform.position);

                    if (TryMax(priority, ref bestPriority))
                        bestTarget = candidate;
                }

            return bestTarget;
        }

        private bool IsTargetVisible(Component target, float viewAngle, out RaycastHit hit)
        {
            hit = default;

            return Vector3.Angle(target.transform.position - body.cameraSlot.position, body.cameraSlot.forward) <= viewAngle
                && Physics.Raycast(body.cameraSlot.position,
                    target.transform.position - body.cameraSlot.position,
                    out hit,
                    Vector3.Distance(body.cameraSlot.position, target.transform.position),
                    layerMask,
                    QueryTriggerInteraction.Ignore)
                && hit.collider.gameObject.Equals(target.gameObject);
        }

        private Vector2 OnMouseMove(float dt)
        {
            // Set look direction
            if (sight.target is Character character)
            {
                if (character.TryGetComponent(out CharacterBody body))
                    sight.focalPoint = body.cameraAnchor.position;
                else
                    sight.focalPoint = character.transform.position;
            }
            else if (touch.target && Time.time - touch.time < 3)
                sight.focalPoint = transform.position - touch.normal.normalized * 20f;
            else if (path.Count > 0)
                sight.focalPoint = Navigator.GetPositionOnPath(path, pathPosition, 5f) + body.transform.up * body.cap.height / 2f;
            else
                sight.focalPoint = transform.TransformPoint(transform.forward * 1000f);

            // Velocity Prediction
            Vector3 velocity = Vector3.zero;

            if (sight.target is Component component)
                if (component.TryGetComponent(out IGravityUser gu))
                    velocity = gu.Velocity;
                else if (component.TryGetComponent(out Collider collider))
                    if (collider.attachedRigidbody is Rigidbody rb)
                        velocity = rb.velocity;

            float distance = Vector3.Distance(this.body.cameraSlot.position, sight.focalPoint);
            float impactTime = distance / 100f;
            Vector3 predictOffset = velocity * impactTime;
            sight.focalPoint += predictOffset;

            // Convert point to direction
            sight.lookDirection = sight.focalPoint - body.cameraSlot.position;

            // Combat
            if (hostile && sight.target is Character
                && Vector3.Angle(sight.lookDirection, this.body.cameraSlot.forward) < 5f)
            {
                input.Press("Fire", 0.5f);
            }

            // MouseLook
            Vector3 lookDirX = Vector3.ProjectOnPlane(sight.lookDirection, body.transform.up);
            Vector3 lookDirY = Vector3.ProjectOnPlane(sight.lookDirection, body.transform.right);
            float currentAngleY = Mathf.PingPong(Vector3.Angle(body.transform.forward, body.cameraSlot.forward), 90) * Mathf.Sign(Vector3.Dot(body.transform.up, body.cameraSlot.forward));
            float desiredY = Mathf.PingPong(Vector3.Angle(body.transform.forward, lookDirY), 90) * Mathf.Sign(Vector3.Dot(body.transform.up, lookDirY));
            float mouseVelocity = angularVelocity * Mathf.Clamp(Vector3.Angle(body.cameraSlot.forward, sight.lookDirection) / Mathf.Max(1f, slowAngle), 0.1f, 1f);
            Vector2 mouseDir = new Vector2(
                Vector3.Angle(body.transform.forward, lookDirX) * Mathf.Sign(Vector3.Dot(body.transform.right, lookDirX)),
                desiredY - currentAngleY);

            return Vector2.ClampMagnitude(mouseDir, mouseVelocity * dt);
        }


        private void MoveAlongPath()
        {
            pathPosition = Navigator.GetCoordinatesOnPath(path, transform.position, pathPosition, out float off);
            moveDest = Navigator.GetPositionOnPath(path, pathPosition, Mathf.Max(1.5f, 2f - off));
            if (GetComponentInChildren<SphereCollider>() is var col && col)
                col.transform.position = moveDest;
            if (GetComponentInChildren<BoxCollider>() is var box && box)
                box.transform.position = Navigator.GetPositionOnPath(path, pathPosition);
            Vector3 direction = Vector3.ProjectOnPlane(moveDest - transform.position, transform.up);
            float fAngle = Vector3.Angle(transform.forward, direction);
            float rAngle = Vector3.Angle(transform.right, direction);
            //float distance = Vector3.Distance(transform.position, moveTarget.transform.position);

            if (fAngle < 67.5f)
                input.Press("Forward");
            else if (fAngle > 112.5)
                input.Press("Reverse");

            if (rAngle < 67.5f)
                input.Press("Right");
            else if (rAngle > 112.5)
                input.Press("Left");

            //if (distance < 5f)
            //    input.Press("Walk");
            //else if (distance > 15f)
            input.Press("Sprint");

            if (body.currentState == CharacterBody.MoveState.Grounded)
            {
                float upAngle = Vector3.Angle(transform.up, moveDest - transform.position);

                if (upAngle < 40
                    || Physics.SphereCast(transform.position, body.cap.radius * 0.5f, transform.forward, out _, body.cap.radius * 2))
                {
                    input.Press("Jump", 0.125f);
                    input.Press("Crouch", 0.25f);
                    input.Press("Forward", 0.25f);
                }
            }
        }

        private void OnHit(int damage, GameObject instigator, GameObject conduit, DamageType damageType, Vector3 direction)
        {
            touch.target = instigator;
            touch.normal = direction;
            touch.time = Time.time;
        }

        private void OnPauseUnPause(bool paused)
        {
            if(!character.isPlayer)
                enabled = !paused;
        }

        [ConsoleCommand("posess", "Toggle AI for players")]
        public void Posess()
        {
            if(character.isPlayer)
                enabled = !enabled;
        }
    }
}