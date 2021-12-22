using UnityEngine;

namespace MPCore
{
	public class WallJumpBoots : Inventory
	{
		public override void OnActivate(GameObject owner)
		{
			if (owner.TryGetComponent(out CharacterBody body))
				body.OnWallJump.AddListener(OnWallJump);
		}

		public override void OnDeactivate(GameObject owner)
		{
			if (owner.TryGetComponent(out CharacterBody body))
				body.OnWallJump.RemoveListener(OnWallJump);
		}

		public void OnWallJump(CharacterBody body)
		{
			CollisionBuffer _cb = body.CollisionInfo;

			if (_cb.IsEmpty)
			{
				Vector3 origin = body.transform.position - body.transform.up * (body.HitBox.height * 0.5f - body.HitBox.radius);

				if ((Physics.SphereCast(origin, body.HitBox.radius * 0.9f, body.MoveDirection, out RaycastHit hit, body.HitBox.radius * 1.5f, body.CollisionMask, QueryTriggerInteraction.Ignore)
					|| Physics.SphereCast(origin, body.HitBox.radius * 0.9f, -body.MoveDirection, out hit, body.HitBox.radius * 1.5f, body.CollisionMask, QueryTriggerInteraction.Ignore))
					&& !hit.collider.transform.IsChildOf(body.transform))
					_cb.AddHit(new CBCollision(hit, body.Velocity));
			}

			if (!_cb.IsEmpty)
			{
				float jumpSpeed = Mathf.Sqrt(2f * 9.81f * body.JumpHeight);
				Vector3 iVel = body.Velocity - _cb.Velocity;
				Vector3 dVel = Vector3.ProjectOnPlane(body.Velocity, _cb.Normal)
					- body.Gravity.normalized * jumpSpeed * 0.3f
					+ Vector3.Reflect(iVel, _cb.Normal).normalized * jumpSpeed * 0.3f
					+ _cb.Normal * jumpSpeed * 1.2f;

				body.Velocity = _cb.LimitMomentum(dVel, iVel, body.defaultMaxKickVelocity) + _cb.Velocity;
				body.ResetJumpTimer();
				_cb.AddForce((iVel - body.Velocity) * body.defaultMass * 2);

				body.Sound.PlayJumpSound();
			}
		}
	}
}
