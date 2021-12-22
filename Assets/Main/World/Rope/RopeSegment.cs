using MPCore;
using System.Collections.Generic;
using UnityEngine;

namespace MPWorld
{
    public class RopeSegment : MonoBehaviour, IGravityUser, IInteractable
    {
        public float gravityScale = 1;

        public List<GravityZone> GravityZones { get; set; } = new List<GravityZone>();
        public Vector3 LocalGravity { get; set; }
        public Vector3 Velocity { get; set; }
        public float Mass { get; } = 1000;
        public bool grab = true;

        private Vector3 localHoldPosition = Vector3.zero;
        private Transform holder;
        private SphereCollider sphere;

        private readonly string[] layerNames = { "Default", "Physical", "Player" };
        private int layermask;

        public bool IsKenematic => holder;

        private void Awake()
        {
            sphere = GetComponent<SphereCollider>();
            layermask = LayerMask.GetMask(layerNames);
        }

        public void FixedUpdate()
        {
            Vector3 oldPos = transform.position;

            LocalGravity = GravityZone.SampleGravity(sphere, GravityZones, out _);
            Velocity += LocalGravity * Time.fixedDeltaTime * gravityScale;

            // Interact Follow
            if (holder && !grab)
            {
                Vector3 offset = (holder.TransformPoint(localHoldPosition) - transform.position);
                float minDist = 1f;

                if (offset.magnitude > minDist)
                    Velocity += (offset - offset.normalized * minDist + Vector3.up * 1.5f) * 0.25f * Time.fixedDeltaTime;
            }

            // RAYCAST MOVE
            if (!holder || (holder && !grab))
                if (Physics.SphereCast(transform.position, sphere.radius, Velocity, out RaycastHit hit, Velocity.magnitude * Time.fixedDeltaTime)
                    && hit.collider.gameObject.GetComponent<RopeSegment>() == null)
                    transform.position = hit.point + hit.normal * sphere.radius;
                else
                    transform.position += Velocity * Time.fixedDeltaTime;

            // OVERLAP FIX
            foreach (Collider overlap in Physics.OverlapSphere(transform.position, sphere.radius, layermask, QueryTriggerInteraction.Ignore))
                if (Physics.ComputePenetration(
                        sphere, transform.position, transform.rotation,
                        overlap, overlap.transform.position, overlap.transform.rotation,
                        out Vector3 direction, out float distance)
                    && distance < sphere.radius)
                    transform.position += direction * distance;

            // VELOCITY RECALC
            Velocity = (transform.position - oldPos) / Time.fixedDeltaTime;
        }

        public void OnInteractStart(GameObject other, RaycastHit hit)
        {
            if (!holder && other)
            {
                other = other.GetComponentInChildren<CharacterView>().gameObject;

                if (other)
                {
                    localHoldPosition = other.transform.InverseTransformPoint(transform.position);
                    holder = other.transform;
                }
            }
            else holder = null;
        }
        public void OnInteractHold(GameObject other, RaycastHit hit)
        {
            if (holder && grab)
                transform.position = holder.TransformPoint(localHoldPosition);
        }
        public void OnInteractEnd(GameObject other, RaycastHit hit)
        {
            if (grab)
            {
                holder = null;
            }
        }
    }
}