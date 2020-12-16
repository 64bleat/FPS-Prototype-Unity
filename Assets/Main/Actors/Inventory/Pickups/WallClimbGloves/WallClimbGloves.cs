using MPCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MPCore
{
    public class WallClimbGloves : Inventory
    {
        public bool onlyActivateOnCrouch = true;

        public override bool OnPickup(GameObject owner)
        {
            if (owner.GetComponent<CharacterBody>() is var cb && cb)
                cb.wallClimb = this;

            return cb;
        }

        public override bool OnDrop(GameObject owner, Vector3 position, Quaternion rotation)
        {
            if (owner.GetComponent<CharacterBody>() is var cb && cb)
                cb.wallClimb = null;

            return true;
        }
    }
}
