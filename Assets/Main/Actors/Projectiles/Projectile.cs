using MPWorld;
using System.Collections.Generic;
using UnityEngine;

namespace MPCore
{
    [SelectionBase]
    public class Projectile : MonoBehaviour, IGravityUser
    {
        public ProjectileShared shared;
        public Transform visuals;
        public Transform trail;

        [HideInInspector] public GameObject instigator;

        private const int frameBounces = 10;
        private float lifeTime;
        private float travelDistance;
        private bool hasHitWall;
        private SphereCollider sphere;
        private MaterialPropertyBlock mpb;
        private Renderer[] renderers;

        private static LayerMask mask;
        private static LayerMask playerMask;
        private static readonly Collider[] cBuffer = new Collider[5];
        private static readonly string[] maskNames = new string[]{
            "Default",
            "Physical",
            "Player"};

        //IGravityUser
        public List<GravityZone> GravityZones { get; set; } = new List<GravityZone>();
        public Vector3 Velocity { get; set; }
        public Vector3 Gravity { get; set; }
        public float Mass { get; } = 1;

        private void Awake()
        {
            sphere = GetComponent<SphereCollider>();
            mask = LayerMask.GetMask(maskNames);
            playerMask = LayerMask.GetMask("Player");
            mpb = new MaterialPropertyBlock();
            renderers = GetComponentsInChildren<Renderer>();
        }

        private void OnEnable()
        {
            // color creation
            Color originalColor = Color.HSVToRGB(Mathf.Clamp01(Random.Range(0.02f, 0.12f) + shared.hue), 0.6f, 4f, true);
            mpb.SetColor("_EmissionColor", originalColor);
            foreach(Renderer r in renderers)
                r.SetPropertyBlock(mpb);
            
            // mechanics
            hasHitWall = false;
            transform.localScale = new Vector3(1, 1, 1);
            lifeTime = Random.Range(-shared.lifeSpanDeviation, shared.lifeSpanDeviation);
            travelDistance = 0f;
            Velocity = transform.forward * shared.exitSpeed;

            GravityZones.Clear();
        }

        private void FixedUpdate()
        {
            Vector3 position = transform.position;
            Quaternion rotation = transform.rotation;
            float dt = Time.fixedDeltaTime;

            Gravity = GravityZone.GetPointGravity(position, GravityZones);

            lifeTime += dt;

            // Subfixed Update
            float sdt = dt; // sub-delta-time
            int fb = frameBounces;
            while (Velocity.sqrMagnitude != 0 && sdt > 0 && fb-- > 0)
            {
                if (Physics.SphereCast(position, sphere.radius, Velocity, out RaycastHit hit, Velocity.magnitude * sdt, mask))
                {
                    if (hasHitWall || hit.collider.gameObject != instigator || travelDistance > 2f)
                    {
                        travelDistance += hit.distance;
                        position += Velocity.normalized * hit.distance;
                        sdt -= hit.distance / Velocity.magnitude;
                        Hit(hit);
                    }
                }
                else
                    break;
            }

            if (sdt > 0)
            {
                travelDistance += (Velocity * sdt).magnitude;
                position += Velocity * sdt;
            }

            Velocity += 2 * Gravity * dt * shared.gravityFactor;
            transform.SetPositionAndRotation(
                position,
                Quaternion.Lerp(rotation, Quaternion.LookRotation(Velocity, transform.up), Velocity.sqrMagnitude));

            // SPARKLECANNON STUFF
            if (lifeTime > shared.lifeSpan)
            {
                transform.localScale *= 1f - 10 * dt;

                if (transform.localScale.x < 0.2f)
                    GameObjectPool.DestroyMember(gameObject);
            }

            // Visual offset effect
            if (!hasHitWall && visuals)
                visuals.localPosition -= Vector3.ClampMagnitude(visuals.localPosition, Mathf.Min(visuals.localPosition.magnitude, dt / 0.5f));

            // trail effect scale
            if (trail)
                trail.localScale = new Vector3(1, 1, Mathf.Lerp(0, 6, Velocity.magnitude / 100f));
        }

