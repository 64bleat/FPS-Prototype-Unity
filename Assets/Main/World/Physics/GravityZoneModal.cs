using System.Collections.Generic;
using UnityEngine;

namespace MPWorld
{
    /// <summary> 
    ///     Added by GravityZones to manage non-kinematic, gravity-using, Rigidbodies when they enter a gravity zone. 
    /// </summary>
    public class GravityZoneModal : MonoBehaviour, IGravityUser
    {
        private Rigidbody rb;

        public List<GravityZone> GravityZones { get; set; } = new List<GravityZone>();
        public Vector3 Gravity { get; set; }
        public Vector3 Velocity { 
            get => rb.velocity; 
            set => rb.velocity = value; }
        public float Mass { get => rb.mass; }

        private void OnEnable()
        {
            rb = GetComponent<Rigidbody>();
            rb.useGravity = false;
        }

        private void OnDestroy()
        {
            rb.useGravity = true;
        }

        void FixedUpdate()
        {
            Gravity = GravityZone.GetPointGravity(transform.position, GravityZones);
            rb.AddForce(Gravity, ForceMode.Acceleration);
        }
    }
}
