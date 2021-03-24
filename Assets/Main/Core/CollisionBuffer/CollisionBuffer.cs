using MPWorld;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MPCore
{
    /// <summary> Processes collisions for CharacterBody </summary>
    public class CollisionBuffer
    {
        public delegate void CollisionDelegate(CBCollision hit, float deltaSpeed);
        public event CollisionDelegate OnCollision;

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

        private readonly Transform transform;
        private readonly IGravityUser gravityUser;
        private readonly CharacterBody body;
        private readonly Dictionary<Rigidbody, CBCollision> rigidbodies = new Dictionary<Rigidbody, CBCollision>();
        private readonly Dictionary<IGravityUser, CBCollision> gravityUsers = new Dictionary<IGravityUser, CBCollision>();
        private readonly List<Vector3> normals = new List<Vector3>();

        public CollisionBuffer(GameObject gameObject)
        {
            transform = gameObject.transform;
            gameObject.TryGetComponent(out body);
            gameObject.TryGetComponent(out gravityUser);
        }

        public void AddHit(CBCollision collision)
        {
            //if (collision.collider && buffer.Count < maxCapacity)
                //&& Vector3.Dot(collision.PointVelocity - body.Velocity, collision.normal) >= 0)
            {
                bool isGround = IsGround(collision);
                float mass = collision.gravityUser?.Mass ?? (collision.rigidbody ? collision.rigidbody.mass : float.MaxValue);
                float massRatio = collision.isNotInfluenceable ? 1 : Mathf.Clamp01(mass / (gravityUser.Mass + mass) * 10);

                IsEmpty = false;
                CollisionMass = Mathf.Max(CollisionMass, collision.rigidbody ? collision.rigidbody.mass : collision.gravityUser?.Mass ?? 0);
                Normal = (Normal + collision.normal).normalized;
                normals.Add(collision.normal);
                Velocity = Vector3.ClampMagnitude(Velocity + collision.pointVelocity, Mathf.Max(Velocity.magnitude, collision.pointVelocity.magnitude));
                Point = body.cap.ClosestPoint(
                    transform.InverseTransformPoint(
                        transform.TransformPoint(Point) +
                        transform.TransformPoint(collision.point)));

                if ((collision.rigidbody && !collision.rigidbody.isKinematic && !collision.isNotInfluenceable) || collision.gravityUser != null)
                    IsMoveable = true;

                if (isGround)
                    FloorNormal = (FloorNormal + collision.normal).normalized;
                else
                    WallNormal = (WallNormal + collision.normal).normalized;

                if (isGround)
                    PlatformVelocity = Vector3.ClampMagnitude(PlatformVelocity + collision.pointVelocity, Mathf.Max(PlatformVelocity.magnitude, collision.pointVelocity.magnitude));
                else
                    WallVelocity = Vector3.ClampMagnitude(WallVelocity + collision.pointVelocity, Mathf.Max(WallVelocity.magnitude, collision.pointVelocity.magnitude));

                if (collision.rigidbody)
                    CenterOfMass = collision.rigidbody.worldCenterOfMass;

                if (collision.rigidbody)
                    AngularVelocity += collision.rigidbody.angularVelocity;

                if (collision.rigidbody && !rigidbodies.ContainsKey(collision.rigidbody))
                    rigidbodies.Add(collision.rigidbody, collision);

                if (collision.gravityUser != null && !gravityUsers.ContainsKey(collision.gravityUser))
                    gravityUsers.Add(collision.gravityUser, collision);

                if (collision.gameObject)
                    if (collision.gameObject.TryGetComponent(out SurfaceFlagObject sfo))
                        foreach (SurfaceFlag flag in sfo.surfaceFlags)
                            flag.OnTouch(transform.gameObject, collision);

                if (collision.collider.gameObject.TryGetComponent(out ITouchable it))
                    it.OnTouch(transform.gameObject, null);

                if (body)
                {
                    Vector3 oldVel = body.Velocity;
                    body.Velocity = ApplyCollision(body.Velocity, body.moveDir, collision);
                    OnCollision?.Invoke(collision, Vector3.Dot(body.Velocity - oldVel, collision.normal));
                }
            }
        }

        public void Clear()
        {
            normals.Clear();
            rigidbodies.Clear();
            gravityUsers.Clear();
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
        public Vector3 ApplyCollision(Vector3 velocity, Vector3 moveDir, CBCollision collision)
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
                    Vector3 forceGravity = body && IsGround(collision) 
                        ? gravityUser.Gravity * massA * gravityUser.Gravity.magnitude * 0.5f
                        : Vector3.zero;
                    Vector3 forcePush = body && IsGround(collision)
                        ? -moveDir * Mathf.Max(0, body.MoveSpeed - Vector3.Dot(velocity, moveDir) 
                        + Vector3.Dot(collision.pointVelocity, moveDir)) 
                        * body.defaultGroundAcceleration / 2 
                        * body.defaultMass
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

        public Vector3 LimitMomentum(Vector3 desiredVel, Vector3 initialVel, float maxSpeed)
        {
            if (!IsMoveable || (desiredVel - initialVel).magnitude * gravityUser.Mass / CollisionMass <= maxSpeed)
                return desiredVel;
            else
                return desiredVel.normalized * maxSpeed * CollisionMass / gravityUser.Mass + initialVel;
        }

        public void AddForce(Vector3 force)
        {
            foreach (CBCollision collision in rigidbodies.Values)
                if (!collision.isNotInfluenceable && !collision.rigidbody.isKinematic)
                        collision.rigidbody.AddForceAtPosition(force, collision.point, ForceMode.Impulse);

            foreach(CBCollision  collision in gravityUsers.Values)
                if(!collision.isNotInfluenceable)
                    collision.gravityUser.Velocity += force / collision.gravityUser.Mass;
        }

        public float AngularVelocityX
        {
            get
            {
                float deltaX = 0f;

                foreach (CBCollision collision in rigidbodies.Values)
                    if (body && IsGround(collision))
                    {
                        Vector3 deltaVelocity = collision.rigidbody.GetPointVelocity(transform.position) - collision.rigidbody.velocity;
                        Vector3 xVelocity = Vector3.ProjectOnPlane(deltaVelocity, transform.up);
                        Vector3 offsetPoint = collision.point + xVelocity;
                        float partDeltaX = Vector3.Angle(collision.point - collision.rigidbody.worldCenterOfMass, offsetPoint - collision.rigidbody.worldCenterOfMass);
                        partDeltaX *= Mathf.Sign(Vector3.Dot(xVelocity, body.cameraSlot.right));

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

                foreach (CBCollision collision in rigidbodies.Values)
                    if (body && collision.rigidbody && IsGround(collision))
                    {
                        Vector3 deltaVelocity = collision.rigidbody.GetPointVelocity(transform.position) - collision.rigidbody.velocity;
                        Vector3 yVelocity = Vector3.ProjectOnPlane(deltaVelocity, body.cameraAnchor.right);
                        Vector3 offsetPoint = collision.point + yVelocity;
                        float partDeltaY = Vector3.Angle(collision.point - collision.rigidbody.worldCenterOfMass, offsetPoint - collision.rigidbody.worldCenterOfMass);
                        partDeltaY *= Mathf.Sign(Vector3.Dot(yVelocity, transform.up));

                        deltaY = partDeltaY;
                    }

                return deltaY;
            }
        }

        /// <summary> Check if a normal in a Collision counts as ground.</summary>
        public bool IsGround(CBCollision collision)
        {
            if (body && collision.collider)
                return collision.isStep || collision.isAlwaysGround || Vector3.Angle(-body.falseGravity, collision.normal) <= body.defaultslopeLimit;
            else
                return false;
        }

        public float MinDot(Vector3 n)
        {
            float min = 1f;

            foreach (Vector3 normal in normals)
                min = Mathf.Min(min, Vector3.Dot(n, normal));

            return min;
        }
    }
}
