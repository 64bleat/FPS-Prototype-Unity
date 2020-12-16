using MPCore;
using MPWorld;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class collisionManager
{
    public Vector3 Normal { get; private set; } = Vector3.zero;
    public Vector3 FloorNormal { get; private set; } = Vector3.zero;
    public Vector3 WallNormal { get; private set; } = Vector3.zero;
    public Vector3 Velocity { get; private set; } = Vector3.zero;
    public Vector3 PlatformVelocity { get; private set; } = Vector3.zero;
    public Vector3 WallVelocity { get; private set; } = Vector3.zero;

    // OWNER
    private readonly float cosGround;
    private readonly GameObject owner;
    private readonly IGravityUser body;
    private readonly CharacterBody character;

    // DATA
    private readonly HashSet<Collision> buffer = new HashSet<Collision>();

    // CONSTRUCTOR
    public collisionManager(GameObject owner, float groundAngle = 46)
    {
        this.owner = owner;
        body = owner.GetComponent<IGravityUser>();
        character = owner.GetComponent<CharacterBody>();
        cosGround = Mathf.Cos(groundAngle);
    }

    // RESET
    public void Clear()
    {
        buffer.Clear();
        Normal = Vector3.zero;
        WallNormal = Vector3.zero;
        FloorNormal = Vector3.zero;
        Velocity = Vector3.zero;
        WallVelocity = Vector3.zero;
        PlatformVelocity = Vector3.zero;
    }

    // ADDING
    public void AddHit(params RaycastHit[] hits)
    {
        foreach (RaycastHit hit in hits)
        {
            Collision collision = new Collision(hit);

            if (ProcessHit(collision))
            {
                bool isGround = IsGround(collision);
                float massRatio = collision.noninfluencable
                    ? 1
                    : collision.gravityUser != null
                    ? Mathf.Clamp01(collision.gravityUser.Mass / (collision.gravityUser.Mass + body.Mass))
                    : collision.rigidbody && !collision.rigidbody.isKinematic
                    ? Mathf.Clamp01(collision.rigidbody.mass / (collision.rigidbody.mass + body.Mass))
                    : 1;

                // normals
                Normal = Vector3.Lerp(Normal, hit.normal, 0.5f);

                if (isGround)
                    FloorNormal = Vector3.Lerp(FloorNormal, hit.normal, 0.5f);
                else
                    WallNormal = Vector3.Lerp(WallNormal, hit.normal, 0.5f);

                // velocities
                Velocity = Combine(Velocity, collision.pointVelocity * massRatio);

                if (isGround)
                    PlatformVelocity = Combine(PlatformVelocity, collision.pointVelocity * massRatio);
                else
                    WallVelocity = Combine(WallVelocity, collision.pointVelocity * massRatio);

                // store
                buffer.Add(collision);
            }
        }
    }

    // COLLISION PHYSICS
    private bool ProcessHit(Collision c)
    {
        if (Vector3.Dot(c.pointVelocity - body.Velocity, c.normal) >= 0)
        {
            if((c.noninfluencable || !c.rigidbody || c.rigidbody.isKinematic) && c.gravityUser == null)
            {
                body.Velocity = Vector3.ProjectOnPlane(body.Velocity, c.normal)
                    + Vector3.Project(c.pointVelocity, c.normal);
            }
            else if(c.gravityUser != null)
            {
                float sThis = Vector3.Dot(body.Velocity, c.normal);
                float sThat = Vector3.Dot(c.pointVelocity, c.normal);
                float mThis = sThis * body.Mass;
                float mThat = sThat * c.gravityUser.Mass;
                float sFinal = (mThis + mThat) / (body.Mass + c.gravityUser.Mass);

                c.gravityUser.Velocity += c.normal * (sFinal - sThat);
                body.Velocity += c.normal * (sFinal - sThis);
            }
            else if(c.rigidbody != null)
            {
                float sThis = Vector3.Dot(body.Velocity, c.normal);
                float sThat = Vector3.Dot(c.pointVelocity, c.normal);
                float mThis = sThis * body.Mass;
                float mThat = sThat * c.rigidbody.mass;
                float sFinal = (mThis + mThat) / (body.Mass + c.rigidbody.mass);

                c.rigidbody.AddForceAtPosition(c.normal * (sFinal - sThat), c.point, ForceMode.VelocityChange);
                body.Velocity += c.normal * (sFinal - sThis);
            }

            return true;
        }
        else 
            return false;
    }

    // INTERACTION
    public void ApplyForce(Vector3 force, float maxDeltaVelocity)
    {
        
    }

    // HELPERS
    public static Vector3 Combine(Vector3 a, Vector3 b)
    {
        if (b.sqrMagnitude > a.sqrMagnitude)
            return b + Vector3.ProjectOnPlane(a, b);
        else
            return a + Vector3.ProjectOnPlane(b, a);
    }

    private bool IsGround(Collision collision)
    {
        return collision.isStep
            || collision.forceGround
            || Vector3.Dot(owner.transform.up, collision.normal) <= cosGround;
    }

    // COLLISION
    private class Collision
    {
        public readonly GameObject gameObject;
        public readonly Rigidbody rigidbody;
        public readonly Collider collider;
        public readonly IGravityUser gravityUser;
        public readonly Vector3 point, normal, pointVelocity;
        public readonly bool forceGround, noninfluencable, isStep;

        public Collision(RaycastHit hit, bool isStep = false)
        {
            gameObject = hit.collider.gameObject;
            rigidbody = hit.collider.attachedRigidbody;
            gravityUser = hit.collider.gameObject.GetComponent<IGravityUser>();
            collider = hit.collider;
            point = hit.point;
            normal = hit.normal;
            this.isStep = isStep;
            pointVelocity = rigidbody ?
                rigidbody.GetPointVelocity(point) : gravityUser != null ?
                gravityUser.Velocity :
                Vector3.zero;

            SurfaceFlagObject sfo = (rigidbody ? rigidbody.gameObject : gameObject).GetComponent<SurfaceFlagObject>();

            if (sfo)
            {
                forceGround = sfo._SurfaceFlags.Contains(SurfaceFlags.Stairs);
                noninfluencable = sfo._SurfaceFlags.Contains(SurfaceFlags.NoInfluence);
            }
        }
    }
}
