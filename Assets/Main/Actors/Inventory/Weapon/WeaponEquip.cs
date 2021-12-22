using MPGUI;
using UnityEngine;
using System;

namespace MPCore
{
	public class WeaponEquip : MonoBehaviour
	{
		public Transform firePoint;
		public AudioClip fireSound;
		public LineRenderer ropePrefab;

		[NonSerialized] public GameObject owner;

		GameModel _gameModel;
		private ParticleSystem muzzleFlash;
		private Weapon weapon;
		private GameObjectPool projectilePool;
		private CharacterBody body;
		private Character character;
		private InputManager input;
		protected AudioSource audioSource;
		private JiggleDriver recoil;
		private float fireWait;
		private static int layerMask;
		private static readonly string[] layermask = new string[]{
			"Default", "Physical", "Player"};

		private void Awake()
		{
			_gameModel = Models.GetModel<GameModel>();
			_gameModel.isPaused.Subscribe(SetPaused);

			if (TryGetComponent(out InventoryItem inv))
				weapon = inv.item as Weapon;

			projectilePool = GameObjectPool.GetPool(weapon.projectilePrimary, 100);
			audioSource = GetComponent<AudioSource>();
			recoil = GetComponentInParent<JiggleDriver>();
			owner = GetComponentInParent<Character>().gameObject;
			body = GetComponentInParent<CharacterBody>();
			input = GetComponentInParent<InputManager>();
			character = GetComponentInParent<Character>();
			muzzleFlash = GetComponentInChildren<ParticleSystem>();

			layerMask = LayerMask.GetMask(layermask);
		}

		private void OnEnable()
		{
			input.Bind("Fire", TryFire, this, KeyPressType.Held);

			fireWait = weapon.refireRatePrimary;
		}

		private void OnDisable()
		{
			input.Unbind("Fire", TryFire);
		}

		private void OnDestroy()
		{
			_gameModel.isPaused.Unsubscribe(SetPaused);
		}

		private void SetPaused(DeltaValue<bool> paused)
		{
			enabled = !paused.newValue;
		}

		private void Update()
		{
			fireWait -= Time.deltaTime;
		}

		public void TryFire()
		{
			if (fireWait <= 0)
				Fire();
		}

		public void Fire()
		{
			if (audioSource)
				audioSource.Play();

			if (muzzleFlash)
				muzzleFlash.Play();

			if (recoil) 
				recoil.AddForce(new Vector3(0, 0, -3f));

			ProjectileFire(projectilePool);

			fireWait = weapon.refireRatePrimary;
		}

		/// <summary> Weapon fires a projectile </summary>
		/// <param name="projectilePool"> projectiles are pooled to avoid instantiation </param>
		public void ProjectileFire(GameObjectPool projectilePool)
		{
			Vector3 point = GetFirePoint();

			Projectile.Fire(projectilePool, point, body.cameraSlot.rotation, firePoint, owner, character.Info, body.CollisionInfo.PlatformVelocity);
		}

		/// <summary> Ensures projectiles don't shoot through close walls </summary>
		private Vector3 GetFirePoint()
		{
			float distance = body.HitBox.radius;

			if (Physics.Raycast(
				origin: body.cameraSlot.position,
				direction: body.cameraSlot.forward,
				hitInfo: out RaycastHit hit,
				maxDistance: distance,
				layerMask: layerMask,
				queryTriggerInteraction: QueryTriggerInteraction.Ignore))
				distance = hit.distance;

			distance -= body.HitBox.radius * 0.25f;

			return body.cameraSlot.transform.position + body.cameraSlot.transform.forward * distance;
		}

		// GRAPPLE TEST

		//Grapple
		//public Transform grappleTransform;
		//private Collider grappleCollider;
		//public Vector3 grapplePoint;
		//private const float maxGrappleDistance = 25f;
		//private const float reelSpeedMax = 5f;
		//private const float reelSpeedAccel = 6f;
		//private float reelSpeed = 0;
		//private float grappleDistance;
		/*private void Grapple()
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
						CBCollision collision = new CBCollision(grappleCollider, grappleDirection.normalized, grappleWorldPos);
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
		*/

	}
}
