using MPWorld;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using MPCore;

public class PhysicalBody : MonoBehaviour, IGravityUser
{
    public bool gravity = true;
    public bool raycasts = true;
    public bool overlap = true;
    public bool kinematic = false;
    public float gravityScale = 1;
    public float mass = 10;

    public List<GravityZone> GravityZones { get; set; } = new List<GravityZone>();
    public Vector3 Gravity { get; set; }
    public Vector3 Velocity { get; set; }
    public float Mass => mass;

    private readonly HashSet<Collider> colliders = new HashSet<Collider>();
    private CollisionBuffer cb;

    private void Awake()
    {
        cb = new CollisionBuffer(gameObject);

        // Gather Colliders
        foreach (Collider collider in GetComponentsInChildren<Collider>())
            if (!(collider is MeshCollider))
                colliders.Add(collider);
    }

    private void FixedUpdate()
    {
        if (!kinematic)
        {
            if (gravity)
            {// GRAVITY
                //Gravity = Physics.gravity;

                //foreach (GravityZone gz in GravityZones)
                //    Gravity = gz.Calc(Gravity, transform);
                Gravity = GravityZone.GetVolumeGravity(GetComponent<Collider>(), GravityZones, out _);

                Velocity += Gravity * Time.fixedDeltaTime * gravityScale;
            }

            Velocity = Move(Velocity, Time.fixedDeltaTime, cb) * 0.99f;

            cb.Clear();
        }
    }

    public Vector3 Move(Vector3 velocity, float deltaTime, CollisionBuffer cb)
    {
        if(raycasts)
        {// RAYCASTING MOVEMENT
            float timeBudget = deltaTime;
            float iterations = 10;

            while (velocity.sqrMagnitude != 0 && timeBudget > 0 && iterations-- > 0)
            {
                RaycastHit finalHit = new RaycastHit
                {
                    distance = velocity.magnitude * timeBudget
                };

                foreach (Collider collider in colliders)
                {
                    Vector3 center = CenterOf(collider);

                    if(collider is SphereCollider sphere
                        && Physics.SphereCast(
                            origin: center,
                            radius: sphere.radius,
                            direction: velocity,
                            hitInfo: out RaycastHit hit,
                            maxDistance: finalHit.distance)
                        && !colliders.Contains(hit.collider))
                        finalHit = hit;
                    else if(collider is CapsuleCollider capsule
                        && Physics.CapsuleCast(
                            point1: center + transform.up * (capsule.height / 2 - capsule.radius),
                            point2: center - transform.up * (capsule.height / 2 - capsule.radius),
                            radius: capsule.radius,
                            direction: velocity,
                            maxDistance: finalHit.distance,
                            hitInfo: out hit)
                        && !colliders.Contains(hit.collider))
                        finalHit = hit;
                    else if(collider is BoxCollider box
                        && Physics.BoxCast(
                            center: center,
                            halfExtents: box.size / 2,
                            direction: velocity,
                            orientation: transform.rotation,
                            maxDistance: finalHit.distance,
                            hitInfo: out hit)
                        && !colliders.Contains(hit.collider))
                        finalHit = hit;
                }

                transform.position += Vector3.ClampMagnitude(velocity, finalHit.distance);

                if (finalHit.normal != Vector3.zero)
                {
                    cb.AddHit(new CollisionBuffer.Collision(finalHit));
                    //velocity = cb.ApplyAllCollisions(velocity, Vector3.zero);
                    cb.Clear();
                }

                timeBudget = Mathf.Max(0, timeBudget - finalHit.distance / velocity.magnitude);
            }
        }

        // OVERLAP FIX
        if (overlap)
            foreach (Collider collider in colliders)
                foreach (Collider overlap in GetOverlapsFor(collider))
                    if (!colliders.Contains(overlap)
                        && !overlap.isTrigger
                        && Physics.ComputePenetration(
                            colliderA: collider,
                            positionA: CenterOf(collider),
                            rotationA: transform.rotation,
                            colliderB: overlap,
                            positionB: CenterOf(overlap),
                            rotationB: overlap.transform.rotation,
                            direction: out Vector3 direction,
                            distance: out float distance))
                    {
                        transform.position += direction * distance;
                        cb.AddHit(new CollisionBuffer.Collision(overlap, direction, transform.position));
                    }

        return velocity;//cb.ApplyAllCollisions(velocity, Vector3.zero);
    }

    private static Vector3 CenterOf(Collider c)
    {
        if (c is SphereCollider sphere)
            return c.transform.TransformPoint(sphere.center);
        else if (c is CapsuleCollider capsule)
            return c.transform.TransformPoint(capsule.center);
        else if (c is BoxCollider box)
            return c.transform.TransformPoint(box.center);
        else
            return c.transform.position;
    }

    private static Collider[] GetOverlapsFor(Collider c)
    {
        if (c is SphereCollider sphere)
        {
            Vector3 center = c.transform.TransformPoint(sphere.center);
            return Physics.OverlapSphere(center, sphere.radius);
        }
        else if (c is CapsuleCollider capsule)
        {
            Vector3 center = c.transform.TransformPoint(capsule.center);
            Vector3 offset = c.transform.up * (capsule.height / 2 - capsule.radius);
            return Physics.OverlapCapsule(center + offset, center - offset, capsule.radius);
        }
        else if (c is BoxCollider box)
        {
            Vector3 center = c.transform.TransformPoint(box.center);
            return Physics.OverlapBox(center, box.size / 2, c.transform.rotation);
        }
        else
            return new Collider[0];
    }
}
