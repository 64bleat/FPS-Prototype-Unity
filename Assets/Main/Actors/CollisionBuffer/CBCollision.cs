using MPWorld;
using UnityEngine;

namespace MPCore
{
	public struct CBCollision
	{
		public GameObject gameObject;
		public Rigidbody rigidbody;
		public Collider collider;
		public IGravityUser gravityUser;
		public Vector3 point;
		public Vector3 normal;
		public Vector3 pointVelocity;
		public Vector3 hitVelocity;
		public float distance;
		public bool isAlwaysGround;
		public bool isNotInfluenceable;
		public bool isStep;

		private void SetValues()
		{
			gameObject.TryGetComponent(out SurfaceFlagObject sfoGet);
			SurfaceFlagObject sfo = sfoGet;

			if (rigidbody)
			{
				pointVelocity = rigidbody.GetPointVelocity(point);

				if (rigidbody.gameObject.TryGetComponent(out sfoGet))
					sfo = sfoGet;
			}
			else if (gravityUser != null)
			{
				pointVelocity = gravityUser.Velocity;
			}

			if (sfo)
			{
				isAlwaysGround = sfo._SurfaceFlags.Contains(global::MPWorld.SurfaceFlags.Stairs);
				isNotInfluenceable = sfo._SurfaceFlags.Contains(global::MPWorld.SurfaceFlags.NoInfluence);
			}
		}

		private static IGravityUser GetGravityUser(Transform t)
		{
			while (t)
			{
				if (t.TryGetComponent(typeof(IGravityUser), out Component c))
					return c as IGravityUser;
				else
					t = t.parent;
			}

			return default;
		}

		//public CBCollision(ControllerColliderHit hit, bool isStep = false)
		//{
		//    gameObject = hit.gameObject;
		//    rigidbody = hit.collider.attachedRigidbody;
		//    collider = hit.collider;
		//    gravityUser = GetGravityUser(hit.collider.transform);//hit.collider.GetComponentInParent<IGravityUser>();
		//    point = hit.point;
		//    normal = hit.normal;
		//    isAlwaysGround = false;
		//    isNotInfluenceable = false;
		//    this.isStep = isStep;
		//    pointVelocity = Vector3.zero;
		//    hitVelocity = Vector3.zero;

		//    SetValues();
		//}

		public CBCollision(RaycastHit hit, Vector3 hitVelocity = default, bool isStep = false)
		{
			gameObject = hit.collider.gameObject;
			rigidbody = hit.rigidbody;
			collider = hit.collider;
			gravityUser = GetGravityUser(hit.collider.transform);//hit.collider.GetComponentInParent<IGravityUser>();
			point = hit.point;
			normal = hit.normal;
			isAlwaysGround = false;
			isNotInfluenceable = false;
			this.isStep = isStep;
			pointVelocity = Vector3.zero;
			this.hitVelocity = hitVelocity;
			distance = hit.distance;

			SetValues();
		}

		public CBCollision(Collider collider, Vector3 normal, Vector3 point, Vector3 hitVelocity = default, bool isStep = false)
		{
			gameObject = collider.gameObject;
			rigidbody = collider.attachedRigidbody;
			this.collider = collider;
			gravityUser = GetGravityUser(collider.transform);//hit.collider.GetComponentInParent<IGravityUser>();
			this.point = point;
			this.normal = normal.normalized;
			isAlwaysGround = false;
			isNotInfluenceable = false;
			this.isStep = isStep;
			pointVelocity = Vector3.zero;
			this.hitVelocity = hitVelocity;
			distance = 0f;

			SetValues();
		}
	}
}
