using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MPConsole;
using MPWorld;
using System;
using Unity.Jobs;
using UnityEngine.Profiling;

namespace MPCore.AI
{
    public class Idle : StateMachineBehaviour
    {
        public float viewAngle = 45;
        public LineRenderer debugLine;

        //Combat
        public bool hostile = true;

        private KeyModel _keyModel;

        // Character References
        private bool enabled = false;
        private Transform transform;
        private Animator animator;
        private InventoryContainer container;
        private CharacterBody body;
        private DamageEvent damageEvent;
        private InputManager input;
        private CharacterInput characterInput;
        private Character character;
        private WeaponSwitcher weapons;
        private int id;

        // Path
        private JobHandle pathJob = default;
        private readonly List<Vector3> path = new List<Vector3>();
        private float pPosition = 0;
        private Vector3 position;

        // Targeting
        private float nextTargetTime = 0;
        private float targetSatisfactionDistance = 1f;
        private OmniInfo omni;
        private SightInfo sight;
        private TouchInfo touch;
        private Vector3 pathDestination;
        private dynamic moveTarget;

        // Skills
        private float velocityExtrapolation = 1f;
        private float accuracy = 0.6f;

        // Skill-Related Values
        private float maxInaccuracyDistance = 5f;
        private const float angularVelocity = 420;
        private const float slowAngle = 45f;
        private float projectileSpeed = 100f;
        private Vector3 projectileOffset;
        private Vector3 accuracyOffset;

        // Debug
        private GameModel _gameModel;
        private LineRenderer pathLine;

        private static readonly string[] layers = { "Default", "Physical", "Player" };
        private static int layerMask;
        private static readonly HashSet<Type> attackTargets = new HashSet<Type>() { typeof(Character) };
        private static readonly Dictionary<Type, float> satisfactionDistances = new Dictionary<Type, float>()
        {
            {typeof(Character), 0.5f },
            {typeof(InventoryPickup), 0.5f }
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

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            // Awake
            _gameModel = Models.GetModel<GameModel>();
            _keyModel = Models.GetModel<KeyModel>();
            this.animator = animator;
            transform = animator.transform;

            animator.TryGetComponent(out container);
            animator.TryGetComponent(out body);
            animator.TryGetComponent(out input);
            animator.TryGetComponent(out characterInput);
            animator.TryGetComponent(out character);
            animator.TryGetComponent(out weapons);
            animator.TryGetComponent(out damageEvent);
            layerMask = LayerMask.GetMask(layers);
            id = animator.gameObject.GetInstanceID() % 4096;

            // Enable
            input.OnMouseMove += OnMouseMove;
            damageEvent.OnHit += OnHit;

            enabled = true;
        }

        public override void OnStateMachineExit(Animator animator, int stateMachinePathHash)
        {
            input.OnMouseMove -= OnMouseMove;
            damageEvent.OnHit -= OnHit;
        }

        private void OnDestroy()
        {
            if (pathLine)
                Destroy(pathLine.gameObject);
        }


        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            Transform transform = animator.transform;

            // Update Path Cursor
            position = transform.position - transform.up * body.cap.height * 0.5f;

            // Invalid State
            if (Time.timeScale < float.Epsilon)
                return;

