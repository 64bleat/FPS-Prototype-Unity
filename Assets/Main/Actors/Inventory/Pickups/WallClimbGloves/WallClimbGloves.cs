using UnityEngine;

namespace MPCore
{
    public class WallClimbGloves : Inventory
    {
        public bool onlyActivateOnCrouch = true;

        public override void OnActivate(GameObject owner)
        {
            if(owner.TryGetComponent(out CharacterBody body))
                body.wallClimb = this;
        }

        public override void OnDeactivate(GameObject owner)
        {
            if (owner.TryGetComponent(out CharacterBody body))
                body.wallClimb = null;
        }
    }
}
