#pragma warning disable CS0108 // Member hides inherited member; missing new keyword

using System.Collections.Generic;
using UnityEngine;

namespace MPWorld
{
    /// <summary> 
    ///     Attaches to non-kinematic, gravity-using, Rigidbodies when they enter a gravity zone. 
    /// </summary>
    public class GravityZoneModal : MonoBehaviour, IGravityUser
    {
        private Rigidbody rigidbody;

        public List<GravityZone> GravityZones { get; set; } = new List<GravityZone>();
        public Vector3 Gravity { get; set; }
        public Vector3 Velocity { 
            get => rigidbody.velocity; 
            set => rigidbody.velocity = value; }
        public float Mass { get => rigidbody.mass; }

        private void OnEnable()
        {
            rigidbody = GetComponent<Rigidbody>();
            rigidbody.useGravity = false;
        }

        private void OnDestroy()
        {
            rigidbody.useGravity = true;
        }

        void FixedUpdate()
        {
            Gravity = GravityZone.GetPointGravity(transform.position, GravityZones);
            rigidbody.AddForce(Gravity, ForceMode.Acceleration);
        }
    }
}
