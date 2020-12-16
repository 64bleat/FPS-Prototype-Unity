using MPCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MPCore
{
    public class Glider : Inventory
    {
        [SerializeField] public float defaultGlideAngle = 60f;
        [SerializeField] public float defaultSpeedToLift = 9f;
        [SerializeField] public float defaultDrag = 2.5f;
        [SerializeField] public float defaultJetAccel = 0f;
        [SerializeField] public float defaultJetSpeed = 200f;

        public override bool OnPickup(GameObject owner)
        {
            if (owner.GetComponent<CharacterBody>() is var cb && cb)
                cb.glider = this;

            return cb;
        }

        public override bool OnDrop(GameObject owner, Vector3 position, Quaternion rotation)
        {
            if (owner.GetComponent<CharacterBody>() is var cb && cb)
                cb.glider = null;

            return true;
        }

        public void OnGlide(CharacterBody cb, Vector3 zoneVelocity)
        {
            cb.Velocity -= zoneVelocity;
            float speedFactor = Mathf.Clamp01((cb.Velocity.magnitude - defaultSpeedToLift) / defaultSpeedToLift);

            cb.Velocity = Vector3.RotateTowards(cb.Velocity, cb.cameraAnchor.forward, defaultGlideAngle * Mathf.Deg2Rad * Time.fixedDeltaTime * speedFactor, 0f);
            cb.Velocity = Vector3.ClampMagnitude(cb.Velocity, Mathf.Max(0, cb.Velocity.magnitude - defaultDrag * Time.fixedDeltaTime * speedFactor));

            if (Vector3.Project(cb.Velocity, cb.cameraAnchor.forward).magnitude < defaultJetSpeed)
                cb.Velocity += cb.cameraAnchor.forward * defaultJetAccel * Time.fixedDeltaTime;
            cb.Velocity += zoneVelocity;
        }
    }
}
