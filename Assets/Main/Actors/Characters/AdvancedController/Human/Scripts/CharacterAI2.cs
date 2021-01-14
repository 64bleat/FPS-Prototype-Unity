using MPConsole;
using MPWorld;
using System;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Profiling;

namespace MPCore
{
    [ContainsConsoleCommands]
    public class CharacterAI2 : MonoBehaviour
    {
        public float viewAngle = 45;
        public LineRenderer debugLine;

        //Combat
        public bool hostile = true;

        // Character References
        private CharacterBody body;
        private InputManager input;
        private CharacterInput characterInput;
        private Character character;
        private WeaponSwitcher weapons;
        private int id;

        // Path
        private JobHandle pathJob = default;
        private readonly List<Vector3> path = new List<Vector3>();
        private float pathPosition = 0;

        // Targeting
        private float nextTargetTime = 0;
        private float targetSatisfactionDistance = 1f;
        private OmniInfo omni;
        private SightInfo sight;
        private TouchInfo touch;
        private Vector3 moveDest;
        private dynamic moveTarget;

        // Ability
        private const float angularVelocity = 420;
        private const float slowAngle = 45f;
        private float projectileSpeed = 100f;

        // Debug
        private LineRenderer pathLine;

        private static readonly string[] layers = { "Default", "Physical", "Player" };
        private static int layerMask;
        private static readonly HashSet<Type> attackTargets = new HashSet<Type>() { typeof(Character) };
        private static readonly Dictionary<Type, float> satisfactionDistances = new Dictionary<Type, float>()
        {
            {typeof(Character), 0.5f },
            {typeof(InventoryObject), 0.5f }
        };

        public struct OmniInfo
        {
            public dynamic target;
            public float priority;
            public Vector3 position;
        }

        public struct SightInfo
        {
            public float priority;
            public float lastSeen;
            public dynamic target;
            public Vector3 focalPoint;
            public Vector3 lookDirection;
            public Vector3 lastSeenPosition;
        }

        public struct TouchInfo
        {
            public CharacterInfo instigator;
            public float time;
            public Vector3 direction;
            public Vector3 point;
        }

        private void Awake()
        {
            TryGetComponent(out body);
            TryGetComponent(out input);
            TryGetComponent(out characterInput);
            TryGetComponent(out character);
            TryGetComponent(out weapons);

            layerMask = LayerMask.GetMask(layers);
            id = gameObject.GetInstanceID() % 4096;

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
            if (pathLine)
                Destroy(pathLine.gameObject);

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
                || 0.75f >= Vector3.Distance(transform.position, path[path.Count - 1])
                ||  (!(moveTarget is InventoryObject) 
                    && sight.target is Character
                    && weapons.heldWeapon 
                    && weapons.heldWeapon.preferredCombatDistance <= Vector3.Distance(sight.lastSeenPosition, path[path.Count - 1])
                    )
                ))
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
                if (pathLine)
                {
                    pathLine.positionCount = path.Count;

                    for (int i = 0; i < path.Count; i++)
                        pathLine.SetPosition(i, path[i]);

                }
                else if (debugLine)
                    pathLine = Instantiate(debugLine).GetComponent<LineRenderer>();
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
            pathPosition = 0;

            if (!moveTarget || !satisfactionDistances.TryGetValue(moveTarget.GetType(), out targetSatisfactionDistance))
                targetSatisfactionDistance = 0.5f;