        public virtual bool Hit(RaycastHit hit)
        {
            // OnHit += Momentum Transfer
            Vector3 momentum;
            float momentumMag = shared.hitMomentumTransfer;

            if (hit.collider.transform.TryGetComponentInParent(out Character character))
                momentumMag *= shared.characterHitMomentumScale;

            momentum = (Velocity - Vector3.ProjectOnPlane(Velocity, hit.normal) * (1 - shared.hitFrictionFactor)).normalized * momentumMag;
            momentum *= Velocity.magnitude / shared.exitSpeed;

            if (hit.collider.attachedRigidbody is var rb && rb && !rb.isKinematic)
            {
                momentum /= Mathf.Max(1, shared.minimumTransferMass / rb.mass);
                rb.AddForceAtPosition(momentum, hit.point, ForceMode.Impulse);
                Velocity += Vector3.Project(rb.velocity, hit.normal);   
            }
            else if(hit.collider.TryGetComponent(out IGravityUser gu))
            {
                momentum /= Mathf.Max(1, shared.minimumTransferMass / gu.Mass);
                gu.Velocity += momentum * Time.fixedDeltaTime;
                Velocity += Vector3.Project(gu.Velocity, hit.normal);
            }

            // OnHit += Reflection
            float hitDot = Vector3.Dot(Velocity.normalized, hit.normal);

            if (hitDot < 0)
                Velocity = Vector3.Reflect(Velocity * Mathf.Lerp(shared.bounceScaleMax, shared.bounceScaleMin, -hitDot), hit.normal);

            Velocity = Vector3.RotateTowards(Velocity, Random.onUnitSphere, shared.bounceAngle * Mathf.Deg2Rad * Random.value, 0);

            // OnHit += HitEffects
            if (visuals)
                visuals.localPosition = Vector3.zero;

            if (hit.collider.TryGetComponent(out CharacterBody body) && body.shotEffect
                && GameObjectPool.TryGetPool(body.shotEffect.gameObject, 100, out GameObjectPool pool))
            {
                GameObject b = pool.Spawn(hit.point, Random.rotation);

                if (b.TryGetComponent(out Rigidbody prb))
                    prb.velocity = Vector3.RotateTowards(Velocity * 0.25f, Random.insideUnitSphere, Random.value * 45f, 0);
            }
            else if (shared.wallHitParticle && Velocity.sqrMagnitude > 1)
            {
                pool = GameObjectPool.GetPool(shared.wallHitParticle.gameObject, 400);
                GameObject p = pool.Spawn(transform.position, transform.rotation);
                Transform pt = p.transform;

                if (p.TryGetComponent(out Rigidbody prb))
                    prb.velocity = Velocity;

                int childCount = p.transform.childCount;

                for (int i = 0; i < childCount; i++)
                    if (pt.GetChild(i).TryGetComponent(out MeshRenderer mr))
                        mr.SetPropertyBlock(mpb);
            }

            // OnHit += Character Hit
            if (hit.collider.TryGetComponentInParent(out character))
                CharacterHit(character, shared.hitDamage);
            else if (!hasHitWall)
            {
                int overlapCount = Physics.OverlapSphereNonAlloc(transform.position, sphere.radius, cBuffer, playerMask);

                while (overlapCount-- > 0)
                    if (cBuffer[overlapCount].TryGetComponent(out Character ch))
                        CharacterHit(ch, shared.hitDamage);

                hasHitWall = true;
            }



            return true;
        }

        public virtual void CharacterHit(Character target, int damage)
        { 
            target.Damage(damage, instigator, gameObject, shared.damageType);
            GameObjectPool.DestroyMember(gameObject);
        }

        /// <summary>
        /// Use Fire to spawn projectiles rather than Instantiate
        /// </summary>
        /// <param name="pool">the pool of projectile GameObjects to pull from</param>
        /// <param name="position">world spawn position</param>
        /// <param name="rotation">world spawn rotation</param>
        /// <param name="firePoint">where the projectile appears to be shot from</param>
        /// <param name="origin">where the projectile is coming from</param>
        /// <param name="relativeVel">Added to initial velocity</param>
        public static void Fire(GameObjectPool pool, Vector3 position, Quaternion rotation, Transform firePoint, GameObject origin, Vector3 relativeVel = default)
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
                        p.instigator = origin;

                        if (p.shared.randomSpeedDeviation)
                            p.Velocity *= 1f + Random.Range(-shared.speedDeviation, shared.speedDeviation);
                        else
                            p.Velocity *= 1f - shared.speedDeviation * (i - 1) / shared.fireCount;

                        p.Velocity += relativeVel;

                        // Projectiles move one frame immediately upon spawn
                        p.FixedUpdate();
                    }
                }
            }
        }
    }
}
