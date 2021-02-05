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
                body.OnGlide += OnGlideNew;

                //if (body.glider)
                //    body.glider.SetActive(owner, false);

                //body.glider = this;
            }
        }

        public override void OnDeactivate(GameObject owner)
        {

            if (owner.TryGetComponent(out CharacterBody body))
            {
                body.OnGlide -= OnGlideNew;
                //body.glider = null;
            }
        }

        public void OnGlideNew(CharacterBody body)
        {
            OnGlide(body, Vector3.zero);
        }

        public void OnGlide(CharacterBody cb, Vector3 zoneVelocity)
        {
            cb.Velocity -= cb.zoneVelocity;
            float speedFactor = Mathf.Clamp01((cb.Velocity.magnitude - defaultSpeedToLift) / defaultSpeedToLift);

            cb.Velocity = Vector3.RotateTowards(cb.Velocity, cb.cameraAnchor.forward, defaultGlideAngle * Mathf.Deg2Rad * Time.fixedDeltaTime * speedFactor, 0f);
            cb.Velocity = Vector3.ClampMagnitude(cb.Velocity, Mathf.Max(0, cb.Velocity.magnitude - defaultDrag * Time.fixedDeltaTime * speedFactor));

            if (Vector3.Project(cb.Velocity, cb.cameraAnchor.forward).magnitude < defaultJetSpeed)
                cb.Velocity += cb.cameraAnchor.forward * defaultJetAccel * Time.fixedDeltaTime;
            cb.Velocity += cb.zoneVelocity;
        }
    }
}
