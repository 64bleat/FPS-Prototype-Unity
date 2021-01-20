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
        public bool ricochet = true;

        public delegate void HitDelegate(RaycastHit hit);
        public event HitDelegate OnHit;

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

        private void Hit(MyRaycastHit hit)
        {
            // OnHit += Momentum Transfer
            Vector3 hitVelocity = this.body.Velocity;
            Vector3 momentum;
            float momentumMag = shared.hitMomentumTransfer;

            if (hit.collider.TryGetComponentInParent(out Character _))
                momentumMag *= shared.characterHitMomentumScale;

            momentum = (this.body.Velocity - Vector3.ProjectOnPlane(this.body.Velocity, hit.normal) * (1 - shared.hitFrictionFactor)).normalized * momentumMag;
            momentum *= this.body.Velocity.magnitude / shared.exitSpeed;

            if (hit.collider.attachedRigidbody is var rb && rb && !rb.isKinematic)
            {
                momentum /= Mathf.Max(1, shared.minimumTransferMass / rb.mass);
                rb.AddForceAtPosition(momentum, hit.point, ForceMode.Impulse);
                this.body.Velocity += Vector3.Project(rb.velocity, hit.normal);   
            }
            else if(hit.collider.TryGetComponent(out IGravityUser gu))
            {
                momentum /= Mathf.Max(1, shared.minimumTransferMass / gu.Mass);
                gu.Velocity += momentum * Time.fixedDeltaTime;
                this.body.Velocity += Vector3.Project(gu.Velocity, hit.normal);
            }

            // OnHit += Reflection


            // OnHit += HitEffects
            if (visuals)
                visuals.localPosition = Vector3.zero;

            // Hit Effects
            HitEffect hitEffect = default;
            SurfaceType surfaceType;

            if (hit.collider.TryGetComponent(out SurfaceFlagObject surface))
                surfaceType = surface.surfaceType;
            else
                surfaceType = null;

            foreach(HitEffect surfaceEffect in shared.hitEffects)
                if(surfaceEffect.surfaceType == surfaceType)
                {
                    hitEffect = surfaceEffect;
                    break;
                }


            Vector3 direction = Vector3.ProjectOnPlane(Random.insideUnitSphere, hit.normal);
            Quaternion rotation = Quaternion.LookRotation(direction, hit.normal);

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

            if (hit.collider.TryGetComponentInParent(out Character character))
                character.Damage(shared.hitDamage, gameObject, instigator, shared.damageType, hitVelocity);

            switch (hitEffect.hitBehaviour)
            {
                case HitEffect.ProjectileHitBehaviour.Destroy:
                    GameObjectPool.Return(gameObject);
                    break;
                case HitEffect.ProjectileHitBehaviour.Reflect:
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

                    break;
                case HitEffect.ProjectileHitBehaviour.Stick:
                    break;
                case HitEffect.ProjectileHitBehaviour.Nothing:
                    break;
                case HitEffect.ProjectileHitBehaviour.Explode:
                    break;
            }
        }

        public virtual void CharacterHit(Character target, int damage, Vector3 direction)
        { 
            target.Damage(damage, gameObject, instigator, shared.damageType, direction);
            GameObjectPool.Return(gameObject);
        }

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