            return Navigator.RequestPath(transform.position, moveDestination, path, body.cap.height / 2f);
        }

        private Vector3 GetMoveDestination(Component _)
        {
            if (moveTarget is Character character && character
            && 0.5f >= Time.time - sight.lastSeen
            && weapons.heldWeapon)
            {
                Vector3 rand = UnityEngine.Random.insideUnitSphere * weapons.heldWeapon.preferredCombatDistance;

                if (Physics.Raycast(character.transform.position, rand, out RaycastHit hit, rand.magnitude, layerMask, QueryTriggerInteraction.Ignore))
                    return hit.point;
                else
                    return character.transform.position + rand;
            }
            else if (moveTarget is Component component && component)
                return component.transform.position;
            else
                return Navigator.RandomPoint(body.cap.height / 2);
        }

        private static bool TryMax(float p, ref float priority) => priority != p && (priority = Mathf.Max(p, priority)) == p;

        private Component FindTarget(HashSet<Type> types = null)
        {
            (Component bestTarget, float bestPriority) sightTarget = default;
            (Component bestTarget, float bestPriority) omniTarget = default;

            foreach (Component candidate in AiInterestPoints.interestPoints)
                if (candidate && candidate != this.character && (types == null || types.Contains(candidate.GetType())))
                    if (candidate is Character && IsTargetVisible(candidate, viewAngle, out _))
                    {
                        float priority = 100f - Vector3.Distance(transform.position, candidate.transform.position);

                        if (TryMax(priority, ref sightTarget.bestPriority))
                            sightTarget.bestTarget = candidate;
                    }
                    else if (candidate is InventoryObject io)
                        if (io.inventory is HealthPickup && character.Health < 100 && character.Health != 0)
                        {
                            float priority = (100f - character.Health) * 5f - Vector3.Distance(transform.position, candidate.transform.position);

                            if (TryMax(priority, ref omniTarget.bestPriority))
                                omniTarget.bestTarget = io;
                        }

            if (sightTarget.bestTarget)
            {
                sight.target = sightTarget.bestTarget;
                sight.lastSeen = Time.time;
                sight.lastSeenPosition = sightTarget.bestTarget.transform.position;
            }

            //if(moveTarget.bestTarget)
            {
                omni.target = omniTarget.bestTarget;
                omni.priority = omniTarget.bestPriority;

                if(omniTarget.bestTarget)
                    omni.position = omniTarget.bestTarget.transform.position;
            }

            if (omni.priority > sight.priority)
                moveTarget = omni.target;
            else if (Time.time - sight.lastSeen < 0.5f)
                moveTarget = sight.target;
            else
                moveTarget = null;

            return sightTarget.bestTarget;
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
            float tick = Time.frameCount + id;
            // Set look direction
            if (0.5f >= Time.time - sight.lastSeen && sight.target is Character character)
            {
                if (character.TryGetComponent(out CharacterBody body))
                    sight.focalPoint = body.cameraAnchor.position;
                else
                    sight.focalPoint = character.transform.position;
            }
            else if (touch.instigator && Time.time - touch.time < 1.5f)
                sight.focalPoint = transform.position - touch.direction.normalized * 20f;
            else if (1.5f >= Time.time - sight.lastSeen)
                sight.focalPoint = sight.lastSeenPosition;
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
            float projectileInterceptTime = distance / projectileSpeed;
            Vector3 predictOffset = velocity * projectileInterceptTime;
            sight.focalPoint += predictOffset;

            // Convert point to direction
            sight.lookDirection = sight.focalPoint - body.cameraSlot.position;

            // Weapon Switch
            if (tick % 120 == 0)
            {
                (Weapon weapon, float priority) switchWeapon = (null, float.MaxValue);

                foreach (Inventory i in this.character.inventory)
                    if (i is Weapon w)
                    {
                        float dist = Mathf.Abs(w.preferredCombatDistance - distance);

                        if (dist < switchWeapon.priority)
                            switchWeapon = (w, dist);
                    }

                if (switchWeapon.weapon
                    && switchWeapon.weapon.projectilePrimary
                    && switchWeapon.weapon.projectilePrimary.TryGetComponent(out Projectile p))
                    projectileSpeed = p.shared.exitSpeed;
                else
                    projectileSpeed = float.MaxValue * 0.5f;

                weapons.DrawWeapon(switchWeapon.weapon);
            }

            // Combat
            if (tick % 30 == 0
                && hostile
                && weapons.heldWeapon
                && sight.target is Character
                && weapons.heldWeapon.validFireAngle >= Vector3.Angle(sight.lookDirection, this.body.cameraSlot.forward)
                && weapons.heldWeapon.engagementRange >= Vector3.Distance(sight.focalPoint, this.body.cameraSlot.position))
                input.Press("Fire", 0.5f);

            

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

            if(!character.isPlayer || !input.loadKeyBindList.alwaysRun)
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

        private void OnHit(int damage, GameObject conduit, CharacterInfo instigator, DamageType damageType, Vector3 direction)
        {
            touch.instigator = instigator;
            touch.direction = direction;
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