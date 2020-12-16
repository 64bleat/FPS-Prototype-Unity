using MPWorld;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MPCore
{
    /// <summary> Processes collisions for CharacterBody </summary>
    public class CollisionBuffer
    {
        public bool IsEmpty { get; private set; } = true;
        public bool IsMoveable { get; private set; } = false;
        public float CollisionMass { get; set; } = 0;
        public Vector3 Normal { get; private set; } = Vector3.zero;
        public Vector3 FloorNormal { get; private set; } = Vector3.zero;
        public Vector3 WallNormal { get; private set; } = Vector3.zero;
        public Vector3 Velocity { get; private set; } = Vector3.zero;
        public Vector3 PlatformVelocity { get; private set; } = Vector3.zero;
        public Vector3 WallVelocity { get; private set; } = Vector3.zero;
        public Vector3 Point { get; set; }// = null;
        public Vector3 CenterOfMass { get; private set; } = Vector3.zero;
        public Vector3 AngularVelocity { get; private set; } = Vector3.zero;
        public HashSet<SurfaceFlag> SurfaceFlags { get; private set; } = new HashSet<SurfaceFlag>();

        public event Action<Collision, float> CollisionEvent;
        private const int maxCapacity = 20;
        private readonly Transform transform;
        private readonly IGravityUser gravityUser;
        private readonly CharacterBody character;
        private readonly List<Collision> buffer = new List<Collision>(maxCapacity);
        private readonly HashSet<Collider> colliders = new HashSet<Collider>();
        private readonly Dictionary<Rigidbody, Collision> rigidbodies = new Dictionary<Rigidbody, Collision>();
        private readonly Dictionary<IGravityUser, Collision> gravityUsers = new Dictionary<IGravityUser, Collision>();

        //private readonly HashSet<Collision> unappliedCollisions = new HashSet<Collision>();
        //private readonly float cosSlope = 0.5f;

        // CONSTRUCTOR
        public CollisionBuffer(GameObject gameObject)
        {
            transform = gameObject.transform;
            character = gameObject.GetComponent<CharacterBody>();
            gravityUser = gameObject.GetComponent<IGravityUser>();
        }

        // ADDING
        public void AddHit(Collision collision)
        {
            if (collision.collider && buffer.Count < maxCapacity)
                //&& Vector3.Dot(collision.PointVelocity - body.Velocity, collision.normal) >= 0)
            {
                bool isGround = IsGround(collision);
                float mass = collision.gravityUser?.Mass ?? (collision.rigidbody ? collision.rigidbody.mass : float.MaxValue);
                float massRatio = collision.isNotInfluenceable ? 1 : Mathf.Clamp01(mass / (gravityUser.Mass + mass) * 10);

                //Buffer Data
                buffer.Add(collision);
                if(collision.collider)
                    colliders.Add(collision.collider);
                if (collision.rigidbody && !rigidbodies.ContainsKey(collision.rigidbody))
                    rigidbodies.Add(collision.rigidbody, collision);
                if (collision.gravityUser != null && !gravityUsers.ContainsKey(collision.gravityUser))
                    gravityUsers.Add(collision.gravityUser, collision);
                if (collision.gameObject)
                    if(collision.gameObject.TryGetComponent(out SurfaceFlagObject sfo))
                        foreach (SurfaceFlag flag in sfo.surfaceFlags)
                            SurfaceFlags.Add(flag);

                // bools
                IsEmpty = false;

                if ((collision.rigidbody && !collision.rigidbody.isKinematic && !collision.isNotInfluenceable) || collision.gravityUser != null)
                    IsMoveable = true;

                // floats
                CollisionMass = Mathf.Max(CollisionMass, collision.rigidbody ? collision.rigidbody.mass : collision.gravityUser?.Mass ?? 0);

                // normals
                Normal = (Normal + collision.normal).normalized;

                if (isGround)
                    FloorNormal = (FloorNormal + collision.normal).normalized;
                else
                    WallNormal = (WallNormal + collision.normal).normalized;

                // velocities
                Velocity = Vector3.ClampMagnitude(Velocity + collision.pointVelocity, Mathf.Max(Velocity.magnitude, collision.pointVelocity.magnitude));

                if (isGround)
                    PlatformVelocity = Vector3.ClampMagnitude(PlatformVelocity + collision.pointVelocity, Mathf.Max(PlatformVelocity.magnitude, collision.pointVelocity.magnitude));
                else
                    WallVelocity = Vector3.ClampMagnitude(WallVelocity + collision.pointVelocity, Mathf.Max(WallVelocity.magnitude, collision.pointVelocity.magnitude));

                //center of mass
                if (collision.rigidbody)
                    CenterOfMass = collision.rigidbody.worldCenterOfMass;

                if (collision.rigidbody)
                    AngularVelocity += collision.rigidbody.angularVelocity;

                // point
                Point = character.cap.ClosestPoint(
                    transform.InverseTransformPoint(
                        transform.TransformPoint(Point) +
                        transform.TransformPoint(collision.point)));

                if (character)
                {
                    Vector3 oldVel = character.Velocity;
                    character.Velocity = ApplyCollision(character.Velocity, character.moveDir, collision);
                    CollisionEvent?.Invoke(collision, Vector3.Dot(character.Velocity - oldVel, collision.normal));
                }
            }
        }

        // CLEARING
        public void Clear()
        {
            if(character)
                ActivateTriggers(character.gameObject);

            // Buffer Data
            buffer.Clear();
            colliders.Clear();
            rigidbodies.Clear();
            gravityUsers.Clear();
            SurfaceFlags.Clear();

            IsEmpty = true;
            IsMoveable = false;

            CollisionMass = 0;

            Normal = Vector3.zero;
            FloorNormal = Vector3.zero;
            WallNormal = Vector3.zero;

            Velocity = Vector3.zero;
            PlatformVelocity = Vector3.zero;
            WallVelocity = Vector3.zero;

            CenterOfMass = Vector3.zero;
            AngularVelocity = Vector3.zero;

            Point = transform.position;
        }

        /// <summary> Take an input velocity and determine how it interacts with colliding objects. </summary>
        /// <param name="velocity"> input velocity </param>
        /// <param name="moveDir"> Optional push force direction </param>
        /// <returns> new velocity taking account for physical objects interacted with </returns>
        public Vector3 ApplyCollision(Vector3 velocity, Vector3 moveDir, Collision collision)
        {
            if (Vector3.Dot(collision.pointVelocity - velocity, collision.normal) >= -0.1f)
            {
                Rigidbody rb = collision.rigidbody;
                IGravityUser gu = collision.gravityUser;

                if (collision.isNotInfluenceable || (!rb || rb.isKinematic || collision.isNotInfluenceable) && gu == null)
                    velocity = Vector3.ProjectOnPlane(velocity, collision.normal) + Vector3.Project(collision.pointVelocity, collision.normal);
                else
                {
                    float massA = gravityUser.Mass;
                    float massB = gu?.Mass ?? rb.mass;
                    float speedA = Vector3.Dot(velocity, collision.normal);
                    float speedB = Vector3.Dot(collision.pointVelocity, collision.normal);
                    float momentumA = speedA * massA;
                    float momentumB = speedB * massB;
                    float speedStatic = (momentumA + momentumB) / (massA + massB);
                    float deltaSpeedA = speedStatic - speedA;
                    float deltaSpeedB = speedStatic - speedB;
                    Vector3 forceGravity = character && IsGround(collision) 
                        ? gravityUser.Gravity * massA * gravityUser.Gravity.magnitude * 0.5f
                        : Vector3.zero;
                    Vector3 forcePush = character && IsGround(collision)
                        ? -moveDir * Mathf.Max(0, character.MoveSpeed - Vector3.Dot(velocity, moveDir) 
                        + Vector3.Dot(collision.pointVelocity, moveDir)) 
                        * character.defaultGroundAcceleration / 2 
                        * character.defaultMass
                        : Vector3.zero;
                    Vector3 deltaVelocityB = collision.normal * deltaSpeedB + (forceGravity + forcePush) / massB * Time.fixedDeltaTime;
                    Vector3 deltaVelocityA = collision.normal * deltaSpeedA;

                    if (gu != null)
                        gu.Velocity += deltaVelocityB;
                    else if (rb)
                        rb.AddForceAtPosition(deltaVelocityB, collision.point, ForceMode.VelocityChange);

                    velocity += deltaVelocityA;
                }
            }

            return velocity;
        }

        // MASS ===============================================================================================================================================
        public Vector3 LimitMomentum(Vector3 desiredVel, Vector3 initialVel, float maxSpeed)
        {
            if (!IsMoveable || (desiredVel - initialVel).magnitude * gravityUser.Mass / CollisionMass <= maxSpeed)
                return desiredVel;
            else
                return desiredVel.normalized * maxSpeed * CollisionMass / gravityUser.Mass + initialVel;
        }

        public void AddForce(Vector3 force)
        {
            foreach (Collision collision in rigidbodies.Values)
                if (!collision.isNotInfluenceable && !collision.rigidbody.isKinematic)
                        collision.rigidbody.AddForceAtPosition(force, collision.point, ForceMode.Impulse);

            foreach(Collision  collision in gravityUsers.Values)
                if(!collision.isNotInfluenceable)
                    collision.gravityUser.Velocity += force / collision.gravityUser.Mass;
        }

        public float AngularVelocityX
        {
            get
            {
                float deltaX = 0f;

                foreach (Collision collision in rigidbodies.Values)
                    if (character && IsGround(collision))
                    {
                        Vector3 deltaVelocity = collision.rigidbody.GetPointVelocity(transform.position) - collision.rigidbody.velocity;
                        Vector3 xVelocity = Vector3.ProjectOnPlane(deltaVelocity, transform.up);
                        Vector3 offsetPoint = collision.point + xVelocity;
                        float partDeltaX = Vector3.Angle(collision.point - collision.rigidbody.worldCenterOfMass, offsetPoint - collision.rigidbody.worldCenterOfMass);
                        partDeltaX *= Mathf.Sign(Vector3.Dot(xVelocity, character.cameraSlot.right));

                        deltaX = partDeltaX;
                    }

                return deltaX;
            }
        }

        public float AngularVelocityY
        {
            get
            {
                float deltaY = 0f;

                foreach (Collision collision in rigidbodies.Values)
                    if (character && collision.rigidbody && IsGround(collision))
                    {
                        Vector3 deltaVelocity = collision.rigidbody.GetPointVelocity(transform.position) - collision.rigidbody.velocity;
                        Vector3 yVelocity = Vector3.ProjectOnPlane(deltaVelocity, character.cameraAnchor.right);
                        Vector3 offsetPoint = collision.point + yVelocity;
                        float partDeltaY = Vector3.Angle(collision.point - collision.rigidbody.worldCenterOfMass, offsetPoint - collision.rigidbody.worldCenterOfMass);
                        partDeltaY *= Mathf.Sign(Vector3.Dot(yVelocity, transform.up));

                        deltaY = partDeltaY;
                    }

                return deltaY;
            }
        }

        // GROUNDING ==========================================================================================================================================
        /// <summary> Check if a normal in a Collision counts as ground.</summary>
        public bool IsGround(Collision collision)
        {
            if (character && collision.collider)
                return collision.isStep || collision.isAlwaysGround || Vector3.Angle(-character.falseGravity, collision.normal) <= character.defaultslopeLimit;
            else
                return false;
        }

        /// <summary> Activate any TriggerTouch. </summary>
        public void ActivateTriggers(GameObject touchedBy)
        {
            if (touchedBy)
            {
                foreach (Collider c in colliders)
                    if (c && c.gameObject && c.gameObject.TryGetComponent(out ITouchable it))
                        it.OnTouch(touchedBy, null);

                foreach (SurfaceFlag flag in SurfaceFlags)
                    flag.OnTouch(touchedBy, null);
            }
        }

        /// <summary> Collision data to be used by CollisionBuffer </summary>
        public struct Collision
        {
            public GameObject gameObject;
            public Rigidbody rigidbody;
            public Collider collider;
            public IGravityUser gravityUser;
            public Vector3 point;
            public Vector3 normal;
            public Vector3 pointVelocity;
            public bool isAlwaysGround;
            public bool isNotInfluenceable;
            public bool isStep;

            private void SetValues()
            {
                gameObject.TryGetComponent(out SurfaceFlagObject sfoGet);
                SurfaceFlagObject sfo = sfoGet;

                if (rigidbody)
                {
                    pointVelocity = rigidbody.GetPointVelocity(point);

                    if (rigidbody.gameObject.TryGetComponent(out sfoGet))
                        sfo = sfoGet;
                }
                else if (gravityUser != null)
                {
                    pointVelocity = gravityUser.Velocity;
                }

                if(sfo)
                {
                    isAlwaysGround = sfo._SurfaceFlags.Contains(global::MPWorld.SurfaceFlags.Stairs);
                    isNotInfluenceable = sfo._SurfaceFlags.Contains(global::MPWorld.SurfaceFlags.NoInfluence);
                }
            }

            private static IGravityUser GetGravityUser(Transform t)
            {
                while (t)
                {
                    if (t.TryGetComponent(typeof(IGravityUser), out Component c))
                        return c as IGravityUser;
                    else
                        t = t.parent;
                }

                return default;
            }

            public Collision(ControllerColliderHit hit, bool isStep = false)
            {
                gameObject = hit.gameObject;
                rigidbody = hit.collider.attachedRigidbody;
                collider = hit.collider;
                gravityUser = GetGravityUser(hit.collider.transform);//hit.collider.GetComponentInParent<IGravityUser>();
                point = hit.point;
                normal = hit.normal;
                isAlwaysGround = false;
                isNotInfluenceable = false;
                this.isStep = isStep;
                pointVelocity = Vector3.zero;

                SetValues();
            }

            public Collision(RaycastHit hit, bool isStep = false)
            {
                gameObject = hit.collider.gameObject;
                rigidbody = hit.collider.attachedRigidbody;
                collider = hit.collider;
                gravityUser = GetGravityUser(hit.collider.transform);//hit.collider.GetComponentInParent<IGravityUser>();
                point = hit.point;
                normal = hit.normal;
                isAlwaysGround = false;
                isNotInfluenceable = false;
                this.isStep = isStep;
                pointVelocity = Vector3.zero;

                SetValues();
            }

            public Collision(Collider collider, Vector3 normal, Vector3 point, bool isStep = false)
            {
                gameObject = collider.gameObject;
                rigidbody = collider.attachedRigidbody;
                this.collider = collider;
                gravityUser = GetGravityUser(collider.transform);//hit.collider.GetComponentInParent<IGravityUser>();
                this.point = point;
                this.normal = normal.normalized;
                isAlwaysGround = false;
                isNotInfluenceable = false;
                this.isStep = isStep;
                pointVelocity = Vector3.zero;

                SetValues();
            }
        }
    }
}
