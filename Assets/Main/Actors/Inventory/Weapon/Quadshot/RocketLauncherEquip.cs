#pragma warning disable CS0108 // Member hides inherited member; missing new keyword

using MPGUI;
using MPWorld;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MPCore
{
	public class RocketLauncherEquip : MonoBehaviour
	{
		public Transform firePoint;
		public AudioClip fireSound;
		public Weapon weapon;

		private ParticleSystem muzzleFlash;
		private Animator animator;
		private InputManager input;
		private Character character;
		private CharacterBody body;
		private Transform attackPoint;
		private CharacterInfo instigator;
		private AudioSource audio;
		private GameObjectPool projectilePool;
		private JiggleDriver recoil;
		private int animFireId;
		private float fireWait;

		private static readonly string[] layerNames = new string[] { "Player", "Default", "Physical" };
		private static int layermask;

		private void Awake()
		{
			Component c = this;

			c.TryGetComponentInChildren(out animator);
			c.TryGetComponentInParent(out input);
			c.TryGetComponentInParent(out character);
			c.TryGetComponentInParent(out body);
			c.TryGetComponentInParent(out audio);
			c.TryGetComponentInChildren(out muzzleFlash);
			c.TryGetComponentInParent(out recoil);
			instigator = character.Info;

			projectilePool = GameObjectPool.GetPool(weapon.projectilePrimary, 100);

			if (c.TryGetComponentInParent(out CharacterBody cb))
				attackPoint = cb.cameraSlot;

			layermask = LayerMask.GetMask(layerNames);
			animFireId = Animator.StringToHash("Fire");
		}

		private void OnEnable()
		{
			FireUp();

			input.Bind("Fire", FireDown, this, KeyPressType.Down);
			input.Bind("Fire", FireUp, this, KeyPressType.Up);

			if (input.GetKey("Fire"))
				FireDown();
		}

		private void OnDisable()
		{
			input.Unbind(this);
		}

		private void FireDown()
		{
			animator.SetBool(animFireId, true);
		}

		private void FireUp()
		{
			animator.SetBool(animFireId, false);
		}

		public void Fire()
		{
			audio.Play();

			if (muzzleFlash)
				muzzleFlash.Play();

			if (recoil)
				recoil.AddForce(new Vector3(0, 0, -3f));

			ProjectileFire(projectilePool);

			fireWait = weapon.refireRatePrimary;
		}

		public void ProjectileFire(GameObjectPool projectilePool)
		{
			Vector3 point = GetFirePoint();

			Projectile.Fire(projectilePool, point, body.cameraSlot.rotation, firePoint, character.gameObject, character.Info, body.CollisionInfo.PlatformVelocity);
		}
		private Vector3 GetFirePoint()
		{
			float distance = body.HitBox.radius;

			if (Physics.Raycast(
				origin: body.cameraSlot.position,
				direction: body.cameraSlot.forward,
				hitInfo: out RaycastHit hit,
				maxDistance: distance,
				layerMask: layermask,
				queryTriggerInteraction: QueryTriggerInteraction.Ignore))
				distance = hit.distance;

			distance -= body.HitBox.radius * 0.25f;

			return body.cameraSlot.transform.position + body.cameraSlot.transform.forward * distance;
		}
	}
}
