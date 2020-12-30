using MPGUI;
using MPWorld;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MPCore
{
    public class CharacterBody : MonoBehaviour, IGravityUser
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
        public Glider glider;
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
        public Transform rightHand;
        public Transform leftHand;
        public GameObject deadBody;
        [Header("References")]
        public DamageType impactDamageType;
        public StringEvent onSpeedSet;
        [Header("Effects")]
        public Particle shotEffect;

        // NONSERIALIZED
        [NonSerialized] public CharacterSound characterSound;
        [NonSerialized] public CollisionBuffer cb;
        [NonSerialized] public CapsuleCollider cap;
        [NonSerialized] private CharacterInput input;
        [NonSerialized] private Character character;
        [NonSerialized] private CharacterCamera characterCamera;
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
        private int layerMask;
        [NonSerialized] public MoveState currentState;
        [NonSerialized] private readonly StateMachine crouchState = new StateMachine();

        // PROPERTIES
        public Transform WeaponHand => leftHanded
            ? leftHand : rightHand;
        public float JumpHeight => input.Crouch
            ? defaultCrouchJumpHeight : defaultJumpHeight;
        public float MoveSpeed => input.Walk || input.Crouch || cap.height < defaultCrouchHeight
            ? defaultWalkSpeed : input.Sprint ? defaultSprintSpeed : defaultMoveSpeed;

        // IGravityUser
        public List<GravityZone> GravityZones { get; set; } = new List<GravityZone>();
        public Vector3 Gravity { get; set; } = Physics.gravity;
        public Vector3 Velocity { get; set; } = Vector3.zero;
        public float Mass => defaultMass;

        private void Awake()
        {
            // Components
            cap = gameObject.GetComponent<CapsuleCollider>();
            character = GetComponent<Character>();
            characterCamera = GetComponentInChildren<CharacterCamera>();
            characterSound = GetComponent<CharacterSound>();
            input = GetComponent<CharacterInput>();

            // CharacterController
            stepOffset = defaultStepOffset;

            // CollisionBuffer
            cb = new CollisionBuffer(gameObject);
            cb.OnCollision += Impact;

            // Gravity
            GravityZones.Clear();
            Gravity = Physics.gravity;
            falseGravity = Gravity;
            equilibrium = -transform.up;

            // layermask
            layerMask = LayerMask.GetMask(collisionLayers);

            // Make Crouch States
            InitializeCrouchRoutine();
            PauseManager.Add(OnPauseUnPause);

            character.OnPlayerSet += OnSetPlayer;
        }

        private void InitializeCrouchRoutine()
        {
            float appliedCrouchHeight;
            float appliedHeight;

            void SharedStart() 
            {
                appliedCrouchHeight = defaultCrouchHeight - defaultCrouchHeight / defaultHeight * defaultStepOffset;
                appliedHeight = defaultHeight - defaultStepOffset;
            }

            void SharedEnd()
            {
                stepOffset = cap.height / defaultHeight * defaultStepOffset;
            }

            crouchState.Add(new State(name: "Idle"),
                new State(name: "GoDown",
                fixedUpdate: () => 
                    {
                        SharedStart();

                        if (input.Crouch && appliedCrouchHeight < cap.height)
                            cap.height = Mathf.Max(appliedCrouchHeight, cap.height - defaultCrouchDownSpeed * Time.fixedDeltaTime);
                        else if (!input.Crouch) 
                            crouchState.SwitchTo("GoUp");

                        SharedEnd();
                    }),
                new State(name: "GoUp",
                fixedUpdate: () =>
                    {
                        SharedStart();

                        if (!input.Crouch && appliedHeight < defaultHeight)
                        {
                            float desiredHeight = Mathf.Min(appliedHeight, cap.height + defaultCrouchUpSpeed * Time.fixedDeltaTime);
                            float rayDistance = appliedHeight - cap.height;
                            Vector3 rayStart = transform.position - transform.up * (cap.height / 2 - cap.radius);
                            int oCount = Physics.OverlapSphereNonAlloc(rayStart, cap.radius * 0.9f, cBuffer, layerMask, QueryTriggerInteraction.Ignore);
                            bool hasOverlap = false;

                            for (int i = 0; i < oCount; i++)
                                if (!cBuffer[i].transform.IsChildOf(transform))
                                {
                                    hasOverlap = true;
                                    break;
                                }

                            if (!hasOverlap
                                && !Physics.SphereCast(rayStart, cap.radius * 0.9f, transform.up, out RaycastHit hit, rayDistance, layerMask))
                            {
                                transform.position += transform.up * (desiredHeight - cap.height) * 0.5f;
                                cap.height = desiredHeight;
                            }
                        }
                        else if (input.Crouch) 
                            crouchState.SwitchTo("GoDown");

                        SharedEnd();
                    }));

            crouchState.Initialize("GoDown");
        }

        private void OnEnable()
        {
            if (character.isPlayer)
                onSpeedSet.Invoke("0");
        }

        private void OnDisable()
        {
            if (character.isPlayer)
                onSpeedSet.Invoke("");

            character.OnPlayerSet -= OnSetPlayer;
        }

        private void OnDestroy()
        {
            PauseManager.Remove(OnPauseUnPause);
        }

        private void OnSetPlayer(bool isPlayer)
        {
            if (isPlayer)
                CameraManager.target = cameraSlot ? cameraSlot.gameObject : gameObject;
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

            // Set Speed text to total or horizontal velocity.
            if (character.isPlayer)
                SetSpeedText();
        }

        private void FixedUpdate()
        {
            cb.Clear();
            Gravity = GravityZone.GetVolumeGravity(cap, GravityZones, out Vector3 zoneVelocity);

            //if(GetComponentInChildren<SparkleCannon>() is var weap && weap && weap.grappleTransform)
            //{
            //    Vector3 direction = (weap.grappleTransform.TransformPoint(weap.grapplePoint) - cameraAnchor.position);

            //    if (direction.magnitude > 2f)
            //    {
            //        direction = direction.normalized;
            //        float verticalFactor = Vector3.Dot(direction, Gravity.normalized);
            //        Velocity += direction * Time.fixedDeltaTime * Gravity.magnitude * (1f - verticalFactor) * 1.5f;
            //        Velocity -= Velocity.normalized * Time.fixedDeltaTime * Mathf.Max(0, -verticalFactor) * 3f;
            //    }
            //}

            crouchState.FixedUpdate();
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
                moveDir = Quaternion.FromToRotation(-transform.up, Gravity) * (transform.forward * input.Forward + transform.right * input.Right).normalized;
            else
                moveDir = Quaternion.FromToRotation(-transform.up, falseGravity) * (transform.forward * input.Forward + transform.right * input.Right).normalized;

            /*  FALSE GRAVITY & WALL WALKING ..................................
                FalseGravity is used for orientation and "falling" while
                Gravity is still used for all other physical interactions.
                in normal circumstances, falsGravity == Gravity.             */

            if (!input.Crawl)
                falseGravity = Gravity;
            else if (input.ProcessStep && moveDir.sqrMagnitude != 0
                && Physics.SphereCast(transform.position - transform.up * (cap.height / 2 - cap.radius), cap.radius, Vector3.ProjectOnPlane(moveDir, falseGravity), out RaycastHit hit, stepOffset * 3, layerMask, QueryTriggerInteraction.Ignore)
                || Physics.SphereCast(transform.position, cap.radius, -transform.up, out hit, cap.height / 2 - cap.radius + stepOffset * 2, layerMask, QueryTriggerInteraction.Ignore))
            {
                falseGravity = -hit.normal * Gravity.magnitude;

                cb.AddHit(new CBCollision(hit, Velocity));
                currentState = MoveState.Grounded;
            }
            else if (cb.Normal.sqrMagnitude != 0)
                falseGravity = -cb.Normal * Gravity.magnitude;

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

                /*  Launch Velocity............................................
                    If a platform accelerates downward too quickly, the
                    character will be set to airborn and can be
                    launched off platforms.                                  */

                if (Vector3.Dot(cb.PlatformVelocity - lastPlatformVelocity, falseGravity.normalized) > falseGravity.magnitude)
                {
                    currentState = MoveState.Airborne;
                }
                else
                {
                    Vector3 relativeVel = Velocity - cb.PlatformVelocity;
                    Vector3 moveVel = Vector3.ProjectOnPlane(relativeVel, moveDir);
                    Vector3 slopeDir = Vector3.ProjectOnPlane(moveDir.sqrMagnitude != 0 ? moveDir : Velocity.sqrMagnitude != 0 ? Velocity : transform.forward, transform.up);
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

                    Velocity += moveDir * acc - moveVel.normalized * dec;
                    //Velocity += Gravity * Time.fixedDeltaTime;

                    /*  JUMP...................................................
                        When a character jumps, the energy required to reach
                        jump velocity is transferred into the colliders.     */

                    if (input.Jump)
                    {
                        Vector3 jumpVel = -falseGravity.normalized * Mathf.Sqrt(2f * 9.81f * JumpHeight);
                        Vector3 verticalVel = Vector3.ProjectOnPlane(relativeVel, falseGravity);
                        Vector3 desiredVel = cb.LimitMomentum(verticalVel + jumpVel, verticalVel, defaultMaxKickVelocity);

                        Velocity = desiredVel + cb.PlatformVelocity;

                        currentState = MoveState.Airborne;
                        input.jumpTimer.Restart();
                        cb.AddForce((relativeVel - desiredVel) * Mass * 2);
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
                Vector3 rVel = Velocity - zoneVelocity;
                Vector3 gVel = Vector3.Project(rVel, Gravity);
                Vector3 hVel = rVel - gVel;
                float acc = defaultAirAcceleration * Time.fixedDeltaTime * Mathf.Lerp(cb.IsEmpty ? 4f : 0.5f, cb.IsEmpty ? 1f : 0.1f, (Vector3.Dot(moveDir, hVel) + 1) / 2);
                float maxSpeed = MoveSpeed;
                float moveDirSpeed = Vector3.Dot(hVel, moveDir);

                // Clamp move speed.
                if (moveDirSpeed != 0 && moveDirSpeed + acc > maxSpeed)
                    acc = moveDirSpeed > maxSpeed ? 0 : moveDirSpeed + acc - maxSpeed;

                // Keep original excessive speeds, but never exceed it.
                hVel = Vector3.ClampMagnitude(hVel + moveDir * acc, Mathf.Max(hVel.magnitude, maxSpeed));
                gVel += Gravity * Time.fixedDeltaTime;
                Velocity = hVel + gVel + zoneVelocity;

                /* WALL JUMP...................................................
                   Wall jumps essentially bounce the character off collided
                   objects with a little extra vertical speed. A couple 
                   raycasts are taken to find nearby colliders, so the 
                   character does not have to be touching walls to perform a
                   successfull jump.                                         */

                //TODO: When I find out what's wrong, Move this block to wallJump.OnWallJump()

                if (wallJump && input.Jump)
                {
                    if (cb.IsEmpty)
                    {
                        Vector3 origin = transform.position - transform.up * (cap.height * 0.5f - cap.radius);

                        if ((Physics.SphereCast(origin, cap.radius * 0.9f, moveDir, out RaycastHit hit, cap.radius * 1.5f, layerMask, QueryTriggerInteraction.Ignore)
                            || Physics.SphereCast(origin, cap.radius * 0.9f, -moveDir, out hit, cap.radius * 1.5f, layerMask, QueryTriggerInteraction.Ignore))
                            && !hit.collider.transform.IsChildOf(transform))
                            cb.AddHit(new CBCollision(hit, Velocity));
                    }

                    if (!cb.IsEmpty)
                    {
                        float jumpSpeed = Mathf.Sqrt(2f * 9.81f * JumpHeight);
                        Vector3 iVel = Velocity - cb.Velocity;
                        Vector3 dVel = Vector3.ProjectOnPlane(Velocity, cb.Normal)
                            - Gravity.normalized * jumpSpeed * 0.3f
                            + Vector3.Reflect(iVel, cb.Normal).normalized * jumpSpeed * 0.3f
                            + cb.Normal * jumpSpeed * 1.2f;

                        Velocity = cb.LimitMomentum(dVel, iVel, defaultMaxKickVelocity) + cb.Velocity;
                        input.jumpTimer.Restart();
                        cb.AddForce((iVel - Velocity) * defaultMass * 2);
                    }
                }

                // Glider
                if (glider && input.Glide)
                    glider.OnGlide(this, zoneVelocity);
            }

            // Finish up.
            lastPlatformVelocity = cb.PlatformVelocity;
        }

        private void SetSpeedText()
        {
            int speed;

            if (glider)
                speed = (int)Mathf.Round(Velocity.magnitude * 10);
            else
                speed = (int)Mathf.Round(Vector3.ProjectOnPlane(Velocity, transform.up).magnitude * 10);

            onSpeedSet.Invoke(speed.ToString());
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
                    CBCollision collision = new CBCollision(hit, Velocity);
                    float trigOpposite = Vector3.ProjectOnPlane(transform.position - collision.point, transform.up).magnitude;
                    float trigAdjacent = new Vector2(cap.radius, trigOpposite).magnitude;
                    float floorDelta = hit.distance - stepOffset - offset + trigAdjacent - cap.radius;

                    point = transform.position - transform.up * (cap.height / 2f);

                    if (Physics.Raycast(point, -transform.up, out hit, distance, layerMask, QueryTriggerInteraction.Ignore)
                     || Physics.Raycast(point + transform.forward * cap.radius / 2f, -transform.up, out hit, distance, layerMask, QueryTriggerInteraction.Ignore))
                        collision = new CBCollision(hit, Velocity);

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
            int oCount = Physics.OverlapCapsuleNonAlloc(point0, point1, cap.radius, cBuffer, layerMask, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < oCount; i++)
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
                    if (Vector3.Dot(finalOffset, direction) < 0 && Vector3.Dot(Velocity, fpd) < 0)
                        Velocity = Vector3.Project(Velocity, Vector3.Cross(finalOffset, direction));

                    finalOffset = fpd;

                    cb.AddHit(new CBCollision(collider, normal, transform.position, Velocity));
                    transform.position += direction;
                }
            }

            float squeeze = Vector3.Distance(oldPos, transform.position);
            GameObject method = cBuffer[0] ? cBuffer[0].gameObject : null;

            if (squeeze > cap.radius)
                character.Damage((int)(squeeze * 200), gameObject, method, impactDamageType);
        }

        private void Move()
        {
            float dt = Time.fixedDeltaTime;
            float iterations = 3;

            while (Velocity.sqrMagnitude != 0 && dt > 0 && iterations-- > 0)
            {
                float distance = Velocity.magnitude * dt;
                float velDist = cap.radius * 0.5f;
                Vector3 heightOff = transform.up * (cap.height / 2f - cap.radius);
                Vector3 velOff = -Velocity.normalized * velDist;
                Vector3 center = transform.TransformPoint(cap.center);

                if (Physics.CapsuleCast(
                            point1: center + velOff + heightOff,
                            point2: center + velOff - heightOff,
                            radius: cap.radius,
                            direction: Velocity,
                            maxDistance: distance + velDist,
                            hitInfo: out RaycastHit hit,
                            layerMask: layerMask,
                            queryTriggerInteraction: QueryTriggerInteraction.Ignore)
                    && !hit.transform.IsChildOf(transform)
                    && hit.distance > velDist)
                {
                    hit.distance -= velDist;
                    CBCollision collision = new CBCollision(hit, Velocity);
                    cb.AddHit(collision);
                    distance = Mathf.Min(hit.distance, distance);
                }

                transform.position += Vector3.ClampMagnitude(Velocity, distance);

                dt -= distance / Velocity.magnitude;
            }
        }

        public void Impact(CBCollision hit, float impactSpeed)
        {

            int damage = (int)(Mathf.Pow(impactSpeed - defaultMaxSafeImpactSpeed, 1.5f) * 2.5f);

            if (damage > 5)
                character.Damage(damage, gameObject, hit.gameObject, impactDamageType);

            if (characterSound)
                characterSound.PlayImpact(impactSpeed - 3f);
        }
    }
}
