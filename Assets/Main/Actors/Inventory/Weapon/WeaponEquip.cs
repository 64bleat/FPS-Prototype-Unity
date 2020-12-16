using MPConsole;
using MPGUI;
using MPCore;
using UnityEngine;

namespace MPCore
{
    public class WeaponEquip : MonoBehaviour
    {
        public Weapon weapon;
        public Transform firePoint;
        public AudioClip fireSound;
        public LineRenderer ropePrefab;
        public HudBroadcaster hudBroadcaster;

        [HideInInspector] public GameObject owner;

        private GameObjectPool primProjPool;
        private CharacterBody body;
        private Character character;
        private InputManager input;
        protected AudioSource audioSource;
        private ImpactJiggler recoil;
        private LineRenderer rope;
        private readonly StateMachine state = new StateMachine();
        private static int layerMask;
        private static readonly string[] layermask = new string[]{
            "Default", "Physical", "Player"};

        //Grapple
        public Transform grappleTransform;
        private Collider grappleCollider;
        public Vector3 grapplePoint;
        private const float maxGrappleDistance = 25f;
        private const float reelSpeedMax = 5f;
        private const float reelSpeedAccel = 6f;
        private float reelSpeed = 0;
        private float grappleDistance;

        private void Awake()
        {
            primProjPool = GameObjectPool.GetPool(weapon.projectilePrimary, 100);

            audioSource = GetComponent<AudioSource>();
            recoil = GetComponentInParent<ImpactJiggler>();
            owner = GetComponentInParent<Character>().gameObject;
            body = GetComponentInParent<CharacterBody>();
            input = GetComponentInParent<InputManager>();
            character = GetComponentInParent<Character>();

            layerMask = LayerMask.GetMask(layermask);

            input.Bind("AltFire", Grapple, this, KeyPressType.Down);
            input.Bind("AltFire", GrappleRelease, this, KeyPressType.Up);
            input.Bind("Fire", SwitchToFire, this, KeyPressType.Held);

            state.Add(new State("Idle"));
            state.Add(new State("Firing", start: FiringBegin, update: FiringUpdate));
            state.Initialize("Idle");
        }

        private void OnEnable()
        {
            if (character && character.isPlayer && hudBroadcaster)
                hudBroadcaster.OnSetCrosshair.Broadcast(weapon.crosshair);
        }

        private void OnDisable()
        {
            if (character && character.isPlayer && hudBroadcaster)
                hudBroadcaster.OnSetCrosshair.Broadcast(null);
        }

        private void Update()
        {
            if(!Console.Paused)
                state.Update();
        }

        private void FixedUpdate()
        {
            GrappleUpdate();

            if(!Console.Paused)
                state.FixedUpdate();
        }

        // GRAPPLE TEST
        private void Grapple()
        {
            if (Physics.Raycast(body.cameraSlot.position, body.cameraSlot.forward, out RaycastHit hit, maxGrappleDistance, layerMask, QueryTriggerInteraction.Ignore))
            {
                grappleTransform = hit.transform;
                grapplePoint = grappleTransform.InverseTransformPoint(hit.point);
                grappleCollider = hit.collider;
                grappleDistance = Vector3.Distance(body.transform.position, hit.point);
                reelSpeed = 0;
                rope = Instantiate(ropePrefab.gameObject).GetComponent<LineRenderer>();
                rope.positionCount = 2;
                rope.SetPositions(new Vector3[]
                {
                    firePoint.position,
                    hit.point
                });
            }
        }

        private void GrappleUpdate()
        {
            if (grappleTransform)
            {
                Vector3 grappleWorldPos = grappleTransform.TransformPoint(grapplePoint);
                Vector3 grappleDirection = grappleWorldPos - body.transform.position;
                float grappleDistance = grappleDirection.magnitude;

                rope.SetPositions(new Vector3[]{
                    firePoint.position,
                    grappleWorldPos});

                if (Input.GetKey(KeyCode.Mouse3))
                {
                    reelSpeed = Mathf.Clamp(reelSpeed - reelSpeedAccel * Time.fixedDeltaTime, -reelSpeedMax, 0);
                    this.grappleDistance = Mathf.Max(0, this.grappleDistance + reelSpeed * Time.fixedDeltaTime);
                    body.Velocity = Vector3.ProjectOnPlane(body.Velocity, grappleDirection) + grappleDirection.normalized * reelSpeed;
                }
                else
                {
                    reelSpeed = 0;
                }

                if (grappleDistance > this.grappleDistance)
                {
                    if (!Input.GetKey(KeyCode.Mouse4) || this.grappleDistance >= maxGrappleDistance)
                    {
                        CollisionBuffer.Collision collision = new CollisionBuffer.Collision(grappleCollider, grappleDirection.normalized, grappleWorldPos);
                        Vector3 snapPosition = grappleWorldPos - Vector3.ClampMagnitude(grappleDirection, this.grappleDistance);
                        body.Velocity = body.cb.ApplyCollision(body.Velocity, Vector3.zero, collision);
                        body.cb.ApplyCollision(-grappleDirection.normalized * reelSpeed, Vector3.zero, collision);

                        if (Vector3.Dot(body.cb.Normal, grappleDirection) < 1)
                            body.transform.position += Vector3.ProjectOnPlane(snapPosition - body.transform.position, body.cb.Normal);
                        else
                            body.transform.position = snapPosition;
                    }

                    this.grappleDistance = Vector3.Distance(body.transform.position, grappleWorldPos);
                }
            }
        }

        private void GrappleRelease()
        {
            grappleTransform = null;

            if(rope)
                Destroy(rope.gameObject);
        }

        //IDLE=================================================================
        private void SwitchToFire()
        {
            if (state.IsCurrentState("Idle"))
                state.SwitchTo("Firing");
        }

        //FIRING===============================================================
        public void FiringBegin()
        {
            //GameObjectPool projectilePool = GameObjectPool.GetPool(weapon.projectilePrimary, 100);

            if (audioSource)
                audioSource.Play();

            if (recoil) 
                recoil.AddForce(new Vector3(0, 0, -3f));

            ProjectileFire(primProjPool);
        }

        public void FiringUpdate()
        {
            if (state.StateTime >= weapon.refireRatePrimary)
                state.SwitchTo("Idle");
        }

        public void FiringEnd() { }

        //ALTFIRING============================================================

        public void AltFiringBegin() { }

        public void AltFiringUpdate() { }

        public void AltFiringEnd() { }

        /// <summary> Weapon fires a projectile </summary>
        /// <param name="projectilePool"> projectiles are pooled to avoid instantiation </param>
        public void ProjectileFire(GameObjectPool projectilePool)
        {
            float distance = body.cap.radius * 2f;

            if (Physics.Raycast(
                origin: body.cameraSlot.position,
                direction: body.cameraSlot.forward,
                hitInfo: out RaycastHit hit,
                maxDistance: distance,
                layerMask: layerMask,
                queryTriggerInteraction: QueryTriggerInteraction.Ignore))
                distance = hit.distance;

            distance -= body.cap.radius * 0.25f;
            Vector3 point = body.cameraSlot.transform.position + body.cameraSlot.transform.forward * distance;

            Projectile.Fire(projectilePool, point, body.cameraSlot.rotation, firePoint, owner, body.lastPlatformVelocity);
        }
    }
}
