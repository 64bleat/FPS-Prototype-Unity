using UnityEngine;
using MPWorld;
using MPCore;
using System.Diagnostics;

namespace MPGUI
{
	/// <summary>
	/// Attatches to a camera owned by a CharacterBody and makes your equipment JIGGLE
	/// </summary>
	public class JiggleDriver : MonoBehaviour
	{
		public bool runSway = true;
		public float bodyImpactScale = 0.13f;
		public float swayScale = 0.6f;
		public float swayPeriod = 5;
		public float gravity = 9.81f;
		public float distanceMax = 0.05f;
		public float decceleration = 1f;

		private IGravityUser gravityUser;
		private CharacterBody body;
		private CharacterInput input;
		private Vector3 lastBodyVelocity;
		private Vector3 velocity;
		private Vector3 _origin;
		float animTimer;

		private void Awake()
		{
			gravityUser = GetComponentInParent<IGravityUser>();
			body = GetComponentInParent<CharacterBody>();
			input = GetComponentInParent<CharacterInput>();
			_origin = transform.localPosition;
		}

		void OnEnable()
		{
			lastBodyVelocity = gravityUser.Velocity;
		}

		void FixedUpdate()
		{
			float deltaTime = Time.fixedDeltaTime;

			if (deltaTime > 0f)
			{
				Vector3 oLocalPosition = transform.localPosition;
				Vector3 originOffset = transform.localPosition - _origin;

				// Drag
				//velocity -= velocity.normalized * Mathf.Min(velocity.magnitude, deccereration * deltaTime);
				velocity = Vector3.MoveTowards(velocity, Vector3.zero, decceleration * deltaTime);

				// Gravity Delta
				if (gravityUser != null)
				{
					Vector3 gravDelta = transform.parent.InverseTransformVector(gravityUser.Velocity - lastBodyVelocity);
					velocity -= gravDelta * bodyImpactScale;
					lastBodyVelocity = gravityUser.Velocity;
				}

				// Spring
				float springForce = Mathf.Min(originOffset.magnitude * 120, gravity);
				//velocity -= originOffset.normalized * Mathf.Min(originOffset.magnitude, gravity * deltaTime);
				velocity -= Vector3.ClampMagnitude(originOffset, springForce * deltaTime);

				// Running Animation
				if (runSway)
					if (Mathf.Approximately(body.MoveDirection.sqrMagnitude, 0f) || body.currentState != CharacterBody.MoveState.Grounded)
						animTimer = Time.time;
					else if (!input.Walk && !input.Crouch)
					{
						float bodySpeedFactor = Mathf.Clamp01(gravityUser.Velocity.magnitude / body.defaultMoveSpeed);
						float period = (Time.time - animTimer) * Mathf.PI * swayPeriod;
						float scale = swayScale * bodySpeedFactor;

						originOffset += Vector3.up * Mathf.Sin(period) * scale * deltaTime * deltaTime * 120;

						if (input.Sprint)
							originOffset += Vector3.right * Mathf.Sin(period / 2f) * scale * deltaTime * deltaTime * 120;
					}

				// Apply Velocity
				transform.localPosition = _origin + Vector3.ClampMagnitude(originOffset + velocity * deltaTime, distanceMax);

				// Recalculate Velocity
				velocity = (transform.localPosition - oLocalPosition) / deltaTime;
			}
		}

		public void AddForce(Vector3 acceleration)
		{
			velocity += acceleration;
		}
	}
}