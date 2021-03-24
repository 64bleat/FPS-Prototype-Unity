#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
using System.Collections.Generic;
using UnityEngine;

namespace MPWorld
{
    public class GravitySampler : MonoBehaviour, IGravityUser
    {
        public List<GravityZone> GravityZones { get;  set; } = new List<GravityZone>();
        public Vector3 Gravity { get; set; } = Physics.gravity;
        public Vector3 Velocity { get; set; } = Vector3.zero;
        public Vector3 ZoneVelocity { get; private set; } = Vector3.zero;

        public float Mass => body.mass;

        private Collider collider;
        private Rigidbody body;

        private void Awake()
        {
            TryGetComponent(out collider);
            TryGetComponent(out body);
        }

        void FixedUpdate()
        {
            Gravity = GravityZone.GetVolumeGravity(collider, GravityZones, out Vector3 zoneVelocity);
            ZoneVelocity = zoneVelocity;
        }
    }
}
