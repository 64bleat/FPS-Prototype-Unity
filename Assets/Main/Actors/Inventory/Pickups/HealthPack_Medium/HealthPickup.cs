using MPCore;
using MPGUI;
using UnityEngine;

namespace MPCore
{
    public class HealthPickup : Inventory
    {
        [Header("Health")]
        public ResourceType restoreResourceType;
        public int restoreAmount = 25;
        public float percentOfMax = 0.5f;

        public override bool OnPickup(GameObject pickedBy)
        {
            if (pickedBy && restoreResourceType && pickedBy.TryGetComponent(out Character character))
                foreach (ResourceValue health in character.resources)
                    if (health.resourceType == restoreResourceType)
                    {
                        int healValue = (int)Mathf.Clamp(health.value + restoreAmount, health.value, health.maxValue * percentOfMax ) - health.value;

                        character.Heal(healValue, character.gameObject, character.characterInfo, pickedBy);

                        return healValue > 0;
                    }

            return false;
        }
    }
}
