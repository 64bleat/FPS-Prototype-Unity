using UnityEngine;

namespace MPCore
{
    public class Glider : Inventory
    {
        public float defaultGlideAngle = 60f;
        public float defaultSpeedToLift = 9f;
        public float defaultDrag = 2.5f;
        public float defaultJetAccel = 0f;
        public float defaultJetSpeed = 200f;

        public override void OnActivate(GameObject owner)
        {
            if (owner.TryGetComponent(out CharacterBody body))
            {
                if (body.glider)
                    body.glider.SetActive(owner, false);

                body.glider = this;
                active = true;
            }
        }

        public override void OnDeactivate(GameObject owner)
        {
            if (owner.TryGetComponent(out CharacterBody body))
                body.glider = null;

            active = false;
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
