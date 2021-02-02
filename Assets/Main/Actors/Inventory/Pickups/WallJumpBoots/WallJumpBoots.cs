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
                body.wallJump = this;
        }

        public override void OnDeactivate(GameObject owner)
        {
            if (owner.TryGetComponent(out CharacterBody body))
                body.wallJump = null;
        }

        public void OnWallJump()
        {
           //TODO: move wall jump code from body to here.
        }
    }
}
