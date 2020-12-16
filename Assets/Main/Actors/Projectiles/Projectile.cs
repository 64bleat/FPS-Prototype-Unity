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
            float dt = Time.fixedDeltaTime;

            if (!hasHitWall && visuals)
                visuals.localPosition -= Vector3.ClampMagnitude(visuals.localPosition, Mathf.Min(visuals.localPosition.magnitude, dt / 0.5f));

            Gravity = GravityZone.GetPointGravity(transform.position, GravityZones);

            lifeTime += dt;

            //// overlap test
            //if (hasHitWall)
            //{
            //    int oCount = Physics.OverlapSphereNonAlloc(transform.position, sphere.radius, cBuffer, playerMask);

            //    //while (oCount-- > 0)
            //    for (int i = 0; i < oCount; i++)
            //    {
            //        Collider over = cBuffer[i];

            //        if (over.gameObject.TryGetComponent(out Character _)
            //            && Physics.ComputePenetration(over, over.transform.position, over.transform.rotation,
            //            sphere, transform.position, transform.rotation,
            //            out Vector3 direction, out float _))
            //            Hit(over, -direction, over.transform.position);
            //    }
            //}

            //detect hits: Projectiles are fast and can bounce multiple times in one frame. Limit the number of bounces to prevent possible infinite loops.
            float timeBudget = dt;
            int fb = frameBounces;
            while (Velocity.sqrMagnitude != 0 && timeBudget > 0 && fb-- > 0)
            {
                if (Physics.SphereCast(transform.position, sphere.radius, Velocity, out RaycastHit hit, Velocity.magnitude * timeBudget, mask))
                {
                    if (hit.collider.gameObject != instigator || (travelDistance > 2f && hit.collider.gameObject == instigator))
                    {
                        travelDistance += hit.distance;
                        transform.position += Velocity.normalized * hit.distance;
                        timeBudget = Mathf.Max(0, timeBudget - hit.distance / Velocity.magnitude);
                        Hit(hit.collider, hit.normal, hit.point);
                    }
                }
                else
                    break;
            }

            if (timeBudget > 0)
            {
                travelDistance += (Velocity * timeBudget).magnitude;
                transform.position += Velocity * timeBudget;
            }

            Velocity += 2 * Gravity * shared.gravityFactor * dt;

            // SPARKLECANNON STUFF
            if (lifeTime > shared.lifeSpan)
            {
                transform.localScale *= 1f - 10 * dt;

                if (transform.localScale.x < 0.2f)
                    GameObjectPool.DestroyMember(gameObject);
            }

            // trail effect rotation
            if (Velocity.magnitude > 1f)
                transform.rotation = Quaternion.LookRotation(Velocity, transform.up);

            // trail effect scale
            if (trail)
                trail.localScale = new Vector3(1, 1, Mathf.Lerp(0, 6, Velocity.magnitude / 100f));
        }

        public virtual bool Hit(Collider collider, Vector3 normal, Vector3 position)
        {
            Character character = null;
            Transform t = collider.transform;

            if (visuals)
                visuals.localPosition = Vector3.zero;

            while (t)
            {
                if (t.TryGetComponent(typeof(Character), out Component c))
                {
                    character = c as Character;
                    break;
                }

                t = t.parent;
            }

            hasHitWall = true;

            // Momentum Transfer
            Vector3 momentum;
            float momentumMag = shared.hitMomentumTransfer;

            if (character)
                momentumMag *= shared.characterHitMomentumScale;

            momentum = (Velocity - Vector3.ProjectOnPlane(Velocity, normal) * (1 - shared.hitFrictionFactor)).normalized * momentumMag;
            momentum *= Velocity.magnitude / shared.exitSpeed;

            if (collider.attachedRigidbody is var rb && rb && !rb.isKinematic)
            {
                momentum /= Mathf.Max(1, shared.minimumTransferMass / rb.mass);
                rb.AddForceAtPosition(momentum, position, ForceMode.Impulse);
                Velocity += Vector3.Project(rb.velocity, normal);   
            }
            else if(collider.TryGetComponent(typeof(IGravityUser), out Component c))
            {
                IGravityUser gu = c as IGravityUser;
                momentum /= Mathf.Max(1, shared.minimumTransferMass / gu.Mass);
                gu.Velocity += momentum * Time.fixedDeltaTime;
                Velocity += Vector3.Project(gu.Velocity, normal);
            }

            // Reflection
            float hitDot = Vector3.Dot(Velocity.normalized, normal);

            if (hitDot < 0)
                Velocity = Vector3.Reflect(Velocity * Mathf.Lerp(shared.bounceScaleMax, shared.bounceScaleMin, -hitDot), normal);

            Velocity = Vector3.RotateTowards(Velocity, Random.onUnitSphere, shared.bounceAngle * Mathf.Deg2Rad * Random.value, 0);

            ////overlapFix
            //{
            //    int oCount = Physics.OverlapSphereNonAlloc(transform.position, sphere.radius, cBuffer, mask);

            //    while(oCount-- > 0)
            //    {
            //        Collider c = cBuffer[oCount];

            //        if (Physics.ComputePenetration(sphere, transform.position, transform.rotation, c, c.transform.position, c.transform.rotation, out Vector3 dir, out float des))
            //            transform.position += dir * des;
            //    }
            //}

            // Hit Effects
            if (collider.TryGetComponent(out CharacterBody body) && body.shotEffect
                && GameObjectPool.TryGetPool(body.shotEffect.gameObject, 100, out GameObjectPool pool))
            {
                GameObject b = pool.Spawn(position, Random.rotation);

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

            if (character)
                CharacterHit(character, shared.hitDamage);

            return true;
        }

        public virtual void CharacterHit(Character target, int damage)
        { 
            target.Damage(damage, instigator, gameObject, shared.damageType);
            GameObjectPool.DestroyMember(gameObject);
        }

        public static void Fire(GameObjectPool pool, Vector3 position, Quaternion rotation, Transform firePoint, GameObject instigator, Vector3 relativeVel = default)
        {
            Projectile r = pool.resource.GetComponent<Projectile>();

            for(int i = r.shared.fireCount; i > 0; i--)
            {
                float angle = Random.Range(-r.shared.fireAngle, r.shared.fireAngle);
                float choke = Mathf.Pow(Mathf.Abs(angle / Mathf.Max(r.shared.fireAngle, float.Epsilon)), r.shared.fireAngleChoke);
                Quaternion direction = Quaternion.AngleAxis(angle * choke, Quaternion.AngleAxis(Random.Range(0f, 360f), firePoint.forward) * firePoint.up);
                GameObject o = pool.Spawn(position, direction * rotation);
                Projectile p = o.GetComponent<Projectile>();

                p.visuals.position = firePoint.position;
                p.instigator = instigator;

                if (p.shared.randomSpeedDeviation)
                    p.Velocity *= 1f + Random.Range(-r.shared.speedDeviation, r.shared.speedDeviation);
                else
                    p.Velocity *= 1f - r.shared.speedDeviation * (i - 1) / r.shared.fireCount;

                p.Velocity += relativeVel;

                p.FixedUpdate();
            }
        }
    }
}
