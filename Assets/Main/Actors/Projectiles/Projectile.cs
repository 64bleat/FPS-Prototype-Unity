using MPWorld;
using System.Collections.Generic;
using UnityEngine;

namespace MPCore
{
    [SelectionBase]
    [RequireComponent(typeof(GravityZonePoint))]
    public class Projectile : MonoBehaviour
    {
        public ProjectileShared shared;
        public Transform visuals;

        //public delegate void HitDelegate(RaycastHit hit);
        //public event HitDelegate OnHit;

        private CharacterInfo instigator;
        private GameObject owner;
        private float lifeTime;
        private float travelDistance;
        private bool hasHitWall;
        private IGravityUser body;
        private SphereCollider sphere;

        private static LayerMask layerMask;
        private static LayerMask playerMask;
        private static readonly Collider[] cBuffer = new Collider[5];
        private static readonly string[] layerMaskNames = new string[]{
            "Default",
            "Physical",
            "Player"};

        private void Awake()
        {
            layerMask = LayerMask.GetMask(layerMaskNames);
            playerMask = LayerMask.GetMask("Player");

            TryGetComponent(out sphere);
            TryGetComponent(out body);
            
            //OnHit += Hit;
        }

        private void OnEnable()
        {            
            // mechanics
            hasHitWall = false;
            transform.localScale = new Vector3(1, 1, 1);
            lifeTime = Random.Range(-shared.lifeSpanDeviation, shared.lifeSpanDeviation);
            travelDistance = 0f;
            body.Velocity = transform.forward * shared.exitSpeed;
        }

        private void FixedUpdate()
        {
            Vector3 position = transform.position;
            Quaternion rotation = transform.rotation;
            float dt = Time.fixedDeltaTime;

            lifeTime += dt;

            // Subfixed Update
            float sdt = dt; // sub-delta-time
            int fb = shared.hitsPerFrame;
            while (body.Velocity.sqrMagnitude != 0 && sdt > 0 && fb-- > 0)
            {
                if (Physics.SphereCast(position, sphere.radius, body.Velocity, out RaycastHit hit, body.Velocity.magnitude * sdt, layerMask))
                {
                    if (hasHitWall || hit.collider.gameObject != owner || travelDistance > 2f)
                    {
                        travelDistance += hit.distance;
                        position += body.Velocity.normalized * hit.distance;
                        sdt -= hit.distance / body.Velocity.magnitude;
                        //OnHit?.Invoke(hit);
                        Hit(new MyRaycastHit(){
                            collider = hit.collider,
                            point = hit.point,
                            normal = hit.normal});

                        if (!gameObject || !gameObject.activeSelf)
                            return;
                    }
                }
                else
                    break;
            }

            if (sdt > 0)
            {
                travelDistance += body.Velocity.magnitude * sdt;
                position += body.Velocity * sdt;
            }

            body.Velocity += 2 * body.Gravity * dt * shared.gravityFactor;
            transform.SetPositionAndRotation(
                position,
                Quaternion.Lerp(rotation, Quaternion.LookRotation(body.Velocity, transform.up), body.Velocity.sqrMagnitude));

            // Make Fade Component
            if (lifeTime > shared.lifeSpan)
            {
                transform.localScale *= 1f - 10 * dt;

                if (transform.localScale.x < 0.2f)
                    GameObjectPool.Return(gameObject);
            }

            // Visual offset effect
            if (!hasHitWall && visuals)
                visuals.localPosition -= Vector3.ClampMagnitude(visuals.localPosition, Mathf.Min(visuals.localPosition.magnitude, dt / 0.5f));
        }

        private struct MyRaycastHit
        {
            public Collider collider;
            public Vector3 point;
            public Vector3 normal;
        }

        private void AddForce(Collider collider, float momentumMag, MyRaycastHit hit)
        {
            Vector3 momentum;
            Rigidbody rigidbody = collider.attachedRigidbody;

            if (collider.TryGetComponentInParent(out Character _))
                momentumMag *= shared.characterHitMomentumScale;

            momentum = (this.body.Velocity - Vector3.ProjectOnPlane(this.body.Velocity, hit.normal) * (1 - shared.hitFrictionFactor)).normalized * momentumMag;
            momentum *= this.body.Velocity.magnitude / shared.exitSpeed;

            if (rigidbody && !rigidbody.isKinematic)
            {
                momentum /= Mathf.Max(1, shared.minimumTransferMass / rigidbody.mass);
                rigidbody.AddForceAtPosition(momentum, hit.point, ForceMode.Impulse);
                this.body.Velocity += Vector3.Project(rigidbody.velocity, hit.normal);
            }
            else if (collider.TryGetComponent(out IGravityUser gravityUser))
            {
                momentum /= Mathf.Max(1, shared.minimumTransferMass / gravityUser.Mass);
                gravityUser.Velocity += momentum * Time.fixedDeltaTime;
                this.body.Velocity += Vector3.Project(gravityUser.Velocity, hit.normal);
            }
        }