            // Request Path
            Profiler.BeginSample("RequestPath");
            if (pathJob.IsCompleted
                && (path.Count == 0
                || nextTargetTime < Time.time
                || 0.75f >= Vector3.Distance(position, path[path.Count - 1])
                || (!(moveTarget is InventoryPickup)
                    && sight.target is Character
                    && weapons.currentWeapon
                    && weapons.currentWeapon.preferredCombatDistance <= Vector3.Distance(sight.lastSeenPosition, path[path.Count - 1])
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

            if (_gameModel.debug.Value)
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

        private JobHandle RequestNewPath()
        {
            Component moveTarget = FindTarget();
            Vector3 moveDestination = GetMoveDestination(moveTarget);

            // These should be done on callback
            nextTargetTime = Time.time + 30f;
            pPosition = 0;

            if (!moveTarget || !satisfactionDistances.TryGetValue(moveTarget.GetType(), out targetSatisfactionDistance))
                targetSatisfactionDistance = 0.5f;

            return Navigator.RequestPath(position, moveDestination, path);
        }

        private Vector3 GetMoveDestination(Component _)
        {
            if (moveTarget is Character target && target
            && 0.5f >= Time.time - sight.lastSeen
            && weapons.currentWeapon)
            {
                Vector3 randPosition = UnityEngine.Random.insideUnitSphere * weapons.currentWeapon.preferredCombatDistance;

                // Prevent Random Position from Jumping Rooms
                if (Physics.Raycast(target.transform.position, randPosition, out RaycastHit hit, randPosition.magnitude, layerMask, QueryTriggerInteraction.Ignore))
                    return hit.point;
                else
                    return target.transform.position + randPosition;
            }
            else if (moveTarget is Component component && component)
                return component.transform.position;
            else if (Time.time - sight.lastSeen < 200f)
                return sight.lastSeenPosition + UnityEngine.Random.insideUnitSphere * Mathf.Clamp(Time.time - sight.lastSeen, 0, 3f);
            else
                return Navigator.RandomPoint(body.cap.height / 2);
        }

        private static readonly HashSet<string> storedInventoryTEMP = new HashSet<string>();
        private Component FindTarget(HashSet<Type> types = null)
        {
            (Component bestTarget, float bestPriority) sightTarget = default;
            (Component bestTarget, float bestPriority) omniTarget = default;

            storedInventoryTEMP.Clear();
            foreach (Inventory i in container.inventory)
                storedInventoryTEMP.Add(i.resourcePath);

            foreach (Component candidate in AIBlackboard.visualTargets)
                if (candidate && candidate != this.character && (types == null || types.Contains(candidate.GetType())))
                    if (candidate is Character && IsTargetVisible(candidate, viewAngle, out _))
                    {
                        float distance = Vector3.Distance(transform.position, candidate.transform.position);
                        float priority = 1f / distance;

                        if (transform.TryGetComponent(out WeaponSwitcher mySwitcher)
                            && candidate.TryGetComponent(out WeaponSwitcher theirSwitcher)
                            && mySwitcher.currentWeapon
                            && theirSwitcher.currentWeapon
                            && mySwitcher.currentWeapon.shortName == "Knife"
                            //&& distance >= mySwitcher.currentWeapon.preferredCombatDistance
                            && theirSwitcher.currentWeapon.shortName != "Knife")
                        {
                            priority += 2000f;
                        }

                        if (priority > sightTarget.bestPriority)
                            sightTarget = (candidate, priority);
                    }
                    else if (candidate is InventoryPickup io)
                        if (io.inventory is HealthPickup hp && character.health != null && character.health.value < character.health.maxValue * hp.percentOfMax)
                        {
                            float distance = Vector3.Distance(transform.position, candidate.transform.position);
                            float healthPriority = 1f - character.health.value / character.health.maxValue * hp.percentOfMax;
                            float priority = healthPriority / distance * 0.5f;

                            if (priority > omniTarget.bestPriority)
                                omniTarget = (candidate, priority);
                        }
                        else if (io.inventory is Weapon w && !storedInventoryTEMP.Contains(w.resourcePath))
                        {
                            float distance = Vector3.Distance(transform.position, candidate.transform.position);
                            float priority = 1f / distance;

                            if (priority > omniTarget.bestPriority)
                                omniTarget = (candidate, priority);
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

                if (omniTarget.bestTarget)
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

            // Get look direction
            if (0.5f >= Time.time - sight.lastSeen 
                && sight.target is Character character
                && character)
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
                sight.focalPoint = Navigator.PathLerp(path, pPosition, 5f) + body.transform.up * body.cap.height;
            else
                sight.focalPoint = transform.TransformPoint(transform.forward * 1000f);

            // Shared values
            float targetDistance = Vector3.Distance(this.body.cameraSlot.position, sight.focalPoint);
            Vector3 targetVelocity = Vector3.zero;

            if (sight.target is Component component)
                if (component.TryGetComponent(out IGravityUser gu))
                    targetVelocity = gu.Velocity;
                else if (component.TryGetComponent(out Collider collider))
                    if (collider.attachedRigidbody is Rigidbody rb)
                        targetVelocity = rb.velocity;

            // Weapon Switch
            if (tick % 120 == 0
                && sight.target is Character)
            {
                (Weapon weapon, float priority) switchWeapon = (null, float.MaxValue);

                foreach (Inventory i in container.inventory)
                    if (i is Weapon w)
                    {
                        float dist = Mathf.Abs(w.preferredCombatDistance - targetDistance);

                        if (dist < switchWeapon.priority)
                            switchWeapon = (w, dist);
                    }

                weapons.DrawWeapon(switchWeapon.weapon);
            }

            // Recalculate predicted projectile speed
            if (tick % 30 == 0)
                if (weapons.currentWeapon
                    && weapons.currentWeapon.projectilePrimary
                    && weapons.currentWeapon.projectilePrimary.TryGetComponent(out Projectile p))
                    projectileSpeed = p.shared.exitSpeed;
                else
                    projectileSpeed = float.MaxValue * 0.5f;

            // Extrapolate Velocity
            if (tick % 1 == 0)
            {
                projectileOffset = targetVelocity * targetDistance / projectileSpeed;
                projectileOffset += UnityEngine.Random.insideUnitSphere * (1f - velocityExtrapolation) * projectileOffset.magnitude;
            }

            // Recalculate Accuracy
            if (tick % 60 == 0)
            {
                float velocityScale = Mathf.Clamp01(Vector3.ProjectOnPlane(targetVelocity - body.Velocity * 0.5f, body.cameraSlot.forward).magnitude / body.defaultSprintSpeed);
                accuracyOffset = UnityEngine.Random.insideUnitSphere * (1f - accuracy) * velocityScale;
            }

            sight.focalPoint += projectileOffset;
            sight.focalPoint += accuracyOffset * Mathf.Min(maxInaccuracyDistance, targetDistance);

            // Get Look Direction
            sight.lookDirection = sight.focalPoint - body.cameraSlot.position;

            // Combat
            if (tick % 30 == 0
                && hostile
                && weapons.currentWeapon
                && sight.target is Character
                && weapons.currentWeapon.validFireAngle >= Vector3.Angle(sight.lookDirection, this.body.cameraSlot.forward)
                && weapons.currentWeapon.engagementRange >= Vector3.Distance(sight.focalPoint, this.body.cameraSlot.position))
                input.BotKeyDown("Fire", 0.5f);

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
            //pPosition = Navigator.GetCoordinatesOnPath(path, pathCursor, pPosition, out float offPathDistance);
            Vector3 pClamp = Navigator.ClampToPath(path, position, out float pPosition);
            float pDistance = Vector3.ProjectOnPlane(pClamp - position, animator.transform.up).magnitude;
            pathDestination = Navigator.PathLerp(path, pPosition, Mathf.Max(1f, 1.5f - pDistance));

            Vector3 direction = Vector3.ProjectOnPlane(pathDestination - position, transform.up);
            float fAngle = Vector3.Angle(transform.forward, direction);
            float rAngle = Vector3.Angle(transform.right, direction);

            if (fAngle < 67.5f)
                input.BotKeyDown("Forward");
            else if (fAngle > 112.5)
                input.BotKeyDown("Reverse");

            if (rAngle < 67.5f)
                input.BotKeyDown("Right");
            else if (rAngle > 112.5)
                input.BotKeyDown("Left");

            if (!character.isPlayer || !_keyModel.alwaysRun)
                input.BotKeyDown("Sprint");

            if (body.currentState == CharacterBody.MoveState.Grounded)
            {
                float upAngle = Vector3.Angle(transform.up, pathDestination - position);

                if (upAngle < 40
                    || Physics.SphereCast(transform.position, body.cap.radius * 0.5f, transform.forward, out _, body.cap.radius * 2))
                {
                    input.BotKeyDown("Jump", 0.125f);
                    input.BotKeyDown("Crouch", 0.25f);
                    input.BotKeyDown("Forward", 0.25f);
                }
            }

            if (pDistance > 2f)
                RequestNewPath();
        }

        private void OnHit(DamageTicket ticket)
        {
            touch.instigator = ticket.instigator;
            touch.direction = ticket.momentum;
            touch.time = Time.time;
        }
    }
}
