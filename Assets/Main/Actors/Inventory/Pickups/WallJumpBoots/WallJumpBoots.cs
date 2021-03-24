using MPCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MPCore
{
    public class WallJumpBoots : Inventory
    {
        public override void OnActivate(GameObject owner)
        {
            if (owner.TryGetComponent(out CharacterBody body))
                body.OnWallJump.AddListener(OnWallJump);
        }

        public override void OnDeactivate(GameObject owner)
        {
            if (owner.TryGetComponent(out CharacterBody body))
                body.OnWallJump.RemoveListener(OnWallJump);
        }

        public void OnWallJump(CharacterBody body)
        {
            if (body.cb.IsEmpty)
            {
                Vector3 origin = body.transform.position - body.transform.up * (body.cap.height * 0.5f - body.cap.radius);

                if ((Physics.SphereCast(origin, body.cap.radius * 0.9f, body.moveDir, out RaycastHit hit, body.cap.radius * 1.5f, body.layerMask, QueryTriggerInteraction.Ignore)
                    || Physics.SphereCast(origin, body.cap.radius * 0.9f, -body.moveDir, out hit, body.cap.radius * 1.5f, body.layerMask, QueryTriggerInteraction.Ignore))
                    && !hit.collider.transform.IsChildOf(body.transform))
                    body.cb.AddHit(new CBCollision(hit, body.Velocity));
            }

            if (!body.cb.IsEmpty)
            {
                float jumpSpeed = Mathf.Sqrt(2f * 9.81f * body.JumpHeight);
                Vector3 iVel = body.Velocity - body.cb.Velocity;
                Vector3 dVel = Vector3.ProjectOnPlane(body.Velocity, body.cb.Normal)
                    - body.Gravity.normalized * jumpSpeed * 0.3f
                    + Vector3.Reflect(iVel, body.cb.Normal).normalized * jumpSpeed * 0.3f
                    + body.cb.Normal * jumpSpeed * 1.2f;

                body.Velocity = body.cb.LimitMomentum(dVel, iVel, body.defaultMaxKickVelocity) + body.cb.Velocity;
                body.input.jumpTimer.Restart();
                body.cb.AddForce((iVel - body.Velocity) * body.defaultMass * 2);

                body.voice.PlayJump();
            }
        }
    }
}