        private void AddForce2(Collider collider, float momentumMag, MyRaycastHit hit)
        {
            Rigidbody rigidbody = collider.attachedRigidbody;
            Vector3 momentum = -hit.normal * momentumMag;

            if (collider.TryGetComponentInParent(out Character _))
                momentum *= shared.characterHitMomentumScale;

            if (rigidbody && !rigidbody.isKinematic)
            {
                momentum /= Mathf.Max(1, shared.minimumTransferMass / rigidbody.mass);
                rigidbody.AddForceAtPosition(momentum, hit.point, ForceMode.Impulse);
                this.body.Velocity += Vector3.Project(rigidbody.velocity, hit.normal);
            }
            else if (collider.TryGetComponent(out IGravityUser gravityUser))
            {
                momentum /= Mathf.Max(1, shared.minimumTransferMass / gravityUser.Mass);
                gravityUser.Velocity += momentum * Time.fixedDeltaTime;
                this.body.Velocity += Vector3.Project(gravityUser.Velocity, hit.normal);
            }
        }

        private void Hit(MyRaycastHit hit)
        {
            Vector3 hitVelocity = this.body.Velocity;
            Vector3 direction = Vector3.ProjectOnPlane(Random.insideUnitSphere, hit.normal);
            Quaternion rotation = Quaternion.LookRotation(direction, hit.normal);

            AddForce(hit.collider, shared.hitMomentumTransfer, hit);

            // OnHit += HitEffects
            if (visuals)
                visuals.localPosition = Vector3.zero;

            // Hit Effects
            HitEffect hitEffect = default;
            HitEffect nullEffect = default;
            SurfaceType surfaceType;

            if (hit.collider.TryGetComponent(out SurfaceFlagObject surface))
                surfaceType = surface.surfaceType;
            else
                surfaceType = null;

            foreach (HitEffect surfaceEffect in shared.hitEffects)
            {
                if (surfaceEffect.surfaceType == null)
                    nullEffect = surfaceEffect;

                if (surfaceEffect.surfaceType == surfaceType)
                {
                    hitEffect = surfaceEffect;
                    break;
                }
            }

            if (hitEffect.surfaceType == null)
                hitEffect = nullEffect;

            if (hitEffect.effect)
            {
                GameObjectPool p = GameObjectPool.GetPool(hitEffect.effect, 100);
                p.Spawn(hit.point, rotation);
            }

            if (hitEffect.hitMark)
            {
                GameObjectPool p = GameObjectPool.GetPool(hitEffect.hitMark, 100);
                p.Spawn(hit.point, rotation, hit.collider.transform);
            }

            if (hit.collider.TryGetComponentInParent(out DamageEvent damageEvenht))
                damageEvenht.Damage(shared.hitDamage, gameObject, instigator, shared.damageType, hitVelocity);

            switch (hitEffect.hitBehaviour)
            {
                case HitEffect.ProjectileHitBehaviour.Destroy:
                    GameObjectPool.Return(gameObject);
                    break;
                case HitEffect.ProjectileHitBehaviour.Reflect:
                    Reflect(hit);
                    break;
                case HitEffect.ProjectileHitBehaviour.Stick:
                    break;
                case HitEffect.ProjectileHitBehaviour.Nothing:
                    break;
                case HitEffect.ProjectileHitBehaviour.Explode:
                    Explode(hit);
                    break;
            }
        }

