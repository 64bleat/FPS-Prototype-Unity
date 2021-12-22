#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
using System.Collections.Generic;
using UnityEngine;

namespace MPWorld
{
	public class GravitySampler : MonoBehaviour, IGravityUser
	{
		Collider _collider;
		Rigidbody _rigidbody;

		public List<GravityZone> GravityZones { get; set; } = new List<GravityZone>();
		public Vector3 LocalGravity { get; set; } = Physics.gravity;
		public Vector3 Velocity { get; set; } = Vector3.zero;
		public Vector3 ZoneVelocity { get; private set; } = Vector3.zero;
		public float Mass => _rigidbody.mass;

		void Awake()
		{
			_collider = GetComponent<Collider>();
			_rigidbody = _collider.attachedRigidbody;
		}

		void FixedUpdate()
		{
			LocalGravity = GravityZone.SampleGravity(_collider, GravityZones, out Vector3 zoneVelocity);
			ZoneVelocity = zoneVelocity;
		}
	}
}
