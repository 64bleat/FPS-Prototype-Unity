using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MPCore;
using System;

namespace MPWorld
{
	/// <summary> 
	///     Gravity Zones warp gravity in various ways for objects within their collider. 
	/// </summary>
	public class GravityZone : MonoBehaviour
	{
		public enum GravityType { Spherical, Cylindrical, Planar, Normal }

		public GravityType gravityType = GravityType.Planar;
		public float force = -9.81f;
		public Transform origin;

		static readonly List<float> _offsetBuffer = new List<float>();

		Collider _volume;

		public void Awake()
		{
			if (GetComponent<MeshRenderer>() is var mr && mr)
				mr.enabled = false;

			_volume = GetComponent<Collider>();

			if (!origin)
				origin = transform;
		}

		public static Vector3 SampleGravity(Collider collider, List<GravityZone> zones, out Vector3 zoneVelocity)
		{
			collider.TryGetComponent(out Transform ctransform);
			Vector3 gravity = Vector3.zero;
			Vector3 boundSize = collider.bounds.size;
			float totalOffset = 0;
			float minSize = 
				Mathf.Max(
					Mathf.Min(boundSize.x,
					Mathf.Min(boundSize.y, boundSize.z)),
					float.Epsilon);

			zoneVelocity = Vector3.zero;

			for (int i = 0; i < zones.Count; i++)
				if (Physics.ComputePenetration(collider, ctransform.position, ctransform.rotation,
					zones[i]._volume, zones[i].transform.position, zones[i].transform.rotation,
					out Vector3 direction, out float distance))
				{
					direction *= distance;
				   
					float offset = 
						Mathf.Max(
							Mathf.Min(Mathf.Abs(direction.x), boundSize.x),
						Mathf.Max(
							Mathf.Min(Mathf.Abs(direction.y), boundSize.y),
							Mathf.Min(Mathf.Abs(direction.z), boundSize.z)));

					totalOffset += offset;
					_offsetBuffer.Add(offset);
				}
				else
					_offsetBuffer.Add(0f);

			if (totalOffset > float.Epsilon)
				for (int i = 0; i < zones.Count; i++)
				{
					gravity += zones[i].ValueAt(ctransform) * _offsetBuffer[i] / totalOffset;

					if (zones[i]._volume.attachedRigidbody)
						zoneVelocity = Vector3.Max(zoneVelocity, zones[i]._volume.attachedRigidbody.velocity);
				}

			gravity = Vector3.Lerp(Physics.gravity, gravity, totalOffset / minSize);

			_offsetBuffer.Clear();

			return gravity;
		}

		public static Vector3 GetPointGravity(Vector3 point, List<GravityZone> zones)
		{
			if (zones.Count != 0)
			{
				Vector3 gravity = Vector3.zero;

				foreach (GravityZone zone in zones)
					gravity += zone.ValueAt(point);
				
				return gravity / zones.Count;
			}
			else
				return Physics.gravity;
		}

		Vector3 ValueAt(Transform t)
		{
			switch (gravityType)
			{
				case GravityType.Spherical:
					return origin.TransformVector(origin.InverseTransformPoint(t.position)).normalized * force;
				case GravityType.Cylindrical:
					return origin.TransformVector(Vector3.ProjectOnPlane(origin.InverseTransformPoint(t.position), new Vector3(0, 1, 0))).normalized * force;
				case GravityType.Planar:
					return origin.up * force;
				default:
					return Physics.gravity;
			}
		}

		Vector3 ValueAt(Vector3 worldPosition)
		{
			switch (gravityType)
			{
				case GravityType.Spherical:
					return origin.TransformVector(origin.InverseTransformPoint(worldPosition)).normalized * force;
				case GravityType.Cylindrical:
					return origin.TransformVector(Vector3.ProjectOnPlane(origin.InverseTransformPoint(worldPosition), new Vector3(0, 1, 0))).normalized * force;
				case GravityType.Planar:
					return origin.up * force;
				default:
					return Physics.gravity;
			}
		}

		void OnTriggerEnter(Collider collider)
		{
			IGravityUser gu = collider.gameObject.GetComponent<IGravityUser>();
			Rigidbody rb = collider.attachedRigidbody;

			if(gu == null && rb && rb.useGravity && !rb.isKinematic)
				gu = rb.gameObject.GetComponent<IGravityUser>() ?? rb.gameObject.AddComponent<GravityZoneModal>();

			gu?.GravityZones?.Add(this);
		}

		void OnTriggerExit(Collider other)
		{
			foreach(var gravityUser in other.GetComponents<IGravityUser>())
			{
				gravityUser.GravityZones.Remove(this);

				if (gravityUser is GravityZoneModal && gravityUser.GravityZones.Count == 0)
					Destroy(gravityUser as Component);
			}
		}
	}
}
