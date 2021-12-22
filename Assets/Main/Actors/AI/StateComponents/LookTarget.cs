using MPWorld;
using UnityEngine;

namespace MPCore.AI
{
	public class LookTarget : StateMachineBehaviour
	{
		public bool hostile = true;
		public float focusTime = 0.5f;

		private int id;
		private CharacterAIAnimator ai;
		private CharacterBody _body;
		private InventoryManager invContainer;
		private WeaponSwitcher weapons;
		private InputManager input;
		private const float mouseSpeed = 420;
		private const float mouseAcceleration = 45f;
		private float projectileSpeed = 100f;
		private Vector3 projectileOffset;
		private float velocityExtrapolation = 1f;
		private float accuracy = 0.6f;
		private Vector3 accuracyOffset;
		private float maxInaccuracyDistance = 5f;

		public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			animator.TryGetComponent(out ai);
			animator.TryGetComponent(out _body);
			animator.TryGetComponent(out invContainer);
			animator.TryGetComponent(out weapons);
			animator.TryGetComponent(out input);

			id = animator.gameObject.GetInstanceID() % 4096;
			input.OnMouseMove += OnMouseMove;
		}

		public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			input.OnMouseMove -= OnMouseMove;
		}

		private Vector2 OnMouseMove(float dt)
		{
			float tick = Time.frameCount + id;
			float lastSeen = Time.time - ai.visualTarget.lastSeen;
			Character character = ai.visualTarget.component as Character;
			Vector3 focalPoint;

			// Get Focal Point
			if (character && lastSeen < 0.5f)
			{
				if (character.TryGetComponent(out CharacterBody characterBody))
					focalPoint = characterBody.cameraAnchor.position;
				else
					focalPoint = character.transform.position;
			}
			else if (ai.touchTarget.component && Time.time - ai.touchTarget.lastSeen < 1.5f)
				focalPoint = ai.touchTarget.component.transform.position;
			else if (character && lastSeen < 1.5f)
				focalPoint = ai.visualTarget.mentalPosition;
			else if (ai.path.Count > 0)
			{
				Vector3 height = _body.transform.up * _body.HitBox.height;
				Navigator.ClampToPath(ai.path, ai.transform.position, out float pIndex);
				focalPoint = Navigator.PathLerp(ai.path, pIndex, 5f) + height;
			}
			else
				focalPoint = _body.View.position + _body.transform.forward * 1000f;

			// Shared values
			float targetDistance = Vector3.Distance(_body.View.position, focalPoint);

			// Velocity
			Vector3 targetVelocity = Vector3.zero;

			if (ai.visualTarget.component)
				if (ai.visualTarget.component.TryGetComponent(out IGravityUser gu))
					targetVelocity = gu.Velocity;
				else if (ai.visualTarget.component.TryGetComponent(out Collider collider))
					if (collider.attachedRigidbody is Rigidbody rb)
						targetVelocity = rb.velocity;

			// Weapon Switch
			if (tick % 120 == 0
				&& ai.visualTarget.component
				&& ai.visualTarget.component is Character)
			{
				(Weapon weapon, float priority) switchWeapon = (null, float.MaxValue);

				if (ai.visualTarget.component.TryGetComponent(out WeaponSwitcher theirs)
					&& theirs.currentWeapon
					&& theirs.currentWeapon.resourcePath == "Knife"
					&& targetDistance < 5f)
				{
					weapons.DrawWeapon(3);
				}
				else
				{
					foreach (Inventory i in invContainer.Inventory)
						if (i is Weapon w)
						{
							float dist = Mathf.Abs(w.preferredCombatDistance - targetDistance);

							if (dist < switchWeapon.priority)
								switchWeapon = (w, dist);
						}

					weapons.DrawWeapon(switchWeapon.weapon);
				}
			}

			// Recalculate predicted projectile speed
			if (tick % 30 == 0)
				if (weapons.currentWeapon
					&& weapons.currentWeapon.projectilePrimary
					&& weapons.currentWeapon.projectilePrimary.TryGetComponent(out Projectile p))
					projectileSpeed = p.shared.exitSpeed;
				else
					projectileSpeed = float.MaxValue * 0.5f;

			// Extrapolate Velocity
			if (tick % 1 == 0)
			{
				projectileOffset = targetVelocity * targetDistance / projectileSpeed;
				projectileOffset += UnityEngine.Random.insideUnitSphere * (1f - velocityExtrapolation) * projectileOffset.magnitude;
			}

			// Recalculate Accuracy
			if (tick % 60 == 0)
			{
				float velocityScale = Mathf.Clamp01(Vector3.ProjectOnPlane(targetVelocity - _body.Velocity * 0.5f, _body.View.forward).magnitude / _body.defaultSprintSpeed);
				accuracyOffset = Random.insideUnitSphere * (1f - accuracy) * velocityScale;
			}

			focalPoint += projectileOffset;
			focalPoint += accuracyOffset * Mathf.Min(maxInaccuracyDistance, targetDistance);

			// Get Look Direction
			Vector3 desiredDir = focalPoint - _body.View.position;

			// Combat
			if (tick % 15 == 0
				&& hostile
				&& lastSeen < 1f
				&& weapons.currentWeapon
				&& ai.visualTarget.component is Character
				&& weapons.currentWeapon.validFireAngle >= Vector3.Angle(desiredDir, _body.View.forward)
				&& weapons.currentWeapon.engagementRange >= Vector3.Distance(focalPoint, _body.View.position))
				input.BotKeyDown("Fire", 0.5f);

			// MouseLook
			Vector3 bodyUp = _body.transform.up;
			Vector3 bodyRight = _body.transform.right;
			Vector3 bodyForward = _body.transform.forward;
			Vector3 viewForward = _body.View.forward;
			Vector3 desiredDirX = Vector3.ProjectOnPlane(desiredDir, bodyUp);
			Vector3 desiredDirY = Vector3.ProjectOnPlane(desiredDir, bodyRight);
			float currentAngleY = Vector3.Angle(viewForward, bodyForward);
			float desiredAngleY = Vector3.Angle(desiredDirY, bodyForward);
			float deltaAngle = Vector3.Angle(viewForward, desiredDir);
			float currentSignY = Mathf.Sign(Vector3.Dot(viewForward, bodyUp));
			float desiredSignY = Mathf.Sign(Vector3.Dot(desiredDirY, bodyUp));
			float currentLookY = Mathf.PingPong(currentAngleY, 90) * currentSignY;
			float desiredLookY = Mathf.PingPong(desiredAngleY, 90) * desiredSignY;
			float mouseScale = mouseSpeed * Mathf.Clamp(deltaAngle / mouseAcceleration, 0.1f, 1f);
			float deltaLookX = Vector3.SignedAngle(bodyForward, desiredDirX, bodyUp);
			float deltaLookY = desiredLookY - currentLookY;
			Vector2 mouseDir = new Vector2(deltaLookX, deltaLookY);

			return Vector2.ClampMagnitude(mouseDir, mouseScale * dt);
		}
	}
}
