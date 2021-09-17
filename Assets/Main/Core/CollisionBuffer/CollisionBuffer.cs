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

        readonly Transform _transform;
        readonly IGravityUser _physics;
        readonly CharacterBody _body;
        readonly Dictionary<Rigidbody, CBCollision> _touchedRigidbodies = new Dictionary<Rigidbody, CBCollision>();
        readonly Dictionary<IGravityUser, CBCollision> _touchedPhysics = new Dictionary<IGravityUser, CBCollision>();
        readonly List<Vector3> _touchedNormals = new List<Vector3>();

        public CollisionBuffer(GameObject gameObject)
        {
            _transform = gameObject.transform;
            gameObject.TryGetComponent(out _body);
            gameObject.TryGetComponent(out _physics);
        }

        public void AddHit(CBCollision collision)
        {
            //if (collision.collider && buffer.Count < maxCapacity)
                //&& Vector3.Dot(collision.PointVelocity - body.Velocity, collision.normal) >= 0)
            {
                bool isGround = IsGround(collision);
                float mass = collision.gravityUser?.Mass ?? (collision.rigidbody ? collision.rigidbody.mass : float.MaxValue);
                float massRatio = collision.isNotInfluenceable ? 1 : Mathf.Clamp01(mass / (_physics.Mass + mass) * 10);

                IsEmpty = false;
                CollisionMass = Mathf.Max(CollisionMass, collision.rigidbody ? collision.rigidbody.mass : collision.gravityUser?.Mass ?? 0);
                Normal = (Normal + collision.normal).normalized;
                _touchedNormals.Add(collision.normal);
                Velocity = Vector3.ClampMagnitude(Velocity + collision.pointVelocity, Mathf.Max(Velocity.magnitude, collision.pointVelocity.magnitude));
                Point = _body.cap.ClosestPoint(
                    _transform.InverseTransformPoint(
                        _transform.TransformPoint(Point) +
                        _transform.TransformPoint(collision.point)));

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

                if (collision.rigidbody && !_touchedRigidbodies.ContainsKey(collision.rigidbody))
                    _touchedRigidbodies.Add(collision.rigidbody, collision);

                if (collision.gravityUser != null && !_touchedPhysics.ContainsKey(collision.gravityUser))
                    _touchedPhysics.Add(collision.gravityUser, collision);

                if (collision.gameObject)
                    if (collision.gameObject.TryGetComponent(out SurfaceFlagObject sfo))
                        foreach (SurfaceFlag flag in sfo.surfaceFlags)
                            flag.OnTouch(_transform.gameObject, collision);

                if (collision.collider.gameObject.TryGetComponent(out ITouchable it))
                    it.OnTouch(_transform.gameObject, null);

                if (_body)
                {
                    Vector3 oldVel = _body.Velocity;
                    _body.Velocity = ApplyCollision(_body.Velocity, _body.moveDir, collision);
                    OnCollision?.Invoke(collision, Vector3.Dot(_body.Velocity - oldVel, collision.normal));
                }
            }
        }

        public void Clear()
        {
            _touchedNormals.Clear();
            _touchedRigidbodies.Clear();
            _touchedPhysics.Clear();
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
            Point = _transform.position;
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
                    float massA = _physics.Mass;
                    float massB = gu?.Mass ?? rb.mass;
                    float speedA = Vector3.Dot(velocity, collision.normal);
                    float speedB = Vector3.Dot(collision.pointVelocity, collision.normal);
                    float momentumA = speedA * massA;
                    float momentumB = speedB * massB;
                    float speedStatic = (momentumA + momentumB) / (massA + massB);
                    float deltaSpeedA = speedStatic - speedA;
                    float deltaSpeedB = speedStatic - speedB;
                    Vector3 forceGravity = _body && IsGround(collision) 
                        ? _physics.Gravity * massA * _physics.Gravity.magnitude * 0.5f
                        : Vector3.zero;
                    Vector3 forcePush = _body && IsGround(collision)
                        ? -moveDir * Mathf.Max(0, _body.MoveSpeed - Vector3.Dot(velocity, moveDir) 
                        + Vector3.Dot(collision.pointVelocity, moveDir)) 
                        * _body.defaultGroundAcceleration / 2 
                        * _body.defaultMass
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
            if (!IsMoveable || (desiredVel - initialVel).magnitude * _physics.Mass / CollisionMass <= maxSpeed)
                return desiredVel;
            else
                return desiredVel.normalized * maxSpeed * CollisionMass / _physics.Mass + initialVel;
        }

        public void AddForce(Vector3 force)
        {
            foreach (CBCollision collision in _touchedRigidbodies.Values)
                if (!collision.isNotInfluenceable && !collision.rigidbody.isKinematic)
                        collision.rigidbody.AddForceAtPosition(force, collision.point, ForceMode.Impulse);

            foreach(CBCollision  collision in _touchedPhysics.Values)
                if(!collision.isNotInfluenceable)
                    collision.gravityUser.Velocity += force / collision.gravityUser.Mass;
        }

        public float AngularVelocityX
        {
            get
            {
                float deltaX = 0f;

                foreach (CBCollision collision in _touchedRigidbodies.Values)
                    if (_body && IsGround(collision))
                    {
                        Vector3 deltaVelocity = collision.rigidbody.GetPointVelocity(_transform.position) - collision.rigidbody.velocity;
                        Vector3 xVelocity = Vector3.ProjectOnPlane(deltaVelocity, _transform.up);
                        Vector3 offsetPoint = collision.point + xVelocity;
                        float partDeltaX = Vector3.Angle(collision.point - collision.rigidbody.worldCenterOfMass, offsetPoint - collision.rigidbody.worldCenterOfMass);
                        partDeltaX *= Mathf.Sign(Vector3.Dot(xVelocity, _body.cameraSlot.right));

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

                foreach (CBCollision collision in _touchedRigidbodies.Values)
                    if (_body && collision.rigidbody && IsGround(collision))
                    {
                        Vector3 deltaVelocity = collision.rigidbody.GetPointVelocity(_transform.position) - collision.rigidbody.velocity;
                        Vector3 yVelocity = Vector3.ProjectOnPlane(deltaVelocity, _body.cameraAnchor.right);
                        Vector3 offsetPoint = collision.point + yVelocity;
                        float partDeltaY = Vector3.Angle(collision.point - collision.rigidbody.worldCenterOfMass, offsetPoint - collision.rigidbody.worldCenterOfMass);
                        partDeltaY *= Mathf.Sign(Vector3.Dot(yVelocity, _transform.up));

                        deltaY = partDeltaY;
                    }

                return deltaY;
            }
        }

        /// <summary> Check if a normal in a Collision counts as ground.</summary>
        public bool IsGround(CBCollision collision)
        {
            if (_body && collision.collider)
                return collision.isStep || collision.isAlwaysGround || Vector3.Angle(-_body.falseGravity, collision.normal) <= _body.defaultslopeLimit;
            else
                return false;
        }

        public float MinDot(Vector3 n)
        {
            float min = 1f;

            foreach (Vector3 normal in _touchedNormals)
                min = Mathf.Min(min, Vector3.Dot(n, normal));

            return min;
        }
    }
}
