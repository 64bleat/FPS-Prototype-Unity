using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Jobs;
using UnityEngine;
using MPConsole;
using UnityEngine.Profiling;

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
        private Component lookTarget;
        private Vector3 moveDest;
        private Vector3 lookDest;
        // Looking
        private Vector3 lookDir;
        private const float angularVelocity = 180;
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

        private void Awake()
        {
            body = GetComponent<CharacterBody>();
            input = GetComponent<InputManager>();
            character = GetComponent<Character>();

            layerMask = LayerMask.GetMask(layers);

            lookDir = body.cameraSlot.forward;

            PauseManager.Add(OnPauseUnPause);
            MPConsole.Console.RegisterInstance(this);
            character.OnPlayerSet += OnSetPlayer;
        }

        private void OnEnable()
        {
            input.OnMouseMove += OnMouseMove;
        }

        private void OnDisable()
        {
            input.OnMouseMove -= OnMouseMove;
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
            Profiler.BeginSample("Invalid State");
            // Invalid State
            if (Time.timeScale < float.Epsilon)
                return;
            Profiler.EndSample();

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
            lookTarget = FindTarget(attackTargets);
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
                        if (io.inventory is HealthPickup hp && character.Health < 100)
                            priority = (100f - character.Health) * 5f - Vector3.Distance(transform.position, candidate.transform.position);

                    if (TryMax(priority, ref bestPriority))
                        bestTarget = candidate;
                }

            //if (bestTarget == null)
            //    bestTarget = this;

            return bestTarget;
        }

        //private void SetPath(Vector3[] p)
        //{
        //    path = new;
        //}

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

        private Vector2 MouseDeltaTarget(float time)
        {
            // Set lookDir
            if (lookTarget 
                && lookTarget is Character 
                && (!Physics.Raycast(
                    origin: body.cameraSlot.position,
                    direction: lookTarget.transform.position - body.cameraSlot.position,
                    hitInfo: out RaycastHit hit,
                    maxDistance: Vector3.Distance(body.cameraSlot.position, lookTarget.transform.position),
                    layerMask: layerMask,
                    queryTriggerInteraction: QueryTriggerInteraction.Ignore)
                || hit.collider.transform.IsChildOf(lookTarget.transform)))
            {
                if (lookTarget is Character c && c.gameObject.GetComponent<CharacterBody>() is CharacterBody b && b)
                    lookDir = b.cameraSlot.position - body.cameraSlot.position;
                else
                    lookDir = lookTarget.transform.position - body.cameraSlot.position;

                // Combat
                if (hostile 
                    && lookTarget is Character 
                    && Vector3.Angle(lookDir, body.cameraSlot.forward) < 5f)
                {
                    input.Press("Fire", 0.5f);
                }
            }
            else if (path.Count > 0)
                lookDir = Navigator.GetPositionOnPath(path, pathPosition, 2f) - transform.position;
            else
                lookDir = transform.forward;

            // MouseLook
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

        private Vector2 OnMouseMove(float dt)
        {
            // Set lookDir
            if (lookTarget
                && lookTarget is Character
                && (!Physics.Raycast(
                    origin: body.cameraSlot.position,
                    direction: lookTarget.transform.position - body.cameraSlot.position,
                    hitInfo: out RaycastHit hit,
                    maxDistance: Vector3.Distance(body.cameraSlot.position, lookTarget.transform.position),
                    layerMask: layerMask,
                    queryTriggerInteraction: QueryTriggerInteraction.Ignore)
                || hit.collider.transform.IsChildOf(lookTarget.transform)))
            {
                if (lookTarget is Character c && c.gameObject.GetComponent<CharacterBody>() is CharacterBody b && b)
                    lookDir = b.cameraSlot.position - body.cameraSlot.position;
                else
                    lookDir = lookTarget.transform.position - body.cameraSlot.position;

                // Combat
                if (hostile
                    && lookTarget is Character
                    && Vector3.Angle(lookDir, body.cameraSlot.forward) < 5f)
                {
                    input.Press("Fire", 0.5f);
                }
            }
            else if (path.Count > 0)
                lookDir = Navigator.GetPositionOnPath(path, pathPosition, 2f) - transform.position;
            else
                lookDir = transform.forward;

            // MouseLook
            Vector3 lookDirX = Vector3.ProjectOnPlane(lookDir, body.transform.up);
            Vector3 lookDirY = Vector3.ProjectOnPlane(lookDir, body.transform.right);
            float currentAngleY = Mathf.PingPong(Vector3.Angle(body.transform.forward, body.cameraSlot.forward), 90) * Mathf.Sign(Vector3.Dot(body.transform.up, body.cameraSlot.forward));
            float desiredY = Mathf.PingPong(Vector3.Angle(body.transform.forward, lookDirY), 90) * Mathf.Sign(Vector3.Dot(body.transform.up, lookDirY));
            float mouseVelocity = angularVelocity * Mathf.Clamp01(Vector3.Angle(body.cameraSlot.forward, lookDir) / Mathf.Max(1f, slowAngle));
            Vector2 mouseDir = new Vector2(
                Vector3.Angle(body.transform.forward, lookDirX) * Mathf.Sign(Vector3.Dot(body.transform.right, lookDirX)),
                desiredY - currentAngleY);

            return mouseDir.normalized * Mathf.Min(mouseDir.magnitude, mouseVelocity * dt);
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