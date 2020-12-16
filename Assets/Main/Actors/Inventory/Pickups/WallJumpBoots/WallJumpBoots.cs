using MPCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MPCore
{
    public class WallJumpBoots : Inventory
    {
        public override bool OnPickup(GameObject owner)
        {
            if (owner.GetComponent<CharacterBody>() is var cb && cb)
                cb.wallJump = this;

            return cb;
        }

        public override bool OnDrop(GameObject owner, Vector3 position, Quaternion rotation)
        {
            if (owner.GetComponent<CharacterBody>() is var cb && cb)
                cb.wallJump = null;

            return true;
        }

        public void OnWallJump()
        {
           //TODO: move wall jump code from body to here.
        }
    }
}
