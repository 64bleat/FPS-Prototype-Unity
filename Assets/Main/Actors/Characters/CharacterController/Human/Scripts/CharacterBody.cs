using MPGUI;
using MPWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MPCore
{
    public class CharacterBody : MonoBehaviour//, IGravityUser
    {
        public enum MoveState { Grounded, Airborne }
        private static readonly string[] collisionLayers = { "Default", "Physical", "Player" };
        private static readonly Collider[] cBuffer = new Collider[20];

        // SERIALIZED
        [Header("Abilities")]
        public bool enableAlwaysRun = true;
        public bool enableCrouch = true;
        public bool enableJump = true;
        [Header("Modules")]
        public WallJumpBoots wallJump;
        public WallClimbGloves wallClimb;
        [Header("Movement")]
        public float defaultWalkSpeed = 2.15f;
        public float defaultMoveSpeed = 5.5f;
        public float defaultStrideSpeed = 7f;
        public float defaultSprintSpeed = 7.5f;
        public float defaultGroundAcceleration = 40f;
        public float defaultStrideAcceleration = 1f;
        public float defaultGroundDecceleration = 25f;
        public float defaultAirAcceleration = 6f;
        public float defaultSpeedDecceleration = 2.5f;
        public float defaultslopeLimit = 46;
        public int fallForgiveness = 5;
        [Header("Crouching")]
        public float defaultCrouchDownSpeed = 5.5f;
        public float defaultCrouchUpSpeed = 4f;
        [Header("Jumping")]
        public float defaultJumpHeight = 0.8f;
        public float defaultCrouchJumpHeight = 0.8f;
        public float defaultBunnyHopRate = 0.5f;
        public float defaultMaxKickVelocity = 15f;
        [Header("Gliding")]
        public float defaultGlideAngle = 60f;
        public float defaultSpeedToLift = 9f;
        public float defaultDrag = 2.5f;
        public float defaultJetAccel = 0f;
        public float defaultJetSpeed = 200f;
        [Header("Other")]
        public bool leftHanded = false;
        public float defaultHeight = 1.65f;
        public float defaultCrouchHeight = 0.7f;
        public float defaultStepOffset = 0.3f;
        public float defaultMaxSafeImpactSpeed = 11f;
        public float defaultGroundStickThreshold = 11f;
        public float defaultMass = 80f;
        [Header("Components")]
        public GameObject thirdPersonBody;
        public Transform cameraAnchor;
        public Transform cameraSlot;
        public Transform cameraHand;
        public Transform rightHand;
        public Transform leftHand;
        public GameObject deadBody;
        [Header("References")]
        public DamageType impactDamageType;
        [Header("Events")]
        public UnityEvent JumpCallback;
        public UnityEvent WalljumpCallback;
        public UnityEvent GroundMoveCallback;
        public UnityEvent<CharacterBody> OnGlide;
        public UnityEvent<CharacterBody> OnWallJump;

        // NONSERIALIZED
        [NonSerialized] public CharacterSound characterSound;
        [NonSerialized] public CollisionBuffer cb;
        [NonSerialized] public CapsuleCollider cap;
        [NonSerialized] public CharacterInput input;
        [NonSerialized] public Character character;
        [NonSerialized] public CharacterVoice voice;
        [NonSerialized] private CharacterCamera characterCamera;
        private GravitySampler phys;
        private DamageEvent damageEvent;
        private CharacterEventManager events;
        [NonSerialized] public Vector3 moveDir = Vector3.zero;
        [NonSerialized] public Vector3 lastPlatformVelocity = Vector3.zero;
        [NonSerialized] public Vector3 falseGravity;
        [NonSerialized] private Vector3 equilibrium;
        [NonSerialized] public float lastStepTime = 0;
        [NonSerialized] public float cameraOffset = 0;
        [NonSerialized] public float lookAngle = 90;
        [NonSerialized] public float lookX = 0;
        [NonSerialized] public float lookY = 0;
        [NonSerialized] public float stepOffset;
        [NonSerialized] public int layerMask;
        [NonSerialized] public MoveState currentState;
        [NonSerialized] public Vector3 zoneVelocity;

        // PROPERTIES
        public float JumpHeight => input.Crouch ? defaultCrouchJumpHeight : defaultJumpHeight;
        public float MoveSpeed => input.Walk || input.Crouch || cap.height < defaultCrouchHeight
            ? defaultWalkSpeed : input.Sprint ? defaultSprintSpeed : defaultMoveSpeed;

        // IGravityUser Holdovers
        public Vector3 Gravity { get => phys.Gravity; set => phys.Gravity = value; }
        public Vector3 Velocity { get => phys.Velocity; set => phys.Velocity = value; } 

        private void Awake()
        {
            // Components
            cap = gameObject.GetComponent<CapsuleCollider>();
            character = GetComponent<Character>();
            characterCamera = GetComponentInChildren<CharacterCamera>();
            characterSound = GetComponent<CharacterSound>();
            input = GetComponent<CharacterInput>();
            voice = GetComponentInChildren<CharacterVoice>();
            TryGetComponent(out damageEvent);
            TryGetComponent(out events);
            TryGetComponent(out phys);

            // CharacterController
            stepOffset = defaultStepOffset;

            // CollisionBuffer
            cb = new CollisionBuffer(gameObject);
            cb.OnCollision += Impact;

            // Orient to Spawn;
            equilibrium = -transform.up;

            // layermask
            layerMask = LayerMask.GetMask(collisionLayers);

            // Events
            PauseManager.AddListener(OnPauseUnPause);
            character.OnRegistered += OnSetPlayer;
        }

        private void OnEnable()
        {
            if (character.isPlayer)
                events.onSpeedSet.Invoke("0");
        }

        private void OnDisable()
        {
            if (character.isPlayer)
                events.onSpeedSet.Invoke("");
        }

        private void OnDestroy()
        {
            PauseManager.RemoveListener(OnPauseUnPause);
            character.OnRegistered -= OnSetPlayer;
        }

        private void OnSetPlayer(bool isPlayer)
        {
            if (thirdPersonBody)
                thirdPersonBody.SetActive(!isPlayer);
            if (cameraAnchor && cameraAnchor.TryGetComponent(out MeshRenderer mr))
                mr.enabled = !isPlayer;
        }

        private void OnPauseUnPause(bool paused)
        {
            enabled = !paused;
        }

        private void Update()
        {
            lookX = input.MouseX;
            lookY = Mathf.Clamp(lookY - input.MouseY, -lookAngle, lookAngle);
            
            //lookX += cb.AngularVelocityX * Time.deltaTime;
            //lookY -= cb.AngularVelocityY * Time.deltaTime;

            transform.rotation = Quaternion.AngleAxis(lookX, -equilibrium) * Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, equilibrium), -equilibrium);
            Quaternion yAngle = Quaternion.AngleAxis(lookY, new Vector3(1, 0, 0));

            if (!float.IsNaN(yAngle.w)) 
                cameraAnchor.localRotation = yAngle;

            //if (character.isPlayer)
            //{
            //    //MasterCamera.ManualUpdateRot();
            //    //MasterCamera.ManualUpdatePos();
            //}
            if (character.isPlayer)
                CameraManager.ManualUpdatePos();
        }

        private void FixedUpdate()
        {
            cb.Clear();

            Move();
            GroundDetection();
            OverlapFix();

            /*  STATE CHANGE ..................................................
                The only thing needed to be counted as grounded is a valid
                floor normal in the collision buffer.                        */
            currentState = cb.FloorNormal.magnitude != 0 ? MoveState.Grounded : MoveState.Airborne;

            /*  MOVE DIRECTION ................................................
                Pressing the move keys takes you in different directions based
                on move state and collisions.                                */
            if (currentState == MoveState.Grounded)
                moveDir = Vector3.Cross(transform.right * input.Forward - transform.forward * input.Right, cb.FloorNormal).normalized;
            else if (cb.Normal.sqrMagnitude == 0)
                moveDir = Quaternion.FromToRotation(-transform.up, phys.Gravity) * (transform.forward * input.Forward + transform.right * input.Right).normalized;
            else
                moveDir = Quaternion.FromToRotation(-transform.up, falseGravity) * (transform.forward * input.Forward + transform.right * input.Right).normalized;

            if (currentState == MoveState.Grounded /*&& moveDir.sqrMagnitude > 0.5f*/)
                GroundMoveCallback?.Invoke();

            /*  FALSE GRAVITY & WALL WALKING ..................................
                FalseGravity is used for orientation and "falling" while
                phys.phys.Gravity is still used for all other physical interactions.
                in normal circumstances, falsGravity == phys.phys.Gravity.             */

            if (!input.Crawl)
                falseGravity = phys.Gravity;
            else if (input.ProcessStep && moveDir.sqrMagnitude != 0
                && Physics.SphereCast(transform.position - transform.up * (cap.height / 2 - cap.radius), cap.radius, Vector3.ProjectOnPlane(moveDir, falseGravity), out RaycastHit hit, stepOffset * 3, layerMask, QueryTriggerInteraction.Ignore)
                || Physics.SphereCast(transform.position, cap.radius, -transform.up, out hit, cap.height / 2 - cap.radius + stepOffset * 2, layerMask, QueryTriggerInteraction.Ignore))
            {
                falseGravity = -hit.normal * phys.Gravity.magnitude;

                cb.AddHit(new CBCollision(hit, phys.Velocity));
                currentState = MoveState.Grounded;
            }
            else if (cb.Normal.sqrMagnitude != 0)
                falseGravity = -cb.Normal * phys.Gravity.magnitude;

            /*  EQUILIBRIUM ...................................................
                This is how the character orients itself to the direction of
                falseGravity. Equilibrium gradulally interpolates toward
                falseGravity. transform.up will always be in the opposite
                direction of equilibrium.                                    */
            if(Vector3.Angle(equilibrium, falseGravity) is float currentAngle && currentAngle != 0)
            {
                float maxAngleDelta = 8f * Mathf.Clamp01(currentAngle / 60f) * Time.fixedDeltaTime;
                float faceFactor = (1f + Vector3.Dot(falseGravity.normalized, transform.up)) / 2f;
                Vector3 eqNew = Vector3.RotateTowards(equilibrium, falseGravity, maxAngleDelta, 0);
                eqNew = Vector3.Slerp(eqNew, Vector3.ProjectOnPlane(eqNew, transform.forward).normalized, faceFactor);
                Vector3 yOld = Vector3.ProjectOnPlane(equilibrium, transform.right);
                Vector3 yNew = Vector3.ProjectOnPlane(eqNew, transform.right);
                float yAngle = Vector3.SignedAngle(yOld, yNew, transform.right);
                lookY -= yAngle;

                equilibrium = eqNew.normalized;
            }

            /*  GROUND MOVEMENT ...............................................
                This is the main walking code. Lots of confusing things going
                on in order to make movement as smooth as possible.          */
            if (currentState == MoveState.Grounded)
            {

                /*  Launch phys.Velocity............................................
                    If a platform accelerates downward too quickly, the
                    character will be set to airborn and can be
                    launched off platforms.                                  */

                if (Vector3.Dot(cb.PlatformVelocity - lastPlatformVelocity, falseGravity.normalized) > falseGravity.magnitude * 0.25f)
                {
                    currentState = MoveState.Airborne;
                }
                else
                {
                    Vector3 relativeVel = phys.Velocity - cb.PlatformVelocity;
                    Vector3 moveVel = Vector3.ProjectOnPlane(relativeVel, moveDir);
                    Vector3 slopeDir = Vector3.ProjectOnPlane(moveDir.sqrMagnitude != 0 ? moveDir : phys.Velocity.sqrMagnitude != 0 ? phys.Velocity : transform.forward, transform.up);
                    float slopeFactor = (0.5f - Vector3.Angle(slopeDir, cb.FloorNormal) / 180f);
                    float wallFactor = cb.WallNormal.sqrMagnitude != 0 ? Mathf.Abs(Vector3.Dot(moveDir, cb.WallNormal)) : 1;
                    float slopeSpeedFactor = (1f + slopeFactor) * Mathf.Max(1f - wallFactor, wallFactor);
                    float topSpeed = MoveSpeed * slopeSpeedFactor;
                    float strideSpeed = defaultStrideSpeed * slopeSpeedFactor;
                    float forwardSpeed = Vector3.Dot(relativeVel, moveDir);
                    float dec = Mathf.Min(moveVel.magnitude, defaultGroundDecceleration * Time.fixedDeltaTime);
                    float acc;

                    if (forwardSpeed <= topSpeed)
                    {
                        float accRate = (1f + slopeFactor) * Time.fixedDeltaTime;

                        if (!input.Sprint || forwardSpeed < strideSpeed)
                            accRate *= defaultGroundAcceleration;
                        else
                            accRate *= defaultStrideAcceleration ;

                        acc = Mathf.Max(0, Mathf.Min(topSpeed - forwardSpeed, accRate));
                    }
                    else
                        acc = -Mathf.Min(forwardSpeed, defaultSpeedDecceleration * Time.fixedDeltaTime * (1f - slopeFactor));

                    if (forwardSpeed < 0)
                        acc *= 2;

                    phys.Velocity += moveDir * acc - moveVel.normalized * dec;

                    /*  JUMP...................................................
                        When a character jumps, the energy required to reach
                        jump velocity is transferred into the colliders.     */

                    if (input.Jump)
                    {
                        Vector3 jumpVel = -falseGravity.normalized * Mathf.Sqrt(2f * 9.81f * JumpHeight);
                        Vector3 verticalVel = Vector3.ProjectOnPlane(relativeVel, falseGravity);
                        Vector3 desiredVel = cb.LimitMomentum(verticalVel + jumpVel, verticalVel, defaultMaxKickVelocity);

                        phys.Velocity = desiredVel + cb.PlatformVelocity;

                        currentState = MoveState.Airborne;
                        input.jumpTimer.Restart();
                        cb.AddForce((relativeVel - desiredVel) * defaultMass * 2);

                        voice.PlayJump();
                    }
                }
            }

            /*  AIRBORNE MOVEMENT..............................................
                The character moves differently in the air than while grounded.
                Air acceleration is usually lower than ground acceleration, but
                trying to move backward from your current velocity will 
                increase that acceleration for easier platforming.           */

            else // if(currentState == State.Airborne)
            {
                Vector3 rVel = phys.Velocity - zoneVelocity;
                Vector3 gVel = Vector3.Project(rVel, phys.Gravity);
                Vector3 hVel = rVel - gVel;
                float acc = defaultAirAcceleration * Time.fixedDeltaTime * Mathf.Lerp(cb.IsEmpty ? 4f : 0.5f, cb.IsEmpty ? 1f : 0.1f, (Vector3.Dot(moveDir, hVel) + 1) / 2);
                float maxSpeed = MoveSpeed;
                float moveDirSpeed = Vector3.Dot(hVel, moveDir);

                // Clamp move speed.
                if (moveDirSpeed != 0 && moveDirSpeed + acc > maxSpeed)
                    acc = moveDirSpeed > maxSpeed ? 0 : moveDirSpeed + acc - maxSpeed;

                // Keep original excessive speeds, but never exceed it.
                hVel = Vector3.ClampMagnitude(hVel + moveDir * acc, Mathf.Max(hVel.magnitude, maxSpeed));
                gVel += phys.Gravity * Time.fixedDeltaTime;
                phys.Velocity = hVel + gVel + zoneVelocity;

                if (input.Jump)
                    OnWallJump?.Invoke(this);

                if (input.Glide)
                    OnGlide?.Invoke(this);
            }

            lastPlatformVelocity = cb.PlatformVelocity;
        }

        private void GroundDetection()
        {
            if (input.ProcessStep)
            {
                float offset = 0.05f;
                Vector3 point = transform.position - transform.up * (cap.height / 2f - cap.radius - offset);
                float distance = stepOffset * (currentState == MoveState.Grounded ? 3f : 1.5f) - offset;

                if (Physics.SphereCast(point, cap.radius * 0.9f, -transform.up, out RaycastHit hit, distance, layerMask, QueryTriggerInteraction.Ignore))
                {
                    CBCollision collision = new CBCollision(hit, phys.Velocity);
                    float trigOpposite = Vector3.ProjectOnPlane(transform.position - collision.point, transform.up).magnitude;
                    float trigAdjacent = new Vector2(cap.radius, trigOpposite).magnitude;
                    float floorDelta = hit.distance - stepOffset - offset + trigAdjacent - cap.radius;

                    point = transform.position - transform.up * (cap.height / 2f);

                    if (Physics.Raycast(point, -transform.up, out hit, distance, layerMask, QueryTriggerInteraction.Ignore)
                     || Physics.Raycast(point + transform.forward * cap.radius / 2f, -transform.up, out hit, distance, layerMask, QueryTriggerInteraction.Ignore))
                        collision = new CBCollision(hit, phys.Velocity);

                    cb.AddHit(collision);

                    transform.position -= transform.up * floorDelta;
                    characterCamera.stepOffset = Mathf.Clamp(characterCamera.stepOffset + floorDelta, -0.5f, 0.5f);
                }
            }
        }

        private void OverlapFix()
        {
            Vector3 oldPos = transform.position;
            Vector3 finalOffset = Vector3.zero;
            Vector3 capPoint = transform.up * (cap.height * 0.5f - cap.radius);
            Vector3 point0 = transform.position + capPoint;
            Vector3 point1 = transform.position - capPoint;
            int count = Physics.OverlapCapsuleNonAlloc(point0, point1, cap.radius, cBuffer, layerMask, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < count; i++)
            {
                Collider collider = cBuffer[i];

                if (!collider.transform.IsChildOf(transform)
                    && Physics.ComputePenetration(cap, transform.position, transform.rotation,
                        collider, collider.transform.position, collider.transform.rotation,
                        out Vector3 direction, out float distance))
                {
                    Vector3 normal = direction;
                    direction *= distance;
                    Vector3 fpd = finalOffset + direction;

                    //horizontal squeeze prevention
                    if (Vector3.Dot(transform.up, normal) < 0 && cb.FloorNormal.sqrMagnitude != 0)
                        normal = Vector3.ProjectOnPlane(normal, transform.up).normalized;

                    // Vertical squeeze prevention
                    if (Vector3.Dot(finalOffset, direction) < 0 && Vector3.Dot(phys.Velocity, fpd) < 0)
                        phys.Velocity = Vector3.Project(phys.Velocity, Vector3.Cross(finalOffset, direction));

                    finalOffset = fpd;

                    cb.AddHit(new CBCollision(collider, normal, transform.position, phys.Velocity));
                    transform.position += direction;
                }
            }

            Vector3 dir = transform.position - oldPos;
            float squeeze = dir.magnitude;
            int damage = (int)(squeeze * 200);
            GameObject instigatorBody = cBuffer[0] ? cBuffer[0].gameObject : null;
            CharacterInfo instigator;

            // Instigated by Character or self-instigated
            if (instigatorBody && instigatorBody.TryGetComponent(out Character ch) && ch.characterInfo)
                instigator = ch.characterInfo;
            else
                instigator = character.characterInfo;

            if (squeeze > cap.radius)
                damageEvent.Damage(damage, instigatorBody,  instigator, impactDamageType, dir);
        }

        private void Move()
        {
            float dt = Time.fixedDeltaTime;
            float iterations = 3;
            float backup = cap.radius * 0.5f;
            Vector3 pointOffset = transform.up * (cap.height / 2f - cap.radius);

            while (phys.Velocity.sqrMagnitude > 0 && dt > 0 && iterations-- > 0)
            {
                float distance = phys.Velocity.magnitude * dt;
                Vector3 velOff = -phys.Velocity.normalized * backup;
                Vector3 center = transform.TransformPoint(cap.center);

                if (Physics.CapsuleCast(
                            point1: center + velOff + pointOffset,
                            point2: center + velOff - pointOffset,
                            radius: cap.radius,
                            direction: phys.Velocity,
                            maxDistance: distance + backup,
                            hitInfo: out RaycastHit hit,
                            layerMask: layerMask,
                            queryTriggerInteraction: QueryTriggerInteraction.Ignore)
                    && !hit.transform.IsChildOf(transform)
                    && hit.distance > backup)
                {
                    hit.distance -= backup;
                    distance = Mathf.Min(hit.distance, distance);
                    cb.AddHit(new CBCollision(hit, phys.Velocity));
                }

                transform.Translate(Vector3.ClampMagnitude(phys.Velocity, distance), Space.World);

                dt -= distance / phys.Velocity.magnitude;
            }
        }

        public void Impact(CBCollision hit, float impactSpeed)
        {

            int damage = (int)(Mathf.Pow(impactSpeed - defaultMaxSafeImpactSpeed, 1.5f) * 2.5f);
            CharacterInfo instigator;

            if (hit.gameObject.TryGetComponent(out Character ch) && ch.characterInfo)
                instigator = ch.characterInfo;
            else
                instigator = character.characterInfo;

            if (damage > fallForgiveness)
                damageEvent.Damage(damage, hit.gameObject, instigator, impactDamageType, hit.normal);

            if (characterSound)
                characterSound.PlayImpact(impactSpeed - 3f);
        }
    }
}