        private static readonly Collider[] explosionTargets = new Collider[20];
        private void Explode(MyRaycastHit hit)
        {
            if (shared.explosionEffect)
            {
                Vector3 direction = Vector3.ProjectOnPlane(Random.insideUnitSphere, hit.normal);
                Quaternion rotation = Quaternion.LookRotation(direction, hit.normal);
                GameObjectPool.GetPool(shared.explosionEffect, 50).Spawn(hit.point, rotation);
            }

            float keepRadius = sphere.radius;
            int count = Physics.OverlapSphereNonAlloc(transform.position, shared.explosionRadius, explosionTargets, layerMask, QueryTriggerInteraction.Ignore);

            sphere.radius = shared.explosionRadius;

            for(int i = 0; i < count; i++)
            {
                hit.collider = explosionTargets[i];

                if(Physics.ComputePenetration(hit.collider, hit.collider.transform.position, hit.collider.transform.rotation, sphere, transform.position, transform.rotation, out Vector3 direction, out float distance))
                {
                    float explodeFactor = Mathf.Sqrt(Mathf.Clamp01(distance / shared.explosionRadius));

                    if (hit.collider.TryGetComponent(out CharacterBody cb))
                        cb.currentState = CharacterBody.MoveState.Airborne;

                    if (!(hit.collider is MeshCollider mc) || mc.convex)
                        hit.point = hit.collider.ClosestPoint(transform.position);
                    else
                        hit.point = transform.position - direction * distance;

                    if (hit.collider.TryGetComponent(out DamageEvent damageEvent))
                    {
                        int damage = (int)(shared.explosionDamage * explodeFactor);

                        if(hit.collider.TryGetComponent(out Character character) && character.characterInfo == instigator)
                            damage = (int)(shared.selfDamageScale * damage);

                        damageEvent.Damage(damage, gameObject, instigator, shared.damageType, direction);
                    }

                    hit.normal = -direction;
                    AddForce2(hit.collider, shared.explosionMomentum * explodeFactor, hit);
                }
            }

            sphere.radius = keepRadius;
            GameObjectPool.Return(gameObject);
        }

        private void Reflect(MyRaycastHit hit)
        {
            float hitDot = Vector3.Dot(this.body.Velocity.normalized, hit.normal);

            if (hitDot < 0)
                this.body.Velocity = Vector3.Reflect(this.body.Velocity * Mathf.Lerp(shared.bounceScaleMax, shared.bounceScaleMin, -hitDot), hit.normal);

            this.body.Velocity = Vector3.RotateTowards(this.body.Velocity, Random.onUnitSphere, shared.bounceAngle * Mathf.Deg2Rad * Random.value, 0);

            if (!hasHitWall)
            {
                hasHitWall = true;
                int overlapCount = Physics.OverlapSphereNonAlloc(transform.position, sphere.radius, cBuffer, playerMask);

                for (int i = 0; i < overlapCount; i++)
                    if (cBuffer[i].TryGetComponent(out Character ch))
                    {
                        ch.TryGetComponent(out hit.collider);
                        hit.normal = -this.body.Velocity.normalized;
                        Hit(hit);
                    }
            }
        }

        //public virtual void CharacterHit(Character target, int damage, Vector3 direction)
        //{ 
        //    target.Damage(damage, gameObject, instigator, shared.damageType, direction);
        //    GameObjectPool.Return(gameObject);
        //}

        /// <summary>
        /// Use Fire to spawn projectiles rather than Instantiate
        /// </summary>
        /// <param name="pool">the pool of projectile GameObjects to pull from</param>
        /// <param name="position">world spawn position</param>
        /// <param name="rotation">world spawn rotation</param>
        /// <param name="firePoint">where the projectile appears to be shot from</param>
        /// <param name="owner">where the projectile is coming from</param>
        /// <param name="relativeVel">Added to initial velocity</param>
        public static void Fire(GameObjectPool pool, Vector3 position, Quaternion rotation, Transform firePoint, GameObject owner, CharacterInfo instigator, Vector3 relativeVel = default)
        {
            if (pool.resource.TryGetComponent(out Projectile r))
            {
                ProjectileShared shared = r.shared;

                for (int i = shared.fireCount; i > 0; i--)
                {
                    float randSpread = Random.value * shared.fireAngle;
                    float randAngle = Random.value * 360f;
                    Quaternion direction = Quaternion.AngleAxis(randSpread, Quaternion.AngleAxis(randAngle, firePoint.forward) * firePoint.up);
                    GameObject o = pool.Spawn(position, direction * rotation);

                    if (o.TryGetComponent(out Projectile p))
                    {
                        p.visuals.position = firePoint.position;
                        p.owner = owner;
                        p.instigator = instigator;

                        if (o.TryGetComponent(out IGravityUser gu))
                        {
                            if (p.shared.randomSpeedDeviation)
                                gu.Velocity *= 1f + Random.Range(-shared.speedDeviation, shared.speedDeviation);
                            else
                                gu.Velocity *= 1f - shared.speedDeviation * (i - 1) / shared.fireCount;

                            gu.Velocity += relativeVel;
                        }

                        // Projectiles move one frame immediately upon spawn
                        p.FixedUpdate();
                    }
                }
            }
        }
    }
}
