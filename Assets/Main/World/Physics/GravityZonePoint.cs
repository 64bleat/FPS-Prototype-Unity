using System.Collections.Generic;
using UnityEngine;

namespace MPWorld
{
    /// <summary>
    /// Manages gravity zones using a single point;
    /// </summary>
    public class GravityZonePoint : MonoBehaviour, IGravityUser
    {
        [SerializeField] private float mass = 1f;
        public List<GravityZone> GravityZones { get; set; } = new List<GravityZone>();
        public Vector3 LocalGravity { get; set; } = Physics.gravity;
        public Vector3 Velocity { get; set; } = Vector3.zero;
        public float Mass { get => mass; }

        private void OnEnable()
        {
            GravityZones.Clear();
        }

        private void FixedUpdate()
        {
            LocalGravity = GravityZone.GetPointGravity(transform.position, GravityZones);
        }
    }
}
